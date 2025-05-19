using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private Slider healthBar;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private RectTransform backgroundRect;
    [SerializeField] private RectTransform fillRect;

    public int CurrentHealth { get; private set; }

    private float baseWidth = 200f; // Chiều rộng mặc định khi maxHealth = 100

    private void Awake()
    {
        CurrentHealth = maxHealth;
        if (healthBar == null) Debug.LogError($"{name}: HealthBar not assigned!");
        if (healthText == null) Debug.LogError($"{name}: HealthText not assigned!");
        if (healthFillImage == null) Debug.LogError($"{name}: HealthFillImage not assigned!");
        if (backgroundRect == null) Debug.LogError($"{name}: BackgroundRect not assigned!");
        if (fillRect == null) Debug.LogError($"{name}: FillRect not assigned!");
        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        CurrentHealth = Mathf.Max(0, CurrentHealth);
        UpdateHealthBar();
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
        UpdateHealthBar();
    }

    public void SetMaxHealth(int newMaxHealth, bool keepPercent = true)
    {
        float percent = (float)CurrentHealth / maxHealth;
        maxHealth = newMaxHealth;

        if (keepPercent)
            CurrentHealth = Mathf.RoundToInt(maxHealth * percent);
        else
            CurrentHealth = Mathf.Min(CurrentHealth, maxHealth);

        UpdateHealthBar();
    }

    // Gọi hàm này khi tăng máu tối đa từ PlayerStats hoặc khi nhận upgrade
    // Ví dụ: healthSystem.SetMaxHealth(playerStats.maxHealth, true); // full heal khi tăng max máu

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = CurrentHealth;
        }
        if (healthText != null)
        {
            healthText.text = $"HP: {CurrentHealth}/{maxHealth}";
        }
        float percent = Mathf.Clamp01((float)CurrentHealth / maxHealth);

        float width = baseWidth * (maxHealth / 100f);

        if (backgroundRect != null)
            backgroundRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

        // Fill Area là cha của Fill, hãy kéo dài nó
        Transform fillArea = fillRect.parent;
        if (fillArea != null && fillArea is RectTransform fillAreaRect)
            fillAreaRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

        // Fill kéo dài theo tỉ lệ máu
        if (fillRect != null)
            fillRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width * percent);

        if (healthFillImage != null)
            healthFillImage.color = percent <= 0.3f ? Color.red : Color.green;
    }
}