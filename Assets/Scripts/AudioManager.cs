using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Music")]
    public AudioSource musicSource;
    public AudioClip gameMainSong;

    [Header("UI Sound Effects")]
    public AudioSource sfxSource;
    public AudioClip buttonClick;
    public AudioClip menuHover;
    public AudioClip fightEntryButton;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Listen for when we enter a new scene
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        if (gameMainSong != null)
        {
            musicSource.clip = gameMainSong;
            musicSource.Play();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Stop the menu music ONLY if we enter the fighting arena!
        if (scene.name == "MainScene")
        {
            musicSource.Stop();
        }
        // If we are coming back to the menu from a fight, restart the music!
        else if (!musicSource.isPlaying && gameMainSong != null)
        {
            musicSource.clip = gameMainSong;
            musicSource.Play();
        }
    }

    public void PlayClick() { if (buttonClick != null) sfxSource.PlayOneShot(buttonClick); }
    public void PlayHover() { if (menuHover != null) sfxSource.PlayOneShot(menuHover); }
    public void PlayFightEntry() { if (fightEntryButton != null) sfxSource.PlayOneShot(fightEntryButton); }
}