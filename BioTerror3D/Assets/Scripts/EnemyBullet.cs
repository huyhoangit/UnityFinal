using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private int damage = 10; 
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 5f; 

    private Vector3 direction; 

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    public void Initialize(Vector3 shootDirection)
    {
        direction = shootDirection.normalized;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Debug.Log($"{gameObject.name} hit Player for {damage} damage.");
            }
            Destroy(gameObject); 
        }
    }
}