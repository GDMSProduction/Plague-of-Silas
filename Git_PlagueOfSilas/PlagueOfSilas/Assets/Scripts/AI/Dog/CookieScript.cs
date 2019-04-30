using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CookieScript : MonoBehaviour {

    // Use this for initialization
    public GameObject m_NextCookie;
    public float lifeSpan;

    Transform playerTransform;
    float lifeTimer;

    void Start ()
    {
        lifeTimer = lifeSpan;

        m_NextCookie = null;

        playerTransform = GameObject.Find("Player").transform;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Vector3.Distance(playerTransform.position, transform.position) < 2)
            lifeTimer = lifeSpan;

        if (lifeTimer > 0)
            lifeTimer -= Time.deltaTime;
        else
            Destroy(gameObject);
	}

    public void ResetTimer()
    {
        lifeTimer = lifeSpan;
    }
}
