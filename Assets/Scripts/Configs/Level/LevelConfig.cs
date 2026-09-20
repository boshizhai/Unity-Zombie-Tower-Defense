using UnityEngine;

[CreateAssetMenu(
    fileName = "Level_",
    menuName = "ScriptableObject/Config/Level Config"
)]
public class LevelConfig : ScriptableObject
{
    [Header("基础信息")]
    [SerializeField]
    private int id;

    [SerializeField]
    private string levelName;

    [SerializeField]
    [TextArea(3, 5)]
    private string description;

    [SerializeField]
    private Sprite previewImage;

    [SerializeField]
    [Header("场景")]
    [Tooltip("必须和Build Profiles中的场景名称一致")]
    private string sceneName;

    [Header("初始数值")]
    [Min(0)]
    [SerializeField]
    private int startingMoney;

    [Min(1)]
    [SerializeField]
    private int baseTowerMaxHealth;

    public int Id => id;
    public string LevelName => levelName;
    public string Description => description;
    public Sprite PreviewImage => previewImage;
    public string SceneName => sceneName;
    public int StartingMoney => startingMoney;
    public int BaseMaxHealth => baseTowerMaxHealth;
}