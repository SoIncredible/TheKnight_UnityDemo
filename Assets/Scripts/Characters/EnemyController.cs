using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;




//枚举
public enum EnemyStates { GUARD, PATROL, CHASE, DEAD}


//自动添加所需要的组件
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharacterStates))]
public class EnemyController : MonoBehaviour,IEndGameObserver
{

    private EnemyStates enemyStates;
    //FSM状态机 WEBGL游戏的教学视频中有教学
    private NavMeshAgent agent;
    [Header("Basic Settings")]
    //设置怪物的可视范围
    public float sightRadius;
    public float attackRadius; //小怪的攻击范围
    public bool isGuard;
    protected GameObject attackTarget;

    public float lookAtTime;
    private float remainLookAtTime;
    
    private float speed;
    private Animator anim;
    private Collider coll;
    protected CharacterStates characterStates;

    
    private float lastAttackTime;
    private Quaternion guardRotation;

    // bool配合动画
    bool isWalk;
    bool isChase;
    bool isFollow;
    bool isDead;
    bool playerDead = false;
    [Header("Patrol State")]
    public float patrolRange;
    private Vector3 wayPoint;

    private Vector3 guardPos;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStates = GetComponent<CharacterStates>();
        coll = GetComponent<BoxCollider>();
        speed = agent.speed;
        guardPos = transform.position;
        guardRotation = transform.rotation;
        remainLookAtTime = lookAtTime;

    }

    private void Start()
    {
        if (isGuard)
        {
            enemyStates = EnemyStates.GUARD;
        }
        else
        {
            enemyStates = EnemyStates.PATROL;
            GetNewWayPoint();
        }
        //TODO:场景切换后修改掉
        GameManager.Instance.AddObserver(this);
    }


    //切换场景时启用
    /*
    void OnEnable()
    {
        GameManager.Instance.AddObserver(this);
    }
    */
    void OnDisable()
    {
        if (!GameManager.IsInitialized) return;
        GameManager.Instance.RemoveObserver(this);
    }


    private void Update()
    {
        
        if(characterStates.CurrentHealth == 0)
        {
            isDead = true;
        }
        if (!playerDead)
        {
            SwitchStates();
            SwitchAnimation();
            //Hit();
            lastAttackTime -= Time.deltaTime;
        }

       

    }
    void SwitchAnimation()
    {
        anim.SetBool("Walk", isWalk);
        anim.SetBool("Chase", isChase);
        anim.SetBool("Follow", isFollow);
        anim.SetBool("Critical", characterStates.isCritical);
        anim.SetBool("Death", isDead);
    }

    void SwitchStates()
    {

        if (isDead)
        {
            enemyStates = EnemyStates.DEAD;
 
        }

        //如果发现player，切换到CHASE；
        else if (FoundPlayer())
        {
            enemyStates = EnemyStates.CHASE;
           
        }
        switch (enemyStates)
        {
            case EnemyStates.GUARD:
                isChase = false;
                if (transform.position != guardPos)
                {
                    isWalk = true;
                    agent.isStopped = false;
                    agent.destination = guardPos;
                    if(Vector3.SqrMagnitude(guardPos-transform.position) <= agent.stoppingDistance)
                    {
                        isWalk = false;
                        transform.rotation = Quaternion.Lerp(transform.rotation,guardRotation,0.01f);
                    }
                }
                break;
            case EnemyStates.PATROL:

                isChase = false;
                agent.speed = speed * 0.5f;
                //
                if (Vector3.Distance(wayPoint, transform.position) <= agent.stoppingDistance)
                {
                    isWalk = false;
                    if (remainLookAtTime > 0)
                    {
                        remainLookAtTime -= Time.deltaTime;
                    }
                    else
                    {
                        GetNewWayPoint();
                    }
                    
                }else
                {
                    isWalk = true;
                    agent.destination = wayPoint;
                }
                break;
            case EnemyStates.CHASE:
                //TODO:追Player
                //TODO:配合动画
                isWalk = false;
                isChase = true;
                agent.speed = speed;
                if (!FoundPlayer())
                {
                    //TODO:拉脱回到上一个状态
                    isFollow = false;
                    if (remainLookAtTime > 0)
                    {
                        agent.destination = transform.position;
                        remainLookAtTime -= Time.deltaTime;
                    }
                    else if (isGuard) enemyStates = EnemyStates.GUARD;
                    else enemyStates = EnemyStates.PATROL;
                    agent.destination = transform.position;
                }
                //小怪追上来了，并且进入了小怪的攻击范围
                /*
                else if(FoundPlayer() && InAttackRaidus())
                {

                    Debug.Log("进入小怪攻击范围");
                    isChase = true;
                    isFollow = false;
                }
                */
                else
                {
                    //Debug.Log("小怪一直追啊追");
                    isFollow = true;
                    agent.isStopped = false;
                    agent.destination = attackTarget.transform.position;
                }
                if(TargetInAttackrange() || targetInSkillRange())
                {
                    isFollow = false;
                    agent.isStopped = true;
                    if(lastAttackTime < 0)
                    {
                        lastAttackTime = characterStates.attackData.coolDown;
                        //暴击判断
                        characterStates.isCritical = Random.value < characterStates.attackData.criticalChance;
                        //Hit();
                        //执行攻击
                        Attack();
                    }
                }
                break;
            case EnemyStates.DEAD:
                coll.enabled = false;
                //agent.enabled = false;
                agent.radius = 0;
                Destroy(gameObject, 2f);
                break;
        }
    }
    void Attack()
    {
        transform.LookAt(attackTarget.transform);
        if (TargetInAttackrange())
        {
            //近身攻击动画
            anim.SetTrigger("Attack");
        }
        if (targetInSkillRange())
        {
            //技能攻击动画
            anim.SetTrigger("Skill");
        }
    }
    bool FoundPlayer()
    {
        var colliders = Physics.OverlapSphere(transform.position, sightRadius);
        foreach(var target in colliders)
        {
            if (target.CompareTag("Player"))
            {
                attackTarget = target.gameObject;
                return true;
            }
        }
        attackTarget = null;
        return false;
    }
    bool InAttackRaidus()
    {
        var colliders = Physics.OverlapSphere(transform.position, attackRadius);
        foreach(var target in colliders)
        {
            if (target.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }
    bool TargetInAttackrange()
    {
        if (attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStates.attackData.attackRange;
        else
            return false;
    }
    bool targetInSkillRange()
    {

        if (attackTarget != null)
        {
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStates.attackData.skillRange;
        }
            
        else
            return false;
    }

    void GetNewWayPoint()
    {

        remainLookAtTime = lookAtTime;

        float randomX = Random.Range(-patrolRange, patrolRange);
        float randomZ = Random.Range(-patrolRange, patrolRange);
        Vector3 randomPoint = new Vector3(guardPos.x + randomX, transform.position.y, guardPos.z + randomZ);
        //TODO:可能会出问题
        wayPoint = randomPoint;
        NavMeshHit hit;
        wayPoint = NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1) ? hit.position : transform.position;

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRadius);
    }





    void Hit()
    {
        if (attackTarget != null && transform.IsfacingTarget(attackTarget.transform))
        {
            var targetStates = attackTarget.GetComponent<CharacterStates>();
            targetStates.TakeDamage(characterStates, targetStates);
        }
    }

    public void EndNotify()
    {
        //获胜
        //停止所有移动
        //停止Agent
        isChase = false;
        isWalk = false;
        playerDead = true;
        attackTarget = null;
        anim.SetBool("Win", true);

    }
}
