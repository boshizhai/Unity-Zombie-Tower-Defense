using UnityEngine;
using UnityEngine.Events;

public class CameraAnimator : MonoBehaviour
{
    //主摄像机的Animaotr组件
    private Animator animtor;
    //用事件委托
    private UnityAction action;

    void Start()
    {
        animtor=this.gameObject.GetComponent<Animator>();
    }

    //左转
    public void TurnLeft(UnityAction action)
    {
        this.action = action;
        animtor.SetTrigger("TurnLeft");
    }


    //右转
    public void TurnRight(UnityAction action)
    {
        this.action = action;
        animtor.SetTrigger("TurnRight");
    }


    /// <summary>
    /// 播放动画完毕时自动调用，已添加到动画的时间轴窗口上
    /// </summary>
    public void PlayOver()
    {
        action?.Invoke();
        //调用后将事件置空
        action = null;
    }

}
