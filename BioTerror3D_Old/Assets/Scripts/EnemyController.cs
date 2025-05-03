using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    private Transform player;
    private NavMeshAgent agent;

    [Header("Health Settings")]
    public int maxHealth = 50;
    private int currentHealth;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (player != null)
        {
            agent.SetDestination(player.position);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log(gameObject.name + " bị trúng đạn! Máu còn: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    void Die()
    {
        Debug.Log(gameObject.name + " đã bị tiêu diệt!");
        Destroy(gameObject);
    }
    public void SetTarget(Transform main)
    {
        player = main;  
    }
}
