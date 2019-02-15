using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OwlScript : MonoBehaviour {

    [SerializeField] float visionRange;
    [SerializeField] float visionAngle;
    [SerializeField] float damage;
    [SerializeField] float distanceRequirement;
    [SerializeField] float movementSpeed;
    [SerializeField] float patrolTime;
    float currentPatrolTimer;
    int currentPathPosition;
    public int currentPatrolPoint;
    public int nextPatrolPoint;
    OwlNavigation navigation;


    bool isMoving;
    bool isTriggered;
    [SerializeField] List<AudioClip> sounds;

    Transform head;
    Transform player;
    HealthScript healthScript;
    AudioSource audioSource;
    Rigidbody rb;

    // Use this for initialization
    void Start ()
    {
        head = transform.Find("Model").Find("Head");
        player = GameObject.Find("Player").transform.Find("Target");
        healthScript = player.parent.GetComponent<HealthScript>();
        navigation = GameObject.Find("OwlNav").GetComponent<OwlNavigation>();
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        currentPatrolTimer = 0;

        isMoving = false;

        //int i = 0;
                //for (; i < navigation.pathToTake.Count; i++)
                //    if (navigation.pathToTake[currentPathPosition] == navigation.mainNodes[i].transform)
                //        break;

                //currentPatrolPoint = i;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isMoving)
        {
            //VisionCheck();
            //int index;// = CheckForMovement();

            //if (index != -1)
            //{
            //    FindPathTo(index);
            //}

            currentPatrolTimer += Time.deltaTime;

            if (currentPatrolTimer >= patrolTime)
            {
                nextPatrolPoint = 0;
                //Go to next point
                if (currentPatrolPoint < navigation.mainNodes.Count - 1)
                    nextPatrolPoint = currentPatrolPoint + 1;

                FindPathTo(nextPatrolPoint);
                currentPatrolTimer = 0;
            }

        }
    }

    void VisionCheck()
    {
        if (Vector3.Angle(player.position - transform.position, transform.transform.forward) < visionAngle)
        {
            if (Vector3.Distance(player.position, transform.position) < visionAngle)
            {
                RaycastHit hit;

                if (Physics.Raycast(transform.position, player.position - transform.position, out hit, visionRange))
                {
                    if (hit.transform.tag == "Player")
                    {
                        Debug.DrawLine(transform.position, player.position, Color.green);
                        healthScript.DamagePlayer(damage * Time.deltaTime);
                        healthScript.m_IsOwlWatching = true;

                        Quaternion targetRotation = Quaternion.LookRotation(player.transform.position - head.position);
                        targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);

                        head.rotation = Quaternion.Slerp(head.rotation, targetRotation, 2.5f * Time.deltaTime);
                    }
                    else
                    {
                        Debug.DrawLine(transform.position, player.position, Color.red);
                        healthScript.m_IsOwlWatching = false;
                    }
                }
            }
        }

    }

    //int CheckForMovement()
    //{
    //    float bestDistance = -1;

    //    int index = -1;

    //    for (int i = 0; i < navigation.mainNodes.Count; i++)
    //    {
    //        if (navigation.mainNodes[i].transform == currentPoint)
    //            continue;

    //        float newDistance = Vector3.Distance(navigation.mainNodes[i].transform.position, player.transform.position);

    //        if ( newDistance <= distanceRequirement && ( bestDistance == -1 || newDistance < bestDistance ))
    //        {
    //            index = i;
    //            bestDistance = newDistance;
    //        }
    //    }

    //    return index;
    //}

    void FindPathTo(int index)
    {
        isMoving = true;
        bool hasPath = navigation.BeginSearch(navigation.mainNodes[currentPatrolPoint].transform, navigation.mainNodes[index]);
        if (!hasPath)
            throw new UnityException("There's no path to the destination");

        currentPathPosition = navigation.pathToTake.Count - 1;
        Debug.Log(currentPathPosition);

        //PlaySound
        int audioClip = Random.Range(0, sounds.Count);
        audioSource.clip = sounds[audioClip];
        audioSource.pitch = Random.Range(0.69f, 0.88f);

        if (audioClip == sounds.Count - 1)
            audioSource.spatialBlend = 0.5f;
        else
            audioSource.spatialBlend = 1;

        audioSource.Play();

        GoTo(navigation.pathToTake[currentPathPosition].position);
    }

    void GoTo(Vector3 destination)
    {
        Vector3 direction = (destination - transform.position).normalized;
        rb.velocity = direction * movementSpeed;

        transform.LookAt(destination);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(isMoving && other.CompareTag("PatrolPoint") && !isTriggered)
        {
            isTriggered = true;
            rb.velocity = Vector3.zero;
            if(currentPathPosition > 0)
            {
                currentPathPosition--;
                Debug.Log(currentPathPosition);

                GoTo(navigation.pathToTake[currentPathPosition].position);
            }
            else
            {
                transform.position = navigation.pathToTake[currentPathPosition].position;
                transform.rotation = navigation.pathToTake[currentPathPosition].rotation;

                Debug.Log("llEGUE PUTOS");

                foreach (GameObject gameObject in navigation.debugWaypoints)
                    Destroy(gameObject);

                navigation.debugWaypoints.Clear();

                isMoving = false;

                if (currentPatrolPoint < navigation.mainNodes.Count - 1)
                    currentPatrolPoint++;
                else
                    currentPatrolPoint = 0;
            }
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PatrolPoint") && isTriggered)
            isTriggered = false;
    }
}
