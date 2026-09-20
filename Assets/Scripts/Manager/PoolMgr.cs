using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.LowLevel;

/// <summary>
/// 缓存池中的抽屉
/// </summary>
public class PoolData
{
    //抽屉的父节点，管理同一类型的未激活对象，作为它的子对象
    public GameObject fatherObj;
    //抽屉的容器
    public List<GameObject> fatherList;

    //构造函数要创建抽屉中的父节点，然后正式往容器中存如第一个内容
    public PoolData(GameObject obj, Transform parentTrans)
    {
        fatherObj = new GameObject(obj.name);
        fatherObj.transform.parent = parentTrans;

        //新建容器，并往里面压入第一个内容
        fatherList = new List<GameObject>();
        PushObj(obj);
    }

    /// <summary>
    /// 提供给外部：往容器里面存储内容
    /// </summary>
    /// <param name="obj"></param>
    public void PushObj(GameObject obj)
    {
        if (obj == null)
            return;

        //放置同一个对象重复回收
        if (fatherList.Contains(obj))
        {
            Debug.LogWarning("对象已经在池中" + obj.name);
            return;
        }
        obj.SetActive(false);
        obj.transform.SetParent(fatherObj.transform, false);
        fatherList.Add(obj);
    }

    /// <summary>
    /// 提供给外部 从缓存池中取出内容，保持未激活状态，交给外部初始化
    /// </summary>
    /// <returns></returns>
    public GameObject GetObj(Transform fatherTrans = null)
    {
        while (fatherList.Count > 0)
        {
            //从容器中取出内容返回给外部
            int index = fatherList.Count - 1;
            GameObject obj = fatherList[index];
            fatherList.RemoveAt(index);

            //兼容对象曾被外部Destory的情况
            if (obj == null)
                continue;

            obj.transform.SetParent(fatherTrans, false);
            return obj;
        }
        return null;
    }
}

/// <summary>
/// 缓存池管理模块，管理各种抽屉，真正的存取对象的入口
/// </summary>
public class PoolMgr : BaseManager<PoolMgr>
{

    /// <summary>
    /// Hierarchy窗口中的存放抽屉的根对象
    /// </summary>
    private GameObject PoolRoot;
    private PoolMgr() { }

    private void EnsureRoot()
    {
        if (PoolRoot != null)
            return;

        //场景切换后，旧的池节点可能被销毁
        poolDic.Clear();
        prefabPoolDic.Clear();

        PoolRoot = new GameObject("PoolRoot");

        //池内全部都是待用对象
        //在这个节点下面创建实例，可避免提前触发OnEnable
        PoolRoot.SetActive(false);
    }

    /// <summary>
    /// 缓存池容器的字典，存储抽屉
    /// </summary>
    public Dictionary<string, PoolData> poolDic = new Dictionary<string, PoolData>();

    /// <summary>
    /// 使用预制体引用作为键，避免不同预制体同名时混入同一个池
    /// </summary>
    public Dictionary<GameObject, PoolData> prefabPoolDic = new Dictionary<GameObject, PoolData>();


    /// <summary>
    /// 字符串形式：从缓存池中取出内容
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public void GetObj(string name, UnityAction<GameObject> callBack)
    {
        if (string.IsNullOrEmpty(name) || callBack == null)
            return;

        EnsureRoot();

        //【a】如果字典中存在该抽屉，且抽屉中容器的长度不为零，直接取出对象
        if (poolDic.TryGetValue(name, out PoolData data))
        {
            GameObject obj = data.GetObj();

            if (obj != null)
            {
                //返回已激活的对象的行为
                obj.SetActive(true);
                callBack.Invoke(obj);
                return;
            }
        }
        //【b】否则新增一个对象
        ResMgr.Instance.LoadAsync<GameObject>(name, obj =>
        {
            if (obj != null)
                obj.name = name;

            //创建成功或失败 都通知调用方
            callBack.Invoke(obj);
        });
    }

    /// <summary>
    /// 字符串形式：把内容放入缓存池中
    /// </summary>
    /// <param name="name"></param>
    /// <param name="obj"></param>
    public void PushObj(string name, GameObject obj, UnityAction pushAction = null)
    {
        if (string.IsNullOrEmpty(name) || pushAction == null)
        {
            return;
        }
        EnsureRoot();

        //先执行传入的事件
        pushAction?.Invoke();

        //【a】先将obj对象失活
        obj.SetActive(false);

        //【b】对象池中有该类型的抽屉，压入内容
        if (poolDic.ContainsKey(name))
        {
            poolDic[name].PushObj(obj);
        }
        //【c】否则新建一个抽屉
        else
        {
            poolDic.Add(name, new PoolData(obj, PoolRoot.transform));
        }
    }

    /// <summary>
    /// 预制体形式：从对象池中引用取出对象
    /// 返回对象保持未激活，初始化完成后由调用方激活
    /// </summary>  
    /// <param name="prefab"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public GameObject GetObj(GameObject prefab, Transform parent = null)
    {
        if (prefab == null)
        {
            Debug.Log("取出对象失败：预制体为空");
            return null;
        }

        EnsureRoot();

        //尝试使用字典获取prefab引用的对象
        if (prefabPoolDic.TryGetValue(prefab, out PoolData data))
        {
            GameObject cacheObj = data.GetObj(parent);

            if (cacheObj != null)
                return cacheObj;
        }

        //没有可复用对象时，通过资源管理器ResMgr创建
        //先创建到未激活的根节点下，避免过早触发OnEnable
        GameObject obj = ResMgr.Instance.InstantiatePrefab(prefab, PoolRoot.transform, false);

        if (obj == null)
            return null;

        obj.SetActive(false);
        obj.transform.SetParent(parent, false);
        return obj;
    }

    /// <summary>
    /// 预制体形式：将预制体压入池中
    /// prefab 必须和取出时使用的预制体一致
    /// </summary>
    /// <param name="prefab">传入的预制体，一般是ScriptObject中引用预制体</param>
    /// <param name="obj">真正要压入的对象池的对象，源预制体克隆后的对象，一般还被附加的脚本</param>
    public void PushObj(GameObject prefab, GameObject obj)
    {
        if (prefab == null || obj == null)
            return;

        EnsureRoot();

        if (prefabPoolDic.TryGetValue(prefab, out PoolData data))
        {
            data.PushObj(obj);
        }
        else
        {
            prefabPoolDic.Add(prefab, new PoolData(obj, PoolRoot.transform));
        }
    }


    /// <summary>
    /// 清空缓存池的方法，主要用在场景切换中
    /// </summary>
    public void Clear()
    {
        // 清空字典
        poolDic.Clear();
        prefabPoolDic.Clear();

        // 对象池根对象不为空时：销毁缓存池对象
        if (PoolRoot != null)
        {
            GameObject.Destroy(PoolRoot);
        }
        //清空字段的引用
        PoolRoot = null;
    }
}