using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] protected float speed = 10f;
    [SerializeField] protected float lifetime = 3f;
    protected float damage;
    protected float armorPenetration;
    protected PlayerStats playerStats;

    public virtual void Initialize(PlayerStats stats, float customSpeed = -1f)
    {
        playerStats = stats;
        damage = stats.GetBulletDamage();
        armorPenetration = stats.GetArmorPenetration();
        if (customSpeed >= 0f)
        {
            speed = customSpeed;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * speed;
        }
    }

    protected virtual void Start()
    {
        Destroy(gameObject, lifetime);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage((int)damage, armorPenetration);
                Destroy(gameObject);
            }
        }
    }
}