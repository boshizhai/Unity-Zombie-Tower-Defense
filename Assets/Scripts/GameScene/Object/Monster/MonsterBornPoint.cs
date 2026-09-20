using Unity.VisualScripting;
using UnityEngine;

public class MonsterBornPoint : MonoBehaviour
{
    [SerializeField]
    private MonsterSpawnConfig spawnConfig;

    //当前出生点还剩多少波怪物
    private int remainingWaves;

    //当前波中怪物已经创建的僵尸数
    private int elapsedMonNum;

    /// <summary>
    /// 采用延迟函数的方式创建僵尸
    /// </summary>
    void Start()
    {
        if (spawnConfig == null || spawnConfig.AvailableMonsters.Count == 0)
        {
            Debug.LogError($"出生点 {name} 没有配置有效的 MonsterSpawnConfig");
            enabled = false;
            return;
        }

        remainingWaves = spawnConfig.WaveCount;

        //记录出怪点,添加到关卡管理类中
        GameLevelMgr.Instance.AddMonsterPoint(this);
        //更新最大波数
        GameLevelMgr.Instance.UpdateMaxWaves(remainingWaves);

        Invoke(nameof(StartMonsterWave), spawnConfig.FirstWaveDelay);
    }

    /// <summary>
    /// 开始尸潮函数
    /// </summary>
    private void StartMonsterWave()
    {
        //每次尸潮开始时都重置僵尸数量
        elapsedMonNum = 0;
        //开启尸潮后波数减少
        --remainingWaves;
        //通知管理器，刷新了一波怪
        GameLevelMgr.Instance.UpdateElapsedWaves(1);

        //开始产生Monster
        CreateMonster();
    }

    /// <summary>
    /// 创建僵尸函数SSS
    /// </summary>
    private void CreateMonster()
    {
        //随机得到一个僵尸信息
        MonsterConfig config = spawnConfig.GetRandomMonster();
        if (config == null || config.MonsterPrefab == null)
        {
            Debug.LogError($"出生点 {name} 获取到的怪物配置无效");
            return;
        }

        //直接从对象池中拿到僵尸预制体
        GameObject obj = PoolMgr.Instance.GetObj(config.MonsterPrefab);

        if (obj == null)
            return;

        //从池中取出尚未激活的僵尸对象，先设置出生位置
        obj.transform.SetPositionAndRotation(this.transform.position, this.transform.rotation);
        //从对象池中新创建的才需要添加脚本
        MonsterObj monsterObj = obj.GetComponent<MonsterObj>();
        if (monsterObj == null)
            monsterObj = obj.AddComponent<MonsterObj>();

        //先登记，再初始化并激活僵尸
        ++elapsedMonNum;
        GameLevelMgr.Instance.AddMonster(monsterObj);
        monsterObj.InitMonster(config);


        if (elapsedMonNum < spawnConfig.MonstersPerWave)
        {
            //当前波数僵尸没有创建完成
            Invoke(nameof(CreateMonster), spawnConfig.MonsterSpawnInterval);
        }
        //如果当前波数僵尸为空
        else
        {
            if (remainingWaves >= 1)
            {
                //波数没空，继续开始尸潮
                Invoke(nameof(StartMonsterWave), spawnConfig.WaveInterval);
            }
        }
    }

    /// <summary>
    /// 检查出生点是否已经产生了所有的怪物
    /// </summary>
    /// <returns></returns>
    public bool CheckBornOver()
    {
        return elapsedMonNum == spawnConfig.MonstersPerWave && remainingWaves == 0;
    }
}


public struct WaveInfo
{
    public int remaining;
    public int total;

    public WaveInfo(int remaining, int total)
    {
        this.remaining = remaining;
        this.total = total;
    }
}