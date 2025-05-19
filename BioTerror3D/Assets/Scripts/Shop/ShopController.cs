// using UnityEngine;
// using System.Collections.Generic;

// public class ShopController : MonoBehaviour
// {
//     [Header("Upgrade Card Prefab")]
//     [SerializeField] private GameObject upgradeCardPrefab;
//     [Header("UI")]
//     [SerializeField] private GameObject shopPanel;
//     [SerializeField] private Transform cardParent;

//     [Header("Upgrade Data List")]
//     [SerializeField] private List<IngameUpgradeData> allUpgrades;

//     private List<GameObject> currentCards = new List<GameObject>();

//     public void ShowRollPickShop()
//     {
//         shopPanel.SetActive(true);
//         ClearCards();

//         // Random 3 upgrade cards
//         List<IngameUpgradeData> rollUpgrades = GetRandomUpgrades(3);

//         foreach (var upgrade in rollUpgrades)
//         {
//             GameObject card = Instantiate(upgradeCardPrefab, cardParent);
//             card.GetComponent<UpgradeCardUI>().Setup(upgrade, OnCardPicked);
//             currentCards.Add(card);
//         }
//     }

//     private List<IngameUpgradeData> GetRandomUpgrades(int count)
//     {
//         List<IngameUpgradeData> result = new List<IngameUpgradeData>();
//         List<IngameUpgradeData> pool = new List<IngameUpgradeData>(allUpgrades);

//         for (int i = 0; i < count && pool.Count > 0; i++)
//         {
//             int idx = Random.Range(0, pool.Count);
//             result.Add(pool[idx]);
//             pool.RemoveAt(idx);
//         }
//         return result;
//     }

//     private void OnCardPicked(IngameUpgradeData pickedUpgrade)
//     {
//         // Áp dụng nâng cấp cho player
//         FindObjectOfType<PlayerController>().ApplyUpgrade(pickedUpgrade);

//         // Đóng shop, reset exp, bắt đầu wave mới
//         shopPanel.SetActive(false);
//         GameController.Instance.ResetPlayerExp();
//         GameController.Instance.StartNextWave();
//     }

//     private void ClearCards()
//     {
//         foreach (var card in currentCards)
//         {
//             Destroy(card);
//         }
//         currentCards.Clear();
//     }
// }