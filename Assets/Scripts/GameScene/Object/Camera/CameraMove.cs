using UnityEngine;

/// <summary>
/// 第三人称过肩摄像机：跟随角色移动，位置偏移到肩侧，
/// 视线方向始终与角色朝向平行（只叠加俯仰），
/// 保证屏幕正前方 = 角色 forward = 移动/开火方向。
/// </summary>
public class CameraMove : MonoBehaviour
{
    // 摄像机跟随的玩家
    [HideInInspector]
    public Transform playerTrans;

    [Header("镜头视角")]
    // 摄像机环绕中心点相对角色脚底的高度（约等于角色肩颈处）
    public float bodyHeight = 1.58f;

    // 环绕中心点到摄像机的本地偏移（x=左右，y=上下，z=前后/远近，负值为后方）
    public Vector3 offSet = new Vector3(0.69f, 0.14f, -3.67f);

    [Header("侧肩环绕")]
    [Tooltip("摄像机位置绕角色 Y 轴（playerTrans.up）环绕的角度。0 为正后方，正/负值让摄像机分别绕到左肩、右肩。只影响位置，不影响视线朝向。")]
    [Range(-90f, 90f)]
    [SerializeField]
    private float shoulderYawAngle = -7.2f;

    [Header("瞄准镜头")]
    //瞄准时的侧肩位置和视野大小
    public Vector3 aimOffSet = new Vector3(0.69f, 0.14f, -2.2f);
    public float aimFov = 45f;
    //过渡速度
    public float aimSpeed = 10f;

    [Header("镜头俯仰")]
    [SerializeField]
    private float minPitch = -35f;

    [SerializeField]
    private float maxPitch = 60f;

    // 当前俯仰角，由 AddPitch 累加，正值向上仰、负值向下俯
    private float currentPitch;

    [Header("镜头速度")]
    public float moveSpeed = 30;
    public float rotateSpeed = 20;

    private Vector3 targetPos;
    private Quaternion targetQua;
    private Vector3 followVelocity;
    private Vector3 currentOffSet;
    private Camera cam;
    private float normalFov;
    private bool isAim;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        currentOffSet = offSet;
        if (cam != null)
            normalFov = cam.fieldOfView;
    }

    private void LateUpdate()
    {
        if (playerTrans == null)
            return;

        UpdateAim();
        CalculatePosition();
        CalculateRotation();
    }

    //设置当前是否处于瞄准状态
    public void SetAim(bool value)
    {
        isAim = value;
    }

    //平滑切换普通镜头和瞄准镜头
    private void UpdateAim()
    {
        float lerpValue = 1f - Mathf.Exp(-aimSpeed * Time.deltaTime);
        currentOffSet = Vector3.Lerp(currentOffSet, isAim ? aimOffSet : offSet, lerpValue);

        if (cam != null)
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, isAim ? aimFov : normalFov, lerpValue);
    }

    /// <summary>
    /// 接收鼠标上下移动产生的俯仰增量，并在允许范围内累加。
    /// </summary>
    public void AddPitch(float pitchDelta)
    {
        currentPitch = Mathf.Clamp(currentPitch + pitchDelta, minPitch, maxPitch);
    }

    /// <summary>
    /// 计算并平滑移动摄像机位置：先把偏移环绕到肩侧，再叠加俯仰环绕。
    /// </summary>
    private void CalculatePosition()
    {
        //环绕的中心点（要乘上自定义的身体高度）
        Vector3 focusPoint = playerTrans.position + playerTrans.up * bodyHeight;

        //计算偏移转换到角色的本地方向
        Vector3 localOffest =
            playerTrans.right * currentOffSet.x +
            playerTrans.forward * currentOffSet.z +
            playerTrans.up * currentOffSet.y;

        //第一步：摄像机绕角色Y轴做绕肩 环绕角度shoulderYawAngle
        //得到四元数，语义：绕角色的y轴旋转多少角度
        Quaternion shoulderRotation = Quaternion.AngleAxis(shoulderYawAngle, playerTrans.up);
        //摄像机想要绕肩，就得把偏移位置也做绕肩:四元数乘以一个向量，就是把向量按照四元数记录的方式来旋转
        Vector3 shoulderOffset = shoulderRotation * localOffest;

        //第二步：绕肩环绕后的右方向，做摄像机俯仰环绕，实现上下看
        //必须用环绕后的右方向 否则俯仰轴会和水平偏移方向不垂直
        Vector3 pitchAxis = shoulderRotation * playerTrans.right;
        //俯仰（鼠标上下滑动的角度）也要绕肩之后的右方向，否则会出现俯仰会和偏移方向不垂直的情况
        Quaternion pitchRotation = Quaternion.AngleAxis(currentPitch, pitchAxis);
        Vector3 finalOffset = pitchRotation * shoulderOffset;

        targetPos = focusPoint + finalOffset;

        float smoothTime = 1f / Mathf.Max(moveSpeed, 0.01f);
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref followVelocity, smoothTime);
    }

    /// <summary>
    /// 计算并平滑转向摄像机朝向：视线始终与角色 forward 平行，只叠加俯仰，
    /// 不叠加 shoulderYawAngle —— 位置在肩侧、视线仍看向角色正前方，
    /// 这样屏幕中心方向才会和角色的移动/开火方向保持一致。
    /// </summary>
    private void CalculateRotation()
    {
        Vector3 lookDirection = Quaternion.AngleAxis(currentPitch, playerTrans.right) * playerTrans.forward;
        targetQua = Quaternion.LookRotation(lookDirection, playerTrans.up);

        float rotateLerp = 1f - Mathf.Exp(-rotateSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetQua, rotateLerp);
    }

    public void SetPlayer(Transform playerTrans)
    {
        this.playerTrans = playerTrans;

        if (playerTrans == null)
            return;

        // 清除平滑移动遗留的速度。
        followVelocity = Vector3.zero;

        // 复用现有计算，得到摄像机应该到达的位置和旋转。
        CalculatePosition();
        CalculateRotation();

        // 第一次直接就位，不播放从初始位置移动过来的过程。
        transform.SetPositionAndRotation(targetPos, targetQua);

        followVelocity = Vector3.zero;
    }
}
