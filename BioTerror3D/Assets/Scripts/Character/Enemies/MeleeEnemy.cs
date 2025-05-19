using UnityEngine;

public class MeleeEnemy : EnemyController
{
    protected override void InitializeStats()
    {
        stats.InitializeMeleeStats();
    }

    protected override void PerformAttack()
    {
        if (playerController != null)
        {
            playerController.TakeDamage(stats.attackDamage);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: Cannot attack, PlayerController is null!");
        }
    }
}