using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Sound : MonoBehaviour
{

    [SerializeField] AudioClip[] clips;
    float SoundTime;

    // Start is called before the first frame update
    void Start()
    {
        int i = Random.Range(0, clips.Length);

        SoundTime = clips[i].length;

        GetComponent<AudioSource>().PlayOneShot(clips[i]);
    }

    // Update is called once per frame
    void Update()
    {
        SoundTime -= Time.deltaTime;

        if (SoundTime <= 0)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<BaseGroundAi>())
            other.GetComponent<BaseGroundAi>().Alert(transform);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, GetComponent<SphereCollider>().radius);
    }
}
