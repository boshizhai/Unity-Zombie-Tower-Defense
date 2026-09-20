using UnityEngine;
using UnityEngine.UI;

public class TowerItem : MonoBehaviour
{
    public Image imgTower;
    public Text txtTips;
    public Text txtmoney;

    public void InitInfo(int id, string inputStr)
    {
        //传入的id就是塔防数据表中的id，我们通过该id获取到对应的塔防数据表中的信息
        TowerConfig info = TowerDatabase.Instance.GetById(id);
        if (info == null)
        {
            return;
        }

        imgTower.sprite = info.Icon;
        txtTips.text = inputStr;
        txtmoney.text = "￥" + info.BuildCost;
        
        if(GameLevelMgr.Instance.playerInfo.obtainReward < info.BuildCost)
        {
            // 玩家有足够的金钱购买该塔
            txtTips.text = "金钱不足";
        }

    }
}
