using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GamePanel : BasePanel
{
    private GameInputActions inputActions;

    //控件关联
    public Text txtWave;
    public Text txtAward;
    public Text txtHP;
    public Image imgHp;
    private const int hpWidth = 320;  //HP血条的长宽，用于设置血条的长度

    #region 准星缩放
    public RectTransform imgCrosshair;
    private Vector3 normalCrosshairScale;
    private Vector3 targetCrosshairScale;
    private float crosshairScaleSpeed = 10;
    #endregion

    //防御塔创建面板的控件集合
    public List<TowerItem> towerItems = new List<TowerItem>();

    //下方造塔组合控件的父对象，主要用于控制显隐
    public Transform bottomTransform;
    GameObject heroobj;

    //当前靠近的造塔点，用于显示造塔面板
    private TowerBulidPoint nowSelectPoint;
    //是否显示造塔面板的标记
    private bool canCreateTower = false;

    protected override void Awake()
    {
        base.Awake();
        inputActions = new GameInputActions();
        inputActions.Player.Pause.performed += OnPause;

        normalCrosshairScale = imgCrosshair.localScale;
        targetCrosshairScale = normalCrosshairScale;
    }

    private void OnEnable()
    {
        //【A】更新金币
        inputActions?.Player.Enable();
        EventCenter.Instance.AddEventListener<int>("金币变化", UpdateMoney);

        //面板重新打开时 补上当前逻辑
        var player = GameLevelMgr.Instance.playerInfo;
        if (player != null)
            UpdateMoney(player.obtainReward);

        //【B】更新血量
        EventCenter.Instance.AddEventListener<MainTowerHpInfo>("主塔血量变化", OnTowerHpChanged);

        if (MainTowerObject.Instance != null)
            OnTowerHpChanged(MainTowerObject.Instance.HpInfo);

        //【C】更新僵尸波数
        EventCenter.Instance.AddEventListener<WaveInfo>("僵尸波数变化", OnWaveChanged);

        OnWaveChanged(GameLevelMgr.Instance.CurrentWaveInfo);

        //【D】造塔点更新
        EventCenter.Instance.AddEventListener<TowerBulidPoint>("造塔点更新", UpdateTowerCreatePanel);

        EventCenter.Instance.AddEventListener<TowerBulidPoint>("造塔点关闭", OnBuildPointClosed);

        //【E】准心变化
        EventCenter.Instance.AddEventListener<bool>("瞄准状态变化", SetCrosshairAim);
        // 复用前面金币逻辑已经声明的 player
        SetCrosshairAim(player != null && player.IsAiming);

    }

    private void OnDisable()
    {
        inputActions?.Player.Disable();

        EventCenter.Instance.RemoveEventListener<int>("金币变化", UpdateMoney);
        EventCenter.Instance.RemoveEventListener<MainTowerHpInfo>("主塔血量变化", OnTowerHpChanged);
        EventCenter.Instance.RemoveEventListener<WaveInfo>("僵尸波数变化", OnWaveChanged);
        EventCenter.Instance.RemoveEventListener<TowerBulidPoint>("造塔点更新", UpdateTowerCreatePanel);
        EventCenter.Instance.RemoveEventListener<TowerBulidPoint>("造塔点关闭", OnBuildPointClosed);
        EventCenter.Instance.RemoveEventListener<bool>("瞄准状态变化", SetCrosshairAim);
    }

    private void OnDestroy()
    {
        if (inputActions == null) return;

        inputActions.Player.Pause.performed -= OnPause;
        inputActions.Dispose();
    }


    private void OnPause(InputAction.CallbackContext context)
    {
        PausePanel pausePanel = UIMgr.Instance.GetPanel<PausePanel>();
        if (pausePanel == null)
            UIMgr.Instance.ShowPanel<PausePanel>();
        else
            pausePanel.ResumeGame();
    }


    protected override void Init()
    {

        //初始化中就隐藏炮塔选择面板
        bottomTransform.gameObject.SetActive(false);

    }



    /// <summary>
    /// 更新血条精度
    /// </summary>
    /// <param name="nowHp">当前血量</param>
    /// <param name="maxHp">最大血量</param>
    public void UpdateTowerHp(float nowHp, int maxHp)
    {
        //更新血条显示文字
        txtHP.text = nowHp + "/" + maxHp;
        //更新血条宽度
        imgHp.rectTransform.sizeDelta = new Vector2(Mathf.Clamp01(nowHp / maxHp) * hpWidth, 23.5f);
    }
    //提供给事件中心监听
    private void OnTowerHpChanged(MainTowerHpInfo info)
    {
        if (info.maxHp <= 0) return;

        UpdateTowerHp(info.nowHp, info.maxHp);
    }

    /// <summary>
    /// 更新当前僵尸波数
    /// </summary>
    private void UpdateWave(int remainNum, int totalNum)
    {
        txtWave.text = "剩余波数： " + remainNum + "/" + totalNum;
    }
    //提供给事件中心更新僵尸波数变化
    private void OnWaveChanged(WaveInfo info)
    {
        UpdateWave(info.remaining, info.total);
    }

    /// <summary>
    /// 提供给外部更新金币数量
    /// </summary>
    /// <param name="money"></param>
    public void UpdateMoney(int money)
    {
        txtAward.text = "奖励：" + money + "(" + GameDataMgr.Instance.playerData.haveMoney + ")";
    }




    /// <summary>
    /// 更新当前选中的造塔点 界面的一些变化
    /// </summary>
    /// <param name="bulidPoint"></param>
    public void UpdateTowerCreatePanel(TowerBulidPoint bulidPoint)
    {
        nowSelectPoint = bulidPoint;

        //更新是否可以造防御塔的标记，外部传入的Point为空时不可造塔，标志设置为fasle，然后在Updae中退出
        //如果时false的话，直接退出UpdateTowerCreatePanel函数
        if (!(canCreateTower = nowSelectPoint != null ? true : false))
        {
            //隐藏炮塔选择面板的父对象
            bottomTransform.gameObject.SetActive(false);
            return;
        }


        //下面开始更新炮塔选择面板的内容
        bottomTransform.gameObject.SetActive(true);
        //如果当前没有创建出造塔点，初始化造塔面板的信息
        if (nowSelectPoint.nowBulidTowerInfo == null)
        {
            for (int i = 0; i < towerItems.Count; i++)
            {
                //将可以选择的控件激活并初始化
                towerItems[i].gameObject.SetActive(true);
                towerItems[i].InitInfo(nowSelectPoint.OptionalTowers[i], "数字键" + (i + 1));
            }
        }
        //如果当前已有防御塔，显示它的升级信息
        else
        {
            //第一步：需要将所有的炮塔选择控件隐藏
            for (int i = 0; i < towerItems.Count; i++)
            {
                towerItems[i].gameObject.SetActive(false);
            }
            //第二步：将升级控件显示出来，并初始化升级信息
            towerItems[0].gameObject.SetActive(true);
            towerItems[0].InitInfo(nowSelectPoint.nowBulidTowerInfo.NextUpgrade.Id, "空格键升级");
        }

    }

    //提供给事件中心更新造塔点的显示
    private void OnBuildPointClosed(TowerBulidPoint point)
    {
        // 只关闭当前正在显示的造塔点
        if (nowSelectPoint == point)
            UpdateTowerCreatePanel(null);
    }
    protected override void Update()
    {
        base.Update();

        #region 准星控制
        float lerpValue = 1 -
            Mathf.Exp(-crosshairScaleSpeed * Time.unscaledDeltaTime);
        imgCrosshair.localScale = Vector3.Lerp(
            imgCrosshair.localScale,
            targetCrosshairScale,
            lerpValue);
        #endregion

        //不可造防御塔
        if (!canCreateTower) return;


        //当前造塔点没有防御塔，显示造塔信息
        if (nowSelectPoint.nowBulidTowerInfo == null)
        {
            if (inputActions.Player.BuildTower1.WasPressedThisFrame())
                nowSelectPoint.CreateTower(nowSelectPoint.OptionalTowers[0]);
            if (inputActions.Player.BuildTower2.WasPressedThisFrame())
                nowSelectPoint.CreateTower(nowSelectPoint.OptionalTowers[1]);
            if (inputActions.Player.BuildTower3.WasPressedThisFrame())
                nowSelectPoint.CreateTower(nowSelectPoint.OptionalTowers[2]);
        }
        //如果当前造塔点已经有防御塔了，显示升级信息
        else
        {
            if (inputActions.Player.UpgradeTower.WasPressedThisFrame())
                nowSelectPoint.CreateTower(nowSelectPoint.nowBulidTowerInfo.NextUpgrade.Id);
        }
    }


    public void SetCrosshairAim(bool isAim)
    {
        targetCrosshairScale =
            isAim ? Vector3.one : normalCrosshairScale;
    }

    //控制游戏面板中输出的启动/关闭
    public void SetGamePanelInputEnabled(bool isEnabled)
    {
        if (isEnabled)
            inputActions.Player.Enable();
        else
            inputActions.Player.Disable();
    }
}
