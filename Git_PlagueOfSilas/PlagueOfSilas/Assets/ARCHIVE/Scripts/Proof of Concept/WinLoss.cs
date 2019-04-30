using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinLoss : MonoBehaviour {

    [SerializeField] float maxTime;
    float currentTime;
    [SerializeField] Text timerText;
    [SerializeField] Text gameOverText;
    [SerializeField] PlayerHealth2 player;
	// Use this for initialization
	void Start ()
    {
        currentTime = maxTime;
	}
	
	// Update is called once per frame
	void Update ()
    {
        timerText.text = Mathf.Clamp(Mathf.Ceil(currentTime), 0, maxTime).ToString();
        if(player.playerHealth <= 0)
        {
            gameOverText.enabled = true;
            gameOverText.text = "You Lose!";
        }

        else if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
        }

        else
        {
            gameOverText.enabled = true;
            gameOverText.text = "You Win!";
        }
	}
}
