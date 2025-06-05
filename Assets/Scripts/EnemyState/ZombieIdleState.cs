using UnityEngine;

public class ZombieIdleState : StateMachineBehaviour
{
    float timer;
    public float idleTimer = 0.0f;

    Transform player;
    Enemy enemy;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timer = 0.0f;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        enemy = animator.GetComponent<Enemy>();
    }

    //OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        // --- Transition to walking state ---

        timer += Time.deltaTime;
        if (timer > idleTimer)
        {
            animator.SetBool("isWalking", true);
        }

        // --- Transition to chase state ---

        float distanceFromPlayer = Vector3.Distance(player.position, animator.transform.position);

        if (distanceFromPlayer < enemy.patrolRadius)
        {
            animator.SetBool("isChasing", true);
        }
    }
}
