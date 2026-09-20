using System.Linq;
using UnityEngine;

public class PlayerObject : MonoBehaviour
{
    private HeroConfig heroInfo;

    //英雄角色的状态机
    private Animator animator;
    //角色控制器，用于控制角色的移动
    private CharacterController heroController;
    //开局就拥有的金币数量
    public int obtainReward;

    //Collider用于近战的范围检测 RaycastHit用于射线检测
    private Collider[] colliders;
    private RaycastHit hitInfo;


    //开火点的位置
    //摄像机中心发射射线 → 得到准星瞄准点 → 枪口朝瞄准点发射射线
    //用于从屏幕中心计算准星瞄准位置
    private Camera playerCamera;
    //摄像机的脚本
    private CameraMove cameraMove;
    //false为单发，true为连发
    private bool isAutoFire;
    //模型上的枪口位置,检验枪口是否撞墙
    private Transform firePoint;
    //玩家当前是否在瞄准
    public bool IsAiming { get; private set; }


    //输入控制InputActions
    private GameInputActions inputActions;
    private bool playerInputEnabled = true;

    #region 角色移动的相关变量
    [SerializeField]
    private float gravityMultiplier = 2f;  //重力的 倍率
    [SerializeField]
    private float inputSmoothSpeed = 3f;  //键盘移动输入从0到1的过渡速度
    private const float groundVelocity = -2f;  //贴地时的重力速度，-2是一个经验值
    private float gravityVelocity = 0;  //重力方向的速度
    private Vector2 currentMoveInput;  //平滑处理后的移动输入
    #endregion

    #region 视角控制
    [SerializeField]
    private float horizontalLookSensitivity = 0.01f;

    [SerializeField]
    private float verticalLookSensitivity = 0.01f;
    #endregion


    //初始化
    private void Start()
    {
        //更新当前选择的英雄
        heroInfo = GameDataMgr.Instance.SelectedHero;
        if (heroInfo == null)
        {
            Debug.LogError("玩家初始化失败：没有选择有效的英雄配置");
            enabled = false;
            return;
        }
        //得到角色挂载的Animator组件和CharacterController组件
        animator = this.GetComponent<Animator>();
        heroController = this.gameObject.GetComponent<CharacterController>();

        //给触发器预先分配数组
        colliders = new Collider[10];

        //利用LINQ语法找到 firePoint这个子对象
        firePoint = this.GetComponentsInChildren<Transform>(false).FirstOrDefault(item => item.name == "firePoint");

        //获取摄像机的脚本
        playerCamera = Camera.main;
        cameraMove = playerCamera.GetComponent<CameraMove>();
        if (cameraMove == null)
        {
            Debug.LogError("玩家初始化失败：主摄像机没有挂载 CameraMove");
        }

    }

    #region 生命周期函数
    private void Awake()
    {
        inputActions = new GameInputActions();
    }

    private void OnEnable()
    {
        inputActions?.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions?.Player.Disable();
        UpdateAimState(false);
    }

    private void OnDestroy()
    {
        inputActions?.Dispose();
    }
    #endregion

    //每帧更新：玩家输入，移动逻辑
    private void Update()
    {
        if (!playerInputEnabled)
            return;

        //角色移动
        PlayerMoveWithGravity();
        //摄像机跟随角色移动
        PlayerLook();
        //鼠标右键瞄准
        PlayerAim();


        //操控逻辑
        //1.角色翻滚 ALT键
        if (inputActions.Player.Roll.WasPressedThisFrame())
        {
            animator.SetTrigger("Roll");
        }

        //2.角色开火
        //处理开火输入
        PlayerAttack();
        //3.LShift角色下蹲,下面使用Mathf.MoveTowards均匀过渡
        if (inputActions.Player.Crouch.IsPressed())
            animator.SetLayerWeight(animator.GetLayerIndex("Crouch Layer"),
                                    Mathf.MoveTowards(animator.GetLayerWeight(animator.GetLayerIndex("Crouch Layer")), 1, 7 * Time.deltaTime));
        else
            animator.SetLayerWeight(animator.GetLayerIndex("Crouch Layer"),
                                    Mathf.MoveTowards(animator.GetLayerWeight(animator.GetLayerIndex("Crouch Layer")), 0, 7 * Time.deltaTime));
    }


    /// <summary>
    /// 扳手攻击，利用范围碰撞器检测
    /// </summary>
    private void WrenchAttackEvent()
    {
        //播放音效
        GameDataMgr.Instance.PlaySound("Music/Knife");

        int hitCount = Physics.OverlapSphereNonAlloc(firePoint.position + firePoint.forward + firePoint.up, 2, colliders,
                                      1 << LayerMask.NameToLayer("Monster"), QueryTriggerInteraction.Collide);

        //逻辑需补充：攻击怪物
        for (int i = 0; i < hitCount; i++)
        {
            if (colliders[i] == null) continue;
            MonsterObj monsterObj = colliders[i].gameObject.GetComponent<MonsterObj>();
            if (monsterObj && !monsterObj.isDead)
            {
                //创建近战特效
                if (heroInfo.AttackEffectPrefab != null)
                {
                    Vector3 hitPos = colliders[i].ClosestPoint(firePoint.position);

                    PoolEffect.Spawn(heroInfo.AttackEffectPrefab, hitPos, Quaternion.LookRotation(firePoint.forward), 1f);
                }
                monsterObj.Wound(heroInfo.Attack);
                break;
            }
        }
    }


    /// <summary>
    /// 枪械攻击，利用射线检测器
    /// 进行了两次射线检测
    /// 摄像机射线决定准星正在瞄准哪里
    /// 枪口射线决定子弹能否真正到达那里
    /// 因此即使摄像机位于角色肩膀侧面，子弹也会从枪口飞向屏幕中心；如果枪口前方有墙，子弹也会先撞墙，不会穿过障碍物命中怪物
    /// </summary>
    private void GunAttackEvent()
    {
        //播放开枪音效
        GameDataMgr.Instance.PlaySound("Music/Gun");

        if (playerCamera == null || firePoint == null)
            return;

        float fireDistance = 1000f;

        //从屏幕正中心，也就是准星位置发射射线
        Ray cameraRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));

        //准心击中的默认位置 准星没有碰到物体时，默认瞄准摄像机前方最大射程处
        Vector3 aimPoint = cameraRay.origin + cameraRay.direction * fireDistance;

        //如果准心射线碰到物体 ，就将碰撞点作为瞄准位置，否则继续使用默认位置
        if (Physics.Raycast(
            cameraRay, out RaycastHit camreaHit, fireDistance,
              1 << LayerMask.NameToLayer("Monster"), QueryTriggerInteraction.Collide))
        {
            aimPoint = camreaHit.point;
        }

        //计算枪口指向准星的方向(归一化处理)
        Vector3 fireDirection = (aimPoint - firePoint.position).normalized;
        //计算枪口和准心瞄准位置的距离
        float distanceToAimPoint = Vector3.Distance(firePoint.position, aimPoint);

        //瞄准点位于碰撞器表面，射线多走 5 厘米，避免浮点误差造成末端漏判。
        const float hitDistancePadding = 0.1f;  //血泪教训，做笔记的时候重点！！！ 重点！！！ 重点！！！
        //真正的子弹射线仍然从枪口发出
        if (Physics.Raycast(
            firePoint.position, fireDirection, out hitInfo, distanceToAimPoint + hitDistancePadding,
                1 << LayerMask.NameToLayer("Monster"), QueryTriggerInteraction.Collide))
        {
            //碰撞器可能挂载怪物的子对象上
            MonsterObj monsterObj = hitInfo.collider.gameObject.GetComponentInParent<MonsterObj>();
            if (monsterObj != null && !monsterObj.isDead)
            {
                //创建子弹特效
                if (heroInfo.AttackEffectPrefab != null)
                {
                    PoolEffect.Spawn(heroInfo.AttackEffectPrefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal), 1f);
                }

                monsterObj.Wound(heroInfo.Attack);
            }
        }

    }


    /// <summary>
    /// 更新面板上显示金钱的
    /// </summary>
    private void UpdateMoney(int money)
    {
        EventCenter.Instance.EventTrigger<int>("金币变化", money);
    }


    /// <summary>
    /// 提供一个加钱的功能，玩家一开局就会有一定的金币数量
    /// </summary>
    /// <param name="money"></param>
    public void AddMoney(int money)
    {
        //局内的奖励加上新增奖励
        obtainReward += money;
        UpdateMoney(obtainReward);  //这样就能在GamePanel上更新奖励了:  局内奖励（玩家的金钱数）
    }


    /// <summary>
    /// 控制角色移动和旋转，手动赋予一个向下的重力速度，
    /// </summary>
    private void PlayerMoveWithGravity()
    {
        //控制前后左右动画
        Vector2 targetMoveInput = inputActions.Player.Move.ReadValue<Vector2>();
        currentMoveInput = Vector2.MoveTowards(currentMoveInput, targetMoveInput,
                                               inputSmoothSpeed * Time.deltaTime);
        animator.SetFloat("VSpeed", currentMoveInput.y);
        animator.SetFloat("HSpeed", currentMoveInput.x);

        //得到左右方向的速度
        Vector3 xzVelocity = transform.forward * currentMoveInput.y +
                             transform.right * currentMoveInput.x;
        //钳制XY轴速度单位向量的模长在0~1之间，防止斜向移动速度过快
        //符合角色2D混合动画的设计：（0，0）为原点半径为1的圆
        //钳制之后再乘以角色的速度，得到最终的XZ平面的速度（带方向的）
        xzVelocity = Vector3.ClampMagnitude(xzVelocity, 1) * heroInfo.MoveSpeed * 2f;

        //计算重力方向的速度
        //1.如果角色已经贴地，则给以-2 的贴地速度
        if (heroController.isGrounded && gravityVelocity < 0)
        {
            gravityVelocity = groundVelocity;
        }
        else
        {
            //否则继续计算重力方向的速度
            gravityVelocity += Physics.gravity.y * Time.deltaTime * gravityMultiplier;
        }

        //组合移动速度:XZ平面的速度+Y轴的重力速度
        //y轴单位向量默认为1，不需要钳制在0~1之间，否则重力效果不明显
        //Tips：重力方向要用世界坐标系的Vector3
        Vector3 moveVelocity = xzVelocity + Vector3.up * gravityVelocity;

        heroController.Move(moveVelocity * Time.deltaTime);

    }


    private void PlayerLook()
    {
        Vector2 lookInput = inputActions.Player.Look.ReadValue<Vector2>();

        // Mouse Delta 已经代表本帧鼠标位移，不再乘 Time.deltaTime。
        float yaw = lookInput.x *
                    horizontalLookSensitivity *
                    heroInfo.RotateSpeed;

        transform.Rotate(0, yaw, 0);

        if (cameraMove != null)
        {
            float pitchDelta = -lookInput.y * verticalLookSensitivity;
            cameraMove.AddPitch(pitchDelta);
        }
    }

    //根据鼠标右键切换瞄准镜头
    private void PlayerAim()
    {
        bool isAim = inputActions.Player.Aim.IsPressed();
        if (cameraMove != null)
            cameraMove.SetAim(isAim);

        UpdateAimState(isAim);
    }
    private void UpdateAimState(bool isAim)
    {
        if (IsAiming == isAim) return;

        IsAiming = isAim;
        EventCenter.Instance.EventTrigger<bool>("瞄准状态变化", isAim);
    }

    //处理角色单发、连发和设计模式切换
    private void PlayerAttack()
    {
        //按F键切换单发和连发
        if (inputActions.Player.ToggleFireMode.WasPressedThisFrame())
        {
            isAutoFire = !isAutoFire;

            //防止从连发切换到单发后，连发动画继续播放
            animator.SetBool("LoopFire", false);
        }

        //连发模式：左键按住时播放，松开时停止
        if (isAutoFire)
        {
            animator.SetBool(
                "LoopFire",
                inputActions.Player.Attack.IsPressed()
            );

            return;
        }

        //单发模式：左键每按下一次触发一次
        if (inputActions.Player.Attack.WasPressedThisFrame())
        {
            animator.SetTrigger("Fire");
        }
    }


    public void SetPlayerInputEnabled(bool isEnabled)
    {
        playerInputEnabled = isEnabled;

        if (isEnabled)
            inputActions.Player.Enable();
        else
            inputActions.Player.Disable();

        if (!isEnabled)
        {
            currentMoveInput = Vector2.zero;
            cameraMove?.SetAim(false);
            UpdateAimState(false);

            if (animator != null)
            {
                animator.SetFloat("VSpeed", 0);
                animator.SetFloat("HSpeed", 0);
                animator.SetBool("LoopFire", false);
            }
        }
    }
}
