using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;




//ö��
public enum EnemyStates { GUARD, PATROL, CHASE, DEAD}


//�Զ��������Ҫ�����
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharacterStates))]
public class EnemyController : MonoBehaviour,IEndGameObserver
{

    private EnemyStates enemyStates;
    //FSM״̬�� WEBGL��Ϸ�Ľ�ѧ��Ƶ���н�ѧ
    private NavMeshAgent agent;
    [Header("Basic Settings")]
    //���ù���Ŀ��ӷ�Χ
    public float sightRadius;
    public float attackRadius; //С�ֵĹ�����Χ
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

    // bool��϶���
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
        //TODO:�����л����޸ĵ�
        GameManager.Instance.AddObserver(this);
    }


    //�л�����ʱ����
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

        //�������player���л���CHASE��
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
                //TODO:׷Player
                //TODO:��϶���
                isWalk = false;
                isChase = true;
                agent.speed = speed;
                if (!FoundPlayer())
                {
                    //TODO:���ѻص���һ��״̬
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
                //С��׷�����ˣ����ҽ�����С�ֵĹ�����Χ
                /*
                else if(FoundPlayer() && InAttackRaidus())
                {

                    Debug.Log("����С�ֹ�����Χ");
                    isChase = true;
                    isFollow = false;
                }
                */
                else
                {
                    //Debug.Log("С��һֱ׷��׷");
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
                        //�����ж�
                        characterStates.isCritical = Random.value < characterStates.attackData.criticalChance;
                        //Hit();
                        //ִ�й���
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
            //����������
            anim.SetTrigger("Attack");
        }
        if (targetInSkillRange())
        {
            //���ܹ�������
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
        //TODO:���ܻ������
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
        //��ʤ
        //ֹͣ�����ƶ�
        //ֹͣAgent
        isChase = false;
        isWalk = false;
        playerDead = true;
        attackTarget = null;
        anim.SetBool("Win", true);

    }
}
