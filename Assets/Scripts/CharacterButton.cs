using UnityEngine;
using UnityEngine.UI;

public class CharacterButton : MonoBehaviour
{
    [Header("Button Settings")]
    public int fighterIndex;
    public bool isPlayerOne;

    [Header("Visuals")]
    public GameObject glowEffect;

    void Start()
    {
        // Check the game mode
        string mode = PlayerPrefs.GetString("GameMode", "Versus");
        
        // If this is a Player 2 button AND we are playing a single-player mode...
        if (!isPlayerOne && (mode == "Practice" || mode == "Arcade"))
        {
            // Physically turn off the button component so it cannot hover, glow, or be clicked!
            UnityEngine.UI.Button myButton = GetComponent<UnityEngine.UI.Button>();
            if (myButton != null)
            {
                myButton.interactable = false;
            }
        }
    }

    public void SelectCharacter()
    {
        // If we are in Practice Mode and this is a Player 2 button, completely block the click and glow!
        if (!isPlayerOne && PlayerPrefs.GetString("GameMode", "Versus") == "Practice") return;

        // 1. Look at all the other buttons inside this folder (Player 1 or Player 2 folder)
        // and force their glow effects to turn OFF.
        foreach (Transform sibling in transform.parent)
        {
            CharacterButton otherButton = sibling.GetComponent<CharacterButton>();
            if (otherButton != null)
            {
                otherButton.glowEffect.SetActive(false);
            }
        }

        // 2. Now that everyone else is off, turn THIS button's glow ON!
        glowEffect.SetActive(true);

        // 3. Tell the Manager to show the portrait in the big panel
        CharacterSelectManager.instance.SelectCharacter(isPlayerOne, fighterIndex);
    }
}