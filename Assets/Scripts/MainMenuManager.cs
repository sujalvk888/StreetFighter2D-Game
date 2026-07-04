using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Dynamic Tip System")]
    public Image tipDisplayImage;
    public Sprite[] tipGraphics; 

    void Start()
    {
        if (tipGraphics.Length > 0 && tipDisplayImage != null)
        {
            int randomIndex = Random.Range(0, tipGraphics.Length);
            tipDisplayImage.sprite = tipGraphics[randomIndex];
            tipDisplayImage.preserveAspect = true; 
        }
    }

    public void LoadArcadeMode()
    {
        // Save the game mode in memory so the Game Manager knows to spawn an AI later!
        PlayerPrefs.SetString("GameMode", "Arcade");
        SceneManager.LoadScene("CharacterSelect");
    }

    public void LoadVersusMode()
    {
        PlayerPrefs.SetString("GameMode", "Versus");
        SceneManager.LoadScene("CharacterSelect");
    }

    public void LoadPracticeMode()
    {
        // Save the game mode in memory so the Game Manager knows to spawn a dummy later!
        PlayerPrefs.SetString("GameMode", "Practice");
        SceneManager.LoadScene("CharacterSelect");
    }

    public void QuitGame()
    {
        Debug.Log("Game Exited");
        
        // This quits the actual final built game (.exe)
        Application.Quit();
        
        // This physically stops the play button inside the Unity Editor
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void PlayHoverSound() 
    { 
        if (AudioManager.instance != null) AudioManager.instance.PlayHover(); 
    }
    
    public void PlayClickSound() 
    { 
        if (AudioManager.instance != null) AudioManager.instance.PlayClick(); 
    }
}