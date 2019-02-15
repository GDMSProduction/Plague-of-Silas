using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HealthScript : MonoBehaviour
{

    [SerializeField] float m_Health;
    [SerializeField] float m_MaxHealth;
    [SerializeField] float m_HealAmount;
    [SerializeField] float m_DecreasePerSecond;
    [SerializeField] Slider healthSlider;

    [SerializeField] Image m_EyeIcon;
    [SerializeField] Image m_EyeBackground;
    [SerializeField] Image m_NoseIcon;
    [SerializeField] Image m_NoseBackground;
    [SerializeField] Image m_OwlIcon;
    [SerializeField] Image m_OwlBackground;

    [SerializeField] Image m_GameOverScreen;
    [SerializeField] Text m_GameOverText;
    [SerializeField] Text m_RestartText;

    [SerializeField] Text m_DetectionMeterNum;
    public float Health { get { return m_Health; } }
    public float MaxHealth { get { return m_MaxHealth; } }

    public float detectionMeter;
    public bool beingSniffed;

    private Animator m_Animator;
    private bool m_IsDead;
    public bool m_IsSeen;
    public bool m_IsOwlWatching;

    private void Start()
    {
        m_Animator = GetComponent<Animator>();
        healthSlider.maxValue = m_MaxHealth;
        healthSlider.value = m_Health;
        m_IsDead = false;
    }

    private void Update()
    {
        if(m_IsDead)
        {
            float alpha = Mathf.Lerp(m_GameOverScreen.color.a, 1, 0.05f);
            m_GameOverScreen.color = new Color(0, 0, 0, alpha);
            if(alpha > 0.75)
            {
                m_GameOverText.enabled = true;
                m_RestartText.enabled = true;
                if (Input.GetKeyDown(KeyCode.R))
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            return;
        }

        if(m_IsSeen)
        {
            DamagePlayer(m_DecreasePerSecond * Time.deltaTime);
        }

        if (detectionMeter > 0)
        {
            m_EyeIcon.enabled = m_EyeBackground.enabled = true;

            if (detectionMeter == 100) 
            {
                m_EyeBackground.color = new Vector4(0.788f, 0.039f, 0, 1);
            }
            else if (detectionMeter >= 65)
            {
                m_EyeBackground.color = new Vector4(0.909f, 0.478f, 0.050f, 1);
            }
            else if (detectionMeter > 30)
            {
                m_EyeBackground.color = new Vector4(0.717f, 0.727f,  0.113f, 1);
            }
            else
            {
                m_EyeBackground.color = new Vector4(0.494f, 0.501f, 0.466f, 1);
            }

        }
        else
        {
            m_EyeIcon.enabled = m_EyeBackground.enabled = false;
        }

        m_NoseIcon.enabled = m_NoseBackground.enabled = beingSniffed;

        m_OwlIcon.enabled = m_OwlBackground.enabled = m_IsOwlWatching;

        m_DetectionMeterNum.text = detectionMeter.ToString("0.00");

        
    }

    private void Die()
    {
        m_Animator.Play("Die");
        m_IsDead = true;
    }

    public void DamagePlayer(float damage)
    {
        float result = m_Health - damage;
        m_Health = Mathf.Clamp(result, 0, m_MaxHealth);
        healthSlider.value = m_Health;

        if (m_Health == 0 && !m_IsDead)
            Die();
    }
}
