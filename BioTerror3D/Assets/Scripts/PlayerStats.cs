using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script manages all player stats and related logic like health, damage, and regeneration.
public class PlayerStats : MonoBehaviour
{
    // --- Stat Variables (Configurable in Inspector) ---
    [Header("Movement")]
    [SerializeField] public float walkSpeed = 5f;
    [SerializeField] public float sprintSpeed = 10f;
    [SerializeField] public float jumpForce = 5f;

    [Header("Shooting")]
    [SerializeField] public int bulletCount = 6;
    [SerializeField] public float bulletCooldown = 2f;
    [SerializeField] public float skillCooldown = 5f; // Assuming skill cooldown is a player stat
    [SerializeField] public float shootingRange = 10f;
    [SerializeField] public float bulletDamage = 10f;
    [SerializeField] public float critChance = 5f; // Percentage
    [SerializeField] public float critDamageMultiplier = 1.5f; // e.g., 1.5 = 150% damage
    [SerializeField] public float armorPenetration = 0f;

    [Header("Health & Defense")]
    [SerializeField] public int maxHealth = 100;
    [SerializeField] public float healthRegenRate = 0f; // Percentage of maxHealth per second
    [SerializeField] public float armor = 0f;

    // --- Current State ---
    public int currentHealth { get; private set; } // Read-only from outside

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

    private void Awake()
    {
        StoreInitialStats();
        currentHealth = initialMaxHealth; // Initialize health
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
        healthRegenRate = initialHealthRegenRate;
        armor = initialArmor;

        // Reset current health to the reset max health
        currentHealth = initialMaxHealth;

        Debug.Log("Player stats reset to initial values.");
        // Update UI if needed
        // UIController.Instance?.UpdateHealth(currentHealth, maxHealth);
    }

    // --- Stat Interaction Methods ---

    public void TakeDamage(int incomingDamage)
    {
        // Apply Armor Reduction
        int damageTaken = Mathf.Max(1, incomingDamage - (int)armor); // Flat reduction

        currentHealth -= damageTaken;
        Debug.Log($"Player took {incomingDamage} incoming damage, reduced to {damageTaken} by armor. Current health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            currentHealth = 0; // Clamp health at 0
            Die();
        }
        // Update health UI
        // UIController.Instance?.UpdateHealth(currentHealth, maxHealth);
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return; // Don't heal negative amounts

        int previousHealth = currentHealth;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        if (currentHealth > previousHealth)
        {
             Debug.Log($"Healed {currentHealth - previousHealth}. Current Health: {currentHealth}/{maxHealth}");
             // Update health UI
             // UIController.Instance?.UpdateHealth(currentHealth, maxHealth);
        }
    }

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

    // Called by Enemy script when taking damage
    public float GetArmorPenetration()
    {
        return armorPenetration;
    }

    // Optional getter if needed elsewhere
    public float GetArmor()
    {
        return armor;
    }

    private IEnumerator HealthRegenRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f); // Wait for 1 second

            if (healthRegenRate > 0 && currentHealth < maxHealth)
            {
                // Calculate heal amount based on percentage of maxHealth
                float healAmountFloat = (healthRegenRate / 100f) * maxHealth;
                int healAmountInt = Mathf.FloorToInt(healAmountFloat); // Or CeilingToInt

                if (healAmountInt > 0)
                {
                    Heal(healAmountInt);
                }
            }
        }
    }

    private void Die()
    {
        Debug.Log("Player Died! (PlayerStats)");
        // Handle game over, respawn, etc.
        // Maybe disable player controls via PlayerController, play death animation
        // gameObject.SetActive(false); // Deactivating the whole object might stop other scripts
        // Consider notifying a GameManager
        // FindObjectOfType<GameManager>()?.PlayerDied();
    }

    // --- Method called by PlayerController's ApplyStatBoost via Reflection ---
    // This method is intentionally left empty. ApplyStatBoost in PlayerController
    // will directly modify the public/serialized fields of this component.
    // We might add specific logic here later if a stat change needs immediate complex updates.
    // For example, if maxHealth increases, we might want to call Heal(difference) here.
    public void OnStatChanged(string statName, object newValue)
    {
         // Example: Update health if maxHealth changed
         if (statName == "maxHealth")
         {
             // Ensure currentHealth doesn't exceed new maxHealth
             // Optionally heal the player by the difference if desired
             int newMax = (int)newValue;
             // int difference = newMax - currentHealth; // This isn't quite right
             // Need the *old* maxHealth to calculate difference correctly if healing
             // For simplicity, just clamp current health for now.
             currentHealth = Mathf.Min(currentHealth, newMax);

             Debug.Log($"MaxHealth changed. Current health clamped: {currentHealth}/{newMax}");
             // Update UI
             // UIController.Instance?.UpdateHealth(currentHealth, newMax);
         }
         // Add other specific logic if needed for other stats
    }
}
