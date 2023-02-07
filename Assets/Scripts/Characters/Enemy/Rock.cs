 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class Rock : MonoBehaviour
{
    private Rigidbody rb;


    //·µ»ØÊ¯Í·
    public enum RockStates { HitPlayer, HitEnemy, HitNothing}
    public RockStates rockStates;

    public GameObject breakEffect;
    [Header("Basic Settings")]


    public float force;
    public GameObject target;
    public int damage;

    private Vector3 direction;

    private void Start()
    {
        
        rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.one;
        rockStates = RockStates.HitPlayer;
        FlyToTarget();
    }

    private void FixedUpdate()
    {
        if(rb.velocity.sqrMagnitude < 1f)
        {
            rockStates = RockStates.HitNothing;
        }

    }




    public void FlyToTarget()
    {

        if(target == null)
        {
            target = FindObjectOfType<PlayerController>().gameObject;
        }
        direction = (target.transform.position - transform.position + Vector3.up).normalized;
        rb.AddForce(direction * force, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        switch (rockStates)
        {
            case RockStates.HitPlayer:
                if (collision.gameObject.CompareTag("Player"))
                {
                    collision.gameObject.GetComponent<NavMeshAgent>().isStopped = true;
                    collision.gameObject.GetComponent<NavMeshAgent>().velocity = direction * force;
                    collision.gameObject.GetComponent<Animator>().SetTrigger("Dizzy");
                    collision.gameObject.GetComponent<CharacterStates>().TakeDamage(damage,collision.gameObject.GetComponent<CharacterStates>());
                    rockStates = RockStates.HitNothing;

                }
                break;


            case RockStates.HitEnemy:
                if (collision.gameObject.GetComponent<Golem>())
                {
                    var otherStates = collision.gameObject.GetComponent<CharacterStates>();
                    otherStates.TakeDamage(damage, otherStates);
                    Instantiate(breakEffect, transform.position, Quaternion.identity);
                    otherStates.GetComponent<Animator>().SetTrigger("Hit");
                    Destroy(gameObject);
                }
                break;
        }
    }



}
