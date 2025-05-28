using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieWalkingState : StateMachineBehaviour
{
    Transform player;
    NavMeshAgent navAgent;

    public float detectionArea = 18f;
    public float patrolSpeed = 2f;

    public List<Transform> waypointsList = new List<Transform>();

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        navAgent = animator.GetComponent<NavMeshAgent>();

        navAgent.speed = patrolSpeed;

        // --- Get all the waypoints and move to the first waypoint --- //

        if (waypointsList.Count == 0)
        {
            GameObject waypointCluster = GameObject.FindGameObjectWithTag("Waypoints");
            foreach (Transform t in waypointCluster.transform)
            {
                waypointsList.Add(t);
            }
        }

        Vector3 nextPosition = waypointsList[Random.Range(0, waypointsList.Count)].position;
        navAgent.SetDestination(nextPosition);
    }

    //OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // --- Check if the agent is at the waypoint and move it to another one --- //

        if (navAgent.remainingDistance <= navAgent.stoppingDistance) {
            animator.SetBool("isWalking", false);
            navAgent.SetDestination(waypointsList[Random.Range(0, waypointsList.Count)].position);
        }

        // --- Transition to chase state --- //

        float distanceFromPlayer = Vector3.Distance(player.position, animator.transform.position);

        if (distanceFromPlayer < detectionArea)
        {
            animator.SetBool("isChasing", true);
        }
    }

    //OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // stop the agent
        navAgent.SetDestination(navAgent.transform.position);
    }
}
