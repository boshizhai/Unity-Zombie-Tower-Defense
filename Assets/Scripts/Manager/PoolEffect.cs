

using UnityEngine;

public class PoolEffect : MonoBehaviour
{
    //记录特效的预制体
    private GameObject sourcePrefab;
    //记录特效的所有粒子系统组件
    private ParticleSystem[] particles;
    //特效播放时长的倒计时
    private float remainintTime;
    private bool isPlayering;


    /// <summary>
    /// 真正的播放特效，对特效进行复位，复用逻辑的核心
    /// </summary>
    /// <param name="prefab">要引用的对象</param>
    /// <param name="duration"></param>
    private void Play(GameObject prefab, float duration)
    {
        //设置脚本关联的特效预制体，总播放时长，设置播放状态
        sourcePrefab = prefab;
        remainintTime = duration;
        isPlayering = true;

        //如果脚本组件数组为空，则获取这个特效预制体所有子对象的粒子系统组件
        if (particles == null)
            particles = this.GetComponentsInChildren<ParticleSystem>(true);

        //【A】复位
        //清除所有组件上一次播放留下的粒子
        foreach (ParticleSystem particle in particles)
        {
            particle.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        //【B】播放
        this.gameObject.SetActive(true);

        //当前所有的攻击特效都是使用Play On Awake
        foreach (ParticleSystem particle in particles)
        {
            if (particle.gameObject.activeInHierarchy && particle.main.playOnAwake)
            {
                particle.Play(false);
            }
        }
    }

    /// <summary>
    /// 每帧倒计时，时间到了就自动回收
    /// </summary>
    private void Update()
    {
        if (!isPlayering)
            return;

        if ((remainintTime -= Time.deltaTime) > 0)
            return;

        //【回收】特效的剩余播放小于0，进行自动回收特效
        isPlayering = false;

        foreach (ParticleSystem particle in particles)
        {
            particle.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        /// 通过对象池回收对象：源预制体代表池中的键，克隆后进处理过的GameObject才是真正要放入池中的对象
        PoolMgr.Instance.PushObj(sourcePrefab, this.gameObject);
    }

    //兜底保护
    private void OnDisable()
    {
        isPlayering = false;
    }


    //【A】静态方法，播放特效的入口，在世界坐标下生成特效，比如命中特效、范围技能特效
    public static void Spawn(GameObject prefab, Vector3 position, Quaternion rotation, float duration)
    {
        if (prefab == null)
            return;

        GameObject obj = PoolMgr.Instance.GetObj(prefab);
        if (obj == null)
            return;
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.transform.localScale = prefab.transform.localScale;

        GetEffect(obj).Play(prefab, duration);
    }


    public static void Spawn(GameObject prefab, Transform parent, float duration)
    {
        if (prefab == null)
            return;

        GameObject obj = PoolMgr.Instance.GetObj(prefab, parent);
        if (obj == null)
            return;

        // 恢复预制体原有的局部位置、旋转和缩放。
        obj.transform.localPosition = prefab.transform.localPosition;
        obj.transform.localRotation = prefab.transform.localRotation;
        obj.transform.localScale = prefab.transform.localScale;

        GetEffect(obj).Play(prefab, duration);
    }

    //获取或自动给特效预制体添加 PoolEffect 脚本
    private static PoolEffect GetEffect(GameObject obj)
    {
        PoolEffect effect = obj.GetComponent<PoolEffect>();

        if (effect == null)
            effect = obj.AddComponent<PoolEffect>();

        return effect;
    }

}