using UnityEngine;

public class TankEnemy : EnemyController
{
    protected override void InitializeStats()
    {
        stats.InitializeTankStats();
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