using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Zombie : MonoBehaviour
{
    [SerializeField] private int HP = 100; // SerializeField - show private value in inspector
    private Animator animator;

    private NavMeshAgent navAgent;

    // Start is called before the first frame update
    void Start()
    {
        animator = this.GetComponent<Animator>();
        navAgent = this.GetComponent<NavMeshAgent>();
    }

    public void TakeDamage(int damageAmount) {
        HP -= damageAmount;

        if (HP <= 0)
        {
            // a way to die?
            Destroy(this.gameObject);
        } else {
            animator.SetTrigger("DAMAGE");
        }
    }

    void Update()
    {
        if (navAgent.velocity.magnitude > 0.1f)
        {
            animator.SetBool("isWalking", true);
        }
        else 
        {
            animator.SetBool("isWalking", false);
        }
    }
}
