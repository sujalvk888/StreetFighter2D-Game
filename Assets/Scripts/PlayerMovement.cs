using UnityEngine;
using UnityEngine.UI; // <--- REQUIRED FOR HEALTH BARS!

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerID = 1;
    public string victoryVideoFileName; // e.g., type "P1_Victory.m4v" in the Inspector!

    [Header("Controls")]
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode jumpKey = KeyCode.W;
    public KeyCode attack1Key = KeyCode.J;
    public KeyCode attack2Key = KeyCode.K;
    public KeyCode attack3Key = KeyCode.L;
    public KeyCode blockKey = KeyCode.S; 

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip attack1Sound;
    public AudioClip attack2Sound;
    public AudioClip attack3Sound;
    private string lastAttackUsed = ""; 

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpForce = 12f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Combat")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;
    public int attackDamage = 10;

    [Header("Mode Settings")]
    public bool isDummy = false;
    public bool isAI = false;
    [HideInInspector] public float aiMoveInput = 0f;
    [HideInInspector] public bool aiIsBlocking = false;
    [HideInInspector] public bool aiIsJumping = false;
    public bool isBlocking = false;

    [Header("Health & UI")] // <--- NEW SECTION
    public int maxHealth = 100;
    public int currentHealth;
    public Slider healthBar;
    private bool isDead = false;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private bool isGrounded;
    private float moveInput;
    private bool isAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();

        // Set health to max at the start of the round
        currentHealth = maxHealth;
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }

    void Update()
    {
        if (isDead) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        // MODE 1: HUMAN PLAYER
        if (!isDummy && !isAI)
        {
            isBlocking = Input.GetKey(blockKey) && isGrounded && !isAttacking;

            if (!isAttacking && !isBlocking)
            {
                if (Input.GetKeyDown(attack1Key)) ExecuteAttack("Attack1");
                else if (Input.GetKeyDown(attack2Key)) ExecuteAttack("Attack2");
                else if (Input.GetKeyDown(attack3Key)) ExecuteAttack("Attack3");
            }

            if (isBlocking)
            {
                moveInput = 0f;
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            else if (!isAttacking)
            {
                moveInput = 0f;
                if (Input.GetKey(leftKey)) moveInput = -1f;
                if (Input.GetKey(rightKey)) moveInput = 1f;
                
                if (Input.GetKey(leftKey) && Input.GetKey(rightKey)) moveInput = 0f;

                rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

                if (moveInput > 0) transform.localScale = new Vector3(1, 1, 1);
                else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);

                if (Input.GetKeyDown(jumpKey) && isGrounded)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                }
            }
        }
        // MODE 2: AI CPU
        else if (isAI)
        {
            isBlocking = aiIsBlocking && isGrounded && !isAttacking;

            if (isBlocking)
            {
                moveInput = 0f;
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            else if (!isAttacking)
            {
                moveInput = aiMoveInput;
                rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

                if (moveInput > 0) transform.localScale = new Vector3(1, 1, 1);
                else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);

                if (aiIsJumping && isGrounded)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                    aiIsJumping = false; // Reset the jump trigger
                }
            }
        }
        // MODE 3: PRACTICE DUMMY
        else
        {
            moveInput = 0f;
            isBlocking = false;
        }

        animator.SetFloat("Speed", Mathf.Abs(moveInput));
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsBlocking", isBlocking);
    }

    public void ExecuteAttack(string triggerName)
    {
        isAttacking = true;
        animator.SetTrigger(triggerName);
        moveInput = 0f;
        
        // Remember which punch we just threw, but DO NOT play the sound yet!
        lastAttackUsed = triggerName; 

        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    public void TriggerHitbox()
    {
        if (attackPoint == null) return;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        
        // Track if we actually hit a physical body
        bool landedHit = false; 

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.gameObject != this.gameObject)
            {
                PlayerMovement enemyScript = enemy.GetComponent<PlayerMovement>();
                if (enemyScript != null)
                {
                    enemyScript.TakeDamage(attackDamage);
                    landedHit = true; // Confirmed hit!
                }
            }
        }

        // ONLY play the sound if the punch successfully landed!
        if (landedHit && audioSource != null)
        {
            if (lastAttackUsed == "Attack1" && attack1Sound != null) audioSource.PlayOneShot(attack1Sound);
            else if (lastAttackUsed == "Attack2" && attack2Sound != null) audioSource.PlayOneShot(attack2Sound);
            else if (lastAttackUsed == "Attack3" && attack3Sound != null) audioSource.PlayOneShot(attack3Sound);
        }
    }

    // Now accepts damage amount
    public void TakeDamage(int damageAmount)
    {
        // Don't take damage if already knocked out
        if (isDead) return; 

        // INFINITE HEALTH FOR PRACTICE DUMMY
        if (isDummy)
        {
            animator.SetTrigger("Hit");
            isAttacking = false;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return; // We "return" early so health never goes down!
        }

        // Standard Versus Mode Logic below:
        
        // If the player successfully blocked the attack, negate the damage!
        if (isBlocking)
        {
            // Optional: You could add chip damage here later (e.g., currentHealth -= 1)
            Debug.Log("Player " + playerID + " BLOCKED the attack!");
            return; // We return early to skip the hit animation and health reduction
        }
        
        currentHealth -= damageAmount;
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }

        isAttacking = false;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // Did we drop to 0?
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            animator.SetTrigger("Hit");
        }
    }

    private void Die()
    {
        Debug.Log("Player " + playerID + " is KNOCKED OUT!");
        isDead = true;
        
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        gameObject.layer = LayerMask.NameToLayer("Default"); 
        
        animator.SetTrigger("Knockout");

        // NEW: Tell the Game Manager that I lost!
        if (GameManager.instance != null)
        {
            GameManager.instance.RegisterKnockout(playerID);
        }
    }

    public void EndAttack()
    {
        isAttacking = false;
    }

    public void ResetFighter(Vector3 startPosition)
    {
        // 1. Move back to starting corners
        transform.position = startPosition;
        
        // 2. Heal up
        currentHealth = maxHealth;
        if (healthBar != null) healthBar.value = currentHealth;
        
        // 3. Become punchable again
        gameObject.layer = LayerMask.NameToLayer("Enemy"); 
        isDead = false;
        
        // 4. Force the Animator to snap back to the Idle state immediately
        animator.Play("Idle"); 
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}