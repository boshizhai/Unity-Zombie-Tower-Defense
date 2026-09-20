using System.Collections.Generic;
using UnityEngine;

public class UIMgr
{
    //Canvas是所有UI的根节点，所有的UI都需要挂在Canvas下，所以在UIMgr中需要一个Canvas的引用
    private GameObject canvas;

    //用一个字典来存储所有的面板，key是面板的名字，value是面板的预制体实例
    Dictionary<string ,BasePanel> panelDic=new Dictionary<string ,BasePanel>();


    #region 单例模式
    private static UIMgr instance=new UIMgr();
    private UIMgr()
    {
        //在构造函数中加载Canvas预制体，并将其实例化
        canvas = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Canvas"));
        //在实例化Canvas后，需要将其设置为不销毁，这样在切换场景时，Canvas不会被销毁
        GameObject.DontDestroyOnLoad(canvas);
    }

    public static UIMgr Instance => instance;
    #endregion

    //显示面板
    public T ShowPanel<T>() where T : BasePanel  //泛型约束，防止传入的类型不是BasePanel的子类
    {
        //我们定义一个规则：面板挂载的脚本的类型和面板的预制体的名字是一样的，所以我们可以通过类型的名字来获取预制体的名字
        string panelName = typeof(T).Name;

        //如果面板已经存在，就直接返回字典中存储的实例
        if (panelDic.ContainsKey(panelName)) return panelDic[panelName] as T;

        //实例化一个面板预制体,设置其父对象为Canvas，并获取其脚本
        T panel=GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/UI/"+ panelName),canvas.transform,false).GetComponent<T>();
        //调用这个面板的函数ShowMe函数显示它
        panel.ShowMe();
        //往字典中添加这个面板的实例
        panelDic.Add(panelName, panel);
        return panel;
    }

    //隐藏面板
    public void HidePanel<T>(bool isFade = true) where T : BasePanel
    {
        string panelName = typeof(T).Name;

        //如果面板不存在，就直接返回
        if (!panelDic.ContainsKey(panelName)) return;

        //得到面板的脚本
        T panel = panelDic[panelName] as T;
        switch (isFade)
        {
            //使用淡出效果隐藏面板
            case true:
                //调用该面板的HideMe函数，并传入一个回调函数：销毁面板后，从字典中移除这个面板的实例
                panel.HideMe(() =>
                {
                    GameObject.Destroy(panel.gameObject);
                    //在面板隐藏后，从字典中移除这个面板的实例，并销毁它
                    panelDic.Remove(panelName);
                });
                break;
            //直接隐藏面板
            case false:
                GameObject.Destroy(panel.gameObject);
                //在面板隐藏后，从字典中移除这个面板的实例，并销毁它
                panelDic.Remove(panelName);
                break;
        }
    }


    //得到面板
    public T GetPanel<T>() where T : BasePanel
    {
        string panelName = typeof(T).Name;
        //如果面板不存在，就直接返回null
        if (!panelDic.ContainsKey(panelName)) return null;

        //返回面板脚本
        return panelDic[panelName] as T;
    }
}
