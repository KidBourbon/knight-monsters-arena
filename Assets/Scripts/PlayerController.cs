using System;
using System.Collections;
using UnityEngine;

public enum PlayerAttackType { Vertical, Diagonal }

public class PlayerController : MonoBehaviour
{
    #region Variables

    [SerializeField] private float movementSpeed = 200.0f;
    [SerializeField] private float walkingAnimSpeedValue = 0.4f;
    [SerializeField] private float runningAnimSpeedValue = 1.0f;
    [SerializeField] private float turnSmoothTime = 0.1f;
    [SerializeField] private float life = 100;
    [SerializeField] private float maxLife = 100;

    private float idleAnimSpeedValue = 0f;
    private float turnSmoothVelocity;

    public bool IsImmune { get; set; }
    public bool IsDefending { get; private set; }
    public float MaxLife { get => maxLife; }

    private Rigidbody playerRb;
    private Animator playerAnim;
    private GameManager gameManager;
    
    [SerializeField] private HUD hud;
    [SerializeField] private Transform cam;

    private PlayerAttackType attackType;

    public float Life
    {
        get { return life; }

        set
        {
            value = Mathf.Clamp(value, 0f, maxLife);
            life = value;
        }
    }

    #endregion

    #region Unity methods

    private void Awake()
    {
        attackType = PlayerAttackType.Vertical;
    }

    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        playerAnim = GetPlayerAnimator();
        gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        if (gameManager.IsGameActive && !IsRecoveringFromDeath())
        {
            if (Input.GetMouseButtonDown(0) && !IsRunning() && !IsAttacking())
            {
                Attack();
                ChangeTypeAttack();
            }

            if (Input.GetMouseButton(1))
            {
                Defend();
            }
            else if (Input.GetMouseButtonUp(1))
            {
                StopDefending();
            }
        }
    }

    void FixedUpdate()
    {
        if (gameManager.IsGameActive && !IsRecoveringFromDeath() && !IsAttacking() && !IsDefending && !IsGettingHit())
        {
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");

            if (horizontalInput != 0 || verticalInput != 0)
            {
                Move(horizontalInput, verticalInput);
            }
            else
            {
                StopMoving();
            }
        }
        else
        {
            StopMoving();
        }
    }

    #endregion

    #region Methods for player movement

    // Moves the player based on arrow key input and rotates the player in the direction he is walking
    void Move(float horizontalInput, float verticalInput)
    {
        playerAnim.SetBool("Idle_b", false);

        Vector3 direction = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

        direction = (Quaternion.Euler(0f, angle, 0f) * Vector3.forward).normalized;

        transform.rotation = Quaternion.Euler(0f, angle, 0f);
        playerRb.velocity = direction * GetMovementSpeed() * Time.deltaTime;
    }

    // Stops player movement
    void StopMoving()
    {
        playerRb.velocity = Vector3.zero;

        playerAnim.SetFloat("Speed_f", idleAnimSpeedValue);
        playerAnim.SetBool("Idle_b", true);
    }

    #endregion

    #region Combat actions

    // Plays the player's attack animation and changes player's attack type every time he attacks
    void Attack()
    {
        if (attackType == PlayerAttackType.Vertical)
        {
            playerAnim.SetTrigger("Attack1_trig");
        }
        else if (attackType == PlayerAttackType.Diagonal)
        {
            playerAnim.SetTrigger("Attack2_trig");
        }
    }

    // Changes the player's attack type
    void ChangeTypeAttack()
    {
        if (attackType == PlayerAttackType.Vertical)
        {
            attackType = PlayerAttackType.Diagonal;
        }
        else if (attackType == PlayerAttackType.Diagonal)
        {
            attackType = PlayerAttackType.Vertical;
        }
    }

    // Plays the player's defend animation and defends the player
    void Defend()
    {
        playerAnim.SetBool("Defending_b", true);
        playerAnim.SetBool("Idle_b", false);
        IsDefending = true;
    }

    // Stops the player's defend animation and the player stops defending
    void StopDefending()
    {
        playerAnim.SetBool("Defending_b", false);
        playerAnim.SetBool("Idle_b", true);
        IsDefending = false;
    }

    #endregion

    #region Get methods

    // Gets the player's movement speed and sets the player�s movement animation
    float GetMovementSpeed()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            playerAnim.SetFloat("Speed_f", runningAnimSpeedValue);
            return movementSpeed * 2;
        }
        else
        {
            playerAnim.SetFloat("Speed_f", walkingAnimSpeedValue);
            return movementSpeed;
        }
    }

    // Plays the player's get hit animation and subtracts life from the player
    void GetHitFromEnemy(GameObject enemy)
    {
        if (!IsDefending && !IsRecoveringFromDeath() && gameManager.IsGameActive)
        {
            life -= enemy.GetComponent<EnemyController>().PhysicalDamage;
            hud.UpdatePlayerHealthBarValue(GetLifeInPercent());

            if (life > 0)
            {
                playerAnim.SetTrigger("GetHit_trig");
            }
        }
    }

    // Plays the player's get hit animation and subtracts life from the player
    void GetHitFromLaunchObject(GameObject launchObject)
    {
        if (!IsDefending && !IsRecoveringFromDeath() && gameManager.IsGameActive)
        {
            life -= launchObject.GetComponent<LaunchObjectController>().physicalDamage;
            hud.UpdatePlayerHealthBarValue(GetLifeInPercent());

            if (life > 0)
            {
                playerAnim.SetTrigger("GetHit_trig");
            }
        }
    }

    // Gets the player's animator
    public Animator GetPlayerAnimator()
    {
        return GetComponentInChildren<Animator>();
    }

    // Gets the player's life in percent
    public float GetLifeInPercent()
    {
        return life / maxLife * 100;
    }

    #endregion

    #region Methods for verify player status

    // Check if the player is attacking
    public bool IsAttacking()
    {
        return playerAnim.GetCurrentAnimatorStateInfo(0).IsTag("Attack");
    }

    // Check if the player is running
    bool IsRunning()
    {
        return playerAnim.GetCurrentAnimatorStateInfo(0).IsTag("Run");
    }

    // Check if the player is getting hit
    bool IsGettingHit()
    {
        return playerAnim.GetCurrentAnimatorStateInfo(0).IsTag("GetHit");
    }

    // Check if the player is recovering from death
    public bool IsRecoveringFromDeath()
    {
        return playerAnim.GetCurrentAnimatorStateInfo(0).IsTag("DieRecover");
    }

    #endregion

    #region Collision and trigger

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && !IsImmune)
        {
            StartCoroutine(gameManager.ImmunizePlayer(collision.gameObject.GetComponent<EnemyController>().TimeBeforeAttacking));
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && !IsImmune && collision.gameObject.GetComponent<EnemyController>().IsAttacking())
        {
            GetHitFromEnemy(collision.gameObject);

            if (life <= 0)
            {
                gameManager.GameOver();
            }

            StartCoroutine(gameManager.ImmunizePlayer(gameManager.CombatCooldownTime));
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("LaunchObject"))
        {
            GetHitFromLaunchObject(other.gameObject);
            Destroy(other.gameObject);

            if (life <= 0)
            {
                gameManager.GameOver();
            }

            StartCoroutine(gameManager.ImmunizePlayer(gameManager.ReceiveDamageCooldownTime));
        }
    }

    #endregion
}
