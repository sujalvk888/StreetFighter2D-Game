using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatHUDManager : MonoBehaviour
{
    public static CombatHUDManager instance;

    [Header("Timer Settings")]
    public TextMeshProUGUI timerText;
    public float matchTime = 99f;
    private float currentTime;
    private bool timerIsRunning = false;
    private bool timeOverTriggered = false;

    [Header("Player 1 UI")]
    public Image p1CoverImage;
    public GameObject p1Win1;
    public GameObject p1Win2;

    [Header("Player 2 UI")]
    public Image p2CoverImage;
    public GameObject p2Win1;
    public GameObject p2Win2;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        // 1. Set up the timer
        currentTime = matchTime;
        UpdateTimerDisplay();
        
        // 2. Fetch the correct UI covers from the Data Bridge!
        if (CharacterSelectManager.instance != null)
        {
            // Get the indexes of who was picked
            int p1Index = GetFighterIndex(CharacterSelectManager.instance.p1SelectedPrefab);
            int p2Index = GetFighterIndex(CharacterSelectManager.instance.p2SelectedPrefab);

            // Apply the Left cover for P1 and the Right cover for P2
            if (p1Index != -1) p1CoverImage.sprite = CharacterSelectManager.instance.roster[p1Index].hudCoverLeft;
            if (p2Index != -1) p2CoverImage.sprite = CharacterSelectManager.instance.roster[p2Index].hudCoverRight;
        }
    }

    void Update()
    {
        if (timerIsRunning)
        {
            if (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
                UpdateTimerDisplay();
            }
            else if (!timeOverTriggered)
            {
                currentTime = 0;
                timerIsRunning = false;
                timeOverTriggered = true; // Prevents calling it 100 times a second!
                UpdateTimerDisplay();
                
                // Tell the GameManager time is up!
                if (GameManager.instance != null) GameManager.instance.HandleTimeOver();
            }
        }
    }

    void UpdateTimerDisplay()
    {
        // Convert the float to an integer for the display
        int seconds = Mathf.FloorToInt(currentTime);
        timerText.text = seconds.ToString();
    }

    public void StartTimer()
    {
        timerIsRunning = true;
    }

    public void StopTimer()
    {
        timerIsRunning = false;
    }

    public void ResetTimer()
    {
        currentTime = matchTime;
        timeOverTriggered = false; 
        UpdateTimerDisplay();
    }

    // This updates the glowing gold round markers
    public void UpdateRoundMarkers(int p1Wins, int p2Wins)
    {
        p1Win1.SetActive(p1Wins >= 1);
        p1Win2.SetActive(p1Wins >= 2);

        p2Win1.SetActive(p2Wins >= 1);
        p2Win2.SetActive(p2Wins >= 2);
    }

    // A helper to find the index of the prefab
    private int GetFighterIndex(GameObject selectedPrefab)
    {
        for (int i = 0; i < CharacterSelectManager.instance.roster.Length; i++)
        {
            if (CharacterSelectManager.instance.roster[i].characterPrefab == selectedPrefab)
            {
                return i;
            }
        }
        return -1;
    }
}