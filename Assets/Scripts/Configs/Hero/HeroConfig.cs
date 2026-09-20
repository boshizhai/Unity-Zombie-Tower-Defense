using UnityEngine;

/// <summary>
/// 英雄的攻击方式。
/// 枚举值与旧 RoleInfo.type 保持一致。
/// </summary>
public enum HeroAttackType
{
    Melee = 0,
    Ranged = 1
}

/// <summary>
/// 英雄静态配置。
/// 每个英雄创建一个独立的 HeroConfig 数据资产。
/// </summary>
[CreateAssetMenu(
    fileName = "HeroConfig",
    menuName = "ScriptableObject/Config/Hero Config")]
public class HeroConfig : ScriptableObject
{
    [Header("基础信息")]
    [SerializeField]
    private int id;

    [SerializeField]
    private string heroName;

    [TextArea(2, 5)]
    [SerializeField]
    private string description;

    [Header("资源")]
    [SerializeField]
    private GameObject heroPrefab;

    [SerializeField]
    private GameObject attackEffectPrefab;

    [Header("战斗属性")]
    [Min(0)]
    [SerializeField]
    private int attack;

    [Min(0f)]
    [SerializeField]
    private float moveSpeed;

    [Min(0f)]
    [SerializeField]
    private float rotateSpeed;

    [SerializeField]
    private HeroAttackType attackType;

    [Header("解锁条件")]
    [Min(0)]
    [SerializeField]
    private int unlockPrice;

    public int Id => id;
    public string HeroName => heroName;
    public string Description => description;
    public GameObject HeroPrefab => heroPrefab;
    public GameObject AttackEffectPrefab => attackEffectPrefab;
    public int Attack => attack;
    public float MoveSpeed => moveSpeed;
    public float RotateSpeed => rotateSpeed;
    public HeroAttackType AttackType => attackType;
    public int UnlockPrice => unlockPrice;
}