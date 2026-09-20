
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "TowerDatabase",
    menuName = "ScriptableObject/DataBase/Tower Database"
)]
public class TowerDatabase : SingleScriptableObject<TowerDatabase>
{
    [SerializeField]
    private List<TowerConfig> towers = new List<TowerConfig>();

    public IReadOnlyList<TowerConfig> Towers => towers;

    public TowerConfig GetById(int id)
    {
        TowerConfig tower = towers.Find(item => item != null && item.Id == id);
        if (tower == null)
        {
            Debug.LogError($"没有找到ID为{id}的防御塔配置");
        }
        return tower;
    }
}