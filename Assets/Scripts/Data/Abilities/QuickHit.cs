using System;
using UnityEngine;


[CreateAssetMenu(menuName = "ArmBehavior/QuickHit")]
public class QuickHit : ArmBehaviorData
{
    [Header("QuickHit Parameters")]
    public float speed = 10f;
    public float duration = 0.3f;
    public float cooldown = 1.2f;
    public float multiplier = 1.5f;
    public override IArmBehavior MakeInstance() => new QuickHitInstance { data = this };
    
}
    [Serializable]
    public class QuickHitInstance : IArmBehavior
    {
            public QuickHit data;
            public bool active;
         
    private float actionCooldown = 0f;         // Countdown between hits
    private float currentCooldown;             // Current cooldown after multiplier
    private float baseCooldown;                // Reference value
    private float speedBonus = .2f;             // How much cooldown is reduced

    private float cooldownThreshold = 0.2f;    // Threshold where it resets

    public bool IsContinue;

    public int counter = 0;

    public void Activate(GameObject owner, ArmInstance arm)
    {

        if (actionCooldown <= 0f)
        {
            counter++;
   
          
            // Increase the speed multiplier
            speedBonus *= data.multiplier;

            // Calculate the new cooldown
            currentCooldown = Mathf.Max(data.cooldown - speedBonus, 0f);
            actionCooldown = currentCooldown;
            Debug.Log($"[{counter}x Combo] Hit! Cooldown: " +currentCooldown+ "s");

            // Reset if too fast
            if (currentCooldown <= cooldownThreshold)
            {
                Debug.Log("Resetting hit speed multiplier!");
                speedBonus = 0f;
                counter = 0;
                currentCooldown = data.cooldown;
                actionCooldown = currentCooldown;
            }
        }
    }

    public void Deactivate(GameObject owner, ArmInstance arm)
    {
       
        
        
            
    }


    public void FixedUpdateFromArm(GameObject owner, ArmInstance arm)
    {
         actionCooldown = Mathf.Max(0f, actionCooldown - Time.fixedDeltaTime);
                  
    }

   
        
}


