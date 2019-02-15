using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyScript : MonoBehaviour {

    public enum ENEMY_STATES { IDLE, DISTRACTED, CHASING, MAD};
    NavMeshAgent agent;
    GameObject player;
    public ENEMY_STATES state;
    public ENEMY_TYPE enemyType;
    [SerializeField] float maxDistance;
    [SerializeField] float normalSpeed;
    [SerializeField] float madSpeed;
    

	// Use this for initialization
	void Start ()
    {
        state = ENEMY_STATES.IDLE;
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (state == ENEMY_STATES.IDLE)
        {

            RaycastHit hit;
            if (Physics.Raycast(this.transform.position, player.transform.position - this.transform.position, out hit, maxDistance))
            {
                if (hit.transform.CompareTag("Player"))
                {
                    state = ENEMY_STATES.CHASING;
                    agent.speed = normalSpeed;
                    agent.acceleration = normalSpeed + 5.0f;
                }
            }
        }

        if (state == ENEMY_STATES.CHASING)
        {
            //if (Vector3.Distance(player.transform.position, agent.transform.position) > 2.0f)
                agent.SetDestination(player.transform.position);
        }

        if(state == ENEMY_STATES.MAD)
        {
          //  if (Vector3.Distance(player.transform.position, agent.transform.position) > 2.0f)
                agent.SetDestination(player.transform.position);
        }
	}

    public void AlertPosition(Vector3 pos)
    {
        agent.SetDestination(pos);
    }

    public void FollowDistraction(Item item)
    {
        state = ENEMY_STATES.DISTRACTED;
        StartCoroutine(Follow(item));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Item"))
        {
            if (collision.gameObject.GetComponent<Item>().thrown)
            {
                state = ENEMY_STATES.MAD;
                agent.speed = madSpeed;
                agent.acceleration = madSpeed + 5.0f;
                StopAllCoroutines();
            }
        }
    }

    IEnumerator Follow(Item item)
    {
        yield return new WaitForSeconds(0.5f);
        while(item.thrown)
        {
            agent.SetDestination(item.transform.position);
            yield return null;
        }
        float d = 1.5f + (Vector3.Distance(transform.position, item.transform.position) /6.0f);
        yield return new WaitForSeconds(d);
        state = ENEMY_STATES.IDLE;
    }
}
