using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TipPanel : BasePanel
{
    public Button btnSure;
    public Text txtInfo;

    private bool isGamePauseTip;
    protected override void Init()
    {
        btnSure.onClick.AddListener(CloseTip);
    }

    /// <summary>
    /// 提供给外部，改变提示内容的方法
    /// </summary>
    /// <param name="info"></param>
    public void ChangeInfo(string info, bool puaseGame = false)
    {
        txtInfo.text = info;
        isGamePauseTip = puaseGame;

        //普通的提示
        if (!isGamePauseTip)
            return;

        //游戏中暂停
        Time.timeScale = 0;
        GameLevelMgr.Instance.playerInfo?.SetPlayerInputEnabled(false);
        UIMgr.Instance.GetPanel<GamePanel>().SetGamePanelInputEnabled(false);

        //游戏时间停止后没有淡入淡出效果，所以直接显示
        GetComponent<CanvasGroup>().alpha = 1;

        //解锁并显示鼠标
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    //关闭提示面板
    private void CloseTip()
    {
        if (isGamePauseTip)
        {
            //恢复游戏和输入
            Time.timeScale = 1;
            GameLevelMgr.Instance.playerInfo?.SetPlayerInputEnabled(true);
            UIMgr.Instance.GetPanel<GamePanel>()?.SetGamePanelInputEnabled(true);

            //重新锁定鼠标
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        UIMgr.Instance.HidePanel<TipPanel>();
    }
}