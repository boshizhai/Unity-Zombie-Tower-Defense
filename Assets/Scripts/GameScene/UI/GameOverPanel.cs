using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverPanel : BasePanel
{
    public Button btnSure;
    public Text txtInfo;
    public Text txtAward;

    protected override void Init()
    {
        btnSure.onClick.AddListener(() =>
        {

            //立即移除结束面板
            UIMgr.Instance.HidePanel<GameOverPanel>(false);

            //清空当前关卡数据
            GameLevelMgr.Instance.ClearInfo();

            //鼠标保持解锁，进入开始场景
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            UIMgr.Instance.ShowPanel<LoadingPanel>().LoadScene("BeginScene", () =>
            {
                //切换场景后，在回调中恢复时间
                Time.timeScale = 1;
            });
        });
    }

    public override void ShowMe()
    {
        base.ShowMe();

        //暂停后淡入无法继续，所以直接显示结束面板
        GetComponent<CanvasGroup>().alpha = 1;

        //暂停玩家的输入和隐藏游戏的UI面板
        GameLevelMgr.Instance.playerInfo.SetPlayerInputEnabled(false);
        UIMgr.Instance.HidePanel<GamePanel>(false);

        //暂停游戏
        Time.timeScale = 0;

        //解锁并显示鼠标
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

    }

    /// <summary>
    /// 更新结束页面显示的数据
    /// </summary>
    /// <param name="money"></param>
    /// <param name="isWin"></param>
    public void InitInfo(int money, bool isWin)
    {
        txtInfo.text = "获得" + (isWin ? "通关" : "失败") + "奖励";
        txtAward.text = "￥" + money;
    }
}