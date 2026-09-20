using UnityEngine;
using UnityEngine.UI;

public class BeginPanel : BasePanel
{
    public Button btnStart;
    public Button btnSetting;
    public Button btnAbouting;
    public Button btnExit;
    protected override void Init()
    {
        //开始按钮事件监听
        btnStart.onClick.AddListener(() =>
        {
            //逻辑需补充：开始游戏
            Camera.main.GetComponent<CameraAnimator>().TurnLeft(() =>
            {
                //显示选择英雄界面按钮
                UIMgr.Instance.ShowPanel<ChooseHeroPanel>();
            });
            UIMgr.Instance.HidePanel<BeginPanel>();
        });


        //设置按钮事件监听
        btnSetting.onClick.AddListener(() =>
        {
            //打开设置面板
            UIMgr.Instance.ShowPanel<SettingPanel>();
        });


        //关于按钮事件监听
        btnAbouting.onClick.AddListener(() =>
        {
            //逻辑需补充：打开关于面板
        });

        //退出按钮事件监听
        btnExit.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }
}
