using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Components/Combo")]
public class ComboComponent : PartComponent
{
    [Header("Combo Settings")]
    public int maxComboStack = 6;
    public float comboCooldown = 1.2f;
    public float speedBonus = 1.5f; // Divide cooldown by this per stack
    public float maxTimeBetweenHits = 3f;
    
    [Header("Animation")]
    public string normalAnimTrigger = "ShinkansenNormal";
    public string secondHitBoolParam = "Second";
    
    [Header("VFX (Optional)")]
    public GameObject primaryVFX;
    public GameObject secondaryVFX;
    
    private float currentCooldown;
    private float lastAttackTime;
    private int comboCounter = 1;
    
    public override void Initialize(PartContext context)
    {
        currentCooldown = comboCooldown;
        lastAttackTime = -999f;
        comboCounter = 1;
    }
    
    public override void OnExecute(PartContext context)
    {

         float timeSinceLastAttack = Time.time - lastAttackTime;
        
        // First hit or continuing combo?
        if (lastAttackTime < 0 || timeSinceLastAttack >= maxTimeBetweenHits)
        {
            // First hit or combo expired - reset
            currentCooldown = comboCooldown;
            comboCounter = 1;
            Debug.Log($"[ComboComponent] Starting fresh combo!{comboCounter}");
        }
        else if (comboCounter < maxComboStack)
        {
            // Continuing combo - speed up!
            currentCooldown = Mathf.Max(currentCooldown / speedBonus, 0.1f);
            comboCounter++;
            Debug.Log($"[ComboComponent] Speed stack {comboCounter}! New cooldown: {currentCooldown:F2}s");
        }
        else
        {
            // Already at max stacks
            currentCooldown = 0;
            Debug.Log($"[ComboComponent] Max combo stack reached!");
        }
        
        // Set the cooldown for this attack
        if (context.partInstance != null)
        {
            context.partInstance.InternalCooldown = currentCooldown;
            Debug.Log($"[ComboComponent] Set cooldown to {currentCooldown:F2}s");
        }
        
        // Update last attack time
        lastAttackTime = Time.time;
        
 
             
         // Activate hitbox
        ActivateHitbox(context);
        
        // Temp Anim
        /*if (context.Animator != null)
        {
            bool isSecondAttack = (comboCounter % 2 == 0);
            context.Animator.SetBool(secondHitBoolParam, isSecondAttack);
            context.Animator.SetTrigger(normalAnimTrigger);
            Debug.Log($"[ComboComponent] Animation: {normalAnimTrigger}, Second={isSecondAttack}");
        }*/
        // Play VFX
        if (comboCounter % 2 == 0 && secondaryVFX != null)
        {
            GameObject.Instantiate(secondaryVFX, context.Owner.position, context.Owner.rotation);
        }
        else if (primaryVFX != null)
        {
            GameObject.Instantiate(primaryVFX, context.Owner.position, context.Owner.rotation);
        }
        
       
    }
    
    public override void OnUpdate(PartContext context, float deltaTime)
    {
    
      //TO DO: State combo
        float timeSinceLastAttack = Time.time - lastAttackTime;
        bool inComboWindow = timeSinceLastAttack < maxTimeBetweenHits;
        
        context.CustomData["InComboWindow"] = inComboWindow;
        context.CustomData["ComboTimeRemaining"] = Mathf.Max(0, maxTimeBetweenHits - timeSinceLastAttack);
      
       
    }
}