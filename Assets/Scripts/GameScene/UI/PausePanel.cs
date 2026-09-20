using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PausePanel : BasePanel
{
    public Button btnSure;
    public Button btnCancle;
    protected override void Init()
    {
        btnSure.onClick.AddListener(ReturnToBeginScene);
        btnCancle.onClick.AddListener(ResumeGame);
    }

    public override void ShowMe()
    {
        base.ShowMe();

        // 暂停后 Time.deltaTime 为 0，因此暂停面板直接显示，避免淡入停住。
        GetComponent<CanvasGroup>().alpha = 1;
        Time.timeScale = 0;
        GameLevelMgr.Instance.playerInfo?.SetPlayerInputEnabled(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    protected override void Update()
    {
        base.Update();

        // 暂停期间持续保持鼠标解锁，避免游戏窗口再次捕获鼠标。
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    //继续游戏
    public void ResumeGame()
    {
        Time.timeScale = 1;
        GameLevelMgr.Instance.playerInfo?.SetPlayerInputEnabled(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        UIMgr.Instance.HidePanel<PausePanel>(false);
    }

    //返回主菜单
    private void ReturnToBeginScene()
    {

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        UIMgr.Instance.HidePanel<PausePanel>(false);
        UIMgr.Instance.HidePanel<GamePanel>(false);
        UIMgr.Instance.ShowPanel<LoadingPanel>().LoadScene("BeginScene", () =>
        {
            //场景真正切换完成后，再清理数据并恢复时间
            GameLevelMgr.Instance.ClearInfo();
            Time.timeScale = 1;
        });
    }
}
