using UnityEngine;

public class Skill1 : Bullet
{
    [SerializeField] private float explosionRadius = 2f; 

    public override void Initialize(PlayerStats stats, float customSpeed = -1f)
    {
        base.Initialize(stats, customSpeed);
        damage *= 1.5f;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Collider[] hitEnemies = Physics.OverlapSphere(transform.position, explosionRadius, LayerMask.GetMask("Enemy"));
            foreach (Collider enemyCollider in hitEnemies)
            {
                EnemyController enemy = enemyCollider.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.TakeDamage((int)damage, armorPenetration);
                }
            }
            Destroy(gameObject);
        }
    }
}