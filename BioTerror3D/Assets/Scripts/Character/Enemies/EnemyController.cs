    using UnityEngine;
    using UnityEngine.AI;

    public abstract class EnemyController : MonoBehaviour
    {
        [Header("References")]
        protected Transform player;
        protected NavMeshAgent agent;
        [SerializeField] protected Animator animator;
        protected PlayerController playerController;
        protected GameController gameController;
        protected EnemyStats stats;

        // Animator state flags
        private bool isIdle;
        private bool isWalking;
        private bool isAttacking;
        private bool isSpawning;
        private bool isDead;

        protected virtual void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            stats = GetComponent<EnemyStats>();

            if (agent == null) Debug.LogError($"{gameObject.name}: NavMeshAgent missing!");
            if (animator == null) Debug.LogError($"{gameObject.name}: Animator missing!");
            if (stats == null) Debug.LogError($"{gameObject.name}: EnemyStats missing!");

            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            playerController = FindFirstObjectByType<PlayerController>();
            gameController = FindFirstObjectByType<GameController>();

            if (player == null) Debug.LogWarning($"{gameObject.name}: Player not found!");
            if (playerController == null) Debug.LogWarning($"{gameObject.name}: PlayerController not found!");
            if (gameController == null) Debug.LogWarning($"{gameObject.name}: GameController not found!");

            InitializeStats();
            stats.currentHealth = stats.maxHealth;

            SetSpawn();
            Invoke(nameof(SetIdle), 1.0f);
        }

        protected virtual void Update()
        {
            if (isDead || agent == null || !agent.isOnNavMesh || player == null)
            {
                SetIdle();
                return;
            }

            float distance = Vector3.Distance(transform.position, player.position);

            if (distance <= stats.attackRange)
            {
                agent.isStopped = true;
                if (Time.time - stats.lastAttackTime >= stats.attackCooldown)
                {
                    stats.lastAttackTime = Time.time;
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
                agent.speed = stats.moveSpeed;
                agent.SetDestination(player.position);
                SetWalking();
            }
        }

        protected abstract void PerformAttack();
        protected abstract void InitializeStats();

        public void TakeDamage(int damage, float armorPenetration)
        {
            if (isDead) return;

            float effectiveArmor = Mathf.Max(0, stats.armor - armorPenetration);
            int finalDamage = Mathf.Max(1, damage - (int)effectiveArmor);
            stats.currentHealth -= finalDamage;

            Debug.Log($"{gameObject.name} took {finalDamage} damage (after {effectiveArmor} armor). Health: {stats.currentHealth}/{stats.maxHealth}");

            if (stats.currentHealth <= 0)
            {
                Die();
            }
        }

        protected virtual void Die()
        {
            SetDead();

            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.enabled = false;
            }

            if (playerController != null)
            {
                playerController.AddInGameCurrency(stats.currencyDropAmount);
            }
            else
            {
                Debug.LogWarning($"{gameObject.name}: Cannot drop currency, PlayerController is null!");
            }

            if (gameController != null)
            {
                Debug.Log($"{gameObject.name}: Calling HandleEnemyDeath in GameController.");
                gameController.HandleEnemyDeath();
            }
            else
            {
                Debug.LogWarning($"{gameObject.name}: GameController not found for enemy death!");
            }

            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }

            Destroy(gameObject, 1.5f);
        }

        public void SetTarget(Transform newTarget)
        {
            player = newTarget;
        }

        protected void SetIdle() => SetState(true, false, false, false, false);
        protected void SetWalking() => SetState(false, true, false, false, false);
        protected void SetAttack() => SetState(false, false, true, false, false);
        protected void SetSpawn() => SetState(false, false, false, true, false);
        protected void SetDead() => SetState(false, false, false, false, true);

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