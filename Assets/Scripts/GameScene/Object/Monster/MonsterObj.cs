using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static Unity.VectorGraphics.SVGParser;

public class MonsterObj : MonoBehaviour
{
    //怪物身上挂载的组件
    private Animator animator;
    private NavMeshAgent agent;

    //怪物当前的血量
    public float nowHp;

    //上次攻击的时间，攻击冷却需要用到
    private float frontTime;

    //当前怪物是否死亡的标记
    public bool isDead;

    //记录当前怪物的信息
    private MonsterConfig monsterConfig;

    //用于攻击的检测
    private Collider[] colliders;


    // 缓存怪物身上的碰撞器，死亡时关闭，复用时重新开启。
    private Collider bodyCollider;

    // 标记死亡动画事件是否已经处理，防止重复执行死亡结算或回收。
    private bool deathEventHandled;

    // 标记出生动画是否播放完成，用于控制怪物何时开始移动和攻击。
    private bool bornCompleted;



    private void Awake()
    {
        EnsureComponents();
    }

    private void EnsureComponents()
    {
        //初始化组件,如果不存在就手动添加该组件
        if (animator == null)
            animator = this.GetComponent<Animator>() ?? this.AddComponent<Animator>();
        if (agent == null)
            agent = this.GetComponent<NavMeshAgent>() ?? this.AddComponent<NavMeshAgent>();
        if (bodyCollider == null)
            bodyCollider = this.GetComponent<Collider>();

        //给攻击防御塔的碰撞器数组分配空间，注意：范围检测要求必须传入一个碰撞器数组
        if (colliders == null)
            colliders = new Collider[1];
    }


    /// <summary>
    /// 提供给外部创建怪物时初始化怪物信息
    /// </summary>
    /// <param name="config"></param>
    public void InitMonster(MonsterConfig config)
    {
        if (config == null)
        {
            Debug.LogError("怪物初始化失败：MonsterConfig 为空");
            return;
        }

        EnsureComponents();

        //停止僵尸脚本生上所有的延迟函数,如攻击的重复函数
        CancelInvoke();

        //初始化僵尸的信息
        monsterConfig = config;
        nowHp = config.MaxHealth;
        isDead = false;
        deathEventHandled = false;
        bornCompleted = false;
        frontTime = Time.time;

        //启用当前僵尸组件
        this.enabled = true;

        //死亡时关闭碰撞器，在重新出生时恢复
        bodyCollider.enabled = true;

        //暂时关闭寻路，先完成出生位置和动画初始化
        agent.enabled = false;
        agent.speed = agent.acceleration = config.MoveSpeed;
        agent.angularSpeed = config.RotateSpeed;

        //从僵尸的配置文件中随机取出状态机类型
        animator.runtimeAnimatorController = config.GetRandomAnimatorController();

        //保存出生点，避免重置动画影响根节点姿态(这个位置在出生点生成僵尸时设置成出生点的位置和旋转了)
        Vector3 spawnPosition = transform.position;
        Quaternion spawnRotation = transform.rotation;

        //激活僵尸对象
        gameObject.SetActive(true);

        //清除上一次的动画状态，重新进入默认出生状态
        animator.Rebind();
        animator.ResetTrigger("Wound");
        animator.SetBool("Run", false);
        animator.ResetTrigger("Attack");
        animator.SetBool("Dead", false);

        transform.SetPositionAndRotation(spawnPosition, spawnRotation);

        agent.enabled = true;

        if (agent.isOnNavMesh)
        {
            //重新计算寻路路线
            agent.ResetPath();
            //设置寻路组件的当前速度为零
            agent.velocity = Vector3.zero;
            //先停止寻路，后续播放完出生动画后再开始寻路
            agent.isStopped = true;
        }
        else
        {
            Debug.LogError("僵尸出生点不可在NavMesh上", this);
        }
    }



    private void Update()
    {
        if (isDead || !bornCompleted)
            return;

        //如果怪物的瞬时速度不等于0则播放跑步动画
        animator.SetBool("Run", agent.velocity != Vector3.zero);

        //满足冷却时长，播放攻击动画
        if (Vector3.Distance(transform.position, MainTowerObject.Instance.transform.position) <= 6 && Time.time - frontTime >= monsterConfig.AttackInterval)
        {
            animator.SetTrigger("Attack");
            frontTime = Time.time;
        }
    }


    /// <summary>
    /// 怪物受伤，扣血，播放受伤动画（如果没死亡)
    /// </summary>
    public void Wound(int damage)
    {
        if (isDead) return;

        //如果怪物扣血后没死则播放受伤动画，否则播放死亡动画
        if ((nowHp -= (damage - monsterConfig.Defense)) > 0)
        {
            animator.SetTrigger("Wound");
            Debug.Log("怪物受伤了");
            //逻辑需补充：播放受伤音效
            GameDataMgr.Instance.PlaySound("Music/Wound");
        }
        else
        {
            Dead();
        }
    }

    /// <summary>
    /// 死亡函数，用于触发播放死亡动画
    /// </summary>
    private void Dead()
    {
        //更新死亡标记
        isDead = true;
        bodyCollider.enabled = false;

        animator.SetTrigger("Dead");

        //寻路组件停止移动
        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        //逻辑需补充：播放死亡音效
        //逻辑需补充：加钱
        GameDataMgr.Instance.PlaySound("Music/Dead");
        EventCenter.Instance.EventTrigger<MonsterObj>("怪物死亡", this);
    }

    /// <summary>
    /// 攻击动画事件，范围检测，主防御塔受伤
    /// </summary>
    public void AttackEvent()
    {
        if (isDead || !bornCompleted)
            return;

        GameDataMgr.Instance.PlaySound("Music/Eat");

        Physics.OverlapSphereNonAlloc(transform.position + transform.forward + transform.up,
                                      2, colliders, 1 << LayerMask.NameToLayer("MainTower"));
        foreach (Collider collider in colliders)
        {
            MainTowerObject.Instance.Wound(monsterConfig.Attack);
        }
    }


    /// <summary>
    /// 出生动画事件，开始移动
    /// </summary>
    public void CompletedBorn()
    {
        if (isDead || bornCompleted)
            return;

        bornCompleted = true;

        //出生动画完成之后//怪物朝向生命塔前进
        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.SetDestination(MainTowerObject.Instance.transform.position);
        }
    }


    /// <summary>
    /// 死亡动画事件
    /// </summary>
    public void DeadEvent()
    {
        //放置重复动画事件造成的重复回收
        if (!isDead || deathEventHandled)
            return;

        //死亡动画播放完成标记为真
        deathEventHandled = true;

        //回收前关闭寻路组件
        agent.enabled = false;

        //使用源预制体引用作为对象池的键 重要！！！！ 重要！！！！！
        PoolMgr.Instance.PushObj(monsterConfig.MonsterPrefab, this.gameObject);

        //僵尸死亡动画播放完毕后触发：僵尸退场
        EventCenter.Instance.EventTrigger<MonsterObj>("怪物退场", this);
    }
}
