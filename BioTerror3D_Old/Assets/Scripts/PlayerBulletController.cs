using UnityEngine;

public class PlayerBullet : Bullet
{
    protected override void Start()
    {
        base.Start();
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * speed * 1.5f; 
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            Destroy(gameObject); 
        }
    }
}
