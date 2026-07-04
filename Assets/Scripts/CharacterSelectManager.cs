using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public class FighterData
{
    public string characterName;
    public Sprite portraitPNG;
    public GameObject characterPrefab;
    
    // NEW: The custom HUD covers for this specific character
    public Sprite hudCoverLeft;  
    public Sprite hudCoverRight; 
}

public class CharacterSelectManager : MonoBehaviour
{
    public static CharacterSelectManager instance;

    [Header("Roster")]
    public FighterData[] roster;

    [Header("Player 1 Panel UI")]
    public Image p1PortraitImage;       // The Image component inside P1_Panel -> CharacterMask
    public GameObject p1SelectPrompt;   // The "Select Your Fighter" graphic

    [Header("Player 2 Panel UI")]
    public Image p2PortraitImage;       // The Image component inside P2_Panel -> CharacterMask
    public GameObject p2SelectPrompt;   // The "Select Your Fighter" graphic

    [Header("Practice Mode Settings")]
    public Sprite shadowPortrait;

    [Header("UI Elements")]
    public GameObject fightButtonObject;

    [Header("Map Selection")]
    public GameObject[] mapBackgrounds; // Drag Background Stage 1, 2, 3 here
    public GameObject[] mapTitles;      // Drag Stage 1, 2, 3 titles here
    public Image[] mapDots;             // Drag Dot1, Dot2, Dot3 here
    
    // The GameManager will read this later to know which map to load!
    public int selectedMapIndex { get; private set; } = 0;

    // Data to pass to the GameManager
    public GameObject p1SelectedPrefab { get; private set; }
    public GameObject p2SelectedPrefab { get; private set; }

    private int p1Selected = -1;
    private int p2Selected = -1;

    void Awake()
    {
        // If an old Data Bridge exists from a previous match, destroy it!
        if (instance != null && instance != this)
        {
            Destroy(instance.gameObject);
        }

        // Make THIS fresh one the official Data Bridge
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        SelectMap(0);

        string mode = PlayerPrefs.GetString("GameMode", "Versus");
        
        // Auto-select P2 for both Practice AND Arcade mode
        if (mode == "Practice" || mode == "Arcade")
        {
            int randomP2 = Random.Range(0, roster.Length);
            p2Selected = randomP2;
            p2SelectedPrefab = roster[randomP2].characterPrefab;

            // Show shadow for practice, but show true identity for Arcade!
            if (mode == "Practice" && shadowPortrait != null) p2PortraitImage.sprite = shadowPortrait;
            else p2PortraitImage.sprite = roster[randomP2].portraitPNG; 
            
            p2PortraitImage.enabled = true;
            if (p2SelectPrompt != null) p2SelectPrompt.SetActive(false);
        }
    }

    // The buttons in your grid will call this function
    public void SelectCharacter(bool isPlayerOne, int rosterIndex)
    {
        // Block Player 2 buttons if we are in Practice OR Arcade mode
        string mode = PlayerPrefs.GetString("GameMode", "Versus");
        if (!isPlayerOne && (mode == "Practice" || mode == "Arcade")) return;

        if (rosterIndex < 0 || rosterIndex >= roster.Length) return;

        if (isPlayerOne)
        {
            p1Selected = rosterIndex;
            
            // 1. Show the character portrait
            p1PortraitImage.sprite = roster[rosterIndex].portraitPNG;
            p1PortraitImage.enabled = true;
            
            // 2. Hide the "Select Your Fighter" graphic
            if (p1SelectPrompt != null) p1SelectPrompt.SetActive(false);
            
            // 3. Save the prefab for the fight
            p1SelectedPrefab = roster[rosterIndex].characterPrefab;
            
            Debug.Log("Player 1 selected: " + roster[rosterIndex].characterName);
        }
        else
        {
            // We will hook this up later!
            p2Selected = rosterIndex;
            p2PortraitImage.sprite = roster[rosterIndex].portraitPNG;
            p2PortraitImage.enabled = true;
            if (p2SelectPrompt != null) p2SelectPrompt.SetActive(false);
            p2SelectedPrefab = roster[rosterIndex].characterPrefab;
            
            Debug.Log("Player 2 selected: " + roster[rosterIndex].characterName);
        }

        // Check if both players have locked in. If yes, show the Fight button!
        if (BothPlayersSelected() && fightButtonObject != null)
        {
            fightButtonObject.SetActive(true);
        }
    }

    public bool BothPlayersSelected()
    {
        return p1Selected != -1 && p2Selected != -1;
    }

    public void SelectMap(int mapIndex)
    {
        if (mapIndex < 0 || mapIndex >= mapBackgrounds.Length) return;

        selectedMapIndex = mapIndex;

        for (int i = 0; i < mapBackgrounds.Length; i++)
        {
            // Turn on the correct background and title, turn off the rest
            if (mapBackgrounds[i] != null) mapBackgrounds[i].SetActive(i == mapIndex);
            if (mapTitles[i] != null) mapTitles[i].SetActive(i == mapIndex);

            // Make the selected dot solid white, and the unselected dots semi-transparent gray
            if (mapDots[i] != null)
            {
                mapDots[i].color = (i == mapIndex) ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.5f);
            }
        }
    }

    public void StartFight()
    {
        // Double-check just in case, then load the arena!
        if (BothPlayersSelected())
        {
            SceneManager.LoadScene("MainScene");
        }
    }

    public void LoadMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void PlayClickSound() 
    { 
        if (AudioManager.instance != null) AudioManager.instance.PlayClick(); 
    }
    
    public void PlayFightEntrySound() 
    { 
        if (AudioManager.instance != null) AudioManager.instance.PlayFightEntry(); 
    }
}