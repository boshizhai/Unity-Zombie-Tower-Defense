using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家数据
/// </summary>
public class PlayerData
{
    //当前拥有的钱
    public int haveMoney = 1000;
    //已经解锁的玩家
    public List<int> haveHero = new List<int>(7);
}