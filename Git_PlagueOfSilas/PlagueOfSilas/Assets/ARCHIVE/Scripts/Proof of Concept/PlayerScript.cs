using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ENEMY_TYPE { BEAR, DOG };

public class PlayerScript : MonoBehaviour {

    [SerializeField] Camera cam;
    [SerializeField] Transform rayCastOrigin;
    [SerializeField] float maxReach;
    [SerializeField] float health;
    [SerializeField] float healAmount;
    [SerializeField] int maxHealth;
    [SerializeField] float decreasePerSecond;
    GameObject[] enemies;
    EnemyScript[] enemyScripts;
    [SerializeField] GameObject[] items;
    private GameObject equipped = null;

    private int numSyringes;

    public float Health { get { return Health; } }
    public float MaxHealth { get { return maxHealth; } }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (items[1] && items[1].activeSelf)
                items[1].SetActive(false);

            if (items[0])
            {
                if (equipped == items[0])
                {
                    items[0].SetActive(false);
                    equipped = null;
                }

                else
                {
                    items[0].SetActive(true);
                    equipped = items[0];
                }
            }

            else equipped = null;
        }

        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (items[0] && items[0].activeSelf)
                items[0].SetActive(false);

            if (items[1])
            {
                if (equipped == items[1])
                {
                    items[1].SetActive(false);
                    equipped = null;
                }

                else
                {
                    items[1].SetActive(true);
                    equipped = items[1];
                }
            }

            else equipped = null;
        }


        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(rayCastOrigin.position, rayCastOrigin.forward, out hit, maxReach))
            {
                if (hit.transform.CompareTag("Health"))
                {
                    Spin spin = hit.transform.GetComponent<Spin>();
                    spin.DestroySelf();
                    numSyringes++;
                }

                if(hit.transform.CompareTag("Item"))
                {
                    PickUp(hit.transform.gameObject);
                }
            }
        }

        if(Input.GetMouseButtonDown(1))
        {
            Throw();
        }

        if (health != 0)
        {
            float damage = decreasePerSecond * Time.deltaTime;
            Damage(damage);

        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (numSyringes > 0)
            {
                numSyringes--;

                Heal(healAmount);
            }
        }
    }

    public void Heal(float amount)
    {
        health += amount;
        health = Mathf.Clamp(health, 0, maxHealth);
    }

    public void Damage(float amount)
    {
        health -= amount;
        health = Mathf.Clamp(health, 0, maxHealth);
    }

    public bool PickUp(GameObject item)
    {
        if (!equipped)
        {
            if (items[0] == null)
            {
                items[0] = item;
                equipped = items[0];
            }
            else if (items[1] == null)
            {
                items[1] = item;
                equipped = items[1];
            }
            else return false;
            Rigidbody rb = item.GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeAll;
            item.transform.parent = cam.transform;
            item.transform.localPosition = new Vector3(-0.3f, -0.7f, 1.3f);
            item.transform.rotation = Quaternion.identity;
            item.transform.localRotation = Quaternion.identity;
            return true;
        }
        return false;
    }

    public bool Throw()
    {
        if(equipped)
        {
            Item item = equipped.GetComponent<Item>();
            Rigidbody rb = equipped.GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.None;
            equipped.transform.parent = null;
            rb.AddForce(cam.transform.forward * item.horizontalStrength + cam.transform.up * item.verticalStrength);
            item.thrown = true;
            if (equipped == items[0])
                items[0] = null;
            else if (equipped == items[1])
                items[1] = null;
            equipped = null;
            for (int i = 0; i < enemyScripts.Length; i++)
            {
                if(enemyScripts[i].state == EnemyScript.ENEMY_STATES.CHASING && enemyScripts[i].enemyType == item.enemyType)
                {
                    enemyScripts[i].FollowDistraction(item);
                }
            }
            return true;
        }
        return false;
    }

    public void SetEnemyScripts(EnemyScript[] scripts)
    {
        enemyScripts = new EnemyScript[scripts.Length];
        for (int i = 0; i < enemyScripts.Length; i++)
        {
            enemyScripts[i] = scripts[i];
        }
    }
}
