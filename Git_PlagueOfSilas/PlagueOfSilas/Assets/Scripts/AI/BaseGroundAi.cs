using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BaseGroundAi : MonoBehaviour
{
    [SerializeField] State CurrentState;
    [SerializeField] List<Transform> PatrolPoints;
    [SerializeField] float FieldOfView;
    [SerializeField] float SightRange;
    [SerializeField] float AttackRange;
    Transform Player;
    Vector3 PointOfInterest;
    NavMeshAgent Agent;

    int CurrentPatrolPoint = 0;

    private void Start()
    {
        if (GetComponent<NavMeshAgent>())
            Agent = GetComponent<NavMeshAgent>();
        else
        {
            gameObject.AddComponent<NavMeshAgent>();
            Agent = GetComponent<NavMeshAgent>();
        }

        Player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        switch (CurrentState)
        {
            case State.Partoling:
                Patrol();

                if (SentCheck())
                    CurrentState = State.Tracking;

                if (VisionCheck())
                    CurrentState = State.Chasing;

                break;
            case State.Searching:
                SearchArea();

                if (SentCheck())
                    CurrentState = State.Tracking;

                if (VisionCheck())
                    CurrentState = State.Chasing;

                break;
            case State.Tracking:
                Track();

                if (VisionCheck())
                    CurrentState = State.Chasing;

                break;
            case State.Chasing:
                if (VisionCheck())
                {
                    Agent.SetDestination(Player.position);

                    if (Agent.remainingDistance < AttackRange)
                        CurrentState = State.Attacking;
                }
                else if (Agent.remainingDistance < 1)
                    CurrentState = State.Searching;

                break;
            case State.Attacking:

                if (Vector3.Distance(transform.position, Player.position) > AttackRange)
                    CurrentState = State.Chasing;
                break;
            default:
                break;
        }
    }

    void Patrol()
    {
        Debug.Log(Agent.remainingDistance);
        if (Agent.remainingDistance < 1)
        {
            if (++CurrentPatrolPoint >= PatrolPoints.Count)
                CurrentPatrolPoint = 0;

        }

        Agent.SetDestination(PatrolPoints[CurrentPatrolPoint].position);
    }

    void SearchArea()
    {

    }

    void Track()
    {

    }

    void Attack()
    {

    }

    bool SentCheck()
    {

        return false;
    }

    bool VisionCheck()
    {
        if (Vector3.Distance(transform.position, Player.position) <= SightRange)
        {
            if (Vector3.Angle(transform.forward, Player.position - transform.position) <= FieldOfView)
            {
                RaycastHit hit;

                if (Physics.Raycast(transform.position + transform.forward, Player.position - (transform.position + transform.forward), out hit))
                {
                    Debug.DrawRay(transform.position + transform.forward, Player.position - (transform.position + transform.forward), Color.red);

                    if (hit.collider.gameObject.transform == Player)
                        return true;

                }

            }

        }

        return false;
    }

}

public enum State { Partoling, Searching, Tracking, Chasing, Attacking }