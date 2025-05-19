using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Main Menu Elements")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Settings Panel")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private TMP_Text musicVolumeText;
    [SerializeField] private Slider sfxVolumeSlider; 
    [SerializeField] private TMP_Text sfxVolumeText; 
    [SerializeField] private TMP_Dropdown qualityDropdown; 
    [SerializeField] private TMP_Text qualityText; 
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Text resolutionText; 
    [SerializeField] private Button resetSettingsButton;
    [SerializeField] private Button closeSettingsButton;
    [SerializeField] private Text readmeText;

    [Header("Loading Panel Elements")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Slider loadingSlider; 
    [SerializeField] private TMP_Text loadingText; 

    [SerializeField] private TextMeshProUGUI GametipsText;

    [Header("Audio")]
    [SerializeField] private AudioMixer masterMixer;
    [SerializeField] private AudioSource buttonClickSFX;
    [SerializeField] private AudioSource bgm; 

    private void Awake()
    {
        if (backgroundImage == null) Debug.LogError("MainMenuController: BackgroundImage not assigned!");
        if (playButton == null) Debug.LogError("MainMenuController: PlayButton not assigned!");
        if (settingsButton == null) Debug.LogError("MainMenuController: SettingsButton not assigned!");
        if (quitButton == null) Debug.LogError("MainMenuController: QuitButton not assigned!");
        if (settingsPanel == null) Debug.LogError("MainMenuController: SettingsPanel not assigned!");
        if (musicVolumeSlider == null) Debug.LogError("MainMenuController: MusicVolumeSlider not assigned!");
        if (musicVolumeText == null) Debug.LogError("MainMenuController: MusicVolumeText not assigned!");
        if (sfxVolumeSlider == null) Debug.LogError("MainMenuController: SFXVolumeSlider not assigned!");
        if (sfxVolumeText == null) Debug.LogError("MainMenuController: SFXVolumeText not assigned!");
        if (qualityDropdown == null) Debug.LogError("MainMenuController: QualityDropdown not assigned!");
        if (qualityText == null) Debug.LogError("MainMenuController: QualityText not assigned!");
        if (resolutionDropdown == null) Debug.LogError("MainMenuController: ResolutionDropdown not assigned!");
        if (resolutionText == null) Debug.LogError("MainMenuController: ResolutionText not assigned!");
        if (resetSettingsButton == null) Debug.LogError("MainMenuController: ResetSettingsButton not assigned!");
        if (closeSettingsButton == null) Debug.LogError("MainMenuController: CloseSettingsButton not assigned!");
        if (loadingPanel == null) Debug.LogError("MainMenuController: LoadingPanel not assigned!");
        if (loadingSlider == null) Debug.LogError("MainMenuController: LoadingSlider not assigned!");
        if (loadingText == null) Debug.LogError("MainMenuController: LoadingText not assigned!");
        if (masterMixer == null) Debug.LogError("MainMenuController: MasterMixer not assigned!");
        if (buttonClickSFX == null) Debug.LogError("MainMenuController: ButtonClickSFX not assigned!");
        if (bgm == null) Debug.LogError("MainMenuController: BGM not assigned!");
    }

    private void Start()
    {
        settingsPanel.SetActive(false);
        loadingPanel.SetActive(false);
        GametipsText.gameObject.SetActive(false);

        playButton.onClick.AddListener(() => { PlayButtonSound(); PlayGame(); });
        settingsButton.onClick.AddListener(() => { PlayButtonSound(); OpenSettings(); });
        quitButton.onClick.AddListener(() => { PlayButtonSound(); QuitGame(); });
        closeSettingsButton.onClick.AddListener(() => { PlayButtonSound(); CloseSettings(); });
        resetSettingsButton.onClick.AddListener(() => { PlayButtonSound(); ResetSettings(); });

        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        qualityDropdown.onValueChanged.AddListener(SetQuality);
        resolutionDropdown.onValueChanged.AddListener(SetResolution);

        LoadSettings();
    }

    private void RandomGameTips()
    {
        int randomIndex = Random.Range(0, CommonScript.GameTips.Length);
        GametipsText.text = "Do you know : " + CommonScript.GameTips[randomIndex];
    }

    private void PlayButtonSound()
    {
        buttonClickSFX.Play();
    }

    private void PlayGame()
    {
        readmeText.gameObject.SetActive(false);
        StartCoroutine(LoadGameSceneAsync());
    }

    private void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    private void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    private void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }

    private IEnumerator LoadGameSceneAsync()
    {
        loadingPanel.SetActive(true);
        loadingSlider.value = 0f;
        loadingText.text = "Loading...0%";

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("BioTerror3D"); 
        asyncLoad.allowSceneActivation = false; 

        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            loadingSlider.value = progress;
            int percentage = Mathf.RoundToInt(progress * 100);
            loadingText.text = $"Loading...{percentage}%";

            if (progress >= 1f)
            {
                loadingSlider.value = 1f; 
                loadingText.text = "Loading...100%";
                RandomGameTips();
                GametipsText.gameObject.SetActive(true);
                yield return new WaitForSeconds(0.5f);
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    private void LoadSettings()
    {
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0f);
        masterMixer.SetFloat("MusicVolume", musicVolume);
        masterMixer.SetFloat("SFXVolume", sfxVolume);
        musicVolumeSlider.value = musicVolume;
        sfxVolumeSlider.value = sfxVolume;
    }

    private void SetMusicVolume(float volume)
    {
        masterMixer.SetFloat("MusicVolume", volume);
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }

    private void SetSFXVolume(float volume)
    {
        masterMixer.SetFloat("SFXVolume", volume);
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }

    private void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex, true);
        PlayerPrefs.SetInt("QualityLevel", qualityIndex);
        PlayerPrefs.Save();
    }

    private void SetResolution(int resolutionIndex)
    {
        string[] res = resolutionDropdown.options[resolutionIndex].text.Split('x');
        int width = int.Parse(res[0]);
        int height = int.Parse(res[1]);
        Screen.SetResolution(width, height, Screen.fullScreen);
        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
        PlayerPrefs.Save();
    }

    private void ResetSettings()
    {
        masterMixer.SetFloat("MusicVolume", 0f);
        masterMixer.SetFloat("SFXVolume", 0f);
        musicVolumeSlider.value = 0f;
        sfxVolumeSlider.value = 0f;
        QualitySettings.SetQualityLevel(1, true);
        qualityDropdown.value = 1;
        Screen.SetResolution(1920, 1080, Screen.fullScreen);
        resolutionDropdown.value = 0;

        PlayerPrefs.SetFloat("MusicVolume", 0f);
        PlayerPrefs.SetFloat("SFXVolume", 0f);
        PlayerPrefs.SetInt("QualityLevel", 1);
        PlayerPrefs.SetInt("ResolutionIndex", 0);
        PlayerPrefs.Save();
    }
}