using UnityEngine;
using UnityEngine.Events;

public abstract class BasePanel : MonoBehaviour
{
    //专门用于控制面板透明度的组件
    private CanvasGroup canvasGroup;
    //面板淡入淡出速度
    private float fadeSpeed = 10;
    //控制当前面板是淡入还是淡出
    private bool isShow = false;
    //淡出时的事件回调函数，用于销毁当前面板等一系列操作的组件
    private UnityAction hideCallBack = null;


    protected virtual void Awake()
    {
        //在Awake中一开始就去获取CanvasGroup组件，如果没有就添加一个
        canvasGroup = this.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = this.gameObject.AddComponent<CanvasGroup>();
    }


    /// <summary>
    /// 初始化函数
    /// 注册控件事件的方法 所有的子面板 都需要去注册一些控件事件
    /// 所以写成抽象方法 让子类必须去实现
    /// </summary>
    protected abstract void Init();

    protected virtual void Start()
    {
        //在子脚本的Start函数中执行一次初始化
        Init();
    }

    public virtual void ShowMe()
    {
        canvasGroup.alpha = 0;
        isShow = true;
    }

    public virtual void HideMe(UnityAction hideCallBack = null)
    {
        canvasGroup.alpha = 1;
        isShow = false;
        //在HideMe中将hideCallBack赋值给成员变量hideCallBack，这样在Update中就可以调用这个回调函数了
        //通常外部传入的hideCallBack是一个销毁当前面板的函数，这样在淡出动画结束后就可以销毁当前面板了
        this.hideCallBack = hideCallBack;
    }

    /// <summary>
    /// 在Update中根据isShow的值来控制面板的淡入淡出效果
    /// </summary>
    protected virtual void Update()
    {
        switch (isShow)
        {
            //淡入效果
            case true:
                //只用在canvasGroup.alpha不等于1的时候才去执行淡入操作，这样可以避免重复执行
                if (canvasGroup.alpha != 1)
                {
                    //逐帧添加Alpha值，达到淡入效果
                    canvasGroup.alpha += Time.deltaTime * fadeSpeed;
                    //当Alpha值大于等于1的时候，将Alpha值设置为1，避免出现大于1的情况
                    if (canvasGroup.alpha >= 1)
                        canvasGroup.alpha = 1;
                }
                break;
            //淡出效果
            case false:
                //只用在canvasGroup.alpha不等于0的时候才去执行淡出操作，这样可以避免重复执行
                if (canvasGroup.alpha != 0)
                {
                    canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
                    if (canvasGroup.alpha <= 0)
                    {
                        canvasGroup.alpha = 0;
                        //在淡出动画结束后调用hideCallBack回调函数
                        //空条件运算符号 ?. 可以避免hideCallBack为null时调用Invoke()方法而报错
                        hideCallBack?.Invoke();
                    }
                }
                break;
        }
    }
}
