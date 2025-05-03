using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro; // Quan trọng: Thêm nếu dùng TextMeshPro
using UnityEngine.UI; // Quan trọng: Thêm nếu dùng UI cơ bản

public class IngameShopManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private GameObject shopUIPanel; // Panel chính chứa mọi thứ

    // --- Tham chiếu đến các UI Element cụ thể ---
    [Header("UI References")]
    [SerializeField] private List<UpgradeCardUI> upgradeCardUIs; // Danh sách chứa UI của 3 card
    [SerializeField] private TextMeshProUGUI currencyText;     // Text hiển thị tiền
    [SerializeField] private Button rerollButton;         // Nút Reroll
    [SerializeField] private TextMeshProUGUI rerollCostText; // Text hiển thị giá Reroll
    [SerializeField] private Button closeButton;          // Nút đóng shop
    // --- Kết thúc tham chiếu UI ---

    [Header("Shop Settings")]
    [SerializeField] private List<IngameUpgradeData> availableUpgradesPool; // Danh sách TẤT CẢ nâng cấp có thể xuất hiện
    [SerializeField] private int numberOfChoices = 3; // Số lượng lựa chọn hiển thị
    [SerializeField] private int baseRerollCost = 10;
    private int currentRerollCost;

    private List<IngameUpgradeData> currentChoices = new List<IngameUpgradeData>();
    private bool isClosingShop = false; // Thêm cờ này

    // --- Struct hoặc Class để nhóm các UI element của một card ---
    [System.Serializable]
    public class UpgradeCardUI // Đảm bảo bạn có class/struct này
    {
        public GameObject cardRoot; // GameObject cha của card (để bật/tắt)
        public Image iconImage;       // Image hiển thị icon nâng cấp
        public TextMeshProUGUI titleText; // Text hiển thị tên + level
        public TextMeshProUGUI descriptionText; // Text mô tả
        public TextMeshProUGUI costText; // Text giá tiền
        public Button buyButton;     // Nút "Mua"
    }
    // --- Kết thúc Struct ---

    void Start()
    {
        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>(); // Use FindFirstObjectByType

        if (shopUIPanel != null)
            shopUIPanel.SetActive(false); // Ẩn shop khi bắt đầu

        // Lọc bỏ các nâng cấp không hợp lệ khỏi pool nếu cần
        availableUpgradesPool = availableUpgradesPool.Where(u => u != null && !string.IsNullOrEmpty(u.upgradeID)).ToList();

        // Gán sự kiện cho các nút chính
        if (rerollButton != null) rerollButton.onClick.AddListener(RerollChoices);
        if (closeButton != null) closeButton.onClick.AddListener(CloseShop);
    }

    public void OpenShop()
    {
        isClosingShop = false; // Reset cờ khi mở shop

        if (playerController == null || shopUIPanel == null)
        {
            Debug.LogError("Shop cannot open. Missing references.");
            return;
        }

        currentRerollCost = baseRerollCost; // Reset giá reroll
        GenerateChoices();
        UpdateShopUI(); // Hiển thị các lựa chọn lên UI

        shopUIPanel.SetActive(true);
        Time.timeScale = 0f; // Tạm dừng game
        Debug.Log("Ingame Shop Opened.");
    }

    public void CloseShop()
    {
        if (isClosingShop) // Kiểm tra cờ trước khi thực hiện
        {
            Debug.LogWarning("CloseShop called again rapidly. Ignoring.");
            return;
        }
        isClosingShop = true; // Đặt cờ

        if (shopUIPanel != null)
            shopUIPanel.SetActive(false);
        Time.timeScale = 1f; // Tiếp tục game
        Debug.Log("Ingame Shop Closed.");

        // Tìm GameController và yêu cầu bắt đầu wave tiếp theo
        GameController gc = FindFirstObjectByType<GameController>(); // Hoặc dùng cách khác để lấy tham chiếu nếu có
        if (gc != null)
        {
            gc.StartNextWaveAfterShop(); // Gọi hàm mới trong GameController
        }
        else
        {
            Debug.LogError("Could not find GameController to start next wave after shop!");
        }
    }

    private void GenerateChoices()
    {
        currentChoices.Clear();
        // Log 1: Kiểm tra pool ban đầu
        Debug.Log($"[GenerateChoices] Initial availableUpgradesPool count: {availableUpgradesPool?.Count ?? 0}");

        if (availableUpgradesPool == null || availableUpgradesPool.Count == 0)
        {
            Debug.LogError("[GenerateChoices] availableUpgradesPool is empty or null!");
            return; // Không thể tạo lựa chọn nếu pool trống
        }

        List<IngameUpgradeData> possibleUpgrades = new List<IngameUpgradeData>(availableUpgradesPool);

        // Log 2: Kiểm tra trước khi lọc max level
        Debug.Log($"[GenerateChoices] possibleUpgrades count before filtering max level: {possibleUpgrades.Count}");
        if (playerController == null) Debug.LogError("[GenerateChoices] PlayerController is null, cannot check acquired upgrades!");


        // Loại bỏ các nâng cấp đã đạt max level khỏi danh sách có thể chọn
        if (playerController != null && playerController.acquiredUpgrades != null)
        {
            // Dùng vòng lặp ngược để xóa an toàn
            for (int i = possibleUpgrades.Count - 1; i >= 0; i--)
            {
                IngameUpgradeData data = possibleUpgrades[i];
                if (data == null)
                {
                     Debug.LogWarning($"[GenerateChoices] Found NULL data in possibleUpgrades at index {i}. Removing.");
                     possibleUpgrades.RemoveAt(i);
                     continue;
                }

                int currentLevel = 0;
                playerController.acquiredUpgrades.TryGetValue(data.upgradeID, out currentLevel);

                // --- THÊM LOG CHI TIẾT Ở ĐÂY ---
                Debug.Log($"[GenerateChoices] Checking Filter: '{data.displayName}' (ID: {data.upgradeID}) - Current Level: {currentLevel}, Max Level: {data.maxLevel}");
                // --- KẾT THÚC LOG CHI TIẾT ---

                if (currentLevel >= data.maxLevel)
                {
                    Debug.Log($"[GenerateChoices] Removing '{data.displayName}' because it reached max level ({currentLevel}/{data.maxLevel}).");
                    possibleUpgrades.RemoveAt(i);
                }
            }
        }

        // Log 3: Kiểm tra sau khi lọc max level
        Debug.Log($"[GenerateChoices] possibleUpgrades count after filtering max level: {possibleUpgrades.Count}");


        // Chọn ngẫu nhiên từ danh sách còn lại
        int count = Mathf.Min(numberOfChoices, possibleUpgrades.Count);
        for (int i = 0; i < count; i++)
        {
            if (possibleUpgrades.Count == 0)
            {
                 Debug.LogWarning("[GenerateChoices] No more possible upgrades to choose from.");
                 break; // Hết lựa chọn
            }

            int randomIndex = Random.Range(0, possibleUpgrades.Count);
            IngameUpgradeData chosenUpgrade = possibleUpgrades[randomIndex];
            if (chosenUpgrade != null)
            {
                currentChoices.Add(chosenUpgrade);
                 Debug.Log($"[GenerateChoices] Added choice: {chosenUpgrade.displayName}");
            } else {
                 Debug.LogWarning($"[GenerateChoices] Tried to add a NULL choice at random index {randomIndex}. Skipping.");
                 i--; // Thử lại nếu chọn phải null (hiếm khi xảy ra nếu pool đã được lọc)
            }
            possibleUpgrades.RemoveAt(randomIndex); // Xóa để không bị chọn lại
        }
        // Log 4: Kết quả cuối cùng
        Debug.Log($"[GenerateChoices] Final generated choices count: {currentChoices.Count}");
    }

    // Hàm này sẽ được gọi bởi nút "Mua" trên UI, với index của card được chọn
    public void PurchaseUpgrade(int choiceIndex)
    {
        if (choiceIndex < 0 || choiceIndex >= currentChoices.Count) return;

        IngameUpgradeData selectedUpgrade = currentChoices[choiceIndex];
        int currentLevel = 0;
        playerController.acquiredUpgrades.TryGetValue(selectedUpgrade.upgradeID, out currentLevel);
        int cost = selectedUpgrade.GetCost(currentLevel);

        Debug.Log($"Attempting to purchase {selectedUpgrade.displayName} (Level {currentLevel + 1}) for {cost} currency.");

        if (playerController.SpendInGameCurrency(cost))
        {
            playerController.ApplyUpgrade(selectedUpgrade);
            Debug.Log($"Purchase successful: {selectedUpgrade.displayName}");
            CloseShop(); // Đóng shop sau khi mua thành công
        }
        else
        {
            Debug.Log("Purchase failed: Not enough currency.");
            // Hiển thị thông báo lỗi trên UI (ví dụ: đổi màu text giá tiền)
        }
    }

    // Hàm này sẽ được gọi bởi nút "Reroll"
    public void RerollChoices()
    {
        Debug.Log($"Attempting to reroll for {currentRerollCost} currency.");
        if (playerController.SpendInGameCurrency(currentRerollCost))
        {
            currentRerollCost *= 2; // Tăng giá reroll cho lần sau (ví dụ)
            GenerateChoices();
            UpdateShopUI(); // Cập nhật UI với lựa chọn mới
            Debug.Log($"Reroll successful. Next reroll cost: {currentRerollCost}");
        }
        else
        {
            Debug.Log("Reroll failed: Not enough currency.");
            // Hiển thị thông báo lỗi trên UI
        }
    }

    // --- Hàm cập nhật giao diện shop ---
    private void UpdateShopUI()
    {
        Debug.Log("[IngameShopManager] Starting UpdateShopUI..."); // Log 1: Bắt đầu hàm

        // Kiểm tra tham chiếu cơ bản
        if (playerController == null) Debug.LogError("[IngameShopManager] PlayerController is NULL in UpdateShopUI!");
        if (currencyText == null) Debug.LogWarning("[IngameShopManager] CurrencyText reference is NULL!");
        if (rerollButton == null) Debug.LogWarning("[IngameShopManager] RerollButton reference is NULL!");
        if (rerollCostText == null) Debug.LogWarning("[IngameShopManager] RerollCostText reference is NULL!");
        if (upgradeCardUIs == null || upgradeCardUIs.Count == 0) Debug.LogWarning("[IngameShopManager] UpgradeCardUIs list is NULL or empty!");


        // Cập nhật hiển thị tiền
        if (currencyText != null && playerController != null)
        {
            currencyText.text = $"Currency: {playerController.inGameCurrency}";
            Debug.Log($"[IngameShopManager] Updated Currency Text: {currencyText.text}"); // Log 2: Cập nhật tiền
        }

        // Cập nhật nút Reroll
        if (rerollCostText != null)
        {
            rerollCostText.text = $"Reroll ({currentRerollCost})";
             Debug.Log($"[IngameShopManager] Updated Reroll Cost Text: {rerollCostText.text}"); // Log 3: Cập nhật giá reroll
        }
         if (rerollButton != null && playerController != null)
        {
             rerollButton.interactable = playerController.inGameCurrency >= currentRerollCost;
             Debug.Log($"[IngameShopManager] Reroll Button Interactable: {rerollButton.interactable}"); // Log 4: Cập nhật trạng thái nút reroll
        }


        // Cập nhật các card nâng cấp
        Debug.Log($"[IngameShopManager] Processing {upgradeCardUIs?.Count ?? 0} card UI slots for {currentChoices?.Count ?? 0} choices."); // Log 5: Số lượng card và lựa chọn
        for (int i = 0; i < upgradeCardUIs.Count; i++)
        {
             Debug.Log($"[IngameShopManager] Checking Card Slot {i}..."); // Log 6: Bắt đầu xử lý card i
             UpgradeCardUI cardUI = upgradeCardUIs[i];
             if (cardUI == null)
             {
                 Debug.LogError($"[IngameShopManager] Card UI at index {i} is NULL!");
                 continue;
             }
             if (cardUI.cardRoot == null) Debug.LogWarning($"[IngameShopManager] Card {i}: cardRoot is NULL!");


            if (i < currentChoices.Count) // Nếu có lựa chọn cho card này
            {
                IngameUpgradeData data = currentChoices[i];
                 if (data == null)
                 {
                     Debug.LogError($"[IngameShopManager] Choice data at index {i} is NULL!");
                     if (cardUI.cardRoot != null) cardUI.cardRoot.SetActive(false); // Ẩn card nếu data lỗi
                     continue;
                 }

                Debug.Log($"[IngameShopManager] Card {i}: Displaying Upgrade '{data.displayName}' (ID: {data.upgradeID})"); // Log 7: Thông tin nâng cấp

                int currentLevel = 0;
                playerController.acquiredUpgrades.TryGetValue(data.upgradeID, out currentLevel);
                int cost = data.GetCost(currentLevel);

                // Cập nhật thông tin lên UI
                if (cardUI.titleText != null) cardUI.titleText.text = $"{data.displayName} (Lv.{currentLevel + 1})"; else Debug.LogWarning($"[IngameShopManager] Card {i}: titleText is NULL!");
                if (cardUI.descriptionText != null) cardUI.descriptionText.text = data.GetFormattedDescription(currentLevel + 1); else Debug.LogWarning($"[IngameShopManager] Card {i}: descriptionText is NULL!");
                if (cardUI.costText != null) cardUI.costText.text = cost.ToString(); else Debug.LogWarning($"[IngameShopManager] Card {i}: costText is NULL!");
                if (cardUI.iconImage != null) cardUI.iconImage.sprite = data.icon; else Debug.LogWarning($"[IngameShopManager] Card {i}: iconImage is NULL!");

                // Cập nhật nút Mua
                if (cardUI.buyButton != null)
                {
                    cardUI.buyButton.onClick.RemoveAllListeners();
                    int choiceIndex = i;
                    cardUI.buyButton.onClick.AddListener(() => PurchaseUpgrade(choiceIndex));
                    cardUI.buyButton.interactable = playerController.inGameCurrency >= cost;
                     Debug.Log($"[IngameShopManager] Card {i}: Buy Button Interactable: {cardUI.buyButton.interactable}"); // Log 8: Trạng thái nút mua
                } else Debug.LogWarning($"[IngameShopManager] Card {i}: buyButton is NULL!");

                if (cardUI.cardRoot != null)
                {
                    cardUI.cardRoot.SetActive(true); // Hiện card lên
                    Debug.Log($"[IngameShopManager] Card {i}: SetActive(true)"); // Log 9: Kích hoạt card
                }
            }
            else // Nếu không có đủ lựa chọn cho card này
            {
                 Debug.Log($"[IngameShopManager] Card {i}: No choice data, hiding card."); // Log 10: Ẩn card thừa
                if (cardUI.cardRoot != null) cardUI.cardRoot.SetActive(false); // Ẩn card đi
            }
        }
         Debug.Log("[IngameShopManager] Finished UpdateShopUI."); // Log 11: Kết thúc hàm
    }
}
