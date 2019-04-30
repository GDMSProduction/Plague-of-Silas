using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempEnemySpawn : MonoBehaviour {

    [SerializeField] PlayerScript player;
    [SerializeField] GameObject[] enemyTypes;
    [SerializeField] GameObject item;
    EnemyScript[] enemies;

    // Use this for initialization
    private void Awake()
    {
        GameObject enemy = Instantiate(enemyTypes[Random.Range(0, enemyTypes.Length)], new Vector3(0, 0.5f, 0), Quaternion.identity);
        enemies = new EnemyScript[1];
        enemies[0] = enemy.GetComponent<EnemyScript>();
        player.SetEnemyScripts(enemies);
    }
}
