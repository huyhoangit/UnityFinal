using UnityEngine;

public class PlayerBullet : Bullet
{
    public override void Initialize(PlayerStats stats, float customSpeed = -1f)
    {
        base.Initialize(stats, customSpeed >= 0f ? customSpeed * 1.5f : stats.bulletSpeed * 1.5f);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage((int)damage, armorPenetration);
            }
            Destroy(gameObject);
        }
    }
}