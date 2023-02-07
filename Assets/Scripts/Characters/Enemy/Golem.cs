using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class Golem : EnemyController
{
    [Header("Skill")]
    public float kickForce = 25;

    public GameObject rockPrefab;
    public Transform handPos;
    public void KickOff()
    {
        if(attackTarget != null && transform.IsfacingTarget(attackTarget.transform)){
            var targetStates = attackTarget.GetComponent<CharacterStates>();

            Vector3 direction = (attackTarget.transform.position - transform.position).normalized;
            //direction.Normalize();

            targetStates.GetComponent<NavMeshAgent>().isStopped = true;
            
            //击飞
            targetStates.GetComponent<NavMeshAgent>().velocity = direction * kickForce;
            targetStates.GetComponent<Animator>().SetTrigger("Dizzy");
            targetStates.TakeDamage(characterStates, targetStates);
        }
    }



    public void ThrowRock()
    {
        if(attackTarget != null)
        {
            Debug.Log("该死的快执行啊！");
            var rock = Instantiate(rockPrefab, handPos.position, Quaternion.identity);
            rock.GetComponent<Rock>().target = attackTarget;
        }
    }
}
