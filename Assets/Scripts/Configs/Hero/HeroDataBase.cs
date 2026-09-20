using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "HeroDatabase",
    menuName = "ScriptableObject/DataBase/Hero Database")]
public class HeroDatabase : SingleScriptableObject<HeroDatabase>
{
    [SerializeField]
    [Header("关联角色.Asset资源文件")]
    private List<HeroConfig> heroes = new();

    public IReadOnlyList<HeroConfig> Heroes => heroes;

    public int Count => heroes.Count;

    public HeroConfig GetById(int id)
    {
        HeroConfig hero = heroes.Find(item => item != null && item.Id == id);

        if (hero == null)
        {
            Debug.LogError($"没有找到 ID 为 {id} 的英雄配置");
        }

        return hero;
    }
}