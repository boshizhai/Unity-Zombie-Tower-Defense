using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : BasePanel
{
    //进行脚本控件的绑定关联
    public Button btnClose;
    public Toggle togMusic;
    public Toggle togSound;
    public Slider musicSlider;
    public Slider soundSlider;


    protected override void Init()
    {
        //初始化面板控件的
        MusicData musicData = GameDataMgr.Instance.musicData;
        togMusic.isOn = musicData.isMusicOn;
        togSound.isOn = musicData.isSoundOn;
        musicSlider.value = musicData.musicVolume;
        soundSlider.value = musicData.soundVolume;


        btnClose.onClick.AddListener(() =>
        {
            //关闭设置面板
            UIMgr.Instance.HidePanel<SettingPanel>();
            //退出该面板时才会存储音乐数据
            GameDataMgr.Instance.SaveMusicData();
        });

        //音乐开关事件监听
        togMusic.onValueChanged.AddListener((isOn) =>
        {
            GameDataMgr.Instance.musicData.isMusicOn = isOn;
            EventCenter.Instance.EventTrigger<bool>("音乐开关变化", isOn);
        });

        //音效开关事件监听
        togSound.onValueChanged.AddListener((isOn) =>
        {
            GameDataMgr.Instance.musicData.isSoundOn = isOn;
        });

        //音乐音量滑动条事件监听
        musicSlider.onValueChanged.AddListener((value) =>
        {
            GameDataMgr.Instance.musicData.musicVolume = value;
            EventCenter.Instance.EventTrigger<float>("音乐音量变化", value);
        });
        //音效音量滑动条事件监听
        soundSlider.onValueChanged.AddListener((value) =>
        {
            GameDataMgr.Instance.musicData.soundVolume = value;
        });
    }
}

