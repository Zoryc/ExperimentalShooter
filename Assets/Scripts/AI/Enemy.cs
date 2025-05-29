using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    [SerializeField] private int HP = 100; // SerializeField - show private value in inspector
    private Animator animator;

    private NavMeshAgent navAgent;

    public bool isDead;

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
            isDead = true;

        } else {
            animator.SetTrigger("DAMAGE");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, 4f);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(this.transform.position, 18f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(this.transform.position, 21f);
    }
}
