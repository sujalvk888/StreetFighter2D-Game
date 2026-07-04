using UnityEngine;
using TMPro; 
using System.Collections;
using UnityEngine.Video; // Added for VideoPlayer

public class GameManager : MonoBehaviour
{
    public static GameManager instance; 

    [Header("Match Settings")]
    public int roundsToWin = 2;
    private string currentGameMode;
    private int p1Wins = 0;
    private int p2Wins = 0;

    [Header("Audio & Announcer")]
    public AudioSource announcerSource;
    public AudioClip roundOneClip;
    public AudioClip roundTwoClip;
    public AudioClip finalRoundClip;
    public AudioClip knockoutClip;
    public AudioClip victoryClip;

    [Header("UI & Videos")]
    public TextMeshProUGUI centerText;
    public GameObject postMatchMenu;
    public GameObject practiceExitButton;
    
    // Single Winner Screen
    public GameObject fullscreenVideoObj; 
    public VideoPlayer fullscreenVideoPlayer; 
    
    // Draw/Split Screens
    public GameObject splitScreenContainer;
    public VideoPlayer p1SplitVideo;
    public VideoPlayer p2SplitVideo;

    [Header("Map Environments")]
    public GameObject[] arenaBackgrounds; 

    [Header("Health Bar Links")]
    public UnityEngine.UI.Slider p1HealthBarSlider;
    public UnityEngine.UI.Slider p2HealthBarSlider;

    [HideInInspector] public PlayerMovement player1;
    [HideInInspector] public PlayerMovement player2;
    private Vector3 p1StartPos;
    private Vector3 p2StartPos;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        currentGameMode = PlayerPrefs.GetString("GameMode", "Versus");

        // WIPE THE VIDEO SCREENS CLEAN!
        // if (fullscreenVideoPlayer != null && fullscreenVideoPlayer.targetTexture != null) fullscreenVideoPlayer.targetTexture.Release();
        // if (p1SplitVideo != null && p1SplitVideo.targetTexture != null) p1SplitVideo.targetTexture.Release();
        // if (p2SplitVideo != null && p2SplitVideo.targetTexture != null) p2SplitVideo.targetTexture.Release();

        SpawnPlayers();
        AssignPlayerControls();

        // Hide the ENTIRE Combat UI in Practice Mode
        if (currentGameMode == "Practice")
        {
            if (CombatHUDManager.instance != null) CombatHUDManager.instance.gameObject.SetActive(false);
            if (practiceExitButton != null) practiceExitButton.SetActive(true); // Turn Exit Button ON
        }
        else
        {
            if (practiceExitButton != null) practiceExitButton.SetActive(false); // Keep it OFF in Versus
        }

        if (player1 != null) p1StartPos = player1.transform.position;
        if (player2 != null) p2StartPos = player2.transform.position;
        
        // Ensure all TVs are off
        if (fullscreenVideoObj != null) fullscreenVideoObj.SetActive(false); 
        if (splitScreenContainer != null) splitScreenContainer.SetActive(false);
        
        if (CharacterSelectManager.instance != null && arenaBackgrounds.Length > 0)
        {
            int chosenMap = CharacterSelectManager.instance.selectedMapIndex;
            for (int i = 0; i < arenaBackgrounds.Length; i++)
            {
                if (arenaBackgrounds[i] != null) arenaBackgrounds[i].SetActive(i == chosenMap); 
            }
        }

        centerText.text = "FIGHT!";
        Invoke("ClearText", 2f);
        
        // Play "Round One" at the very start of the match
        if (announcerSource != null && roundOneClip != null) announcerSource.PlayOneShot(roundOneClip);
        
        // Only start the ticking clock if we are NOT in practice mode
        if (CombatHUDManager.instance != null)
        {
            if (currentGameMode == "Practice") 
            {
                CombatHUDManager.instance.timerText.text = "∞"; // Infinite time symbol
            }
            else 
            {
                CombatHUDManager.instance.StartTimer();
            }
        }
    }

    void SpawnPlayers()
    {
        if (CharacterSelectManager.instance != null)
        {
            GameObject p1Obj = Instantiate(CharacterSelectManager.instance.p1SelectedPrefab);
            player1 = p1Obj.GetComponent<PlayerMovement>();
            if (player1 != null) player1.healthBar = p1HealthBarSlider;

            GameObject p2Obj = Instantiate(CharacterSelectManager.instance.p2SelectedPrefab);
            player2 = p2Obj.GetComponent<PlayerMovement>();
            if (player2 != null) player2.healthBar = p2HealthBarSlider;
        }
    }

    void AssignPlayerControls()
    {
        if (player1 != null)
        {
            player1.playerID = 1;
            player1.leftKey = KeyCode.A;
            player1.rightKey = KeyCode.D;
            player1.jumpKey = KeyCode.W;
            player1.attack1Key = KeyCode.J;
            player1.attack2Key = KeyCode.K;
            player1.attack3Key = KeyCode.L;
            player1.blockKey = KeyCode.S; // Player 1 blocks with 'S'
            player1.transform.position = new Vector3(-5f, -1.5f, 0f); 
            player1.transform.localScale = new Vector3(1, 1, 1);
        }

        if (player2 != null)
        {
            player2.playerID = 2;
            player2.transform.position = new Vector3(5f, -1.5f, 0f);
            player2.transform.localScale = new Vector3(-1, 1, 1);

            if (currentGameMode == "Practice")
            {
                player2.isDummy = true; 
                player2.isAI = false;
            }
            else if (currentGameMode == "Arcade")
            {
                player2.isDummy = false;
                player2.isAI = true;
                
                // INJECT THE AI BRAIN!
                AIBrain brain = player2.gameObject.AddComponent<AIBrain>();
                brain.myFighter = player2;
                brain.target = player1.transform;
            }
            else
            {
                // Standard Versus Mode
                player2.isDummy = false;
                player2.isAI = false;
                player2.blockKey = KeyCode.DownArrow;
                player2.leftKey = KeyCode.LeftArrow;
                player2.rightKey = KeyCode.RightArrow;
                player2.jumpKey = KeyCode.UpArrow;
                player2.attack1Key = KeyCode.B;
                player2.attack2Key = KeyCode.N;
                player2.attack3Key = KeyCode.M;
            }
        }
    }

    void ClearText()
    {
        centerText.text = "";
    }

    // THIS IS CALLED WHEN THE TIMER HITS 0!
    public void HandleTimeOver()
    {
        if (player1.currentHealth > player2.currentHealth)
        {
            RegisterKnockout(2); // P1 wins by health, P2 loses
        }
        else if (player2.currentHealth > player1.currentHealth)
        {
            RegisterKnockout(1); // P2 wins by health, P1 loses
        }
        else 
        {
            // PERFECT TIE! Both players get a win.
            p1Wins++;
            p2Wins++;
            if (CombatHUDManager.instance != null) CombatHUDManager.instance.UpdateRoundMarkers(p1Wins, p2Wins);
            
            CheckMatchStatus(0, null); // 0 means draw
        }
    }

    public void RegisterKnockout(int loserID)
    {
        if (CombatHUDManager.instance != null) CombatHUDManager.instance.StopTimer();

        // Count the wins
        if (loserID == 1) p2Wins++;
        else p1Wins++;

        // Calculate if this knockout just ended the entire match
        bool matchIsOver = (p1Wins >= roundsToWin) || (p2Wins >= roundsToWin);

        // Play Knockout ONLY if there is another round coming!
        if (!matchIsOver && announcerSource != null && knockoutClip != null)
        {
            announcerSource.PlayOneShot(knockoutClip);
        }

        if (loserID == 1) CheckMatchStatus(2, player2 != null ? player2.victoryVideoFileName : null); 
        else CheckMatchStatus(1, player1 != null ? player1.victoryVideoFileName : null); 

        if (CombatHUDManager.instance != null) CombatHUDManager.instance.UpdateRoundMarkers(p1Wins, p2Wins);
    }

    void CheckMatchStatus(int roundWinnerID, string winnerVideoName)
    {
        bool p1MatchWin = p1Wins >= roundsToWin;
        bool p2MatchWin = p2Wins >= roundsToWin;

        // SCENARIO 1: BOTH PLAYERS REACH MAX WINS AT THE SAME TIME (DRAW)
        if (p1MatchWin && p2MatchWin) 
        {
            centerText.text = "MATCH DRAW!";
            if (splitScreenContainer != null)
            {
                splitScreenContainer.SetActive(true);
                p1SplitVideo.url = System.IO.Path.Combine(Application.streamingAssetsPath, player1.victoryVideoFileName);
                p2SplitVideo.url = System.IO.Path.Combine(Application.streamingAssetsPath, player2.victoryVideoFileName);
                p1SplitVideo.Play();
                p2SplitVideo.Play();
            }
            if (postMatchMenu != null) postMatchMenu.SetActive(true);
            
            if (announcerSource != null && victoryClip != null) announcerSource.PlayOneShot(victoryClip);
        }
        // SCENARIO 2: PLAYER 1 OR 2 WINS THE MATCH
        else if (p1MatchWin || p2MatchWin) 
        {
            centerText.text = p1MatchWin ? "PLAYER 1 WINS THE MATCH!" : "PLAYER 2 WINS THE MATCH!";
            if (fullscreenVideoObj != null && !string.IsNullOrEmpty(winnerVideoName))
            {
                fullscreenVideoObj.SetActive(true);
                fullscreenVideoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, winnerVideoName);
                fullscreenVideoPlayer.Play();
            }
            if (postMatchMenu != null) postMatchMenu.SetActive(true);
            
            if (announcerSource != null && victoryClip != null) announcerSource.PlayOneShot(victoryClip);
        }
        // SCENARIO 4: NO ONE HAS WON THE FULL MATCH YET, RESET ROUND
        else 
        {
            if (roundWinnerID == 0) centerText.text = "TIME OVER - DRAW!";
            else centerText.text = "PLAYER " + roundWinnerID + " WINS ROUND!";
            StartCoroutine(ResetRoundRoutine());
        }
    }

    IEnumerator ResetRoundRoutine()
    {
        yield return new WaitForSeconds(3f);

        centerText.text = "FIGHT!";
        Invoke("ClearText", 2f);

        // Calculate what round we are entering based on the score
        if (announcerSource != null)
        {
            int totalRoundsPlayed = p1Wins + p2Wins;
            if (totalRoundsPlayed == 1 && roundTwoClip != null) 
                announcerSource.PlayOneShot(roundTwoClip);
            else if (totalRoundsPlayed >= 2 && finalRoundClip != null) 
                announcerSource.PlayOneShot(finalRoundClip);
        }

        if (player1 != null) player1.ResetFighter(p1StartPos);
        if (player2 != null) player2.ResetFighter(p2StartPos);
        
        if (CombatHUDManager.instance != null)
        {
            CombatHUDManager.instance.ResetTimer();
            CombatHUDManager.instance.StartTimer();
        }
    }

    public void RestartMatch()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void ReturnToCharacterSelect()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("CharacterSelect");
    }

    public void LoadMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}