using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public class PlayerController : MonoBehaviour
{
    private Rigidbody playerRb;
    private Animator playerAnimator;

    [Header("References")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Shooting Prefabs")]
    [SerializeField] private GameObject playerBulletPrefab;
    [SerializeField] private GameObject skill1Prefab;

    [Header("Indicator Settings")]
    [SerializeField] private Transform shootIndicatorPrefab;
    private Transform shootIndicatorInstance;

    [Header("Player Gold & Progression")]
    public int playerGold = 0;
    public int inGameCurrency = 0;
    public Dictionary<string, int> acquiredUpgrades = new Dictionary<string, int>();

    private enum ShootMode { ClosestEnemy, MouseDirection }
    private ShootMode shootMode = ShootMode.ClosestEnemy;

    private void Awake()
    {
        playerRb = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();

        if (playerRb == null) Debug.LogError("Rigidbody missing!");
        if (playerAnimator == null) Debug.LogError("Animator missing!");
        if (playerStats == null) playerStats = GetComponent<PlayerStats>();
        if (playerStats == null) Debug.LogError("PlayerStats missing!");
    }

    private void Start()
    {
        Physics.gravity = new Vector3(0, -9.81f, 0);
        StartCoroutine(ShootBulletsRoutine());
        StartCoroutine(ShootSkill1Routine());

        if (shootIndicatorPrefab)
        {
            shootIndicatorInstance = Instantiate(shootIndicatorPrefab, transform.position, Quaternion.identity);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && IsOnGround())
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

        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? playerStats.sprintSpeed : playerStats.walkSpeed;

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
        playerRb.linearVelocity = new Vector3(playerRb.linearVelocity.x, playerStats.jumpForce, playerRb.linearVelocity.z);
        SetOnGround(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            SetOnGround(true);
        }
    }

    private bool isOnGround = true;
    private bool IsOnGround() => isOnGround;
    private void SetOnGround(bool value) => isOnGround = value;

    private IEnumerator ShootBulletsRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(playerStats.bulletCooldown);
            ShootPlayerBullets();
        }
    }

    private IEnumerator ShootSkill1Routine()
    {
        while (true)
        {
            yield return new WaitForSeconds(playerStats.skillCooldown);
            ShootSkill1();
        }
    }

    private void ShootPlayerBullets()
    {
        if (playerBulletPrefab == null)
        {
            Debug.LogWarning("Player bullet prefab is missing!");
            return;
        }

        Vector3 shootDirection = GetShootDirection();
        if (shootDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(shootDirection.x, 0, shootDirection.z));
            transform.rotation = targetRotation;
        }

        UpdateShootIndicator(shootDirection);

        float angleStep = 15f;
        float startAngle = -((playerStats.bulletCount - 1) / 2f) * angleStep;

        for (int i = 0; i < playerStats.bulletCount; i++)
        {
            float currentAngle = startAngle + i * angleStep;
            Quaternion rotation = Quaternion.Euler(0, currentAngle, 0);
            Vector3 bulletDir = rotation * shootDirection;

            GameObject bullet = Instantiate(playerBulletPrefab, transform.position + bulletDir * 1.5f, Quaternion.LookRotation(bulletDir));
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.Initialize(playerStats, playerStats.bulletSpeed);
            }
            else
            {
                Debug.LogWarning("Bullet script missing on bullet prefab!");
            }
        }
    }

    private void ShootSkill1()
    {
        if (skill1Prefab == null)
        {
            Debug.LogWarning("Skill1 prefab is missing!");
            return;
        }

        float angleStep = 360f / playerStats.bulletCount;
        float angle = 0f;

        for (int i = 0; i < playerStats.bulletCount; i++)
        {
            float bulletDirX = Mathf.Cos(angle * Mathf.Deg2Rad);
            float bulletDirZ = Mathf.Sin(angle * Mathf.Deg2Rad);

            Vector3 bulletDirection = new Vector3(bulletDirX, 0, bulletDirZ).normalized;
            Vector3 spawnPosition = transform.position + bulletDirection * 1.5f;

            GameObject bullet = Instantiate(skill1Prefab, spawnPosition, Quaternion.LookRotation(bulletDirection));
            Skill1 skillScript = bullet.GetComponent<Skill1>();
            if (skillScript != null)
            {
                skillScript.Initialize(playerStats, playerStats.skillSpeed);
            }
            else
            {
                Debug.LogWarning("Skill1 script missing on skill prefab!");
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
        float minDistance = playerStats.shootingRange;

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

    public float GetBulletDamage() => playerStats.GetBulletDamage();
    public float GetArmorPenetration() => playerStats.GetArmorPenetration();
    public float GetArmor() => playerStats.GetArmor();
    public void TakeDamage(int incomingDamage) => playerStats.TakeDamage(incomingDamage);

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

        FieldInfo field = playerStats.GetType().GetField(statName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            if (field.FieldType == typeof(float))
            {
                float currentValue = (float)field.GetValue(playerStats);
                float newValue = isPercentage ? currentValue * (1 + value / 100f) : currentValue + value;
                field.SetValue(playerStats, newValue);
                playerStats.OnStatChanged(statName, newValue);
                Debug.Log($"Stat '{statName}' updated to {newValue}");
            }
            else if (field.FieldType == typeof(int))
            {
                int currentValue = (int)field.GetValue(playerStats);
                int newValue = isPercentage ? currentValue + Mathf.FloorToInt(currentValue * (value / 100f)) : currentValue + (int)value;
                field.SetValue(playerStats, newValue);
                playerStats.OnStatChanged(statName, newValue);
                Debug.Log($"Stat '{statName}' updated to {newValue}");
            }
            else
            {
                Debug.LogWarning($"Stat '{statName}' has an unsupported type: {field.FieldType}");
            }
        }
        else
        {
            Debug.LogWarning($"Stat '{statName}' not found in PlayerStats.");
        }
    }

    public void ResetIngameProgress()
    {
        inGameCurrency = 0;
        acquiredUpgrades.Clear();
        playerStats.ResetStats();
        Debug.Log("In-game progress and stats reset.");
    }
}