using UnityEngine;

public enum TowerAttackType
{
    SingleTarget = 1,
    AreaTarget = 2
}

[CreateAssetMenu(
    fileName = "Tower_",
    menuName = "ScriptableObject/Config/Tower Config")]
public class TowerConfig : ScriptableObject
{
    [Header("基础信息")]
    [SerializeField]
    private int id;

    [SerializeField]
    private string towerName;

    [Header("资源")]
    [SerializeField]
    private GameObject towerPrefab;

    [SerializeField]
    private Sprite icon;

    [SerializeField]
    private GameObject attackEffectPrefab;

    [Header("建造与升级")]
    [Min(0)]
    [SerializeField]
    private int buildCost;

    [Tooltip("下一级防御塔。满级防御塔保持为空。")]
    [SerializeField]
    private TowerConfig nextUpgrade;

    [Header("战斗属性")]
    [Min(0f)]
    [SerializeField]
    private float attack;

    [Min(0f)]
    [SerializeField]
    private float attackInterval;

    [Min(0f)]
    [SerializeField]
    private float attackRange;

    [SerializeField]
    private TowerAttackType attackType;

    public int Id => id;
    public string TowerName => towerName;
    public GameObject TowerPrefab => towerPrefab;
    public Sprite Icon => icon;
    public GameObject AttackEffectPrefab => attackEffectPrefab;
    public int BuildCost => buildCost;
    public TowerConfig NextUpgrade => nextUpgrade;
    public float Attack => attack;
    public float AttackInterval => attackInterval;
    public float AttackRange => attackRange;
    public TowerAttackType AttackType => attackType;
}
