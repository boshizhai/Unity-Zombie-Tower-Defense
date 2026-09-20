using UnityEngine;

public class MainTowerObject : MonoBehaviour
{
    //血量相关
    private int nowHp;
    private int maxHp;

    public MainTowerHpInfo HpInfo => new MainTowerHpInfo(nowHp, maxHp);
    private bool isDestroy;

    #region 单例模式
    private static MainTowerObject instance;

    public static MainTowerObject Instance => instance;
    #endregion

    private void Awake()
    {
        instance = this;
    }


    /// <summary>
    /// 更新血量的方法
    /// </summary>
    /// <param name="nowhp"></param>
    /// <param name="maxhp"></param>
    public void UpdateTowerHp(int nowhp, int maxhp)
    {
        this.nowHp = nowhp;
        this.maxHp = maxhp;

        //更新游戏界面上的血量
        EventCenter.Instance.EventTrigger<MainTowerHpInfo>("主塔血量变化", new MainTowerHpInfo(nowHp, maxHp));
    }


    public void Wound(int damage)
    {
        //如果主塔已经被摧毁
        if (isDestroy) return;

        nowHp -= damage;
        //防止血量显示为负数
        nowHp = Mathf.Max(nowHp, 0);

        //更新血量
        UpdateTowerHp(nowHp, maxHp);

        if (nowHp <= 0)
        {
            isDestroy = true;
            EventCenter.Instance.EventTrigger("主塔摧毁");
        }



    }


    /// <summary>
    /// 单例模式过场景时系统自动调用，用于清除单例模式引用对象
    /// </summary>
    private void OnDestroy()
    {
        instance = null;
    }
}



public struct MainTowerHpInfo
{
    public int nowHp;
    public int maxHp;

    public MainTowerHpInfo(int nowHp, int maxHp)
    {
        this.nowHp = nowHp;
        this.maxHp = maxHp;
    }
}
