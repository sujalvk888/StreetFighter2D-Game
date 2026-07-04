using UnityEngine;

public class AIBrain : MonoBehaviour
{
    [Header("Fighter Links")]
    public PlayerMovement myFighter;
    public Transform target;
    
    [Header("AI Settings")]
    public float attackRange = 2.5f; 
    private float decisionTimer = 0f;

    void Update()
    {
        if (myFighter == null || target == null || myFighter.currentHealth <= 0) return;
        
        PlayerMovement targetScript = target.GetComponent<PlayerMovement>();
        if (targetScript != null && targetScript.currentHealth <= 0) 
        {
            myFighter.aiMoveInput = 0f;
            myFighter.aiIsBlocking = false;
            return; 
        }

        float directionToTarget = Mathf.Sign(target.position.x - transform.position.x);
        myFighter.transform.localScale = new Vector3(directionToTarget, 1, 1);

        decisionTimer -= Time.deltaTime;
        
        if (decisionTimer <= 0)
        {
            MakeDecision(directionToTarget);
        }
    }

    void MakeDecision(float directionToTarget)
    {
        float distanceX = Mathf.Abs(target.position.x - transform.position.x);

        // STATE 1: PURSUIT
        if (distanceX > attackRange)
        {
            myFighter.aiIsBlocking = false;
            myFighter.aiMoveInput = directionToTarget; 
            
            if (target.position.y > transform.position.y + 1.5f && Random.Range(0, 100) < 50) 
            {
                myFighter.aiIsJumping = true;
            }
            
            decisionTimer = 0.1f; 
        }
        // STATE 2: COMBAT 
        else
        {
            myFighter.aiMoveInput = 0f; 
            myFighter.aiIsJumping = false; 

            int actionChoice = Random.Range(0, 100);
            
            if (actionChoice < 40) 
            {
                // 40% chance to hold Block (Increased from 25%)
                myFighter.aiIsBlocking = true;
                decisionTimer = Random.Range(0.5f, 1.2f); 
            }
            else if (actionChoice < 60)
            {
                // NEW: 20% chance to do absolutely nothing (gives the player breathing room!)
                myFighter.aiIsBlocking = false;
                decisionTimer = Random.Range(0.5f, 1.0f);
            }
            else
            {
                // 40% chance to Attack (Decreased from 75%)
                myFighter.aiIsBlocking = false;
                
                int attackChoice = Random.Range(1, 4); 
                myFighter.ExecuteAttack("Attack" + attackChoice);
                
                // THE FIX: Massive cooldown after attacking so they don't spam!
                decisionTimer = Random.Range(1.2f, 2.5f); 
            }
        }
    }
}