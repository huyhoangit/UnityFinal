using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public class PlayerController : MonoBehaviour
{
    private Rigidbody playerRb;
    private Animator playerAnimator;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravityModifier = 1f;
    private bool isOnGround = true;

    [Header("Shooting Settings")]
    [SerializeField] private GameObject playerBulletPrefab;
    [SerializeField] private GameObject skill1Prefab;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float skill1Speed = 5f;
    [SerializeField] private int bulletCount = 6;
    [SerializeField] private float bulletCooldown = 2f;
    [SerializeField] private float skillCooldown = 5f;
    [SerializeField] private float shootingRange = 10f;
    [SerializeField] private float bulletDamage = 10f; // Sát thương cơ bản của đạn
    [SerializeField] private float critChance = 5f; // Tỷ lệ chí mạng (%)
    [SerializeField] private float critDamageMultiplier = 1.5f; // Hệ số nhân sát thương chí mạng (1.5 = 150%)
    [SerializeField] private float armorPenetration = 0f; // Khả năng xuyên giáp (giá trị hoặc %)

    [Header("Health & Defense")]
    [SerializeField] private int maxHealth = 100; // Máu tối đa
    [SerializeField] private float healthRegenRate = 0f; // Máu hồi mỗi giây
    [SerializeField] private float armor = 0f; // Chỉ số giáp (giảm sát thương nhận vào)

    [Header("Indicator Settings")]
    [SerializeField] private Transform shootIndicatorPrefab;
    private Transform shootIndicatorInstance;

    [Header("Player Stats")]
    public int playerGold = 100; // Tiền tệ ban đầu
    private int currentHealth; // Máu hiện tại

    [Header("In-Game Progression")]
    public int inGameCurrency = 0;
    public Dictionary<string, int> acquiredUpgrades = new Dictionary<string, int>();

    // --- Variables to store initial stats ---
    private float initialWalkSpeed;
    private float initialSprintSpeed;
    private float initialJumpForce;
    private int initialBulletCount;
    private float initialBulletCooldown;
    private float initialSkillCooldown;
    private float initialShootingRange;
    private float initialBulletDamage;
    private float initialCritChance;
    private float initialCritDamageMultiplier;
    private int initialMaxHealth;
    private float initialArmorPenetration;
    private float initialHealthRegenRate;
    private float initialArmor;

    private enum ShootMode { ClosestEnemy, MouseDirection }
    private ShootMode shootMode = ShootMode.ClosestEnemy;

    private void Awake()
    {
        playerRb = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();

        if (playerRb == null) Debug.LogError("Rigidbody missing!");
        if (playerAnimator == null) Debug.LogError("Animator missing!");

        // --- Store initial stats ---
        initialWalkSpeed = walkSpeed;
        initialSprintSpeed = sprintSpeed;
        initialJumpForce = jumpForce;
        initialBulletCount = bulletCount;
        initialBulletCooldown = bulletCooldown;
        initialSkillCooldown = skillCooldown;
        initialShootingRange = shootingRange;
        initialBulletDamage = bulletDamage;
        initialCritChance = critChance;
        initialCritDamageMultiplier = critDamageMultiplier;
        initialMaxHealth = maxHealth;
        initialArmorPenetration = armorPenetration;
        initialHealthRegenRate = healthRegenRate;
        initialArmor = armor;

        // Initialize current health
        currentHealth = maxHealth;
    }

    private void Start()
    {
        Physics.gravity = new Vector3(0, -9.81f * gravityModifier, 0);
        StartCoroutine(ShootBulletsRoutine());
        StartCoroutine(ShootSkill1Routine());

        if (shootIndicatorPrefab)
        {
            shootIndicatorInstance = Instantiate(shootIndicatorPrefab, transform.position, Quaternion.identity);
        }

        // --- Start Health Regen Coroutine ---
        StartCoroutine(HealthRegenRoutine());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isOnGround)
        {
            Jump();
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            shootMode = shootMode == ShootMode.ClosestEnemy ? ShootMode.MouseDirection : ShootMode.ClosestEnemy;
            UIController.Instance.ShowShootMode("Mode: " + shootMode.ToString(), 1.5f);
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
        float moveX = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
        float moveZ = (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f);

        Vector3 moveDirection = new Vector3(moveX, 0, moveZ).normalized;
        bool isMoving = moveDirection.magnitude > 0f;

        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

        if (isMoving)
        {
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(moveDirection.x, 0, moveDirection.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        Vector3 newPosition = playerRb.position + moveDirection * currentSpeed * Time.fixedDeltaTime;
        playerRb.MovePosition(newPosition);

        playerAnimator.SetBool("IsRunning", isMoving);
        playerAnimator.SetBool("IsIdle", !isMoving);
    }

    private void Jump()
    {
        playerRb.linearVelocity = new Vector3(playerRb.linearVelocity.x, jumpForce, playerRb.linearVelocity.z);
        isOnGround = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isOnGround = true;
        }
    }

    private IEnumerator ShootBulletsRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(bulletCooldown);
            ShootPlayerBullets();
        }
    }

    private IEnumerator ShootSkill1Routine()
    {
        while (true)
        {
            yield return new WaitForSeconds(skillCooldown);
            ShootSkill1();
        }
    }

    private void ShootPlayerBullets()
    {
        if (playerBulletPrefab == null) return;

        Vector3 shootDirection = GetShootDirection();

        if (shootDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(shootDirection.x, 0, shootDirection.z));
            transform.rotation = targetRotation;
        }

        UpdateShootIndicator(shootDirection);

        float angleStep = 15f;
        float startAngle = -((bulletCount - 1) / 2f) * angleStep;

        for (int i = 0; i < bulletCount; i++)
        {
            float currentAngle = startAngle + i * angleStep;
            Quaternion rotation = Quaternion.Euler(0, currentAngle, 0);
            Vector3 bulletDir = rotation * shootDirection;

            GameObject bullet = Instantiate(playerBulletPrefab, transform.position + bulletDir * 1.5f, Quaternion.LookRotation(bulletDir));
            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();

            if (bulletRb != null)
            {
                bulletRb.linearVelocity = bulletDir * bulletSpeed;
            }
        }
    }

    private void ShootSkill1()
    {
        if (skill1Prefab == null) return;

        float angleStep = 360f / bulletCount;
        float angle = 0f;

        for (int i = 0; i < bulletCount; i++)
        {
            float bulletDirX = Mathf.Cos(angle * Mathf.Deg2Rad);
            float bulletDirZ = Mathf.Sin(angle * Mathf.Deg2Rad);

            Vector3 bulletDirection = new Vector3(bulletDirX, 0, bulletDirZ).normalized;
            Vector3 spawnPosition = transform.position + bulletDirection * 1.5f;

            GameObject bullet = Instantiate(skill1Prefab, spawnPosition, Quaternion.LookRotation(bulletDirection));
            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();

            if (bulletRb != null)
            {
                bulletRb.linearVelocity = bulletDirection * skill1Speed;
            }

            angle += angleStep;
        }
    }

    private Vector3 GetShootDirection()
    {
        if (shootMode == ShootMode.ClosestEnemy)
        {
            Transform targetEnemy = FindClosestEnemy();
            if (targetEnemy != null)
            {
                Vector3 dir = targetEnemy.position - transform.position;
                dir.y = 0;
                return dir.normalized;
            }
            else
            {
                Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
                return randomDir;
            }
        }
        else
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            float rayDistance;

            if (groundPlane.Raycast(ray, out rayDistance))
            {
                Vector3 targetPoint = ray.GetPoint(rayDistance);
                Vector3 dir = targetPoint - transform.position;
                dir.y = 0;
                return dir.normalized;
            }
            return transform.forward;
        }
    }

    private Transform FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform closestEnemy = null;
        float minDistance = shootingRange;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestEnemy = enemy.transform;
            }
        }

        return closestEnemy;
    }

    private void UpdateShootIndicator(Vector3 shootDirection)
    {
        if (shootIndicatorInstance)
        {
            shootIndicatorInstance.position = transform.position + Vector3.down * 0.5f;
            shootIndicatorInstance.rotation = Quaternion.LookRotation(new Vector3(shootDirection.x, 0, shootDirection.z));
        }
    }

    // --- Example Usage of Stats ---

    public float GetBulletDamage()
    {
        float finalDamage = bulletDamage;
        // Check for critical hit
        if (Random.Range(0f, 100f) < critChance)
        {
            finalDamage *= critDamageMultiplier;
            Debug.Log("Critical Hit!");
        }
        return finalDamage;
    }

    public float GetArmorPenetration()
    {
        return armorPenetration;
    }

    public float GetArmor()
    {
        return armor;
    }

    public void TakeDamage(int incomingDamage)
    {
        int damageTaken = Mathf.Max(1, incomingDamage - (int)armor);
        currentHealth -= damageTaken;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator HealthRegenRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            if (healthRegenRate > 0 && currentHealth < maxHealth)
            {
                float healAmountFloat = (healthRegenRate / 100f) * maxHealth;
                int healAmountInt = Mathf.FloorToInt(healAmountFloat);

                if (healAmountInt > 0)
                {
                    Heal(healAmountInt);
                }
            }
        }
    }

    private void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"Healed {amount}. Current Health: {currentHealth}");
    }

    private void Die()
    {
        Debug.Log("Player has died.");
        // Add death logic here
    }

    // --- End Example Usage ---

    // --- Public methods for upgrades ---
    public void AddGold(int amount)
    {
        playerGold += amount;
        Debug.Log($"Gold added: {amount}. Current Gold: {playerGold}");
    }

    public bool SpendGold(int amount)
    {
        if (playerGold >= amount)
        {
            playerGold -= amount;
            Debug.Log($"Gold spent: {amount}. Current Gold: {playerGold}");
            return true;
        }
        else
        {
            Debug.Log("Not enough gold!");
            return false;
        }
    }

    public void IncreaseBulletDamage(float amount)
    {
        Debug.Log($"Bullet Damage Increased by {amount}");
    }

    public void IncreaseMovementSpeed(float percentage)
    {
        walkSpeed *= (1 + percentage / 100f);
        sprintSpeed *= (1 + percentage / 100f);
        Debug.Log($"Movement Speed Increased by {percentage}%");
    }
    // --- End Public methods ---

    // --- In-Game Currency & Upgrades ---
    public void AddInGameCurrency(int amount)
    {
        inGameCurrency += amount;
        Debug.Log($"Added {amount} currency. Total: {inGameCurrency}");
    }

    public bool SpendInGameCurrency(int amount)
    {
        if (inGameCurrency >= amount)
        {
            inGameCurrency -= amount;
            Debug.Log($"Spent {amount} currency. Remaining: {inGameCurrency}");
            return true;
        }
        else
        {
            Debug.Log("Not enough in-game currency!");
            return false;
        }
    }

    public void ApplyUpgrade(IngameUpgradeData upgradeData)
    {
        if (upgradeData == null) return;

        int currentLevel = 0;
        acquiredUpgrades.TryGetValue(upgradeData.upgradeID, out currentLevel);

        if (currentLevel >= upgradeData.maxLevel)
        {
            Debug.Log($"Upgrade {upgradeData.displayName} is already at max level ({currentLevel}).");
            return;
        }

        currentLevel++;
        acquiredUpgrades[upgradeData.upgradeID] = currentLevel;
        Debug.Log($"Applied upgrade: {upgradeData.displayName} (Level {currentLevel})");

        switch (upgradeData.upgradeType)
        {
            case IngameUpgradeType.StatBoost:
                ApplyStatBoost(upgradeData.statToModify, upgradeData.valuePerLevel, upgradeData.isPercentage);
                break;
            case IngameUpgradeType.NewWeapon:
                Debug.LogWarning("NewWeapon logic not fully implemented yet.");
                break;
            case IngameUpgradeType.WeaponUpgrade:
                Debug.LogWarning("WeaponUpgrade logic not fully implemented yet.");
                break;
        }
    }

    private void ApplyStatBoost(string statName, float value, bool isPercentage)
    {
        if (string.IsNullOrEmpty(statName))
        {
            Debug.LogWarning("ApplyStatBoost called with an empty or null stat name.");
            return;
        }

        FieldInfo field = this.GetType().GetField(statName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            if (field.FieldType == typeof(float))
            {
                float currentValue = (float)field.GetValue(this);
                float newValue;
                if (isPercentage)
                {
                    newValue = currentValue * (1 + value / 100f);
                }
                else
                {
                    newValue = currentValue + value;
                }
                field.SetValue(this, newValue);
                Debug.Log($"Stat '{statName}' updated to {newValue}");
            }
            else if (field.FieldType == typeof(int))
            {
                int currentValue = (int)field.GetValue(this);
                int newValue;
                if (isPercentage)
                {
                    newValue = currentValue + Mathf.FloorToInt(currentValue * (value / 100f));
                }
                else
                {
                    newValue = currentValue + (int)value;
                }
                field.SetValue(this, newValue);
                Debug.Log($"Stat '{statName}' updated to {newValue}");
            }
            else
            {
                Debug.LogWarning($"Stat '{statName}' has an unsupported type: {field.FieldType}");
            }
        }
        else
        {
            Debug.LogWarning($"Stat '{statName}' not found in PlayerController.");
        }
    }

    public void ResetIngameProgress()
    {
        inGameCurrency = 0;
        acquiredUpgrades.Clear();

        walkSpeed = initialWalkSpeed;
        sprintSpeed = initialSprintSpeed;
        jumpForce = initialJumpForce;
        bulletCount = initialBulletCount;
        bulletCooldown = initialBulletCooldown;
        skillCooldown = initialSkillCooldown;
        shootingRange = initialShootingRange;
        bulletDamage = initialBulletDamage;
        critChance = initialCritChance;
        critDamageMultiplier = initialCritDamageMultiplier;
        maxHealth = initialMaxHealth;
        armorPenetration = initialArmorPenetration;
        healthRegenRate = initialHealthRegenRate;
        armor = initialArmor;

        currentHealth = initialMaxHealth;

        Debug.Log("In-game progress and stats reset.");
    }
    // --- End In-Game Currency & Upgrades ---
}
