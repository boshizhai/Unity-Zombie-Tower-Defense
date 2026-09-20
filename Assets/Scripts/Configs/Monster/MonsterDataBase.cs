using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "MonsterDatabase",
    menuName = "ScriptableObject/DataBase/Monster Database")]
public class MonsterDatabase : SingleScriptableObject<MonsterDatabase>
{
    [SerializeField]
    [Header("关联怪物 .asset 资源文件")]
    private List<MonsterConfig> monsters = new();

    public IReadOnlyList<MonsterConfig> Monsters => monsters;

    public int Count => monsters.Count;

    public MonsterConfig GetById(int id)
    {
        MonsterConfig monster =
            monsters.Find(item => item != null && item.Id == id);

        if (monster == null)
        {
            Debug.LogError($"没有找到 ID 为 {id} 的怪物配置");
        }

        return monster;
    }
}