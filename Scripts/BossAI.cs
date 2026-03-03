using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossAI : MonoBehaviour
{
    [Header("核心配置")]
    [SerializeField] private Player player;
    [SerializeField] private float bossMaxHp = 1000;
    [SerializeField] private float detectDistance = 20f;
    [SerializeField] private float chaseDistance = 15f;
    [SerializeField] private float attackDistance = 3f;
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private int attackDamage = 50;
    [SerializeField] private float knockbackForce = 6f;
    [SerializeField] private float knockbackDuration = 0.2f;

    [Header("血条UI")]
    [SerializeField] private Canvas headHpCanvas;
    [SerializeField] private Image headHpFill;
    [SerializeField] private Image uiBigHpFill;

    [Header("动画配置")]
    [SerializeField] private Animator bossAnimator;
    [SerializeField] private float minAttackCooldown = 3f;
    [SerializeField] private float maxAttackCooldown = 5f;
    [SerializeField] private string attack1Trigger = "attack1";
    [SerializeField] private string attack2Trigger = "attack2";
    [SerializeField] private string attack3Trigger = "attack3";

    private float currentHp;
    private bool isRisen = false;
    private bool isAttacking = false;
    private bool isInAttackCooldown = false;
    private bool isDead = false;
    private float attackCooldownTimer;
    private Vector3 targetDir;
    private float hitCooldown = 4f;
    private float hitCooldownTimer;
    private bool isPlayerKnockingBack = false;

    public GameObject winPanel;

    public bool IsDead => isDead;

    private void Awake()
    {
        if (bossAnimator == null) bossAnimator = GetComponent<Animator>();
        

        if (headHpCanvas != null) headHpCanvas.enabled = false;
        if (uiBigHpFill != null) uiBigHpFill.fillAmount = 0;
    }

    private void Start()
    {
        currentHp = bossMaxHp;
        attackCooldownTimer = 0;
        hitCooldownTimer = 0;
        
        if (player == null) player = FindObjectOfType<Player>();
    }

    private void Update()
    {
        
        if (isDead) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        UpdateHealthBars(distanceToPlayer);

        if (!isRisen && distanceToPlayer <= detectDistance)
        {
            RiseUp();
        }

        if (isRisen)
        {
            UpdateAttackCooldown();
            UpdateHitCooldown();
            LookAtPlayer();

            if (!isAttacking && !isInAttackCooldown)
            {
                if (distanceToPlayer <= attackDistance)
                {
                    TriggerSpecificAttack();
                }
                else if (distanceToPlayer <= chaseDistance)
                {
                    ChasePlayer();
                }
                else
                {
                    bossAnimator.SetBool("walk", false);
                }
            }
        }
    }

    private void UpdateHealthBars(float distance)
    {
        bool isShow = distance <= detectDistance && !isDead;

        if (headHpCanvas != null)
        {
            headHpCanvas.enabled = isShow;
            if (headHpFill != null)
            {
                headHpFill.fillAmount = currentHp / bossMaxHp;
            }
        }

        if (uiBigHpFill != null)
        {
            uiBigHpFill.gameObject.SetActive(isShow);
            uiBigHpFill.fillAmount = currentHp / bossMaxHp;
        }
    }

    private void RiseUp()
    {
        isRisen = true;
        bossAnimator.SetTrigger("Rise");
    }

    private void LookAtPlayer()
    {
        targetDir = new Vector3(player.transform.position.x - transform.position.x,
                                0,
                                player.transform.position.z - transform.position.z);
        if (targetDir.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(targetDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
        }
    }

    private void ChasePlayer()
    {
        bossAnimator.SetBool("walk", true);
        Vector3 moveDir = targetDir.normalized;
        transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);
    }

    private void TriggerSpecificAttack()
    {
        isAttacking = true;
        bossAnimator.SetBool("walk", false);

        int attackType = Random.Range(1, 4);
        switch (attackType)
        {
            case 1:
                bossAnimator.SetTrigger(attack1Trigger);
                break;
            case 2:
                bossAnimator.SetTrigger(attack2Trigger);
                break;
            case 3:
                bossAnimator.SetTrigger(attack3Trigger);
                break;
        }
    }

    public void ShowWinPanel()
    {
        //GameManger.instance.ReleaseCursor();
        winPanel.gameObject.SetActive(true);
    }

    private void UpdateAttackCooldown()
    {
        if (isInAttackCooldown)
        {
            attackCooldownTimer -= Time.deltaTime;
            if (attackCooldownTimer <= 0)
            {
                isInAttackCooldown = false;
                attackCooldownTimer = 0;
            }
        }
    }

    private void UpdateHitCooldown()
    {
        if (hitCooldownTimer > 0)
        {
            hitCooldownTimer -= Time.deltaTime;
        }
    }

    public void OnAnyAttackAnimationEnd()
    {
        isAttacking = false;
        attackCooldownTimer = Random.Range(minAttackCooldown, maxAttackCooldown);
        isInAttackCooldown = true;
    }

    public void OnAttack1End() { OnAnyAttackAnimationEnd(); }
    public void OnAttack2End() { OnAnyAttackAnimationEnd(); }
    public void OnAttack3End() { OnAnyAttackAnimationEnd(); }

    public void ApplyDamage()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (distanceToPlayer <= attackDistance && player != null && !isDead)
        {
            player.TakeDamage(attackDamage);
            if (!isPlayerKnockingBack)
            {
                StartCoroutine(ApplySmoothKnockbackToPlayer());
            }
        }
    }

    private IEnumerator ApplySmoothKnockbackToPlayer()
    {
        isPlayerKnockingBack = true;
        CharacterController playerCC = player.GetComponent<CharacterController>();
        if (playerCC == null)
        {
            isPlayerKnockingBack = false;
            yield break;
        }

        Vector3 knockbackDir = (player.transform.position - transform.position).normalized;
        knockbackDir.y = 0;
        float elapsedTime = 0f;

        while (elapsedTime < knockbackDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / knockbackDuration;
            float currentForce = knockbackForce * (1 - progress);
            Vector3 moveVector = knockbackDir * currentForce * Time.deltaTime;
            playerCC.Move(moveVector);
            yield return null;
        }

        isPlayerKnockingBack = false;
    }

    public void TakeDamage(float damage)
    {
        if (isDead || !isRisen) return;

        currentHp = Mathf.Max(0, currentHp - damage);

        if (hitCooldownTimer <= 0 && !isAttacking)
        {
            bossAnimator.SetTrigger("hit");
            hitCooldownTimer = hitCooldown;
        }

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        isRisen = false;
        isAttacking = false;
        isInAttackCooldown = true;

        bossAnimator.SetTrigger("death");

        if (headHpCanvas != null) headHpCanvas.enabled = false;
        if (uiBigHpFill != null) uiBigHpFill.gameObject.SetActive(false);

        ShowWinPanel();
    }
}