using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeScript : MonoBehaviour
{
    public bool isMainNode;
    public Transform[] neighbors;

    void Update()
    {
        foreach (Transform t in neighbors)
        {
            Debug.DrawLine(transform.position, t.position, Color.magenta);
        }
    }
}


