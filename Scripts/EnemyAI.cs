using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum EnemyState
{
    None,
    Patrol,    
    Chase,     
    Attack,    
    Idle       
}

[RequireComponent(typeof(CharacterController)), RequireComponent(typeof(Animator))]
public class EnemyAI : MonoBehaviour
{
    public EnemyState currentState;
    public string playerTag = "Player";
    private CharacterController controller;
    private Animator animator;
    
    public Transform[] patrolPoints;
    private int currentPatrolPointIndex = 0;
    
    public float patrolSpeed = 2f;
    public float waitTime = 3f;
    private Vector3 targetPatrolPoint;
    private float waitTimer;

 
    public float detectRange = 15f;
    public float chaseRangeBuffer = 2f; 
    public float attackRange = 2f;
    public float attackExitBuffer = 0.5f;
    private Transform playerTransform;
    

    public int attackDamage = 10;
    public float attackInterval = 1f;
    private float attackTimer;
    public float minAttackStateTime = 0.5f;
    private float attackStateEnterTime;
    private bool isAttacking = false; 


    public float chaseSpeed = 4f;
    public float gravity = -9.81f;
    private float verticalVelocity;
    public float rotationSmoothTime = 0.1f; 
    private float rotationSmoothVelocity;


    public string animIsIdleParam = "IsIdle";
    public string animIsWalkingParam = "IsWalking";
    public string animIsAttackingParam = "IsAttacking";
    public string animIsDieParam = "IsDie";
    public string animHitParam = "hit";
    

    public float maxHp;
    public float currentHp;
    public Image hpImage;
    public bool isDie;
    public AudioSource audioSource;


    private bool isStateTransitioning = false;
    private EnemyState nextState = EnemyState.None;
    private float stateChangeCooldown = 0.2f; 
    private float lastStateChangeTime;

    public void TakeDamage(float damage)
    {
        if (isDie) return;
        
        currentHp -= damage;
        hpImage.fillAmount = currentHp / maxHp;
        
        animator.SetTrigger(animHitParam);
        if (currentHp <= 0)
        {
            ScoreManger.instance.AddScore();
            ScoreManger.instance.AddKill();
            isDie = true;
            animator.SetTrigger(animIsDieParam);
            audioSource?.Play();
            Destroy(gameObject, 3f);
        }
    }

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        
        currentState = EnemyState.Patrol;
        GetNextPatrolPoint();
        waitTimer = waitTime;
        attackTimer = attackInterval;
        lastStateChangeTime = Time.time;
    }

    private void Start()
    {
        currentHp = maxHp;
        FindPlayer();
    }

    void Update()
    {

        if (isDie || playerTransform == null) 
        {
            UpdateAnimationStates(true, false);
            return;
        }
        

        if (isStateTransitioning || Time.time - lastStateChangeTime < stateChangeCooldown)
        {
            PerformStateTransition();
            return;
        }


        switch (currentState)
        {
            case EnemyState.Patrol: PatrolLogic(); break;
            case EnemyState.Chase: ChaseLogic(); break;
            case EnemyState.Attack: AttackLogic(); break;
            case EnemyState.Idle: IdleLogic(); break;
        }
    }

    #region 状态切换核心修复
    void RequestStateChange(EnemyState newState)
    {

        if (isStateTransitioning || 
            Time.time - lastStateChangeTime < stateChangeCooldown || 
            currentState == newState) return;
        
        nextState = newState;
        isStateTransitioning = true;
        lastStateChangeTime = Time.time; 
        
 
        if (currentState == EnemyState.Attack && isAttacking)
        {
            CancelInvoke(nameof(ApplyDamage));
            animator.ResetTrigger(animIsAttackingParam);
            isAttacking = false;
        }
        

        PerformStateTransition();
    }

    void PerformStateTransition()
    {
        if (!isStateTransitioning) return;
        

        UpdateAnimationStates(false, false);
        animator.ResetTrigger(animIsAttackingParam);
        
 
        currentState = nextState;
        switch (currentState)
        {
            case EnemyState.Patrol: GetNextPatrolPoint(); break;
            case EnemyState.Idle: waitTimer = waitTime; break;
            case EnemyState.Attack: 
                attackStateEnterTime = Time.time;
                attackTimer = 0; 
                isAttacking = false;
                Debug.Log("进入攻击状态，准备攻击玩家"); 
                break;
        }
 
        isStateTransitioning = false;
        nextState = EnemyState.None;
    }
    #endregion

    #region 核心逻辑
    void PatrolLogic()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        if (distanceToPlayer <= detectRange)
        {
            RequestStateChange(EnemyState.Chase);
            return;
        }
        
        if (Vector3.Distance(transform.position, targetPatrolPoint) < 0.5f)
        {
            RequestStateChange(EnemyState.Idle);
            return;
        }
        
        MoveToTarget(targetPatrolPoint, patrolSpeed);
        SmoothLookAtTarget(targetPatrolPoint);
        UpdateAnimationStates(false, true);
    }

    void IdleLogic()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        if (distanceToPlayer <= detectRange)
        {
            RequestStateChange(EnemyState.Chase);
            return;
        }
        
        UpdateAnimationStates(true, false);
        
        if (waitTimer <= 0)
        {
            RequestStateChange(EnemyState.Patrol);
        }
        else
        {
            waitTimer -= Time.deltaTime;
        }
    }

    void ChaseLogic()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        if (distanceToPlayer > detectRange + chaseRangeBuffer)
        {
            RequestStateChange(EnemyState.Patrol);
            return;
        }

        if (distanceToPlayer <= attackRange)
        {
            RequestStateChange(EnemyState.Attack);
            return;
        }


        MoveToTarget(playerTransform.position, chaseSpeed);
        SmoothLookAtTarget(playerTransform.position); 
        UpdateAnimationStates(false, true);
    }

    void AttackLogic()
    {
        if (isStateTransitioning) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        float currentAttackStateTime = Time.time - attackStateEnterTime;
        
        bool shouldExitToChase = distanceToPlayer > attackRange + attackExitBuffer && 
                                currentAttackStateTime >= minAttackStateTime;
        bool shouldExitToPatrol = distanceToPlayer > detectRange + chaseRangeBuffer;
        
        if (shouldExitToPatrol)
        {
            RequestStateChange(EnemyState.Patrol);
            return;
        }
        if (shouldExitToChase)
        {
            RequestStateChange(EnemyState.Chase);
            return;
        }


        SmoothLookAtTarget(playerTransform.position);
        

        if (!isAttacking)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0)
            {
                PerformAttack();
                attackTimer = attackInterval;
            }
            else
            {
                UpdateAnimationStates(true, false); 
            }
        }
    }
    #endregion

    #region 动画与攻击
    void UpdateAnimationStates(bool isIdle = false, bool isWalking = false)
    {
        if (isAttacking || isDie) return;
        
        animator.SetBool(animIsIdleParam, isIdle);
        animator.SetBool(animIsWalkingParam, isWalking);
        
        if (!isIdle && !isWalking)
        {
            animator.SetBool(animIsIdleParam, true);
            animator.SetBool(animIsWalkingParam, false);
        }
    }

    void PerformAttack()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer > attackRange)
        {
            isAttacking = false;
            return;
        }
        
        isAttacking = true;
 
        animator.SetBool(animIsIdleParam, false);
        animator.SetBool(animIsWalkingParam, false);
        animator.SetTrigger(animIsAttackingParam);
        

        Invoke(nameof(ApplyDamage), 0.3f);
  
        Invoke(nameof(ResetAttackState), 0.8f); 
    }

    void ResetAttackState() 
    {
        isAttacking = false;

    }

    void ApplyDamage()
    {
        if (isDie) return;
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer > attackRange)
        {

            return;
        }
        
        Player player = playerTransform.GetComponent<Player>();
        if (player != null)
        {
            player.TakeDamage(attackDamage);

        }
        else
        {

        }
    }
    #endregion

    #region 辅助方法
    void GetNextPatrolPoint()
    {

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            targetPatrolPoint = transform.position + transform.forward * 5f;
            return;
        }


        currentPatrolPointIndex = (currentPatrolPointIndex + 1) % patrolPoints.Length;
        targetPatrolPoint = patrolPoints[currentPatrolPointIndex].position;
        targetPatrolPoint.y = transform.position.y; 
    }

    void MoveToTarget(Vector3 target, float speed)
    {
        if (!controller.isGrounded) return;
        
        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0; 
        
        verticalVelocity += gravity * Time.deltaTime;
        if (controller.isGrounded) verticalVelocity = -0.5f;
        
        Vector3 moveVector = direction * speed + Vector3.up * verticalVelocity;
        controller.Move(moveVector * Time.deltaTime);
    }


    void SmoothLookAtTarget(Vector3 target)
    {
        Vector3 lookDirection = (target - transform.position).normalized;
        lookDirection.y = 0;
        
        if (lookDirection.sqrMagnitude < 0.01f) return;
        
        float targetYaw = Mathf.Atan2(lookDirection.x, lookDirection.z) * Mathf.Rad2Deg;
        float currentYaw = transform.eulerAngles.y;
        float smoothYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref rotationSmoothVelocity, rotationSmoothTime);
        transform.rotation = Quaternion.Euler(0f, smoothYaw, 0f);
    }

    void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
        }
    }
    #endregion
    
}