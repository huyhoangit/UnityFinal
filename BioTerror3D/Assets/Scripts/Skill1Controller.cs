using UnityEngine;

public class Skill1 : Bullet
{
    protected override void Start()
    {
        base.Start(); // Gọi Start() của Bullet
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * speed;
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
        }
        // KHÔNG hủy đạn khi va chạm
    }
}
