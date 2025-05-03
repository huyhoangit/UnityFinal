using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField] private EnemyController enemyPrefab;
    [SerializeField] private Transform player;
    [SerializeField] private  Vector3 mapBounds;
    [SerializeField] private int enemyCount = 50; 
    [SerializeField] private  float minSpawnDistance = 3.0f;
    [SerializeField] private float spawnCheckRadius = 2.0f;

    void Start()
    {
        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();
            EnemyController enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            enemy.SetTarget(player);
            //enemy.GetComponent<EnemyController>().player = player;
        }
    }

    Vector3 GetRandomSpawnPosition()
    {
        Vector3 randomPosition;
        int attempts = 10;

        while (attempts-- > 0)
        {
            randomPosition = new Vector3(
                Random.Range(-mapBounds.x / 2, mapBounds.x / 2),
                0,
                Random.Range(-mapBounds.z / 2, mapBounds.z / 2)
            );

            if (player == null || Vector3.Distance(randomPosition, player.position) >= minSpawnDistance)
            {
                return randomPosition;
            }
        }

        return Vector3.zero;
    }
}
