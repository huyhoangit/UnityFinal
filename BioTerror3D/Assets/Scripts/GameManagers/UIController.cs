using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private Text shootModeLabel;
    [SerializeField] private CanvasGroup shootModeCanvasGroup;

    public static UIController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ShowShootMode(string modeName, float duration = 1.5f)
    {
        if (shootModeLabel == null) return;

        shootModeLabel.text = modeName;
        shootModeLabel.gameObject.SetActive(true);
        shootModeCanvasGroup.alpha = 1f; 
        StopAllCoroutines();
        StartCoroutine(FadeOutLabel(duration));
    }

    private IEnumerator FadeOutLabel(float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            shootModeCanvasGroup.alpha = 1 - (elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        shootModeCanvasGroup.alpha = 0f;
        shootModeLabel.gameObject.SetActive(false); 
    }
}
