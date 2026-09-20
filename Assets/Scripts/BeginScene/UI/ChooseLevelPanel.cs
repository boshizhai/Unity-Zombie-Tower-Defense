using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChooseLevelPanel : BasePanel
{
    //关联按钮组件
    public Button btnStart;
    public Button btnClose;
    public Button btnLeft;
    public Button btnRight;
    //图片和描述文本
    public Image imgmap;
    public Text txtIntro;

    //用于保存当前的场景信息
    private int nowIndex;
    private LevelConfig nowLevel;


    protected override void Init()
    {

        //开始按钮事件监听
        btnStart.onClick.AddListener(() =>
        {
            UIMgr.Instance.HidePanel<ChooseLevelPanel>();
            //逻辑需补充：显示开始场景

            //异步加载场景，确保场景加载完成后才显示面板，防止面板初始化内容被丢失
            UIMgr.Instance.ShowPanel<LoadingPanel>().LoadScene(nowLevel.SceneName, () =>
            {
                GameLevelMgr.Instance.InitGameInfo(nowLevel);
            });
        });


        //返回按钮事件监听
        btnClose.onClick.AddListener(() =>
        {
            UIMgr.Instance.HidePanel<ChooseLevelPanel>();
            UIMgr.Instance.ShowPanel<ChooseHeroPanel>();
        });


        //左选择按钮事件监听
        btnLeft.onClick.AddListener(() =>
        {
            nowIndex = --nowIndex < 0 ? GameDataMgr.Instance.Levels.Count - 1 : nowIndex;
            ChangeShowScene();
        });


        //右选择按钮事件监听
        btnRight.onClick.AddListener(() =>
        {
            nowIndex = ++nowIndex > GameDataMgr.Instance.Levels.Count - 1 ? 0 : nowIndex;
            ChangeShowScene();
        });

        //初始化的时候也需要改变场景信息
        ChangeShowScene();
    }


    private void ChangeShowScene()
    {
        //得到当前选中的场景信息
        nowLevel = GameDataMgr.Instance.Levels[nowIndex];

        //更改显示的图片
        imgmap.sprite = nowLevel.PreviewImage;

        //更改描述框
        txtIntro.text = "名称：" + nowLevel.LevelName + "\n\n" + "描述：" + nowLevel.Description;
    }
}
