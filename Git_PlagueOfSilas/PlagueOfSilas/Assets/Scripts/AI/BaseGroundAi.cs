using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BaseGroundAi : MonoBehaviour
{
    [SerializeField] State CurrentState;
    [Header("Patrol Stats")]
    [SerializeField] List<Transform> PatrolPoints;
    [SerializeField] float PatrolSpeed;

    [Header("Search Stats")]
    [SerializeField] float MaxSearchTime;
    [SerializeField] float SearchSpeed;
    [SerializeField] float SearchRange;
    [SerializeField] float SearchTime;

    [Header("Vision Stats")]
    [SerializeField] bool CanSee;
    [SerializeField] float FieldOfView;
    [SerializeField] float SightRange;

    [Header("Tracking Stats")]
    [SerializeField] bool CanSmell;
    [SerializeField] float SentRange;
    [SerializeField] float TrackingSpeed;

    [Header("Attack Stats")]
    [SerializeField] float AttackRange;
    [SerializeField] float ChaseSpeed;
    

    Transform Player;
    Vector3 PointOfInterest;
    NavMeshAgent Agent;
    Sent CurrentSent;
    float Timer1 = 0;
    float Timer2 = 0;

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
                   ChangeState(State.Tracking);

                if (VisionCheck())
                    ChangeState(State.Chasing);

                break;
            case State.Searching:
                if (!SearchArea())
                    ChangeState(State.Partoling);

                if (SentCheck())
                    ChangeState(State.Tracking);

                if (VisionCheck())
                    ChangeState(State.Chasing);

                break;
            case State.Tracking:
                if (!Track())
                    ChangeState(State.Searching);

                if (VisionCheck())
                    ChangeState(State.Chasing);

                break;
            case State.Chasing:
                Agent.speed = ChaseSpeed;

                if (VisionCheck())
                {
                    Agent.SetDestination(Player.position);

                    if (Agent.remainingDistance < AttackRange)
                        ChangeState(State.Attacking);
                }
                else if (Agent.remainingDistance < 1)
                    ChangeState(State.Searching);

                break;
            case State.Attacking:
                Attack();

                if (Vector3.Distance(transform.position, Player.position) > AttackRange)
                    CurrentState = State.Chasing;
                break;
            default:
                break;
        }
    }

    void Patrol()
    {
        if (Agent.remainingDistance < 1)
        {
            if (++CurrentPatrolPoint >= PatrolPoints.Count)
                CurrentPatrolPoint = 0;

        }

        Agent.speed = PatrolSpeed;
        Agent.SetDestination(PatrolPoints[CurrentPatrolPoint].position);
    }

    bool SearchArea()
    {
        if (Timer2 > MaxSearchTime)
            return false;
        else
            Timer2 += Time.deltaTime;

        if (Timer1 < 0)
        {
            Vector3 Point = (Random.insideUnitSphere * SearchRange) + PointOfInterest;

            NavMeshHit hit;
            NavMesh.SamplePosition(Point, out hit, SearchRange, 1);

            Agent.SetDestination(hit.position);

            Timer1 = SearchTime;
        }
        else
            Timer1 -= Time.deltaTime;

        Agent.speed = SearchSpeed;
        return true;
    }

    bool Track()
    {
        if (Agent.remainingDistance < 1)
        {
            if (CurrentSent.GetNext() == null)
                return false;
            else
                CurrentSent = CurrentSent.GetNext();
        }

        if (CurrentSent == null)
            SentCheck();
        else
            Agent.SetDestination(CurrentSent.transform.position);

        Agent.speed = TrackingSpeed;
        return true;
    }

    virtual protected void Attack()
    {

    }

    bool SentCheck()
    {
        if (!CanSmell || Sent.Trail == null)
            return false;

        foreach (Sent sent in Sent.Trail)
        {
            if(Vector3.Distance(transform.position, sent.transform.position) <= SentRange)
            {
                CurrentSent = sent;
                PointOfInterest = CurrentSent.transform.position;
                return true;
            }
        }

        return false;
    }

    bool VisionCheck()
    {
        if (!CanSee)
            return false;

        if (Vector3.Distance(transform.position, Player.position) <= SightRange)
        {
            if (Vector3.Angle(transform.forward, Player.position - transform.position) <= FieldOfView)
            {
                RaycastHit hit;

                if (Physics.Raycast(transform.position + transform.forward, Player.position - (transform.position + transform.forward), out hit))
                {

                    Debug.DrawRay(transform.position + transform.forward, Player.position - (transform.position + transform.forward), Color.red);
                    Debug.Log(hit.collider.name);
                    if (hit.collider.gameObject.transform == Player)
                    {
                        PointOfInterest = Player.position; 
                        return true;
                    }
                }

            }

        }

        return false;
    }

    public void Alert(Transform _point)
    {
        PointOfInterest = _point.position;
        CurrentState = State.Chasing;
        Agent.SetDestination(_point.position);
    }

    public void ChangeState(State state)
    {
        Timer1 = 0;
        Timer2 = 0;
        CurrentState = state;
    }

    private void OnDrawGizmos()
    {
        if (CanSmell)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, SentRange);
        }
        if(CanSee)
        {

        }
    }
}

public enum State { Partoling, Searching, Tracking, Chasing, Attacking }