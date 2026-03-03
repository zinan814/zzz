using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] CharacterController characterController;
    private Quaternion targetDirQuaternion;
    [SerializeField] float moveSpeed = 1;
    [SerializeField] float runSpeed = 2.5f;
    private float speed;
    public float timer = 5f;  
    public bool isRunning = false;
    private bool canStartRunning = true; 
    
    [Header("跳跃设置")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;
    private Vector3 velocity;
    private bool isJumping = false;
    
    private Transform currentPlatform; 
    private bool isOnPlatformTrigger; 

    [Header("攻击")]
    [SerializeField] private bool canAttack;
    public GameObject losePanel;
    
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackRadius = 0.5f;
    [SerializeField] private int attackDamage = 30;
    [SerializeField] private float knockbackDistance = 0.5f;
    [SerializeField] private LayerMask enemyLayer; 

    public GameObject sphere;

    [Header("HP")] public float maxHp = 100;
    public float currentHp;
    public Image hpImage;
    
    public AudioSource audioSource;
    
    public void DoAttackDamage()
    {
        sphere.gameObject.SetActive(true);
        Invoke("Hide",0.1f);


        Vector3 attackCenter = transform.position + transform.forward * (attackRange / 2);

        Collider[] hitColliders = Physics.OverlapSphere(attackCenter, attackRadius, enemyLayer);

        foreach (Collider collider in hitColliders)
        {
   
            EnemyAI enemyAI = collider.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
    
                enemyAI.TakeDamage(attackDamage);
                
            }
            
            BossAI bossAI = collider.GetComponent<BossAI>();
            if (bossAI != null && !bossAI.IsDead)
            {
                bossAI.TakeDamage(attackDamage);
            }
        }
    }

    public void Hide()
    {
        sphere.gameObject.SetActive(false);
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        currentHp =  maxHp;
        //GameManger.instance.LockCursor();
        
        canAttack = true;
        characterController.skinWidth = 0.01f;
        characterController.minMoveDistance = 0.001f;
    }

    private void Update()
    {
        Move();
        Attack();
        UpdatePlatformParent(); 
    }

    public void TakeDamage(float damage)
    {
        currentHp -= damage;
        hpImage.fillAmount = currentHp / maxHp;

        if (currentHp <= 0)
        {
            losePanel.SetActive(true);
            //GameManger.instance.ReleaseCursor();
        }
    }

    public void AddHp(int amount)
    {
        currentHp+= amount;
        if (currentHp >= maxHp)
        {
            currentHp = maxHp;
        }

        hpImage.fillAmount = currentHp / maxHp;
    }

    private void Move()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = new Vector3(h, 0f, v);

        if (moveDirection.magnitude > 0)
        {
            animator.SetBool("Walk", true);
            float cameraAxisY = Camera.main.transform.rotation.eulerAngles.y;
            moveDirection = Quaternion.Euler(0f, cameraAxisY, 0f) * moveDirection;
            moveDirection.Normalize();

            targetDirQuaternion = Quaternion.LookRotation(moveDirection);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetDirQuaternion, Time.deltaTime * 10f);
        }
        else
        {
            animator.SetBool("Walk", false);
        }


        if (characterController.isGrounded)
        {
            velocity.y = -0.5f;
            isJumping = false;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                isJumping = true;
                animator.SetTrigger("Jump");
                if (currentPlatform != null)
                {
                    transform.SetParent(null);
                    currentPlatform = null;
                }
            }
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        if (!isRunning)
        {
            timer = Mathf.Min(timer + Time.deltaTime, 5f);
            if (timer >= 5f)
            {
                canStartRunning = true;
            }
        }
        
        if (Input.GetKey(KeyCode.LeftShift) && canStartRunning && !isJumping && moveDirection.magnitude > 0)
        {
            isRunning = true;
            speed = runSpeed;
            animator.SetBool("Running", true);
            timer = Mathf.Max(timer - Time.deltaTime, 0);
            
            if (timer <= 0)
            {
                canStartRunning = false;
            }
        }
        else
        {
            isRunning = false;
            speed = moveSpeed;
            animator.SetBool("Running", false);
        }
        
        moveDirection.y = velocity.y;
        characterController.Move(moveDirection * speed * Time.deltaTime);
    }
    
    private void UpdatePlatformParent()
    {
        if (isOnPlatformTrigger && characterController.isGrounded && !isJumping && currentPlatform == null)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, 0.5f);
            foreach (var col in colliders)
            {
                if (col.CompareTag("Platform") && col.isTrigger)
                {
                    currentPlatform = col.transform; 
                    transform.SetParent(currentPlatform);
                    break;
                }
            }
        }
        
        if ((!isOnPlatformTrigger || !characterController.isGrounded || isJumping) && currentPlatform != null)
        {
            transform.SetParent(null);
            currentPlatform = null;
        }
    }

    private void Attack()
    {
        if (Input.GetMouseButton(0) && !isJumping)
        {
            if (canAttack)
            {
                animator.SetTrigger("attack");
                audioSource.Play();
                canAttack = false;
                Invoke("RecoverAttack", 0.7f);
            }
        }
    }

    void RecoverAttack() => canAttack = true;
    
}