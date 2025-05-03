using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [Header("References")]
    private Transform player;
    private NavMeshAgent agent;
    [SerializeField] private Animator animator;

    [Header("Health Settings")]
    public int maxHealth = 50;
    private int currentHealth;

    [Header("Attack Settings")]
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    public int attackDamage = 10;
    private float lastAttackTime;

    [Header("Rewards")]
    public int currencyDropAmount = 5; // Số tiền rơi ra khi chết

    // Animator state flags
    private bool isIdle;
    private bool isWalking;
    private bool isAttacking;
    private bool isSpawning;
    private bool isDead;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentHealth = maxHealth;

        // if (animator == null)
        // {
        //     Debug.LogError($"{gameObject.name}: Không tìm thấy Animator!");
        // }
        // else
        // {
        //     Debug.Log($"{gameObject.name}: Đã lấy được Animator thành công.");
        // }

        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        SetSpawn();
        Invoke(nameof(SetIdle), 1.0f);
    }

    void Update()
    {
        if (isDead) return;

        // Add check for agent validity and NavMesh status
        if (agent == null || !agent.isOnNavMesh) return;

        if (player == null)
        {
            SetIdle();
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            agent.isStopped = true;

            if (Time.time - lastAttackTime > attackCooldown)
            {
                lastAttackTime = Time.time;
                SetAttack();
                PerformAttack();
            }
            else
            {
                SetIdle();
            }
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            SetWalking();
        }
    }

    private void PerformAttack()
    {

        // if (player.TryGetComponent(out  player))
        // {
        //     player.TakeDamage(attackDamage);
        // }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        SetDead();

        // Add check before accessing agent properties
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.enabled = false; // Disable agent after stopping
        }

        // Thêm tiền cho người chơi
        PlayerController player = FindFirstObjectByType<PlayerController>(); // Use FindFirstObjectByType
        if (player != null)
        {
            player.AddInGameCurrency(currencyDropAmount);
        }

        // Gọi GameController để xử lý cái chết
        GameController gc = FindFirstObjectByType<GameController>();
        if (gc != null)
        {
            Debug.Log($"[{gameObject.name}] Calling HandleEnemyDeath in GameController."); // DEBUG LINE
            gc.HandleEnemyDeath(); // Đảm bảo dòng này được gọi
        }
        else {
             Debug.LogError($"[{gameObject.name}] Không tìm thấy GameController để báo cáo enemy death!"); // DEBUG LINE
        }

        // Optional: Disable collider to prevent further interactions
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        Destroy(gameObject, 1.5f); // Destroy after animation
    }

    public void SetTarget(Transform newTarget)
    {
        player = newTarget;
    }

    // Animator State Setters
    private void SetIdle()
    {
        SetState(true, false, false, false, false);
    }

    private void SetWalking()
    {
        SetState(false, true, false, false, false);
    }

    private void SetAttack()
    {
        SetState(false, false, true, false, false);
    }

    private void SetSpawn()
    {
        SetState(false, false, false, true, false);
    }

    private void SetDead()
    {
        SetState(false, false, false, false, true);
    }

    private void SetState(bool idle, bool walking, bool attacking, bool spawning, bool dead)
    {
        if (isIdle != idle) animator.SetBool("IsIdle", idle);
        if (isWalking != walking) animator.SetBool("IsWalking", walking);
        if (isAttacking != attacking) animator.SetBool("IsAttack", attacking);
        if (isSpawning != spawning) animator.SetBool("IsSpawn", spawning);
        if (isDead != dead) animator.SetBool("IsDead", dead);

        isIdle = idle;
        isWalking = walking;
        isAttacking = attacking;
        isSpawning = spawning;
        isDead = dead;
    }
}
