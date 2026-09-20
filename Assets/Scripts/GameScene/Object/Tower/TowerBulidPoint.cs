
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TowerBulidPoint : MonoBehaviour
{
    //当前造塔点创建出来的塔和它的塔防信息
    private GameObject nowBulidTower;
    public TowerConfig nowBulidTowerInfo;

    //当前造塔点可以创建哪些塔防，
    public List<int> OptionalTowers = new List<int> { 1, 4, 7 };


    /// <summary>
    /// 提供给外部创建防御塔的方法
    /// </summary>
    /// <param name="id"></param>
    public void CreateTower(int id)
    {
        //开始创建防御塔
        TowerConfig info = TowerDatabase.Instance.GetById(id);
        if (info == null)
        {
            return;
        }
        if (GameLevelMgr.Instance.playerInfo.obtainReward < info.BuildCost)
        {
            //玩家金钱不够，无法建造防御塔
            UIMgr.Instance.ShowPanel<TipPanel>().ChangeInfo("金钱不足，无法建造防御塔", true);
            return;
        }

        nowBulidTowerInfo = info;

        //扣除玩家金钱
        GameLevelMgr.Instance.playerInfo.AddMoney(-nowBulidTowerInfo.BuildCost);
        if (nowBulidTower != null)
        {
            GameLevelMgr.Instance.DeleteTower(nowBulidTower);
            Destroy(nowBulidTower);
        }
        //创建防御塔
        nowBulidTower = Instantiate(nowBulidTowerInfo.TowerPrefab, transform.position, Quaternion.identity);
        //初始化防御塔信息
        nowBulidTower.GetComponent<TowerObject>().InitTowerInfo(nowBulidTowerInfo);
        GameLevelMgr.Instance.AddTower(nowBulidTower);

        //创建了防御塔之后需要更新造塔面板上的内容
        //1.如果防御塔的等级已经是最高级了，那么就不需要再升级了
        if (nowBulidTowerInfo.NextUpgrade == null)
            // 已经满级，传 null，通知面板隐藏造塔选项
            EventCenter.Instance.EventTrigger<TowerBulidPoint>("造塔点关闭", this);
        else
            EventCenter.Instance.EventTrigger<TowerBulidPoint>("造塔点更新", this);
    }


    /// <summary>
    /// 触发器函数，玩家靠经防御塔位置
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        //如果碰撞的物体不是玩家，那么就不需要显示造塔面板
        if (LayerMask.NameToLayer("Player") != other.gameObject.layer)
            return;
        if (nowBulidTowerInfo != null && nowBulidTowerInfo.NextUpgrade == null)
            return;
        EventCenter.Instance.EventTrigger<TowerBulidPoint>("造塔点更新", this);
    }


    /// <summary>
    /// 触发器函数，玩家离开防御塔
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player"))
            return;

        EventCenter.Instance.EventTrigger<TowerBulidPoint>("造塔点关闭", this);
    }
}
