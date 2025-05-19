using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] public float walkSpeed = 5f;
    [SerializeField] public float sprintSpeed = 10f;
    [SerializeField] public float jumpForce = 5f;

    [Header("Shooting")]
    [SerializeField] public int bulletCount = 6;
    [SerializeField] public float bulletCooldown = 2f;
    [SerializeField] public float skillCooldown = 5f;
    [SerializeField] public float shootingRange = 10f;
    [SerializeField] public float bulletDamage = 10f;
    [SerializeField] public float critChance = 5f; // Percentage
    [SerializeField] public float critDamageMultiplier = 1.5f;
    [SerializeField] public float armorPenetration = 0f;
    [SerializeField] public float bulletSpeed = 10f;
    [SerializeField] public float skillSpeed = 15f;

    [Header("Health & Defense")]
    [SerializeField] public int maxHealth = 100;
    [SerializeField] public float healthRegenRate = 0f; 
    [SerializeField] public float armor = 0f;

    [Header("Health System Reference")]
    [SerializeField] private HealthSystem healthSystem;

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
    private float initialArmorPenetration;
    private float initialBulletSpeed;
    private float initialSkillSpeed;
    private int initialMaxHealth;
    private float initialHealthRegenRate;
    private float initialArmor;

    private void Awake()
    {
        StoreInitialStats();
        if (healthSystem == null) healthSystem = GetComponent<HealthSystem>();
        if (healthSystem == null) Debug.LogError("PlayerStats: HealthSystem not assigned!");
        healthSystem.SetMaxHealth(initialMaxHealth);
    }

    private void Start()
    {
        StartCoroutine(HealthRegenRoutine());
    }

    private void StoreInitialStats()
    {
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
        initialBulletSpeed = bulletSpeed;
        initialSkillSpeed = skillSpeed;
        initialHealthRegenRate = healthRegenRate;
        initialArmor = armor;
        Debug.Log("Initial player stats stored.");
    }

    public void ResetStats()
    {
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
        bulletSpeed = initialBulletSpeed;
        skillSpeed = initialSkillSpeed;
        healthRegenRate = initialHealthRegenRate;
        armor = initialArmor;

        if (healthSystem != null)
        {
            healthSystem.SetMaxHealth(initialMaxHealth);
            healthSystem.Heal(initialMaxHealth);
        }
        Debug.Log("Player stats reset to initial values.");
    }

    public void TakeDamage(int incomingDamage)
    {
        int damageTaken = Mathf.Max(1, incomingDamage - (int)armor);
        Debug.Log($"Player took {incomingDamage} incoming damage, reduced to {damageTaken} by armor.");

        if (healthSystem != null)
        {
            healthSystem.TakeDamage(damageTaken);
            if (healthSystem.CurrentHealth <= 0)
            {
                Die();
            }
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || healthSystem == null) return;
        healthSystem.Heal(amount);
    }

    public float GetBulletDamage()
    {
        float finalDamage = bulletDamage;
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

    private IEnumerator HealthRegenRoutine()
    {
        while (true)
        {
            if (healthRegenRate > 0 && healthSystem != null && healthSystem.CurrentHealth < maxHealth)
            {
                float healAmountFloat = (healthRegenRate / 100f) * maxHealth;
                int healAmountInt = Mathf.FloorToInt(healAmountFloat);
                if (healAmountInt > 0)
                {
                    healthSystem.Heal(healAmountInt);
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    private void Die()
    {
        Debug.Log("Player Died! (PlayerStats)");
        GameController gameManager = FindFirstObjectByType<GameController>();
        if (gameManager != null)
        {
            gameManager.ShowLosePanel();
        }
    }

    public void OnStatChanged(string statName, object newValue)
    {
        if (statName == "maxHealth" && healthSystem != null)
        {
            int newMax = (int)newValue;
            healthSystem.SetMaxHealth(newMax);
            Debug.Log($"MaxHealth changed. Current health clamped: {healthSystem.CurrentHealth}/{newMax}");
        }
        else if (statName == "bulletSpeed" || statName == "skillSpeed")
        {
            Debug.Log($"Stat '{statName}' changed to {newValue}. Bullets/Skills updated.");
        }
    }
}