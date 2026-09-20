using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "LevelDatabase",
    menuName = "ScriptableObject/DataBase/Level Database")]
public class LevelDatabase : SingleScriptableObject<LevelDatabase>
{
    [SerializeField]
    [Header("关联关卡 .asset 资源文件")]
    private List<LevelConfig> levels = new();

    public IReadOnlyList<LevelConfig> Levels => levels;

    public int Count => levels.Count;

    public LevelConfig GetById(int id)
    {
        LevelConfig level =
            levels.Find(item => item != null && item.Id == id);

        if (level == null)
        {
            Debug.LogError($"没有找到 ID 为 {id} 的关卡配置");
        }

        return level;
    }
}