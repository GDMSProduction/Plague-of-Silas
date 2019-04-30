using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth2 : MonoBehaviour
{
    public Camera cam;
    public GameObject obj;
    public Image crosshair;
    public Slider healthSlider;
    public float maxReach;
    public Text syringeText;
    public float playerHealth;
    public float healAmount;
    public int maxHealth;
    public float decreasePerSecond;
    public Text WinLose;

    int numSyringes;


    void Start()
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = playerHealth;
    }

    void Update ()
    {
        //if (playerHealth <= 0 || WinLose.enabled)
        //{
        //    GetComponent<FirstPersonController>().enabled = false;
        //    return;
        //}

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(obj.transform.position, obj.transform.forward, out hit, maxReach))
            {
                if (hit.transform.CompareTag("Health"))
                {
                    Spin spin = hit.transform.GetComponent<Spin>();
                    spin.DestroySelf();
                    numSyringes++;
                }
            }
        }


        if (playerHealth != 0)
        {
            float damage = decreasePerSecond * Time.deltaTime;
            DamagePlayer(damage);
            
        }

        if(Input.GetKeyDown(KeyCode.F))
        {
            if (numSyringes > 0)
            {
                numSyringes--;

                HealPlayer(healAmount);
            }


        }

        syringeText.text = "x " + numSyringes;
        healthSlider.value = playerHealth;

    }

    public void HealPlayer(float amount)
    {
        playerHealth += amount;
        playerHealth = Mathf.Clamp(playerHealth, 0, maxHealth);
    }

    public void DamagePlayer(float amount)
    {
        playerHealth -= amount;
        playerHealth = Mathf.Clamp(playerHealth, 0, maxHealth);
    }
}
