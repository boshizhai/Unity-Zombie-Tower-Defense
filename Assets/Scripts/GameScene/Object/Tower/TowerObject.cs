using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TowerObject : MonoBehaviour
{
    //炮台的头部和枪口
    public Transform head;
    public Transform gunPoint;

    //炮口的旋转速度，写死了
    private const float rotateSpeed = 50;

    //当前炮台的信息，用于初始化
    private TowerConfig towerInfo;

    //用于单体攻击的群体攻击的目标僵尸对象
    private MonsterObj monsterObj;
    private List<MonsterObj> monsterObjs;
    //炮台应该指向的位置
    private Vector3 targetPos;

    //攻击的冷却时间
    private float frontTime;


    /// <summary>
    /// 提供给外部初始化防御塔信息
    /// </summary>
    /// <param name="info"></param>
    public void InitTowerInfo(TowerConfig info)
    {
        towerInfo = info;
    }

    /// <summary>
    /// 每帧轮询进行攻击检测
    /// </summary>
    public void Update()
    {
        //如果是单体攻击的炮塔
        if (towerInfo.AttackType == TowerAttackType.SingleTarget)
        {
            //如果当前攻击目标为空、目标死亡、目标不在范围内
            if (monsterObj == null ||
               monsterObj.isDead ||
               Vector3.Distance(gunPoint.position, monsterObj.transform.position) > towerInfo.AttackRange)
            {
                //重新查找攻击对象
                monsterObj = GameLevelMgr.Instance.FindMonster(transform.position, towerInfo.AttackRange);
            }

            //如果还是找不到对象则 跳到下一帧继续检测
            if (monsterObj == null) return;

            //否则开始炮台转向
            //得到应该转向的僵尸位置
            targetPos = monsterObj.transform.position;
            targetPos.y = head.position.y;  //让炮台的头部只在水平面上旋转
            head.rotation = Quaternion.Slerp(head.rotation, Quaternion.LookRotation(targetPos - head.position), rotateSpeed * Time.deltaTime);

            //当二者的角度小于5°时才开火
            if (Vector3.Angle(head.forward, targetPos - head.position) < 5 && Time.time - frontTime >= towerInfo.AttackInterval)
            {
                //开火、播放声音、播放特效
                monsterObj.Wound((int)towerInfo.Attack);
                GameDataMgr.Instance.PlaySound("Music/Tower");
                PoolEffect.Spawn(towerInfo.AttackEffectPrefab, gunPoint, 0.5f);
                //重置伤害时间
                frontTime = Time.time;
            }

        }
        //范围攻击的炮塔
        else
        {
            //先得到符合条件的僵尸群
            monsterObjs = GameLevelMgr.Instance.FindMonsters(transform.position, (int)towerInfo.AttackRange);

            if (Time.time - frontTime >= towerInfo.AttackInterval && monsterObjs.Count > 0)
            {
                //遍历整个目标僵尸群
                for (int i = 0; i < monsterObjs.Count; i++)
                {
                    monsterObjs[i].Wound((int)towerInfo.Attack);
                }

                //播放特效
                PoolEffect.Spawn(towerInfo.AttackEffectPrefab, transform.position + transform.up * 0.5f, Quaternion.identity, 0.5f);
                frontTime = Time.time;
            }


        }
    }

}
