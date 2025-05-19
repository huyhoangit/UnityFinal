using UnityEngine;

public class TableObjectController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public enum UpgradeType { BulletDamage, MovementSpeed, BulletCount, SkillCooldown } // Thêm các loại nâng cấp khác nếu cần

[CreateAssetMenu(fileName = "New Shop Item", menuName = "Shop/Shop Item")]
public class ShopItemData : ScriptableObject
{
    public string itemName = "New Item";
    public string description = "Item Description";
    public int cost = 10;
    public UpgradeType upgradeType;
    public float upgradeValue = 1f; // Giá trị nâng cấp (số lượng, phần trăm, etc.)
    public Sprite icon; // Icon cho UI (tùy chọn)
}

public enum IngameUpgradeType
{
    StatBoost,      // Nâng cấp chỉ số (HP, Speed, Damage...)
    NewWeapon,      // Thêm vũ khí mới
    WeaponUpgrade   // Nâng cấp vũ khí hiện có
    // Thêm các loại khác nếu cần (e.g., SkillModification)
}

[CreateAssetMenu(fileName = "New Ingame Upgrade", menuName = "Shop/Ingame Upgrade")]
public class IngameUpgradeData : ScriptableObject
{
    [Header("Core Info")]
    public string upgradeID; // ID duy nhất để xác định nâng cấp (e.g., "player_movespeed", "weapon_rifle_damage")
    public string displayName = "New Upgrade";
    [TextArea] public string descriptionFormat = "Upgrade Description {0}"; // {0} sẽ được thay bằng giá trị nâng cấp
    public Sprite icon;

    [Header("Upgrade Details")]
    public IngameUpgradeType upgradeType;
    public int maxLevel = 5; // Số lần nâng cấp tối đa cho chỉ số/vũ khí này

    // --- Chỉ số (StatBoost) ---
    public string statToModify; // Tên chỉ số cần thay đổi (e.g., "walkSpeed", "bulletDamage") - Phải khớp với tên biến trong PlayerController
    public float valuePerLevel = 1.0f; // Giá trị tăng thêm mỗi level
    public bool isPercentage = false; // Giá trị là % hay số cố định?

    // --- Vũ khí (NewWeapon / WeaponUpgrade) ---
    public GameObject weaponPrefab; // Prefab vũ khí (chỉ dùng cho NewWeapon)
    // Có thể thêm các trường khác cho nâng cấp vũ khí cụ thể (e.g., tăng tốc độ bắn, số lượng đạn)

    [Header("Shop Details")]
    public int baseCost = 50; // Giá gốc
    public int costIncreasePerLevel = 25; // Giá tăng thêm mỗi cấp

    // Hàm tính giá dựa trên cấp độ hiện tại (ví dụ)
    public int GetCost(int currentLevel)
    {
        return baseCost + (currentLevel * costIncreasePerLevel);
    } 

    // Hàm lấy mô tả đã được định dạng với giá trị
    public string GetFormattedDescription(int level)
    {
        float displayValue = valuePerLevel * level; // Hoặc tính toán phức tạp hơn nếu cần
        return string.Format(descriptionFormat, displayValue);
    }
}
