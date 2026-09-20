using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameLevelMgr
{
    #region 单例模式
    private static GameLevelMgr instance = new GameLevelMgr();
    private GameLevelMgr() { }

    public static GameLevelMgr Instance => instance;
    #endregion

    //1.切换游戏场景时需要动态创建玩家
    //2.我们需要通过游戏管理器来判断游戏是否胜利

    //保存当前关卡的玩家信息
    public PlayerObject playerInfo;

    //当前关卡所有尸潮出生点的信息
    private List<MonsterBornPoint> bornPoints = new List<MonsterBornPoint>();

    //记录关卡所有的尸潮波数
    private int maxMonsterWaves;
    //记录已经过去了多少波怪物
    private int elapsedMonsterWaves;

    //记录当前场景中的怪物数量，是否为零
    //private int nowMonsterNum = 0;
    private List<MonsterObj> monstersList = new List<MonsterObj>();

    //记录所有炮台的对象游戏结束时一一删除
    private List<GameObject> towerObjs = new List<GameObject>();


    public WaveInfo CurrentWaveInfo => new WaveInfo(maxMonsterWaves - elapsedMonsterWaves, maxMonsterWaves);

    /// <summary>
    /// 初始化游戏信息
    /// </summary>
    public void InitGameInfo(LevelConfig levelConfig)
    {
        //开头追加一次清理，避免重复初始化造成重复订阅
        EventCenter.Instance.RemoveEventListener<MonsterObj>("怪物死亡", OnMonsterDead);
        EventCenter.Instance.RemoveEventListener<MonsterObj>("怪物退场", OnMonsterRemoved);

        //重置游戏结束状态，注册失败通知事件
        isGameOver = true;
        EventCenter.Instance.RemoveEventListener("主塔摧毁", OnMainTowerDestroyed);

        HeroConfig selectedHero = GameDataMgr.Instance.SelectedHero;
        if (selectedHero == null || selectedHero.HeroPrefab == null)
        {
            Debug.LogError("初始化关卡失败：没有选择有效的英雄配置");
            return;
        }

        //显示游戏界面
        UIMgr.Instance.ShowPanel<GamePanel>();

        //创建英雄
        GameObject heroObj = GameObject.Instantiate(selectedHero.HeroPrefab,
                                               GameObject.Find("HeroPos").transform, false);
        playerInfo = heroObj.AddComponent<PlayerObject>();
        //给摄像机添加追踪英雄对象；没有挂载时自动补充，保证所有关卡一致。
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("初始化关卡失败：场景中没有 MainCamera");
            return;
        }

        CameraMove cameraMove = mainCamera.GetComponent<CameraMove>();
        if (cameraMove == null)
            cameraMove = mainCamera.gameObject.AddComponent<CameraMove>();

        cameraMove.SetPlayer(heroObj.transform);

        //设置鼠标属性
        Cursor.lockState = CursorLockMode.Locked;

        //初始化玩家的局内金钱数
        playerInfo.AddMoney(levelConfig.StartingMoney);

        //初始化保护区的血量
        MainTowerObject.Instance.UpdateTowerHp(levelConfig.BaseMaxHealth, levelConfig.BaseMaxHealth);

        //往事件中心添加怪物死亡的函数-增加玩家的金币数量
        EventCenter.Instance.AddEventListener<MonsterObj>("怪物死亡", OnMonsterDead);
        //僵尸退场
        EventCenter.Instance.AddEventListener<MonsterObj>("怪物退场", OnMonsterRemoved);
        EventCenter.Instance.AddEventListener("主塔摧毁", OnMainTowerDestroyed);
        isGameOver = false;
    }

    /// <summary>
    /// 创建新的出怪点时添加到关卡管理器中
    /// </summary>
    /// <param name="monsterBornPoint"></param>
    public void AddMonsterPoint(MonsterBornPoint monsterBornPoint)
    {
        bornPoints.Add(monsterBornPoint);
    }

    /// <summary>
    /// 更新最大尸潮波数状态
    /// </summary>
    /// <param name="num"></param>
    public void UpdateMaxWaves(int num)
    {
        maxMonsterWaves += num;

        EventCenter.Instance.EventTrigger<WaveInfo>("僵尸波数变化", CurrentWaveInfo);
    }


    /// <summary>
    /// 尸潮波数的减少
    /// </summary>
    /// <param name="num"></param>
    public void UpdateElapsedWaves(int num)
    {
        elapsedMonsterWaves += num;

        EventCenter.Instance.EventTrigger<WaveInfo>("僵尸波数变化", CurrentWaveInfo);
    }


    /// <summary>
    /// 判断尸潮是否已经停止,并且所有怪物对象死亡后，游戏胜利
    /// </summary>
    /// <returns></returns>
    public bool CheckGameOver()
    {
        foreach (var item in bornPoints)
        {
            //只要一个出生点没有结束尸潮则返回false
            if (!item.CheckBornOver()) return false;
        }

        if (monstersList.Count > 0)
        {
            return false;
        }
        Debug.Log("游戏胜利");
        //默认返回true,游戏结束胜利
        return true;
    }


    ///// <summary>
    ///// 改变当前场景上怪物的数量
    ///// </summary>
    ///// <param name="num"></param>
    //public void AddMonsterNum(int num)
    //{
    //    nowMonsterNum += num;
    //}


    /// <summary>
    /// 增加怪物到集合中
    /// </summary>
    /// <param name="info"></param>
    public void AddMonster(MonsterObj info)
    {
        monstersList.Add(info);
    }
    /// <summary>
    /// 从集合中删除怪物
    /// </summary>
    /// <param name="info"></param>
    public void DeleteMonster(MonsterObj info)
    {
        if (monstersList.Contains(info))
            monstersList.Remove(info);
    }


    /// <summary>
    /// 在怪物列表中找到一个符合条件的怪物给外部
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="Range"></param>
    /// <returns></returns>
    public MonsterObj FindMonster(Vector3 pos, float Range)
    {
        //使用LINQ语法
        return monstersList.FirstOrDefault(item => Vector3.Distance(pos, item.transform.position) <= Range && !item.isDead);
    }
    public List<MonsterObj> FindMonsters(Vector3 pos, int Range)
    {
        //使用LINQ语法
        return monstersList.Where(item => Vector3.Distance(pos, item.transform.position) <= Range && !item.isDead).ToList();
    }

    //提供给事件中心处理怪物死亡的方法
    private void OnMonsterDead(MonsterObj monster)
    {
        if (isGameOver) return;
        // 只处理当前关卡登记的怪物
        if (monster == null || !monstersList.Contains(monster))
            return;

        if (playerInfo != null)
            playerInfo.AddMoney(100);
    }
    private void OnMonsterRemoved(MonsterObj monster)
    {
        // 移除成功才处理，避免重复通知
        if (!monstersList.Remove(monster))
            return;

        if (!isGameOver && playerInfo != null && CheckGameOver())
            EndGame(true);
    }
    //游戏结束的标记
    private bool isGameOver = true;
    //游戏结束
    private void EndGame(bool isWin)
    {
        if (isGameOver || playerInfo == null) return;

        // 先标记，防止重复结算
        isGameOver = true;

        int reward = isWin
            ? playerInfo.obtainReward
            : (int)(playerInfo.obtainReward * 0.2f);

        GameDataMgr.Instance.playerData.haveMoney += reward;
        GameDataMgr.Instance.SavePlayerData();

        GameOverPanel panel = UIMgr.Instance.ShowPanel<GameOverPanel>();
        panel.InitInfo(reward, isWin);
    }
    private void OnMainTowerDestroyed()
    {
        EndGame(false);
    }

    public void AddTower(GameObject towrObj)
    {
        towerObjs.Add(towrObj);
    }
    public void DeleteTower(GameObject towrObj)
    {
        if (towerObjs.Contains(towrObj))
            towerObjs.Remove(towrObj);
    }

    /// <summary>
    /// 清空当前关卡的数据
    /// </summary>
    public void ClearInfo()
    {
        //触发游戏结束，退订结束事件
        isGameOver = true;
        EventCenter.Instance.RemoveEventListener("主塔摧毁", OnMainTowerDestroyed);
        //退出关卡时退订
        EventCenter.Instance.RemoveEventListener<MonsterObj>("怪物死亡", OnMonsterDead);
        EventCenter.Instance.RemoveEventListener<MonsterObj>("怪物退场", OnMonsterRemoved);

        foreach (MonsterBornPoint bornPoint in bornPoints)
        {
            if (bornPoint != null)
                bornPoint.CancelInvoke();
        }
        bornPoints.Clear();
        monstersList.Clear();
        maxMonsterWaves = 0;
        elapsedMonsterWaves = 0;
        playerInfo = null;
        foreach (var item in towerObjs)
        {
            GameObject.Destroy(item);
        }
        towerObjs.Clear();
    }

}
