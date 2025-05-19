using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Header("Health & Defense")]
    public int maxHealth = 50;
    public float armor = 0f;
    public int currentHealth { get; set; }

    [Header("Movement")]
    public float moveSpeed = 3.5f;

    [Header("Attack")]
    public int attackDamage = 10;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    public float lastAttackTime;

    [Header("Rewards")]
    public int currencyDropAmount = 5;

    public void InitializeMeleeStats()
    {
        maxHealth = 50;
        armor = 5f;
        moveSpeed = 3.5f;
        attackDamage = 15;
        attackRange = 2f;
        attackCooldown = 1.5f;
        currencyDropAmount = 5;
    }

    public void InitializeRangedStats()
    {
        maxHealth = 30;
        armor = 2f;
        moveSpeed = 5f;
        attackDamage = 8;
        attackRange = 10f;
        attackCooldown = 2f;
        currencyDropAmount = 4;
    }

    public void InitializeTankStats()
    {
        maxHealth = 100;
        armor = 15f;
        moveSpeed = 2f;
        attackDamage = 20;
        attackRange = 2f;
        attackCooldown = 2f;
        currencyDropAmount = 8;
    }

    public void InitializeArtilleryStats()
    {
        maxHealth = 40;
        armor = 3f;
        moveSpeed = 3f;
        attackDamage = 25;
        attackRange = 15f;
        attackCooldown = 3f;
        currencyDropAmount = 6;
    }
}