using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class PlayerController : MonoBehaviour
{

    private NavMeshAgent agent;
    private Animator anim;
    private GameObject attackTarget;
    private float lastAttackTime;
    private CharacterStates characterStates;
    private bool isDead;
    private float stopDistance;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStates = GetComponent<CharacterStates>();


        stopDistance = agent.stoppingDistance;
    }
    private void OnEnable()
    {
        GameManager.Instance.RigisterPlayer(characterStates);
        MouseManager.Instance.OnMouseClicked += MoveToTarget;
        MouseManager.Instance.OnEnemyClicked += EventAttack;

    }
    private void Start()
    {
       
        SaveManager.Instance.LoadPlayerdata();
    }
    private void OnDisable()
    {
        if (!MouseManager.IsInitialized) return;
        MouseManager.Instance.OnMouseClicked -= MoveToTarget;
        MouseManager.Instance.OnEnemyClicked -= EventAttack;
    }
    private void Update()
    {
        isDead = characterStates.CurrentHealth == 0;

        if (isDead)
            GameManager.Instance.NotifyObserver();
        
        SwitchAnimation();

        lastAttackTime -= Time.deltaTime;
    }

    public void MoveToTarget(Vector3 target)
    {
        StopAllCoroutines();

        if (isDead) return;
        agent.stoppingDistance = stopDistance;
        agent.isStopped = false;
        agent.destination = target;
    }

    private void SwitchAnimation()
    {
        anim.SetFloat("Speed", agent.velocity.sqrMagnitude);
        anim.SetBool("Death", isDead);
    }
    private void EventAttack(GameObject target)
    {
        if (isDead) return;
        //判断角色是否死亡，如果死亡的话就不能移动了
        if (target != null)
        {
            //走到敌人面前停下来
            attackTarget = target;
            characterStates.isCritical = UnityEngine.Random.value < characterStates.attackData.criticalChance;
            StartCoroutine(MoveToAttackTarget());
        } 
    }
    //协程 
    IEnumerator MoveToAttackTarget()
    {
        agent.isStopped = false;
        agent.stoppingDistance = characterStates.attackData.attackRange;
        transform.LookAt(attackTarget.transform);
        while (Vector3.Distance(attackTarget.transform.position, transform.position) > characterStates.attackData.attackRange)
        {
            agent.destination = attackTarget.transform.position;
            yield return null;
        }
        agent.isStopped = true;
        if (lastAttackTime < 0)//设置攻击间隔
        {
            anim.SetBool("Critical", characterStates.isCritical);
            anim.SetTrigger("Attack");
            //重置冷却时间
            lastAttackTime = characterStates.attackData.coolDown;
        }
    }

    void Hit()
    {
        if (attackTarget.CompareTag("Attackable"))
        {
            if (attackTarget.GetComponent<Rock>() && attackTarget.GetComponent<Rock>().rockStates == Rock.RockStates.HitNothing)
            {
                attackTarget.GetComponent<Rock>().rockStates = Rock.RockStates.HitEnemy;
                attackTarget.GetComponent<Rigidbody>().velocity = Vector3.one;
                attackTarget.GetComponent<Rigidbody>().AddForce(transform.forward * 20, ForceMode.Impulse);
            
            }
        }
        else
        {
            var targetStates = attackTarget.GetComponent<CharacterStates>();
            targetStates.TakeDamage(characterStates, targetStates);
        } 
        
        
    
    }

}
    

