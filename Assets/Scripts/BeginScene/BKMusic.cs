using UnityEngine;

public class BKMusic : MonoBehaviour
{
    #region 单例模式
    private static BKMusic instance;
    public static BKMusic Instance => instance;
    private BKMusic() { }
    #endregion

    //播放音乐的组件
    private AudioSource audioSource;

    private void Awake()
    {
        instance = this;
        //获取当前挂载对象的播放音乐的组件
        audioSource = this.GetComponent<AudioSource>();
    }


    /// <summary>
    /// 设置音乐开关
    /// </summary>
    /// <param name="isOn"></param>
    public void SetMusicState(bool isOn)
    {
        audioSource.mute = !isOn;
    }

    /// <summary>
    /// 设置音乐音量
    /// </summary>
    /// <param name="volume"></param>
    public void SetMusicVolume(float volume)
    {
        audioSource.volume = volume;
    }


    private void OnEnable()
    {
        EventCenter.Instance.AddEventListener<bool>("音乐开关变化", SetMusicState);

        EventCenter.Instance.AddEventListener<float>("音乐音量变化", SetMusicVolume);

        // 启用时同步最新设置
        MusicData data = GameDataMgr.Instance.musicData;
        SetMusicState(data.isMusicOn);
        SetMusicVolume(data.musicVolume);
    }

    private void OnDisable()
    {
        EventCenter.Instance.RemoveEventListener<bool>(
            "音乐开关变化", SetMusicState);

        EventCenter.Instance.RemoveEventListener<float>(
            "音乐音量变化", SetMusicVolume);
    }
}
