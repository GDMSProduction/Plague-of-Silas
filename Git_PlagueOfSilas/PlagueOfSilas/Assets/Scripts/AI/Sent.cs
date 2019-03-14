using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sent : MonoBehaviour
{

    public static List<Transform> Trail;
    [SerializeField] float LifeTime;

    private void Awake()
    {
        if (Trail == null)
            Trail = new List<Transform>();

        Trail.Insert(0, transform);
    }

    private void OnDestroy()
    {
        Trail.Remove(transform);  
    }

    void Update()
    {
        LifeTime -= Time.deltaTime;

        if (LifeTime <= 0)
            Destroy(gameObject);
    }
}