using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

public class IngameMenuController : MonoBehaviour
{
    [Header("Pause Menu Elements")]
    [SerializeField] private GameObject pausePanel; 
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider; 
    [SerializeField] private Toggle musicToggle; 
    [SerializeField] private Toggle sfxToggle;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button newGameButton; 
    [SerializeField] private Button homeButton;
    [SerializeField] private Button backButton; 

    [Header("Audio")]
    [SerializeField] private AudioMixer masterMixer; 
    [SerializeField] private AudioSource gameBGM;

    private bool isPaused = false;
    private bool isMusicOn = true;
    private bool isSFXOn = true;
    private bool isGameEnded = false;

    private void Awake()
    {
        if (pausePanel == null) Debug.LogError("IngameMenuController: PausePanel not assigned!");
        if (musicVolumeSlider == null) Debug.LogError("IngameMenuController: MusicVolumeSlider not assigned!");
        if (sfxVolumeSlider == null) Debug.LogError("IngameMenuController: SFXVolumeSlider not assigned!");
        if (musicToggle == null) Debug.LogError("IngameMenuController: MusicToggle not assigned!");
        if (sfxToggle == null) Debug.LogError("IngameMenuController: SFXToggle not assigned!");
        if (resumeButton == null) Debug.LogError("IngameMenuController: ResumeButton not assigned!");
        if (newGameButton == null) Debug.LogError("IngameMenuController: NewGameButton not assigned!");
        if (homeButton == null) Debug.LogError("IngameMenuController: HomeButton not assigned!");
        if (backButton == null) Debug.LogError("IngameMenuController: BackButton not assigned!");
        if (masterMixer == null) Debug.LogError("IngameMenuController: MasterMixer not assigned!");
        if (gameBGM == null) Debug.LogError("IngameMenuController: GameBGM not assigned!");
    }

    private void Start()
    {
        pausePanel.SetActive(false);

        resumeButton.onClick.AddListener(ResumeGame);
        newGameButton.onClick.AddListener(StartNewGame);
        homeButton.onClick.AddListener(ReturnToMainMenu);
        backButton.onClick.AddListener(BackToGame);

        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        musicToggle.onValueChanged.AddListener(ToggleMusic);
        sfxToggle.onValueChanged.AddListener(ToggleSFX);

        LoadSettings();
    }

    private void Update()
    {
        if (isGameEnded) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    private void PauseGame()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        gameBGM.Pause();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        gameBGM.UnPause();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
    }

    public void StartNewGame()
    {
        Time.timeScale = 1f;
        gameBGM.Stop();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        gameBGM.Stop();
        SceneManager.LoadScene("MainMenuScene");
    }

    private void BackToGame()
    {
        ResumeGame();
    }

    private void ToggleMusic(bool isOn)
    {
        isMusicOn = isOn;
        if (isOn)
            gameBGM.UnPause();
        else
            gameBGM.Pause();
        masterMixer.SetFloat("MusicVolume", isOn ? 0f : -80f);
        PlayerPrefs.SetInt("MusicOn", isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void ToggleSFX(bool isOn)
    {
        isSFXOn = isOn;
        masterMixer.SetFloat("SFXVolume", isOn ? 0f : -80f);
        PlayerPrefs.SetInt("SFXOn", isOn ? 1 : 0);
        PlayerPrefs.Save();
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

    private void LoadSettings()
    {
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0f);
        masterMixer.SetFloat("MusicVolume", musicVolume);
        masterMixer.SetFloat("SFXVolume", sfxVolume);
        musicVolumeSlider.value = musicVolume;
        sfxVolumeSlider.value = sfxVolume;

        isMusicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        isSFXOn = PlayerPrefs.GetInt("SFXOn", 1) == 1;
        musicToggle.isOn = isMusicOn;
        sfxToggle.isOn = isSFXOn;
        ToggleMusic(isMusicOn);
        ToggleSFX(isSFXOn);
    }

    public void OnGameWin()
    {
        isGameEnded = true;
        pausePanel.SetActive(false);
        Time.timeScale = 0f;
        gameBGM.Pause();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnGameLose()
    {
        isGameEnded = true;
        pausePanel.SetActive(false);
        Time.timeScale = 0f;
        gameBGM.Pause();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}