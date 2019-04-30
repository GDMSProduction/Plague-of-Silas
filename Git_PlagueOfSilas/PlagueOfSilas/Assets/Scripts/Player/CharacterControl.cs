using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControl : MonoBehaviour
{

    #region Cookies stuff

    [SerializeField] float m_CookieSpawnFrecuency;

    [SerializeField] float m_CookieSpawnTimer;

    [SerializeField] GameObject m_CookieRef;

    GameObject m_PreviousCookie;

    bool m_SpawnCookies;

    #endregion

    [SerializeField] private float m_WalkSpeed = 2.0f;
    [SerializeField] private float m_RunSpeed = 6.0f;
    [SerializeField] private float m_CrouchSpeed = 1.0f;
    [SerializeField] private float m_JumpForce = 250;
    [SerializeField] private float m_Gravity;
    [SerializeField] private float m_CrouchAmount = 0.5f;
    [SerializeField] private float m_LeanXOffset = 0.3f;
    [SerializeField] private float m_LeanXSpeed = 1.0f;
    [SerializeField] private float m_LeanRotationSpeed = 60.0f;
    [SerializeField] private float m_MaxLeanAngle = 15.0f;
    [SerializeField] private GameObject m_ModerateNoiseRadius;
    [SerializeField] private GameObject m_LoudNoiseRadius;
    [SerializeField] [Range(1, 5)] private float m_MouseSensitivity = 3.0f;
    [SerializeField] private AudioClip[] m_FootSteps;
    [SerializeField] LayerMask RaycastMask;
    [SerializeField] float RayLength;

    private Animator m_Animator;
    private Rigidbody m_RigidBody;
    private AudioSource m_AudioSource;
    private CapsuleCollider m_PlayerCollider;
    private Camera m_Camera;
    private Transform m_CameraPivot;
    private Vector3 m_CamDefaultPosition;
    private Light m_Lamp;
    private bool m_IsCrouching;
    private bool m_IsGrounded;
    private bool m_IsDead;
    private float m_DefaultPlayerHeight;
    private float m_ForwardMovement;
    private float m_SideMovement;
    private float m_RotationY;
    private float m_RotationX;
    private float m_CurrentAngle;
    private float m_CurrentOffset;
    private float m_DebugNewNoiseRadius;
    private bool m_DebugNewNoise;

    private IEnumerator crouch;
    private IEnumerator stand;


    // Use this for initialization
    private void Start()
    {
        // Cookie 
        m_CookieSpawnTimer = 0;
        m_SpawnCookies = true;
        m_PreviousCookie = m_CookieRef;

        // Player Components
        m_Animator = GetComponent<Animator>();
        m_RigidBody = GetComponent<Rigidbody>();
        m_AudioSource = GetComponent<AudioSource>();
        m_PlayerCollider = GetComponent<CapsuleCollider>();
        m_DefaultPlayerHeight = m_PlayerCollider.height;

        // Camera Components
        m_Camera = Camera.main;
        m_CamDefaultPosition = m_Camera.transform.localPosition;
        m_CameraPivot = m_Camera.transform.parent.transform;
        m_Lamp = GetComponentInChildren<Light>();

        // Mouse Default Position
        Cursor.lockState = CursorLockMode.Locked;

        // Coroutine Methods
        crouch = ModifyHeight(m_DefaultPlayerHeight - m_CrouchAmount, 0.1f);
        stand = ModifyHeight(m_DefaultPlayerHeight, 0.1f);

        // PlayerState
        m_IsDead = false;
        m_DebugNewNoise = false;
    }

    // Update is called once per frame
    private void Update()
    {
        if (m_IsDead)
        {
            m_RigidBody.velocity = new Vector3(0, -m_Gravity, 0);
            return;
        }

        UpdateCameraRotation();
        UpdatePlayerVelocity();
        ProcessPlayerActions();
        #region Cookies stuff

        if (Input.GetKeyDown(KeyCode.C))
        {
            m_SpawnCookies = !m_SpawnCookies;
        }

        if (m_SpawnCookies)
        {
            if (m_CookieSpawnTimer >= m_CookieSpawnFrecuency)
            {
                //Create cookie
                if (m_PreviousCookie != null && m_PreviousCookie != m_CookieRef)
                    if (Vector3.Distance(transform.position, m_PreviousCookie.transform.position) < 2)
                        return;

                GameObject newCookie = Instantiate(m_CookieRef, transform);
                newCookie.transform.parent = null;
                newCookie.transform.position = transform.position;
                newCookie.SetActive(true);

                m_PreviousCookie = newCookie;

                m_CookieSpawnTimer = 0;
            }
            else
                m_CookieSpawnTimer += Time.deltaTime;

        }

        #endregion
    }

    private void UpdateCameraRotation()
    {
        m_RotationY += Input.GetAxis("Mouse X");
        m_RotationX -= Input.GetAxis("Mouse Y");
        m_RotationX = Mathf.Clamp(m_RotationX, -70.0f / (m_MouseSensitivity / 3.0f), 70.0f / (m_MouseSensitivity / 3.0f));

        transform.eulerAngles = new Vector2(0, m_RotationY * m_MouseSensitivity / 3.0f);
        m_Camera.transform.localEulerAngles = new Vector2(m_RotationX * m_MouseSensitivity / 3.0f, 0);
    }

    private void UpdatePlayerVelocity()
    {
        m_ForwardMovement = Input.GetAxis("Vertical");
        m_SideMovement = Input.GetAxis("Horizontal");

        float speed = GetCurrentSpeed();

        Vector3 gravityVector = new Vector3(0, m_Gravity, 0);
        Vector3 playerDirection = Vector3.Normalize(transform.forward * m_ForwardMovement + transform.right * m_SideMovement);

        m_RigidBody.velocity -= gravityVector;
        m_RigidBody.velocity = (playerDirection * speed) + (Vector3.up * m_RigidBody.velocity.y);


        m_Animator.SetFloat("walkSpeed", speed);

        bool isWalking = IsMoving() && IsGrounded();
        m_Animator.SetBool("Walking", isWalking);
        //Debug.Log(IsGrounded());
    }

    private void ProcessPlayerActions()
    {
        // Light
        if (Input.GetKeyDown(KeyCode.F))
            m_Lamp.enabled = !m_Lamp.enabled;

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
            StartCoroutine(Jump());

        // Crouch
        if (Input.GetKeyDown(KeyCode.LeftControl))
            ToggleCrouch();

        // Lean
        float leanDest = 0.0f;
        float offset = 0.0f;

        if (Input.GetKey(KeyCode.Q))
        {
            Debug.DrawRay(transform.position, -transform.right);
            if (!Physics.Raycast(transform.position, -transform.right, 0.75f, RaycastMask))
            {
                leanDest = m_MaxLeanAngle;
                offset = -m_LeanXOffset;
            }
        }

        else if (Input.GetKey(KeyCode.E))
        {
            Debug.DrawRay(transform.position, -transform.right);
            if (!Physics.Raycast(transform.position, transform.right, 0.75f, RaycastMask))
            {
                leanDest = -m_MaxLeanAngle;
                offset = m_LeanXOffset;
            }
        }

        m_CurrentOffset = Mathf.MoveTowards(m_CurrentOffset, offset, m_LeanXSpeed * Time.deltaTime);
        m_CurrentAngle = Mathf.MoveTowardsAngle(m_CurrentAngle, leanDest, m_LeanRotationSpeed * Time.deltaTime);
        m_CameraPivot.transform.localRotation = Quaternion.AngleAxis(m_CurrentAngle, Vector3.forward);
        m_CameraPivot.transform.localPosition = new Vector3(m_CurrentOffset, m_CameraPivot.transform.localPosition.y, Mathf.Abs(m_CurrentOffset / 6.0f));
    }

    private float GetCurrentSpeed()
    {
        if (IsCrouching())
            return m_CrouchSpeed;

        else if (IsRunning())
            return m_RunSpeed;

        else
            return m_WalkSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Cookie")
            m_IsGrounded = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag != "Cookie")
            m_IsGrounded = false;
    }

    private void ToggleCrouch()
    {
        if (!IsCrouching())
        {
            StopCoroutine(stand);
            StartCoroutine(crouch);
            m_IsCrouching = true;
        }
        else
        {
            StopCoroutine(crouch);
            StartCoroutine(stand);
            m_IsCrouching = false;
        }
    }

    public void Die()
    {
        m_IsDead = true;
    }

    public void MakeStepNoise()
    {
        Collider[] colliders;
        if (GetCurrentSpeed() == m_WalkSpeed)
        {
            if (m_ModerateNoiseRadius != null)
                Instantiate(m_ModerateNoiseRadius, transform.position, transform.rotation);
        }
        else if (GetCurrentSpeed() == m_RunSpeed)
        {
            if (m_LoudNoiseRadius != null)
                Instantiate(m_LoudNoiseRadius, transform.position, transform.rotation);
        }
    }

    IEnumerator ModifyHeight(float newHeight, float rate)
    {
        while (m_PlayerCollider.height != newHeight)
        {
            m_PlayerCollider.height = Mathf.Lerp(m_PlayerCollider.height, newHeight, rate);
            yield return null;
        }
    }

    IEnumerator Jump()
    {
        Debug.Log("JUMPSTARTED");
        // Set Y velocity to 0 and add an initial force
        m_RigidBody.velocity -= new Vector3(0, m_RigidBody.velocity.y, 0);
        m_RigidBody.AddForce(new Vector3(0, m_JumpForce, 0));

        // Wait until y velocity has passed a specific velocity
        while (m_RigidBody.velocity.y <= 0.6f)
        {
            yield return null;
        }

        // Wait until y velocity is less than a specific velocity
        while (m_RigidBody.velocity.y > 0.6f)
        {
            yield return null;
        }

        float tempGravity = m_Gravity * 0.5f;
        while (!IsGrounded())
        {
            m_RigidBody.velocity -= new Vector3(0, tempGravity, 0);
            yield return null;
        }

        m_Animator.Play("FinishJump");

        Debug.Log("JUMP FINISHED");
        StopCoroutine(Jump());
    }

    private void OnDrawGizmos()
    {
        if (m_DebugNewNoise)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, m_DebugNewNoiseRadius);
            m_DebugNewNoise = false;
        }
    }

    #region Conditional Methods
    bool IsGrounded()
    {
        return m_IsGrounded;
    }


    bool IsRunning()
    {
        return Input.GetKey(KeyCode.LeftShift) && !IsCrouching();
    }

    bool IsMoving()
    {
        return !(m_ForwardMovement == 0 && m_SideMovement == 0);
    }

    bool IsCrouching()
    {
        return m_IsCrouching;
    }
    #endregion

    #region Animator Methods
    public void PlayRightFoot()
    {
       // int index = Random.Range(0, m_FootSteps.Length / 2 - 1);
        //m_AudioSource.clip = m_FootSteps[0];
        //m_AudioSource.Play();
    }

    public void PlayLeftFoot()
    {
       // int index = Random.Range(m_FootSteps.Length / 2 - 1, m_FootSteps.Length - 1);
        //m_AudioSource.clip = m_FootSteps[0];
        //m_AudioSource.Play();
    }
    #endregion
}
