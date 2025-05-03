using UnityEngine;
using UnityEngine.AI; // Add this namespace
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct EnemySpawnInfo // Struct mới
{
    public EnemyController enemyPrefab; // Prefab của loại enemy
    public int count;                   // Số lượng của loại enemy này
}

[System.Serializable]
public class WaveData
{
    public List<EnemySpawnInfo> enemySpawns; // Thay thế bằng danh sách này
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
    [SerializeField] private IngameShopManager ingameShopManager; // Kéo IngameShopManager vào đây
    [SerializeField] private int wavesPerShop = 3; // Mở shop sau mỗi 3 wave

    private int currentWave = 0;
    private bool isSpawning = false;
    private int enemiesAlive = 0;

    // Gọi hàm Reset của PlayerController khi bắt đầu game mới (ví dụ trong Start hoặc hàm riêng)
    void Start()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>(); // Use FindFirstObjectByType
        if (player != null)
        {
            player.ResetIngameProgress(); // Reset nâng cấp tạm thời khi game bắt đầu
        }
        else
        {
            Debug.LogError("PlayerController not found on Start!");
        }

        StartCoroutine(SpawnWave(currentWave));
    }

    void Update()
    {
        // Đoạn xử lý phím N đã bị loại bỏ
    }

    private IEnumerator SpawnWave(int waveIndex)
    {
        if (waveIndex >= waves.Count)
        {
            Debug.Log("Đã hoàn thành tất cả các wave.");
            yield break;
        }

        Debug.Log($"Spawning Wave {waveIndex + 1}...");

        isSpawning = true;
        WaveData wave = waves[waveIndex];
        enemiesAlive = 0; // Reset enemiesAlive
        // Tính tổng số enemy cần spawn trong wave này
        foreach (var spawnInfo in wave.enemySpawns)
        {
            enemiesAlive += spawnInfo.count;
        }

        // Duyệt qua từng loại enemy trong wave
        foreach (var spawnInfo in wave.enemySpawns)
        {
            // Spawn số lượng enemy cho loại này
            for (int i = 0; i < spawnInfo.count; i++)
            {
                Vector3 spawnPosition = GetRandomSpawnPoint(wave.spawnPoints);
                if (spawnPosition == Vector3.zero)
                {
                    Debug.LogWarning($"Không thể spawn enemy loại {spawnInfo.enemyPrefab.name} do không tìm được vị trí hợp lệ.");
                    enemiesAlive--; // Giảm số lượng nếu không spawn được
                    continue;
                }

                // Instantiate đúng prefab enemy
                EnemyController enemy = Instantiate(spawnInfo.enemyPrefab, spawnPosition, Quaternion.identity);
                enemy.SetTarget(player);
                // enemy.OnDeath += HandleEnemyDeath; // Nếu bạn dùng event

                yield return new WaitForSeconds(delayBetweenSpawns);
            }
        }

        isSpawning = false;
        Debug.Log($"Wave {waveIndex + 1} spawned with {enemiesAlive} enemies.");
    }

    public void HandleEnemyDeath() // Đảm bảo hàm này được gọi khi enemy chết
    {
        enemiesAlive--;
        Debug.Log($"[GameController] HandleEnemyDeath called. Enemies remaining: {enemiesAlive}"); // Log 1

        // --- THÊM LOG KIỂM TRA TRẠNG THÁI ---
        Debug.Log($"[GameController] Checking conditions: enemiesAlive = {enemiesAlive}, isSpawning = {isSpawning}");
        // --- KẾT THÚC LOG KIỂM TRA ---

        if (enemiesAlive <= 0 && !isSpawning)
        {
            Debug.Log($"[GameController] Wave {currentWave + 1} completed. Checking shop condition..."); // Log 2

            bool shouldOpenShop = (currentWave + 1) % wavesPerShop == 0;
            // --- THÊM LOG KIỂM TRA SHOP ---
            Debug.Log($"[GameController] Shop Check: currentWave = {currentWave}, wavesPerShop = {wavesPerShop}, shouldOpenShop = {shouldOpenShop}");
            // --- KẾT THÚC LOG KIỂM TRA SHOP ---

            if (shouldOpenShop)
            {
                if (ingameShopManager != null)
                {
                    Debug.Log("[GameController] Conditions met. Opening In-game Shop..."); // Log 4
                    ingameShopManager.OpenShop();
                }
                else
                {
                    Debug.LogWarning("[GameController] IngameShopManager not assigned. Skipping shop and starting next wave.");
                    StartNextWave(); // Bắt đầu wave tiếp theo nếu không có shop
                }
            }
            else
            {
                Debug.Log("[GameController] Shop conditions not met. Proceeding to next wave."); // Log 5: Thông báo không mở shop
                StartNextWave(); // Bắt đầu wave tiếp theo nếu không mở shop
            }
        }
        else if (enemiesAlive > 0)
        {
            Debug.Log($"[GameController] Wave {currentWave + 1} ongoing. Enemies alive: {enemiesAlive}"); // Log 6: Wave chưa xong
        }
        else if (isSpawning)
        {
            Debug.Log($"[GameController] Wave {currentWave + 1} still spawning. Enemies alive: {enemiesAlive}"); // Log 7: Đang spawn
        }
    }

    // Hàm này có thể được gọi từ IngameShopManager khi người chơi đóng shop
    public void StartNextWaveAfterShop()
    {
        Debug.Log("Starting next wave after shop closed.");
        StartNextWave();
    }

    private Vector3 GetRandomSpawnPoint(List<Vector3> spawnPoints)
    {
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogError("Danh sách điểm spawn trống trong wave.");
            return Vector3.zero;
        }

        int attempts = 10; // Limit attempts to find a valid point
        while (attempts-- > 0)
        {
            Vector3 potentialPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

            NavMeshHit hit;
            // Find the closest point on the NavMesh within a 2.0f radius
            if (NavMesh.SamplePosition(potentialPoint, out hit, 2.0f, NavMesh.AllAreas))
            {
                // Optional: Add checks for distance from player or other enemies here if needed
                return hit.position; // Return the valid position on the NavMesh
            }
        }

        Debug.LogWarning("Không tìm thấy điểm spawn hợp lệ trên NavMesh sau nhiều lần thử.");
        return Vector3.zero; // Return zero if no valid point found
    }

    public void StartNextWave()
    {
        currentWave++;
        if (currentWave < waves.Count)
        {
            StartCoroutine(SpawnWave(currentWave));
        }
        else
        {
            Debug.Log("Không còn wave nào nữa. Game Over or Victory!");
        }
    }
}
