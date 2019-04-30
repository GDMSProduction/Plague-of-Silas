using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour {

    [SerializeField] float vertical;
    [SerializeField] float horizontal;
    public ENEMY_TYPE enemyType;
    public bool thrown { get; set; }

    public float verticalStrength { get { return vertical; } }
    public float horizontalStrength { get { return horizontal; } }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            thrown = false;
        }
    }
}
