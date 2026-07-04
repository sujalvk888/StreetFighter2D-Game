using UnityEngine;

public class AnimationRelay : MonoBehaviour
{
    private PlayerMovement playerMovement;

    void Start()
    {
        // This looks UP the hierarchy to find the script on your root Player object
        playerMovement = GetComponentInParent<PlayerMovement>();
    }

    // This is the function the Animator will now be able to see
    public void EndAttack()
    {
        if (playerMovement != null)
        {
            // It relays the message to your actual movement script
            playerMovement.EndAttack();
        }
    }

    public void TriggerHitbox()
    {
        if (playerMovement != null)
        {
            playerMovement.TriggerHitbox();
        }
    }
}