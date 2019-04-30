using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {
    [SerializeField] PlayerHealth2 player;
    [SerializeField] Transform[] spawners;
    [SerializeField] GameObject[] enemies;
    int spawnedLevel = 0;
    List<GameObject> spawnedEnemies;
    List<int> taken;


	// Use this for initialization
	void Start ()
    {
        spawnedEnemies = new List<GameObject>();
        taken = new List<int>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (player.playerHealth <= player.maxHealth && player.playerHealth > player.maxHealth * 2.0f / 3.0f && spawnedLevel != 1)
        {
            if (spawnedLevel == 2)
                DeleteLevel();

            SpawnLevel1();
            spawnedLevel = 1;
        }

        if (player.playerHealth <= player.maxHealth * 2.0f / 3.0f && player.playerHealth > player.maxHealth / 3.0f && spawnedLevel != 2)
        {
            if (spawnedLevel == 3)
                DeleteLevel();

            SpawnLevel2();
            spawnedLevel = 2;
        }

        if (player.playerHealth <= player.maxHealth / 3.0f && player.playerHealth > 0.0f && spawnedLevel != 3)
        {
            SpawnLevel3();
            spawnedLevel = 3;
        }
    }

    void SpawnLevel1()
    {
        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            if(spawnedEnemies[i].CompareTag("First"))
            {
                return;
            }
        }
        while (true)
        {
        int spawner = Mathf.RoundToInt(Random.Range(0, spawners.Length - 1));
            Vector3 screenPosition = player.cam.WorldToViewportPoint(spawners[spawner].position);
            if (!OnScreen(screenPosition) && !IsTaken(spawner))
            {
                GameObject temp = Instantiate(enemies[0], spawners[spawner].position, Quaternion.identity);
                temp.tag = "First";
                spawnedEnemies.Add(temp);
                taken.Add(spawner);
                break;
            }
        }
    }

    void SpawnLevel2()
    {
        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            if (spawnedEnemies[i].CompareTag("Second"))
            {
                return;
            }
        }
        for (int i = 0; i <= 1; i++)
        {
            while (true)
            {
                int spawner = Mathf.RoundToInt(Random.Range(0, spawners.Length - 1));
                Vector3 screenPosition = player.cam.WorldToViewportPoint(spawners[spawner].position);
                if (!OnScreen(screenPosition) && !IsTaken(spawner))
                {
                    GameObject temp = Instantiate(enemies[i], spawners[spawner].position, Quaternion.identity);
                    temp.tag = "Second";
                    spawnedEnemies.Add(temp);
                    taken.Add(spawner);
                    break;
                }
            }
        }
    }

    void SpawnLevel3()
    {
        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            if (spawnedEnemies[i].CompareTag("Third"))
            {
                return;
            }
        }
        for (int i = 1; i <= 2; i++)
        {
            while (true)
            {
                int spawner = Mathf.RoundToInt(Random.Range(0, spawners.Length - 1));
                Vector3 screenPosition = player.cam.WorldToViewportPoint(spawners[spawner].position);
                if (!OnScreen(screenPosition) && !IsTaken(spawner))
                {
                    GameObject temp = Instantiate(enemies[i], spawners[spawner].position, Quaternion.identity);
                    temp.tag = "Third";
                    spawnedEnemies.Add(temp);
                    taken.Add(spawner);
                    break;
                }
            }
        }
    }


    void DeleteLevel()
    {
        StartCoroutine(Delete(spawnedEnemies[spawnedEnemies.Count - 2]));
        StartCoroutine(Delete(spawnedEnemies[spawnedEnemies.Count - 1]));
        taken.RemoveAt(taken.Count - 2);
        taken.RemoveAt(taken.Count - 1);
    }

    bool OnScreen(Vector3 screenPosition)
    {
        if (screenPosition.x >= 0 && screenPosition.x <= 1 && screenPosition.y >= 0 && screenPosition.y <= 1 && screenPosition.z >= 0 && screenPosition.z <= 1)
            return true;

        return false;
    }

    bool IsTaken(int num)
    {
        for (int i = 0; i < taken.Count; i++)
        {
            if (taken[i] == num)
                return true;
        }

        return false;
    }

    IEnumerator Delete(GameObject gameObject)
    {
        Vector3 screenPosition = player.cam.WorldToViewportPoint(gameObject.transform.position);
        while (OnScreen(screenPosition))
        {
            yield return null;
        }
        spawnedEnemies.Remove(gameObject);
        Destroy(gameObject);
    }
}
