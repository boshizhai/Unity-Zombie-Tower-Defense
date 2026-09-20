using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "SpawnProfile_",
    menuName = "ScriptableObject/Config/Monster Spawn Config")]
public class MonsterSpawnConfig : ScriptableObject
{
    [Header("波次")]
    [Min(1)]
    [SerializeField]
    private int waveCount = 1;

    [Min(1)]
    [SerializeField]
    private int monstersPerWave = 1;

    [Header("怪物种类")]
    [SerializeField]
    private List<MonsterConfig> availableMonsters = new();

    [Header("时间")]
    [Min(0f)]
    [Tooltip("关卡开始后，第一波怪物出现前的等待时间")]
    [SerializeField]
    private float firstWaveDelay;

    [Min(0f)]
    [Tooltip("同一波内，每只怪物之间的生成间隔")]
    [SerializeField]
    private float monsterSpawnInterval = 1f;

    [Min(0f)]
    [Tooltip("每一波怪物之间的等待时间")]
    [SerializeField]
    private float waveInterval = 1f;

    public int WaveCount => waveCount;
    public int MonstersPerWave => monstersPerWave;
    public IReadOnlyList<MonsterConfig> AvailableMonsters => availableMonsters;
    public float FirstWaveDelay => firstWaveDelay;
    public float MonsterSpawnInterval => monsterSpawnInterval;
    public float WaveInterval => waveInterval;

    public MonsterConfig GetRandomMonster()
    {
        if (availableMonsters == null || availableMonsters.Count == 0)
        {
            return null;
        }

        int index = Random.Range(0, availableMonsters.Count);
        return availableMonsters[index];
    }
}