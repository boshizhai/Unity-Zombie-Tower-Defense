

using UnityEngine;

[CreateAssetMenu(
    fileName = "Monster_",
    menuName = "ScriptableObject/Config/Monster Config")]
public class MonsterConfig : ScriptableObject
{
    [Header("基础信息")]
    [SerializeField]
    private int id;

    [SerializeField]
    private string monsterName;

    [Header("资源")]
    [SerializeField]
    private GameObject monsterPrefab;

    [Tooltip("该僵尸可以随机使用的动画控制器")]
    [SerializeField]
    private RuntimeAnimatorController[] animatorControllers;

    [Header("战斗属性")]
    [Min(0f)]
    [SerializeField]
    private float maxHealth;

    [Min(0)]
    [SerializeField]
    private int attack;

    [Min(0)]
    [SerializeField]
    private int defense;

    [Min(0f)]
    [SerializeField]
    private float moveSpeed;

    [Min(0f)]
    [SerializeField]
    private float rotateSpeed;

    [Min(0f)]
    [SerializeField]
    private float attackInterval;

    public int Id => id;
    public string MonsterName => monsterName;
    public GameObject MonsterPrefab => monsterPrefab;
    public RuntimeAnimatorController[] AnimatorControllers => animatorControllers;
    public float MaxHealth => maxHealth;
    public int Attack => attack;
    public int Defense => defense;
    public float MoveSpeed => moveSpeed;
    public float RotateSpeed => rotateSpeed;
    public float AttackInterval => attackInterval;

    /// <summary>
    /// 随机取得一个动画控制器。
    /// </summary>
    public RuntimeAnimatorController GetRandomAnimatorController()
    {
        if (animatorControllers == null || animatorControllers.Length == 0)
        {
            return null;
        }

        int index = Random.Range(0, animatorControllers.Length);
        return animatorControllers[index];
    }
}
