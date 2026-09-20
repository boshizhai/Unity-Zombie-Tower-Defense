using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 资源加载模块
/// 0.通过加载：引用对象（1.Resources.Load，2.Game.Instantiate<加载ScriptObject中的引用>)
/// 1.异步加载
/// 2.委托和lambda表达式
/// 3.协程
/// 4.泛型
/// </summary>
public class ResMgr : BaseManager<ResMgr>
{
    private ResMgr() { }


    /// <summary>
    /// 同步加载资源，如果是GameObject则克隆一份的GameObject并返回给外部
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public T Load<T>(string path) where T : Object
    {
        T res = Resources.Load<T>(path);
        //如果是一个GameObject类型的 把他实例化后再返回出去 外部就可以直接使用了
        if (res is GameObject)
            return GameObject.Instantiate(res);
        else //TextAsset AudioClip
            return res;
    }

    /// <summary>
    /// 只加载资源不实例化，用于获取预制体：引用音频，资源配置等文件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public T LoadAsset<T>(string path) where T : Object
    {
        T asset = Resources.Load<T>(path);
        if (asset == null)
        {
            Debug.LogError($"资源加载失败：路径：{path}，类型：{typeof(T).Name}");
        }
        return asset;
    }

    /// <summary>
    /// 根据ScriptObejct中已有的预制体引用创建实例
    /// 适用于UI、角色创建、挂载在指定节点下的特效
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="parent"></param>
    /// <param name="worldPositionStays"></param>
    /// <returns></returns>
    public GameObject InstantiatePrefab(GameObject prefab, Transform parent = null, bool worldPositionStays = false)
    {
        if (prefab == null)
        {
            Debug.LogError("创建对象失败，预制体引用为空：" + prefab.name);
        }
        return GameObject.Instantiate(prefab, parent, worldPositionStays);
    }

    /// <summary>
    /// 根据已有的预制体引用，在指定世界位置和旋转下创建实例。
    /// 适用于玩家、怪物、防御塔。
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public GameObject InstantiatePrefab(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (prefab == null)
        {
            Debug.LogError("创建对象失败：预制体引用为空");
        }
        return GameObject.Instantiate(prefab, position, rotation, parent);
    }



    /// <summary>
    /// 异步加载资源
    /// 返回值为空，因为资源不能立马返回的，不过我们可以传入委托，再资源加载完成后执行相对应的逻辑
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="action"></param>
    public void LoadAsync<T>(string path, UnityAction<T> action) where T : Object
    {
        MonoMgr.Instance.StartCoroutine(ReallyLoadAsync(path, action));
    }

    /// <summary>
    /// 真正的协同程序函数，用来开启异步加载对应的资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    private IEnumerator ReallyLoadAsync<T>(string path, UnityAction<T> action) where T : Object
    {
        ResourceRequest operation = Resources.LoadAsync<T>(path);
        //等待资源加载完成
        yield return operation;

        T asset = operation.asset as T;
        if (asset == null)
        {
            Debug.Log($"资源加载失败，路径{path}，类型{typeof(T).Name}");
        }
        //如果加载对象是GameObject,返回实例对象，其他类型返回资源
        if (operation.asset is GameObject)
            action.Invoke(GameObject.Instantiate(operation.asset) as T);
        else
            action.Invoke(operation.asset as T);
    }
}