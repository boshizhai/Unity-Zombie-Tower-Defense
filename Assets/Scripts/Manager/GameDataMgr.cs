using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;


/// <summary>
/// 专门用于管理游戏数据的类
/// </summary>
public class GameDataMgr
{

    #region 单例模式方便使用
    private static GameDataMgr instance = new GameDataMgr();
    public static GameDataMgr Instance => instance;

    //在构造函数中加载游戏数据
    private GameDataMgr()
    {
        //初始化数据
        musicData = JsonMgr.Instance.LoadData<MusicData>("MusicData");
        playerData = JsonMgr.Instance.LoadData<PlayerData>("PlayerData");
    }
    #endregion



    #region 游戏音乐音效数据类
    public MusicData musicData;
    /// <summary>
    /// 提供给外部存储音效和音乐的函数
    /// </summary>
    public void SaveMusicData()
    {
        JsonMgr.Instance.SaveData(musicData, "MusicData");
    }
    #endregion


    #region 角色数据类
    // 所有英雄的 ScriptableObject 配置
    public IReadOnlyList<HeroConfig> Heroes => HeroDatabase.Instance.Heroes;

    // 当前选择的英雄配置
    public HeroConfig SelectedHero { get; private set; }

    // 选择英雄
    public void SelectHero(HeroConfig heroConfig)
    {
        if (heroConfig == null)
        {
            Debug.LogError("无法选择英雄：HeroConfig 为空");
            return;
        }

        SelectedHero = heroConfig;
    }
    #endregion



    #region 玩家相关数据
    public PlayerData playerData;
    /// <summary>
    /// 保存玩家数据的方法
    /// </summary>
    public void SavePlayerData()
    {
        JsonMgr.Instance.SaveData(playerData, "PlayerData");
    }
    #endregion


    #region 关卡数据类
    public IReadOnlyList<LevelConfig> Levels => LevelDatabase.Instance.Levels;
    #endregion

    #region 怪物数据类
    public IReadOnlyList<MonsterConfig> Monsters => MonsterDatabase.Instance.Monsters;
    #endregion

    #region 塔防类
    public IReadOnlyList<TowerConfig> Towers => TowerDatabase.Instance.Towers;
    #endregion


    //通用的音效预制体
    private GameObject soundPrefab;
    //缓存音频资源，避免每次播放都调用资源加载接口
    private readonly Dictionary<string, AudioClip> soundClipCache = new Dictionary<string, AudioClip>();

    /// <summary>
    /// 播放音效的方法，提供给外部播放音效
    /// </summary>
    /// <param name="resname"></param>
    public void PlaySound(string resname)
    {
        if (string.IsNullOrEmpty(resname))
            return;

        //关闭音效时 不创建播放对象
        if (!musicData.isSoundOn)
            return;

        //获取并缓存音频
        if (!soundClipCache.TryGetValue(resname, out AudioClip clip) || clip == null)
        {
            clip = ResMgr.Instance.Load<AudioClip>(resname);
            if (clip == null)
                return;
            soundClipCache[resname] = clip;
        }

        //获取《源预制体》引用，注意不是Load<GameObject>()这个会返回实例化的API
        if (soundPrefab == null)
        {
            soundPrefab = ResMgr.Instance.LoadAsset<GameObject>("Prefabs/Audio/SoundObject");
            if (soundPrefab == null)
            {
                Debug.LogError($"加载音效播放器预制体失败，路径：Prefabs/Audio/SoundObject");
                return;
            }
        }

        //通过源预制体去找对象池拿到具体的实例化音效播放器对象
        GameObject soundObj = PoolMgr.Instance.GetObj(soundPrefab);
        if (soundObj == null)
            return;

        PoolSound sound = soundObj.GetComponent<PoolSound>();
        if (sound == null)
        {
            Debug.LogError("SoundObject 预制体没有挂载PoolSound");
            PoolMgr.Instance.PushObj(soundPrefab, soundObj);
            return;
        }
        sound.Play(soundPrefab, clip, musicData.soundVolume);
    }
}
