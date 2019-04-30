using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MooseScript : MonoBehaviour {

    [SerializeField] GameObject m_Player;
    [SerializeField] float m_MinDistance;
    [SerializeField] Image m_WarningImage;
    [SerializeField] DogScript m_Dog;
    Light m_Lamp;

	// Use this for initialization
    void Start ()
    {
        m_Lamp = m_Player.GetComponentInChildren<Light>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Vector3.Distance(m_Player.transform.position, transform.position) < m_MinDistance)
        {
            if (Vector3.Dot(transform.forward, Vector3.Normalize(m_Player.transform.position - transform.position)) > 0.5f)
            {
                if (IsHiddenFromPlayer())
                    SignalEnemies();
            }
        }
    }

    private bool IsHiddenFromPlayer()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, m_Player.transform.position - transform.position, out hit, m_MinDistance))
        {
            if (hit.transform.CompareTag("Player"))
            {
                return !m_Lamp.enabled;
            }
        }

        return false;
    }

    void SignalEnemies()
    {
        Debug.Log("ENEMIES ALERTED");

        m_Dog.AlertPlayer();
        //m_WarningImage.enabled = true;
        //m_Dog.AlertPosition(transform.position);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, m_MinDistance);
    }
}
