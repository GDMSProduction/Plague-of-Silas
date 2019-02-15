using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor;
using UnityEngine.AI;
//using System.IO;

//namespace Assets.Editor
//{
//    [InitializeOnLoad]
//    public class AutoLayoutChanger
//    {
//        static AutoLayoutChanger()
//        {
//            EditorApplication.playModeStateChanged += HandleOnPlayModeChanged;
//        }

//        static void HandleOnPlayModeChanged(PlayModeStateChange state)
//        {
//            // This method is run whenever the playmode state is changed.
//            if (state == PlayModeStateChange.EnteredPlayMode)
//            {
//                string path = Path.Combine(Directory.GetCurrentDirectory(), "Assets/Editor/Layouts/Your Layout.wlt");
//                EditorUtility.LoadWindowLayout("Play Layout");
//                // do stuff when the editor is played.
//            }
//            else if (state == PlayModeStateChange.ExitingPlayMode)
//            {
//                EditorUtility.LoadWindowLayout("Work Layout");
//                // other than playmode
//            }
//        }
//    }
//}

public class DogScript : MonoBehaviour
{

    // Use this for initialization

    #region Public Variables

    public enum DogStatus { patroling = 0, spotted, suspicious, chasing, sniffing, wandering, distracted };

    public DogStatus m_CurrentState;

    #endregion

    #region Private  Variables

    [SerializeField] AudioSource m_AttackSound;

    [SerializeField] AudioSource m_AlertSound;

    NavMeshAgent m_Agent;

    List<Vector3> m_PatrolPoints;

    Transform m_DogHead;

   [SerializeField] Transform m_Interest;

    Transform m_PlayerHead;

    int m_DestinationIndex;

    [SerializeField] bool m_CanSmell;

    [SerializeField] bool m_CanSee;

    [SerializeField] bool m_IsPlayerInSight;

    [SerializeField] bool m_IsWaiting;

    [SerializeField] bool m_IsLastSeenRunning;

    bool m_IsAttacking;

    bool m_IsPlayerInAttackRange;


    [SerializeField] float m_fVisionAngle;

    [SerializeField] float m_fVisionRange;


    [SerializeField] float m_fWalkSpeed;

    [SerializeField] float m_fRunSpeed;

    [SerializeField] float m_fSniffingSpeed;


    [SerializeField] float m_fDetectionMeter;

    [SerializeField] float m_fDetectionRate;

    [SerializeField] float m_fDetectionDrain;

    float m_fTimeoutTimer;

    GameObject m_LastCookie;

    IEnumerator m_WaitEnumerator;
    IEnumerator m_LastSeenEnumerator;
    #endregion


    void Start()
    {
        m_CurrentState = DogStatus.patroling;

        m_Agent = GetComponent<NavMeshAgent>();
        m_Agent.speed = m_fWalkSpeed;

        m_PatrolPoints = new List<Vector3>();

        m_PlayerHead = GameObject.Find("Player").transform.Find("Target");

       // m_PlayerTransform = m_PlayerHead.parent;

        List<Transform> patrolTransforms = new List<Transform>();

        foreach (Transform child in transform)
        {
            if (child.tag == "PatrolPoint")
            {
                m_PatrolPoints.Add(child.position);

                patrolTransforms.Add(child);
            }
        }

        foreach (Transform t in patrolTransforms)
        {
            t.parent = null;
        }

        m_Interest = transform.Find("Interest");
        m_Interest.parent = null;

        m_DogHead = transform.Find("Head");

        m_DestinationIndex = 0;

        m_Agent.SetDestination(m_PatrolPoints[m_DestinationIndex]);

        m_IsPlayerInSight = false;
        m_IsAttacking = false;
        m_IsPlayerInAttackRange = false;

        m_fTimeoutTimer = 0;
    }

    void Update()
    {
        if(m_Agent.speed == m_fWalkSpeed)
        {
            m_Agent.acceleration = 4;
            m_Agent.stoppingDistance = 0;
        }
        else
        {
            m_Agent.acceleration = 60;
            //m_Agent.stoppingDistance = 3.5f;
        }

        m_PlayerHead.parent.GetComponent<HealthScript>().m_IsSeen = m_IsPlayerInSight;
        m_IsPlayerInAttackRange = Vector3.Distance(m_PlayerHead.parent.position, transform.position) < 1.5 ? true : false;

        switch (m_CurrentState)
        {
            case DogStatus.patroling:
                {
                    VisionCheck();

                        if (m_Agent.remainingDistance <= 1)
                        {

                            if (m_DestinationIndex < m_PatrolPoints.Count - 1)
                                m_DestinationIndex++;
                            else
                                m_DestinationIndex = 0;

                            m_Agent.SetDestination(m_PatrolPoints[m_DestinationIndex]);
                        }

                    break;
                }

            case DogStatus.spotted:
                {
                    SmoothLookAt(m_Interest, 3);

                    if (!VisionCheck())
                    {
                        if (!m_IsWaiting)
                        {
                            m_WaitEnumerator = WaitCoroutine(2);
                            StartCoroutine(m_WaitEnumerator);
                        }
                    }
                    else if (m_IsWaiting)
                    {
                        StopCoroutine(m_WaitEnumerator);

                        m_CurrentState = DogStatus.patroling;
                        m_Agent.isStopped = false;
                        m_Agent.updateRotation = true;

                        m_IsWaiting = false;
                    }

                    break;
                }

            case DogStatus.suspicious:
                {
                    if (!VisionCheck())
                    {
                        if (!m_IsWaiting && m_Agent.remainingDistance == 0)
                        {
                            m_WaitEnumerator = WaitCoroutine(3.5f);
                            StartCoroutine(m_WaitEnumerator);
                        }

                        break;
                    }
                    else if (m_IsWaiting)
                    {
                        StopCoroutine(m_WaitEnumerator);

                        m_CurrentState = DogStatus.patroling;
                        m_Agent.isStopped = false;
                        m_Agent.updateRotation = true;

                    }
                        m_Agent.SetDestination(m_Interest.position);


                    break;
                }

            case DogStatus.chasing:
                {
                    if(VisionCheck())
                    {
                        if(m_IsLastSeenRunning)
                        {
                            Debug.Log("LastSeenCoroutine Stopped");
                            StopCoroutine(m_LastSeenEnumerator);
                            m_IsLastSeenRunning = false;
                            m_Agent.isStopped = false;

                        }

                        if (m_IsPlayerInAttackRange)
                        {
                            transform.LookAt(m_Interest);
                            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

                            if (!m_IsAttacking)
                                StartCoroutine(AttackCoroutine());
                        }
                        else
                            m_Agent.SetDestination(m_Interest.position);

                      
                    }
                    else
                    {
                        if(m_Agent.remainingDistance == 0 && !m_IsLastSeenRunning)
                        {
                            Debug.Log("Coroutine started");
                            m_LastSeenEnumerator = LastSeenCoroutine();
                            StartCoroutine(m_LastSeenEnumerator);
                        }
                    }
                    break;
                }

            case DogStatus.sniffing:
                {
                    if (VisionCheck())
                    {
                        m_PlayerHead.parent.GetComponent<HealthScript>().beingSniffed = false;
                        StopCoroutine(m_WaitEnumerator);
                    }
                    else if (m_Agent.pathStatus == NavMeshPathStatus.PathInvalid)
                    {
                        Debug.Log("Error");
                    }

                    break;
                }

        }
    }

    bool VisionCheck()
    {
        m_PlayerHead.parent.GetComponent<HealthScript>().detectionMeter = m_fDetectionMeter;

        if (m_CurrentState == DogStatus.chasing)
        {
            if (Vector3.Distance(m_PlayerHead.parent.position, transform.position) < 1)
                return true;
        }

        if (Vector3.Angle(m_PlayerHead.position - m_DogHead.position, m_DogHead.transform.forward) < m_fVisionAngle)
        {
            if (Vector3.Distance(m_PlayerHead.position, transform.position) < m_fVisionRange)
            {
                RaycastHit hit;

                if (Physics.Raycast(m_DogHead.position, m_PlayerHead.position - m_DogHead.position, out hit, m_fVisionRange))
                {
                    if (hit.transform.tag == "Player")
                    {
                        Debug.DrawLine(m_DogHead.position, m_PlayerHead.position, Color.green);

                        m_fDetectionMeter = Mathf.Clamp(m_fDetectionMeter + m_fDetectionRate / hit.distance, 0, 100);

                        if (!m_IsPlayerInSight)
                        {
                            m_IsPlayerInSight = true;
                        }

                        //Move interest transform
                        if (!m_Interest.gameObject.activeSelf)
                            m_Interest.gameObject.SetActive(true);

                        m_Interest.position = m_PlayerHead.parent.position;

                        if (m_CurrentState != DogStatus.chasing)
                        {
                            if (m_fDetectionMeter == 100) //Chase
                            {
                                m_CurrentState = DogStatus.chasing;
                                m_Agent.speed = m_fRunSpeed;
                                m_Agent.isStopped = false;
                            }
                            else if (m_fDetectionMeter >= 65 && m_fDetectionMeter < 100 && m_CurrentState != DogStatus.suspicious)    //Suspicious
                            {
                                m_CurrentState = DogStatus.suspicious;
                                m_Agent.isStopped = false;
                            }
                            else if (m_fDetectionMeter > 30 && m_fDetectionMeter < 60 && m_CurrentState != DogStatus.spotted) //Spotted
                            {
                                m_CurrentState = DogStatus.spotted;
                                m_Agent.isStopped = true;
                            }
                            else if (m_fDetectionMeter == 0 && m_CurrentState != DogStatus.patroling)
                            {
                                m_CurrentState = DogStatus.patroling;
                                m_Agent.isStopped = false;
                            }
                        }

                        if (m_IsWaiting)
                            StopCoroutine(m_WaitEnumerator);

                        return true;
                    }
                }

            }
        }

        // You can't see the player 
        Debug.DrawLine(m_DogHead.position, m_PlayerHead.position, Color.red);

        if (m_fDetectionMeter > 0 && m_CurrentState == DogStatus.patroling)
            m_fDetectionMeter = Mathf.Clamp(m_fDetectionMeter - m_fDetectionDrain, 0, 100);

        if (m_IsPlayerInSight)
        {
            m_IsPlayerInSight = false;
        }


        return false;
    }

    //IEnumerator WanderingCoroutine()
    //{
    //    m_CurrentState = DogStatus.wandering;

    //    yield return new WaitForSeconds(2f);

    //    m_DestinationIndex = 0;
    //    Vector3 newTarget = m_PatrolPoints[m_DestinationIndex];

    //    for (; m_DestinationIndex < m_PatrolPoints.Count - 1; m_DestinationIndex++)
    //    {
    //        if (Vector3.Distance(transform.position, m_PatrolPoints[m_DestinationIndex]) < Vector3.Distance(transform.position, newTarget))
    //        {
    //            newTarget = m_PatrolPoints[m_DestinationIndex];
    //        }
    //    }

    //    m_Agent.SetDestination(newTarget);

    //    m_fTimeoutTimer = 0;
    //    m_CurrentState = DogStatus.patroling;
    //}

    void SmoothLookAt(Transform target, float speed)
    {
        Quaternion targetRotation = Quaternion.LookRotation(target.transform.position - transform.position);
        targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed * Time.deltaTime);
    }

    IEnumerator LastSeenCoroutine()
    {
        Debug.Log("LastSeenCoroutine Started");
        m_IsLastSeenRunning = true;
        float timer = 0;

        while (timer <= .5f)
        {
            timer += Time.deltaTime;
            m_Agent.SetDestination(m_PlayerHead.transform.position);

            yield return null;
        }

        m_Agent.isStopped = true;
        yield return new WaitForSeconds(2f);

        m_fDetectionMeter = 0;
        m_CurrentState = DogStatus.patroling;
        m_Agent.speed = m_fWalkSpeed;
        m_IsLastSeenRunning = false;
        m_Agent.isStopped = false;

        Debug.Log("LastSeenCoroutine Ended");

    }

    IEnumerator AttackCoroutine()
    {
        m_IsAttacking = true;
        m_Agent.isStopped = true;
        m_Agent.acceleration = 200;

        yield return new WaitForSeconds(.2f);

        if (m_IsPlayerInAttackRange && m_IsPlayerInSight)
        {
            m_AttackSound.Play();
            m_PlayerHead.parent.GetComponent<HealthScript>().DamagePlayer(25);
        }
        else
        {
            m_IsAttacking = false;
            m_Agent.isStopped = false;
            m_Agent.acceleration = 2;

            yield break;
        }

        yield return new WaitForSeconds(1.5f);
        m_IsAttacking = false;
        m_Agent.isStopped = false;
        m_Agent.acceleration = 2;

    }

    IEnumerator WaitCoroutine(float waitTime)
    {
        m_IsWaiting = true;

        float timer = 0;
        m_Agent.isStopped = true;
        m_Agent.updateRotation = false;

        while (timer < waitTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        m_CurrentState = DogStatus.patroling;
        m_Agent.isStopped = false;
        m_Agent.updateRotation = true;

        m_IsWaiting = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (m_CanSmell)
        {
            if (other.tag == "Cookie" && !m_IsPlayerInSight && m_CurrentState == DogStatus.patroling)
            {
                if (other.gameObject.GetComponent<CookieScript>().m_NextCookie != null)
                {
                    m_fTimeoutTimer = 0;

                    Vector3 newDestination = other.gameObject.GetComponent<CookieScript>().m_NextCookie.transform.position;

                    m_Agent.SetDestination(newDestination);

                    m_PlayerHead.parent.GetComponent<HealthScript>().beingSniffed = true;
                    m_CurrentState = DogStatus.sniffing;

                    m_Agent.speed = m_fSniffingSpeed;
                }
                else
                {
                    m_WaitEnumerator = WaitCoroutine(3);
                    StartCoroutine(m_WaitEnumerator);
                    m_Agent.speed = m_fWalkSpeed;
                    m_Agent.stoppingDistance = 0;
                    m_PlayerHead.parent.GetComponent<HealthScript>().beingSniffed = false;
                }
            }
        }

    }

    public void AlertPlayer()
    {
        m_CurrentState = DogStatus.suspicious;
        m_Agent.speed = m_fRunSpeed;
        m_Agent.isStopped = false;
        m_fDetectionMeter = 65;

        m_Interest.position = m_PlayerHead.parent.position;
        m_Agent.SetDestination(m_Interest.position);
    }
}
