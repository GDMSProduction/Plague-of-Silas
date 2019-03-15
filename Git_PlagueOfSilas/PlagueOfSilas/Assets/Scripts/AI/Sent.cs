using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sent : MonoBehaviour
{

    public static List<Sent> Trail;
    Sent Next;
    [SerializeField] float LifeTime;

    private void Awake()
    {
        if (Trail == null)
            Trail = new List<Sent>();

        Trail.Insert(0, this);
        Trail[1].SetNext(this);
    }

    private void OnDestroy()
    {
        Trail.Remove(this);
    }

    void Update()
    {
        LifeTime -= Time.deltaTime;

        if (LifeTime <= 0)
            Destroy(gameObject);
    }

    public void SetNext(Sent sent) { Next = sent; }
    public Sent GetNext() { return Next; }
}