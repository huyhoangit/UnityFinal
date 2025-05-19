using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro; 
using UnityEngine.UI;

public class IngameShopManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private GameObject shopUIPanel;

    [Header("UI References")]
    [SerializeField] private List<UpgradeCardUI> upgradeCardUIs;
    [SerializeField] private TextMeshProUGUI currencyText;  
    [SerializeField] private Button rerollButton; 
    [SerializeField] private TextMeshProUGUI rerollCostText;
    [SerializeField] private Button closeButton; 

    [Header("Shop Settings")]
    [SerializeField] private List<IngameUpgradeData> availableUpgradesPool;
    [SerializeField] private int numberOfChoices = 3;
    [SerializeField] private int baseRerollCost = 10;
    private int currentRerollCost;

    private List<IngameUpgradeData> currentChoices = new List<IngameUpgradeData>();
    private bool isClosingShop = false; 
    private bool[] cardPurchased; // Thêm biến này

    [System.Serializable]
    public class UpgradeCardUI 
    {
        public GameObject cardRoot; 
        public Image iconImage;   
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI descriptionText;
        public TextMeshProUGUI costText;
        public Button buyButton;   
    }

    void Start()
    {
        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>(); 

        if (shopUIPanel != null)
            shopUIPanel.SetActive(false); 

        availableUpgradesPool = availableUpgradesPool.Where(u => u != null && !string.IsNullOrEmpty(u.upgradeID)).ToList();

        if (rerollButton != null) rerollButton.onClick.AddListener(RerollChoices);
        if (closeButton != null) closeButton.onClick.AddListener(CloseShop);
    }

    public void OpenShop()
    {
        isClosingShop = false; 

        if (playerController == null || shopUIPanel == null)
        {
            Debug.LogError("Shop cannot open. Missing references.");
            return;
        }

        currentRerollCost = baseRerollCost; 
        GenerateChoices();
        UpdateShopUI();

        shopUIPanel.SetActive(true);
        Time.timeScale = 0f;
        Debug.Log("Ingame Shop Opened.");
    }

    public void CloseShop()
    {
        if (isClosingShop) 
        {
            Debug.LogWarning("CloseShop called again rapidly. Ignoring.");
            return;
        }
        isClosingShop = true; 

        if (shopUIPanel != null)
            shopUIPanel.SetActive(false);
        Time.timeScale = 1f;
        Debug.Log("Ingame Shop Closed.");

        GameController gc = FindFirstObjectByType<GameController>(); 
        if (gc != null)
        {
            gc.StartNextWaveAfterShop();
        }
        else
        {
            Debug.LogError("Could not find GameController to start next wave after shop!");
        }
    }

    private void GenerateChoices()
    {
        currentChoices.Clear();
        Debug.Log($"[GenerateChoices] Initial availableUpgradesPool count: {availableUpgradesPool?.Count ?? 0}");

        if (availableUpgradesPool == null || availableUpgradesPool.Count == 0)
        {
            Debug.LogError("[GenerateChoices] availableUpgradesPool is empty or null!");
            return;
        }

        List<IngameUpgradeData> possibleUpgrades = new List<IngameUpgradeData>(availableUpgradesPool);

        Debug.Log($"[GenerateChoices] possibleUpgrades count before filtering max level: {possibleUpgrades.Count}");
        if (playerController == null) Debug.LogError("[GenerateChoices] PlayerController is null, cannot check acquired upgrades!");


        if (playerController != null && playerController.acquiredUpgrades != null)
        {
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

                Debug.Log($"[GenerateChoices] Checking Filter: '{data.displayName}' (ID: {data.upgradeID}) - Current Level: {currentLevel}, Max Level: {data.maxLevel}");
                

                if (currentLevel >= data.maxLevel)
                {
                    Debug.Log($"[GenerateChoices] Removing '{data.displayName}' because it reached max level ({currentLevel}/{data.maxLevel}).");
                    possibleUpgrades.RemoveAt(i);
                }
            }
        }

        Debug.Log($"[GenerateChoices] possibleUpgrades count after filtering max level: {possibleUpgrades.Count}");


        int count = Mathf.Min(numberOfChoices, possibleUpgrades.Count);
        for (int i = 0; i < count; i++)
        {
            if (possibleUpgrades.Count == 0)
            {
                 Debug.LogWarning("[GenerateChoices] No more possible upgrades to choose from.");
                 break;
            }

            int randomIndex = Random.Range(0, possibleUpgrades.Count);
            IngameUpgradeData chosenUpgrade = possibleUpgrades[randomIndex];
            if (chosenUpgrade != null)
            {
                currentChoices.Add(chosenUpgrade);
                 Debug.Log($"[GenerateChoices] Added choice: {chosenUpgrade.displayName}");
            } else {
                 Debug.LogWarning($"[GenerateChoices] Tried to add a NULL choice at random index {randomIndex}. Skipping.");
                 i--; 
            }
            possibleUpgrades.RemoveAt(randomIndex); 
        }
        Debug.Log($"[GenerateChoices] Final generated choices count: {currentChoices.Count}");

        // Khởi tạo trạng thái đã mua cho mỗi card
        cardPurchased = new bool[count];
    }

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

            // Đánh dấu đã mua card này
            if (cardPurchased != null && choiceIndex < cardPurchased.Length)
            {
                cardPurchased[choiceIndex] = true;
                Debug.Log($"[PurchaseUpgrade] Marked card {choiceIndex} as purchased.");
            }

            UpdateShopUI();
        }
        else
        {
            Debug.Log("Purchase failed: Not enough currency.");
        }
    }

    public void RerollChoices()
    {
        Debug.Log($"Attempting to reroll for {currentRerollCost} currency.");
        if (playerController.SpendInGameCurrency(currentRerollCost))
        {
            currentRerollCost *= 2;
            GenerateChoices();
            UpdateShopUI(); // All cards will be enabled again after reroll
            Debug.Log($"Reroll successful. Next reroll cost: {currentRerollCost}");
        }
        else
        {
            Debug.Log("Reroll failed: Not enough currency.");
        }
    }

    private void UpdateShopUI()
    {
        Debug.Log("[IngameShopManager] Starting UpdateShopUI...");

        if (playerController == null) Debug.LogError("[IngameShopManager] PlayerController is NULL in UpdateShopUI!");
        if (currencyText == null) Debug.LogWarning("[IngameShopManager] CurrencyText reference is NULL!");
        if (rerollButton == null) Debug.LogWarning("[IngameShopManager] RerollButton reference is NULL!");
        if (rerollCostText == null) Debug.LogWarning("[IngameShopManager] RerollCostText reference is NULL!");
        if (upgradeCardUIs == null || upgradeCardUIs.Count == 0) Debug.LogWarning("[IngameShopManager] UpgradeCardUIs list is NULL or empty!");


        if (currencyText != null && playerController != null)
        {
            currencyText.text = $"Currency: {playerController.inGameCurrency}";
            Debug.Log($"[IngameShopManager] Updated Currency Text: {currencyText.text}");
        }

        if (rerollCostText != null)
        {
            rerollCostText.text = $"Reroll ({currentRerollCost})";
             Debug.Log($"[IngameShopManager] Updated Reroll Cost Text: {rerollCostText.text}");
        }
         if (rerollButton != null && playerController != null)
        {
             rerollButton.interactable = playerController.inGameCurrency >= currentRerollCost;
             Debug.Log($"[IngameShopManager] Reroll Button Interactable: {rerollButton.interactable}"); 
        }


        Debug.Log($"[IngameShopManager] Processing {upgradeCardUIs?.Count ?? 0} card UI slots for {currentChoices?.Count ?? 0} choices."); 
        for (int i = 0; i < upgradeCardUIs.Count; i++)
        {
             Debug.Log($"[IngameShopManager] Checking Card Slot {i}..."); 
             UpgradeCardUI cardUI = upgradeCardUIs[i];
             if (cardUI == null)
             {
                 Debug.LogError($"[IngameShopManager] Card UI at index {i} is NULL!");
                 continue;
             }
             if (cardUI.cardRoot == null) Debug.LogWarning($"[IngameShopManager] Card {i}: cardRoot is NULL!");


            if (i < currentChoices.Count)
            {
                IngameUpgradeData data = currentChoices[i];
                 if (data == null)
                 {
                     Debug.LogError($"[IngameShopManager] Choice data at index {i} is NULL!");
                     if (cardUI.cardRoot != null) cardUI.cardRoot.SetActive(false);
                     continue;
                 }

                Debug.Log($"[IngameShopManager] Card {i}: Displaying Upgrade '{data.displayName}' (ID: {data.upgradeID})"); 

                int currentLevel = 0;
                playerController.acquiredUpgrades.TryGetValue(data.upgradeID, out currentLevel);
                int cost = data.GetCost(currentLevel);

                if (cardUI.titleText != null) cardUI.titleText.text = $"{data.displayName} (Lv.{currentLevel + 1})"; else Debug.LogWarning($"[IngameShopManager] Card {i}: titleText is NULL!");
                if (cardUI.descriptionText != null) cardUI.descriptionText.text = data.GetFormattedDescription(currentLevel + 1); else Debug.LogWarning($"[IngameShopManager] Card {i}: descriptionText is NULL!");
                if (cardUI.costText != null) cardUI.costText.text = cost.ToString(); else Debug.LogWarning($"[IngameShopManager] Card {i}: costText is NULL!");
                if (cardUI.iconImage != null) cardUI.iconImage.sprite = data.icon; else Debug.LogWarning($"[IngameShopManager] Card {i}: iconImage is NULL!");

                if (cardUI.buyButton != null)
                {
                    cardUI.buyButton.onClick.RemoveAllListeners();
                    int choiceIndex = i;
                    cardUI.buyButton.onClick.AddListener(() => PurchaseUpgrade(choiceIndex));
                    // Nếu đã mua thì disable, chưa mua thì kiểm tra tiền
                    bool purchased = (cardPurchased != null && i < cardPurchased.Length && cardPurchased[i]);
                    cardUI.buyButton.interactable = !purchased && playerController.inGameCurrency >= cost;
                    Debug.Log($"[IngameShopManager] Card {i}: Buy Button Interactable: {cardUI.buyButton.interactable} (purchased={purchased})");
                } else Debug.LogWarning($"[IngameShopManager] Card {i}: buyButton is NULL!");

                if (cardUI.cardRoot != null)
                {
                    cardUI.cardRoot.SetActive(true); 
                    Debug.Log($"[IngameShopManager] Card {i}: SetActive(true)");
                }
            }
            else 
            {
                 Debug.Log($"[IngameShopManager] Card {i}: No choice data, hiding card.");
                if (cardUI.cardRoot != null) cardUI.cardRoot.SetActive(false); 
            }
        }
         Debug.Log("[IngameShopManager] Finished UpdateShopUI.");
    }
}
