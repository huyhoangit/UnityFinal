using UnityEngine;

public class RangedEnemy : EnemyController
{
    [SerializeField] private GameObject bulletPrefab; 

    protected override void InitializeStats()
    {
        stats.InitializeRangedStats();
    }

    protected override void PerformAttack()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning($"{gameObject.name}: Bullet prefab missing!");
            return;
        }

        if (player == null) return;

        Vector3 direction = (player.position - transform.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, transform.position + direction * 1f, Quaternion.LookRotation(direction));
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Initialize(null, 8f);
            bulletScript.GetComponent<Collider>().tag = "EnemyBullet"; 
        }
    }
}