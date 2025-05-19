using UnityEngine;

public class TableObjectController : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}

public enum UpgradeType { BulletDamage, MovementSpeed, BulletCount, SkillCooldown } 

[CreateAssetMenu(fileName = "New Shop Item", menuName = "Shop/Shop Item")]
public class ShopItemData : ScriptableObject
{
    public string itemName = "New Item";
    public string description = "Item Description";
    public int cost = 10;
    public UpgradeType upgradeType;
    public float upgradeValue = 1f;
    public Sprite icon;
}

public enum IngameUpgradeType
{
    StatBoost, 
    NewWeapon,    
    WeaponUpgrade
    
}

[CreateAssetMenu(fileName = "New Ingame Upgrade", menuName = "Shop/Ingame Upgrade")]
public class IngameUpgradeData : ScriptableObject
{
    [Header("Core Info")]
    public string upgradeID; 
    public string displayName = "New Upgrade";
    [TextArea] public string descriptionFormat = "Upgrade Description {0}";
    public Sprite icon;

    [Header("Upgrade Details")]
    public IngameUpgradeType upgradeType;
    public int maxLevel = 5;
    public string statToModify; 
    public float valuePerLevel = 1.0f; 
    public bool isPercentage = false; 

    public GameObject weaponPrefab;

    [Header("Shop Details")]
    public int baseCost = 50; 
    public int costIncreasePerLevel = 25;

    public int GetCost(int currentLevel)
    {
        return baseCost + (currentLevel * costIncreasePerLevel);
    } 

    public string GetFormattedDescription(int level)
    {
        float displayValue = valuePerLevel * level; 
        return string.Format(descriptionFormat, displayValue);
    }
}
