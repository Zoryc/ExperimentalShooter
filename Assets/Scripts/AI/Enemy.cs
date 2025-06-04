using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    public int HP = 100;

    private Animator animator;
    private NavMeshAgent navAgent;

    public bool isDead = false;

    [Header("Roaming setting")]
    public bool isRoaming = false;
    public float patrolRadius = 18f;
    public float chaseRadius = 21f;
    public float attackRadius = 4f;

    void Start()
    {
        animator = this.GetComponent<Animator>();
        animator.enabled = isRoaming;

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
        if (isRoaming)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(this.transform.position, attackRadius);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(this.transform.position, patrolRadius);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(this.transform.position, chaseRadius);
        }
    }
}
