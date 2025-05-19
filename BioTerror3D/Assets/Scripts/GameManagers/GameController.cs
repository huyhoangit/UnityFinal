using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct EnemySpawnInfo
{
    public EnemyController enemyPrefab;
    public int count;
}

[System.Serializable]
public class WaveData
{
    public List<EnemySpawnInfo> enemySpawns;
    public List<Vector3> spawnPoints;
}

public class GameController : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private EnemyController enemyPrefab;
    [SerializeField] private float enemyHeightOffset = 1f;

    [Header("Wave Settings")]
    [SerializeField] private List<WaveData> waves;
    [SerializeField] private float delayBetweenSpawns = 0.5f;

    [Header("Map Settings")]
    [SerializeField] private Transform player; 

    [Header("Shop Settings")]
    [SerializeField] private IngameShopManager ingameShopManager; 
    [SerializeField] private int wavesPerShop = 3; 

    [SerializeField] private GameObject WinPanel;
    [SerializeField] private GameObject LosePanel;

    private int currentWave = 0;
    private bool isSpawning = false;
    private int enemiesAlive = 0;

    private void Start()
    {
        HideWinPanel();
        HideLosePanel();

        PlayerController playerController = FindFirstObjectByType<PlayerController>();
        if (playerController != null)
        {
            playerController.ResetIngameProgress();
        }
        else
        {
            Debug.LogError("PlayerController not found in scene!");
        }

        if (waves == null || waves.Count == 0)
        {
            Debug.LogError("No waves configured in GameController!");
            return;
        }

        StartCoroutine(SpawnWave(currentWave));
    }

    private void Update()
    {
        #region  dev tools
        // Dev tools
        PlayerController playerController = FindFirstObjectByType<PlayerController>();
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("[DEV] Clear current wave!");
            ClearCurrentWave();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("[DEV] Force Win Game!");
            ShowWinPanel();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("[DEV] Force Lose Game!");
            playerController?.TakeDamage(1000); 
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
    {
        Debug.Log("[DEV] Add 100 currency!");
        if (playerController != null)
        {
            playerController.AddInGameCurrency(100);
        }
        else
        {
            Debug.LogWarning("PlayerController not found when trying to add currency.");
        }
    }
        #endregion
    }

    private IEnumerator SpawnWave(int waveIndex)
    {
        if (waveIndex >= waves.Count)
        {
            Debug.Log("All waves completed. Game Over or Victory!");
            ShowWinPanel();
            yield break;
        }

        Debug.Log($"Spawning Wave {waveIndex + 1}...");
        isSpawning = true;
        WaveData wave = waves[waveIndex];
        enemiesAlive = 0;

        if (wave.enemySpawns == null || wave.enemySpawns.Count == 0)
        {
            Debug.LogWarning($"Wave {waveIndex + 1} has no enemy spawns configured!");
            isSpawning = false;
            yield break;
        }

        foreach (var spawnInfo in wave.enemySpawns)
        {
            enemiesAlive += spawnInfo.count;
        }

        foreach (var spawnInfo in wave.enemySpawns)
        {
            if (spawnInfo.enemyPrefab == null)
            {
                Debug.LogWarning("Enemy prefab missing in spawn info!");
                enemiesAlive -= spawnInfo.count;
                continue;
            }

            for (int i = 0; i < spawnInfo.count; i++)
            {
                Vector3 spawnPosition = GetRandomSpawnPoint(wave.spawnPoints);
                if (spawnPosition == Vector3.zero)
                {
                    Debug.LogWarning($"Failed to spawn {spawnInfo.enemyPrefab.name}: No valid spawn point.");
                    enemiesAlive--;
                    continue;
                }

                EnemyController enemy = Instantiate(spawnInfo.enemyPrefab, spawnPosition, Quaternion.identity);
                enemy.SetTarget(player);
                yield return new WaitForSeconds(delayBetweenSpawns);
            }
        }

        isSpawning = false;
        Debug.Log($"Wave {waveIndex + 1} spawned with {enemiesAlive} enemies.");
    }

    public void HandleEnemyDeath()
    {
        enemiesAlive--;
        Debug.Log($"Enemy died. Enemies remaining: {enemiesAlive}");

        if (enemiesAlive <= 0 && !isSpawning)
        {
            Debug.Log($"Wave {currentWave + 1} completed.");

            if (currentWave + 1 >= waves.Count)
            {
                Debug.Log("Final wave completed. Showing Win Panel!");
                ShowWinPanel();
                return;
            }

            bool shouldOpenShop = (currentWave + 1) % wavesPerShop == 0;

            if (shouldOpenShop && ingameShopManager != null)
            {
                Debug.Log("Opening In-game Shop...");
                ingameShopManager.OpenShop();
            }
            else
            {
                if (ingameShopManager == null)
                {
                    Debug.LogWarning("IngameShopManager not assigned. Skipping shop.");
                }
                StartNextWave();
            }
        }
    }

    public void StartNextWaveAfterShop()
    {
        Debug.Log("Starting next wave after shop closed.");
        StartNextWave();
    }

    private void StartNextWave()
    {
        currentWave++;
        if (currentWave < waves.Count)
        {
            StartCoroutine(SpawnWave(currentWave));
        }
        else
        {
            Debug.Log("All waves completed. Game Over or Victory!");
            ShowWinPanel();
        }
    }

    private Vector3 GetRandomSpawnPoint(List<Vector3> spawnPoints)
    {
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogError("No spawn points configured in wave!");
            return Vector3.zero;
        }

        int attempts = 10;
        while (attempts-- > 0)
        {
            Vector3 potentialPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
            NavMeshHit hit;

            if (NavMesh.SamplePosition(potentialPoint, out hit, 2.0f, NavMesh.AllAreas))
            {
                return hit.position + Vector3.up * enemyHeightOffset;
            }
        }

        Debug.LogWarning("No valid spawn point found on NavMesh after multiple attempts.");
        return Vector3.zero;
    }

    public void ShowWinPanel()
    {
        FindFirstObjectByType<IngameMenuController>().OnGameWin();
        WinPanel.SetActive(true);
        Time.timeScale = 0f; 
    }

    public void HideWinPanel()
    {
        WinPanel.SetActive(false);
        Time.timeScale = 1f; 
    }

    public void ShowLosePanel()
    {
        FindFirstObjectByType<IngameMenuController>().OnGameLose();
        LosePanel.SetActive(true);
        Time.timeScale = 0f; 
    }

    public void HideLosePanel()
    {
        LosePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void ClearCurrentWave()
    {
        EnemyController[] enemies = FindObjectsOfType<EnemyController>();
        foreach (var enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }
        enemiesAlive = 0;
        HandleEnemyDeath();
    }
}