using System.Collections;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChooseHeroPanel : BasePanel
{
    public Button btnClose;
    public Button btnStart;
    public Button btnBuy;   //购买英雄的按钮
    public Button btnLeft;
    public Button btnRight;
    public Text txtMoney;   //左上角玩家拥有的钱
    public TextMeshProUGUI txtName;
    public TextMeshProUGUI txtIntro;
    //场景中放置展示英雄的位置
    private Transform heroPos;

    //存储当前创建的角色克隆对象
    private GameObject heroObj;
    //当前选中对象的玩家信息
    private HeroConfig nowHero;
    //切换英雄时需要用到的索引下标，对应RoldInfo的id
    private int nowIndex;

    //长按解锁按钮需要用上的字段
    private TextMeshProUGUI txtBuy;  //解锁按钮上显示的钱
    private Image imgProgress;       //按钮的地图组件用来控制FillAmount进度
    private const int BuyHoldDuration = 2;  //设置的动态时长
    private int haveMoneyBeforeBuy;   //开始长按前玩家的钱包
    private int heroPriceBeforeBuy;   //开始长按前英雄的价格
    private Coroutine buyCoroutine;   //负责长按每帧渲染的协程

    protected override void Init()
    {
        //开始注册事件监听
        //1.开始按钮
        btnStart.onClick.AddListener(() =>
        {
            GameDataMgr.Instance.SelectHero(nowHero);
            UIMgr.Instance.HidePanel<ChooseHeroPanel>();
            UIMgr.Instance.ShowPanel<ChooseLevelPanel>();
            //删除当前场景上创建的玩家
            DestroyImmediate(heroObj);
            heroObj = null;
        });

        //2.返回按钮
        btnClose.onClick.AddListener(() =>
        {
            //调用摄像机的右转函数，传入显示开始界面的回调，动画结束后会自动调用该回调
            Camera.main.GetComponent<CameraAnimator>().TurnRight(() =>
            {
                UIMgr.Instance.ShowPanel<BeginPanel>();
                //通过传入事件回调删除当前角色
                Destroy(heroObj);
            });
            //隐藏当前显示面板
            UIMgr.Instance.HidePanel<ChooseHeroPanel>();
        });

        //3.向左切换按钮
        btnLeft.onClick.AddListener(() =>
        {
            //将当前选择英雄的索引下表左移
            nowIndex = --nowIndex < 0 ? GameDataMgr.Instance.Heroes.Count - 1 : nowIndex;
            ChangeShowHero();
        });

        //4.向右按钮
        btnRight.onClick.AddListener(() =>
        {
            //将当前选择英雄的索引下表右移
            nowIndex = ++nowIndex >= GameDataMgr.Instance.Heroes.Count ? 0 : nowIndex;
            ChangeShowHero();
        });


        //5.购买英雄的按钮
        //给btnBuy添加EventTrigger事件监听
        //1.获取EventTrigger脚本
        EventTrigger buyHoldTrigger = btnBuy.GetComponent<EventTrigger>() ?? btnBuy.gameObject.AddComponent<EventTrigger>();
        //2.注册Entry事件容器
        EventTrigger.Entry buyHoldEntry = new EventTrigger.Entry();
        //3.开始注册
        //3.1长按按钮
        buyHoldEntry.eventID = EventTriggerType.PointerDown;
        buyHoldEntry.callback.AddListener(StartBuyHold);
        buyHoldTrigger.triggers.Add(buyHoldEntry);
        //3.2移开按钮
        buyHoldEntry = new EventTrigger.Entry();
        buyHoldEntry.eventID = EventTriggerType.PointerExit;
        buyHoldEntry.callback.AddListener(CancelBuyHold);
        buyHoldTrigger.triggers.Add(buyHoldEntry);
        //3.3提前松手
        buyHoldEntry = new EventTrigger.Entry();
        buyHoldEntry.eventID = EventTriggerType.PointerUp;
        buyHoldEntry.callback.AddListener(CancelBuyHold);
        buyHoldTrigger.triggers.Add(buyHoldEntry);
        txtBuy = btnBuy.GetComponentInChildren<TextMeshProUGUI>();
        imgProgress = btnBuy.GetComponent<Image>();

        //找到场景中用来放置预设体的位置
        heroPos = GameObject.Find("HeroPos").transform;
        //更新左上角玩家拥有的钱
        txtMoney.text = GameDataMgr.Instance.playerData.haveMoney.ToString();
        //在初始化Init中也需要刷新显示的英雄
        ChangeShowHero();

    }

    /// <summary>
    /// 用来改变当前展示英雄的函数
    /// </summary>
    private void ChangeShowHero()
    {
        //一进来就销毁上次创建的英雄模型
        if (heroObj != null) DestroyImmediate(heroObj);

        //得到当前选中的英雄信息
        nowHero = GameDataMgr.Instance.Heroes[nowIndex];  //nowIndex为Int类型，默认值是0
        //更新面板上显示的英雄信息
        //1.克隆英雄模型
        heroObj = Instantiate(nowHero.HeroPrefab, heroPos.transform, false);

        //2.给人物添加拖拽旋转的功能
        //2.1获得事件触发器
        EventTrigger eventTrigger = heroObj.GetComponent<EventTrigger>() ?? heroObj.AddComponent<EventTrigger>();
        EventTrigger.Entry dragEntry = new EventTrigger.Entry();
        dragEntry.eventID = EventTriggerType.Drag;
        dragEntry.callback.AddListener(OnHeroDrag);
        eventTrigger.triggers.Add(dragEntry);

        //2.更新英雄名称
        txtName.text = nowHero.HeroName;
        txtIntro.text = nowHero.Description;

        //更新英雄的解锁情况
        UpdateHeroUnLockStatus();
    }


    /// <summary>
    /// 用来更新英雄的解锁状态的函数
    /// </summary>
    private void UpdateHeroUnLockStatus()
    {
        //如果英雄没解锁：需要的钱数大于0，而且玩家数据的haveHero列表中不包含该ID
        if (nowHero.UnlockPrice > 0 && !GameDataMgr.Instance.playerData.haveHero.Contains(nowHero.Id))
        {
            //角色没解锁，隐藏开始按钮
            btnStart.gameObject.SetActive(false);
            //更新解锁英雄的按钮信息
            btnBuy.gameObject.SetActive(true);
            btnBuy.GetComponent<Image>().fillAmount = 1;  //恢复精度条，防止购买英雄后显示清空
            txtBuy.text = "解锁: " + nowHero.UnlockPrice;

        }
        //否则就代表英雄已经解锁了，默认英雄钱数花费为0，也能进入下面的逻辑
        else
        {
            btnStart.gameObject.SetActive(true);
            btnBuy.gameObject.SetActive(false);
        }
    }


    /// <summary>
    /// 拖拽英雄预制体旋转的函数
    /// </summary>
    /// <param name="eventData"></param>
    private void OnHeroDrag(BaseEventData eventData)
    {
        PointerEventData pointerEventData = eventData as PointerEventData;
        heroObj.transform.Rotate(Vector3.up, 30f * -pointerEventData.delta.x * Time.deltaTime);
    }



    #region 解锁按钮的函数功能：开始长按、协程、完成长按、取消长按

    /// <summary>
    /// 开始长按，内部启动协程
    /// </summary>
    /// <param name="eventData"></param>
    private void StartBuyHold(BaseEventData eventData)
    {
        if (buyCoroutine != null) return;

        //保存当前的玩家的钱和英雄的钱
        haveMoneyBeforeBuy = GameDataMgr.Instance.playerData.haveMoney;
        heroPriceBeforeBuy = nowHero.UnlockPrice;

        //设置btnBuy进度条地图的占比
        btnBuy.GetComponent<Image>().fillAmount = 1;

        //开启协程
        buyCoroutine = StartCoroutine(BuyHoldCoroutine());

    }

    /// <summary>
    /// 负责长按过程中更改每帧信息
    /// </summary>
    /// <returns></returns>
    private IEnumerator BuyHoldCoroutine()
    {
        float elaspedTime = 0;
        float progress;


        while (elaspedTime < BuyHoldDuration)
        {
            //每帧累加持续的事件
            elaspedTime += Time.unscaledDeltaTime;

            //得到当前长按的百分比，后续所有信息根据百分比来显示
            progress = Mathf.Clamp01(elaspedTime / BuyHoldDuration);

            //根据当前进度 计算英雄价格和玩家金钱

            // 当前累计应该扣除的金币
            int paidMoney = Mathf.RoundToInt(heroPriceBeforeBuy * progress);
            //玩家当前应该显示的金币
            int haveMoney = haveMoneyBeforeBuy - paidMoney;
            //英雄应当显示的价钱
            int heroPrice = heroPriceBeforeBuy - paidMoney;



            //如果但钱玩家拥有的钱小于0，证明不够支付英雄价格，退出协程，显示提示面板
            if (haveMoney <= 0)
            {
                ////恢复玩家金币、进度条、英雄价格
                //imgProgress.fillAmount = 1;
                //txtBuy.text = "解锁: " + heroPriceBeforeBuy;
                //txtMoney.text = haveMoneyBeforeBuy.ToString();
                //UIMgr.Instance.ShowPanel<TipPanel>().ChangeInfo("你的金钱不够 ！");
                CancelBuyHold(null);
                UIMgr.Instance.ShowPanel<TipPanel>().ChangeInfo("你的金钱不够 ！");
                //确保协程退出，防止继续更新
                yield break;
            }


            //计算完成之后刷新：进度条、玩家的钱、英雄消耗的钱
            imgProgress.fillAmount = 1f - progress;
            txtMoney.text = haveMoney.ToString();
            txtBuy.text = "解锁: " + heroPrice;

            //使用协程，将循环一帧一帧执行
            yield return null;
        }

        //跳出循环说明长按已经完成
        CompleteBuyHold();
    }


    /// <summary>
    /// 完成长按解锁，置空协程，保存数据
    /// </summary>
    private void CompleteBuyHold()
    {
        //停止协程
        buyCoroutine = null;

        //开始实现购买英雄业务
        //1.扣除玩家的钱
        GameDataMgr.Instance.playerData.haveMoney -= heroPriceBeforeBuy;
        //将英雄增加到玩家持有列表
        GameDataMgr.Instance.playerData.haveHero.Add(nowHero.Id);
        //保存玩家数据
        GameDataMgr.Instance.SavePlayerData();

        //更新最终显示
        imgProgress.fillAmount = 0; //确保精度条清空
        txtBuy.text = "解锁: 0";
        txtMoney.text = GameDataMgr.Instance.playerData.haveMoney.ToString();

        //购买完成后更新显示
        UpdateHeroUnLockStatus();
        //显示提示面板
        UIMgr.Instance.ShowPanel<TipPanel>().ChangeInfo("购买英雄成功 ！");
    }


    /// <summary>
    /// 取消长按，恢复价格，停止协程
    /// </summary>
    private void CancelBuyHold(BaseEventData eventData)
    {
        if (buyCoroutine == null) return;

        //退出协程
        StopCoroutine(buyCoroutine);
        buyCoroutine = null;

        //恢复玩家金币、进度条、英雄价格
        imgProgress.fillAmount = 1;
        txtBuy.text = "解锁: " + heroPriceBeforeBuy;
        txtMoney.text = haveMoneyBeforeBuy.ToString();
    }
    #endregion

}
