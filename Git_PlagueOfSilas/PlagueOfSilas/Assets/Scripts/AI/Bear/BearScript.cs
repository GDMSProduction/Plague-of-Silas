using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;
using UnityEditor;

public class BearScript : MonoBehaviour 
{

    #region Public Variables

    public enum BearState { patrol = 0, wait, suspect, chase, attack, stunned, search };

    public BearState currentState;

    #endregion

    #region Private  Variables

    NavMeshAgent agent;

    Transform bearHead;

    Transform playerTransform;

    int patrolPointIndex;

    int patrolAreaIndex;

    int currentSearchCount = 0;

    int layermask;

    bool isSearching = false;

    string debugState = "STARTED";

    int searchAreaIndex = -1;

    [SerializeField]
    List<PatrolArea> PatrolAreas;

    [SerializeField]
    List<Transform> SearchAreas;


    [SerializeField]
    bool isAttacking;

    [SerializeField]
    bool isWaiting;

    [SerializeField]
    float visionAngle;

    [SerializeField]
    float visionRange;

    [SerializeField]
    float minChaseDistance;

    [SerializeField]
    float minAttackDistance;

    [SerializeField]
    float patrolSpeed;

    [SerializeField]
    float suspectSpeed;

    [SerializeField]
    float chaseSpeed;

    [SerializeField]
    float attackSpeed;

    [SerializeField]
    float rotateSpeed;

    [SerializeField]
    float wallCheckDistance;

    #endregion

    void Start ()
    {
        currentState = BearState.patrol;

        agent = GetComponent<NavMeshAgent>();

        playerTransform = GameObject.Find("Player").transform;

        bearHead = transform.Find("Head");

        patrolAreaIndex = patrolPointIndex = 0;

        agent.SetDestination(GetPatrolPointPosition());

        isAttacking = false;
        isWaiting = false;

        layermask = LayerMask.GetMask("Player", "Terrain");
    }

    void Update()
    {
        switch (currentState)
        {
            case BearState.patrol:
                {
                    debugState = "PATROL";

                    agent.speed = patrolSpeed;

                    if (CheckForPlayerInVision())
                    {
                        agent.SetDestination(playerTransform.position);

                        float distance = Vector3.Distance(this.transform.position, playerTransform.position);

                        if (distance < visionRange)
                            currentState = distance < minChaseDistance ? BearState.chase : BearState.suspect;
                    }

                    else
                    {
                        agent.SetDestination(GetPatrolPointPosition());

                        if (agent.remainingDistance == 0 && !agent.pathPending)
                        {
                            Vector3 newDirection = Vector3.RotateTowards(transform.forward, GetPatrolPointTransform().forward, 0.05f, 0.0f);
                            transform.LookAt(transform.position + newDirection);

                            if (Vector3.Dot(GetPatrolPointTransform().forward, transform.forward) > 0.97f)
                                currentState = BearState.wait;
                        }
                    }
                }
                break;

            case BearState.wait:
                {
                    debugState = "Wait";

                    if (!isWaiting)
                    {
                        if(Random.Range(1, 10) == 1)
                            SetRandomPatrolArea();

                        IncreasePatrolPointIndex();
                        StartCoroutine(WaitAndGoCoroutine());
                        isWaiting = true;
                    }
                    
                    if (CheckForPlayerInVision())
                    {
                        isWaiting = false;
                        StopCoroutine(WaitAndGoCoroutine());

                        agent.SetDestination(playerTransform.position);

                        float distance = Vector3.Distance(this.transform.position, playerTransform.position);

                        if (distance < visionRange)
                            currentState = distance < minChaseDistance ? BearState.chase : BearState.suspect;
                    }
                }
                break;

            case BearState.suspect:
                {
                    debugState = "SUSPECT";

                    // Set Suspect Speed
                    agent.speed = suspectSpeed;

                    // Go to target position
                    if (CheckForPlayerInVision())
                    {
                        agent.SetDestination(playerTransform.position);

                        if (Vector3.Distance(playerTransform.position, transform.position) < minChaseDistance)
                            currentState = BearState.chase;
                    }

                    else if (agent.remainingDistance == 0 && !agent.pathPending)
                    {
                        currentState = BearState.wait;
                    }
                }
                break;

            case BearState.chase:
                {
                    debugState = "CHASE";

                    agent.speed = chaseSpeed;

                    // Bear can see player
                    if (CheckForPlayerInVision())
                    {
                        agent.SetDestination(playerTransform.position);

                        if (agent.remainingDistance <= minAttackDistance)
                            currentState = BearState.attack;
                    }

                    // Player withing chase distance and not hiding
                    else if(CheckForPlayerWithinDistance(visionRange))
                    {
                        agent.SetDestination(playerTransform.position);
                    }

                    // Player hidden
                    else if (agent.remainingDistance == 0 && !agent.pathPending)
                    {
                        currentState = BearState.search;
                    }
                }
                break;

            case BearState.attack:
                {
                    debugState = "ATTACK";

                    Debug.DrawLine(bearHead.position, bearHead.position + bearHead.forward * wallCheckDistance, Color.blue);

                    if(!isAttacking)
                    {
                        StartCoroutine(AttackCoroutine());
                        isAttacking = true;
                    }
                }
                break;

            case BearState.stunned:
                {
                    debugState = "STUNNED";

                    Debug.DrawLine(bearHead.position, bearHead.position + bearHead.forward * wallCheckDistance, Color.blue);

                    if(!isWaiting)
                    {
                        StartCoroutine(StunnedCoroutine());
                        isWaiting = true;
                    }
                }
                break;

            case BearState.search:
                {
                    debugState = "SEARCH";

                    agent.speed = chaseSpeed;

                    if(CheckForPlayerWithinDistance(minChaseDistance) || CheckForPlayerInVision())
                    {
                        currentSearchCount = 0;
                        currentState = BearState.chase;
                        StopAllCoroutines();
                        isSearching = false;
                        return;
                    }

                    if(!isSearching)
                    {
                        if (currentSearchCount < 3)
                        {
                            currentSearchCount++;
                            StartCoroutine(SearchRoomCoroutine());
                            isSearching = true;
                        }

                        else
                        {
                            currentSearchCount = 0;
                            currentState = BearState.patrol;
                        }
                    }
                }
                break;
        }
    }

    private void IncreasePatrolPointIndex()
    {
        if (++patrolPointIndex >= PatrolAreas[patrolAreaIndex].patrolPoints.Count)
            patrolPointIndex = 0;
    }

    private void SetRandomPatrolArea()
    {
        patrolAreaIndex = Random.Range(0, PatrolAreas.Count - 1);
    }

    bool CheckForPlayerInVision()
    {
        if (Vector3.Angle(playerTransform.position - bearHead.position, transform.forward) < visionAngle &&
            Vector3.Distance(playerTransform.position, transform.position) < visionRange)
        {
            Debug.DrawLine(bearHead.position, playerTransform.position, Color.magenta);

            RaycastHit hit;

            if (Physics.Raycast(bearHead.position, playerTransform.position - bearHead.position, out hit, visionRange))
            {
                if (hit.transform.tag == "Player")
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool CheckForPlayerWithinDistance(float distance)
    {
        RaycastHit hit;


        if (Physics.Raycast(bearHead.position, playerTransform.position - bearHead.position, out hit, distance, layermask))
        {
            if (hit.transform.tag == "Player")
            {
                return true;
            }
        }

        return false;
    }

    IEnumerator AttackCoroutine()
    {
        Vector3 attackDirection = Vector3.Normalize(playerTransform.position - transform.position);

        agent.speed = 0.0f;

        yield return new WaitForSeconds(0.3f);

        Vector3 finalAttackPosition = transform.position + attackDirection * 8.0f;

        RaycastHit hit;
        if(Physics.Raycast(bearHead.position, attackDirection, out hit, Vector3.Distance(bearHead.position, finalAttackPosition), layermask))
        {
            if (!hit.transform.CompareTag("Player"))
                finalAttackPosition = hit.point;
        }

        agent.SetDestination(finalAttackPosition);
        agent.speed = attackSpeed;
        agent.acceleration = attackSpeed * 2.0f;

        while (agent.remainingDistance != 0)
        {
            if(CheckWallCollision())
            {
                currentState = BearState.stunned;
                agent.acceleration = 10;
                isAttacking = false;
                StopAllCoroutines();
            }
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        agent.acceleration = 10;
        isAttacking = false;

        currentState = BearState.chase;
    }

    bool CheckWallCollision()
    {
        RaycastHit hit;

        if(Physics.Raycast(bearHead.position, bearHead.forward, out hit, wallCheckDistance))
        {
            if (hit.transform.CompareTag("Wall"))
                return true;
        }

        return false;
    }

    IEnumerator WaitAndGoCoroutine()
    {
        yield return new WaitForSeconds(3.0f);
        isWaiting = false;
        currentState = BearState.patrol;
    }

    IEnumerator StunnedCoroutine()
    {
        yield return new WaitForSeconds(2.5f);

        isWaiting = false;

        currentState = BearState.search;
    }

    IEnumerator SearchRoomCoroutine()
    {
        agent.SetDestination(GetRandomSearchArea());

        while(agent.pathPending)
        {
            yield return null;
        }

        while(agent.remainingDistance != 0)
        {
            yield return null;
        }

        yield return new WaitForSeconds(1.0f);

        isSearching = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(isAttacking)
        {
            if(collision.transform.CompareTag("Player"))
            {
                Debug.Log("Player Damaged!");
                //playerTransform.gameObject.GetComponent<HealthScript>().DamagePlayer(250);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Handles.color = Color.black;
        Handles.Label(transform.position + new Vector3(0, 2.5f, 0), debugState);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minChaseDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minAttackDistance);
    }

    Vector3 GetPatrolPointPosition()
    {
        return PatrolAreas[patrolAreaIndex].patrolPoints[patrolPointIndex].position;
    }

    Transform GetPatrolPointTransform()
    {
        return PatrolAreas[patrolAreaIndex].patrolPoints[patrolPointIndex];
    }

    Vector3 GetPatrolAreaPosition()
    {
        return PatrolAreas[patrolAreaIndex].transform.position;
    }

    Vector3 GetRandomSearchArea()
    {
        int currentIndex = searchAreaIndex;

        while(currentIndex == searchAreaIndex)
        {
            currentIndex = Random.Range(0, SearchAreas.Count - 1);
        }
        return SearchAreas[currentIndex].position;
    }

    public void InvestigateSound()
    {
        
    }
}
