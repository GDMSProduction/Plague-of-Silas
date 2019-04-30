using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolArea : MonoBehaviour
{

    // Use this for initialization
    public List<Transform> patrolPoints;

    private void Start()
    {
        if(patrolPoints.Count == 0)
        {
            Transform[] transforms = GetComponentsInChildren<Transform>();

            if (transforms.Length > 1)
            {
                for (int i = 1; i < transforms.Length; i++)
                {
                    patrolPoints.Add(transforms[i]);
                }
            }

            else
                Debug.Log("You forgot to add patrol points in " + gameObject.name + "!");
        }
    }
}
