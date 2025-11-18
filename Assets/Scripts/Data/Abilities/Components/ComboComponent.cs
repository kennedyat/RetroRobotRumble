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
        Debug.Log($"[ComboComponent] OnExecute called!");
        
        float timeSinceLastAttack = Time.time - lastAttackTime;
        
        // Check combo
        if (timeSinceLastAttack < (maxTimeBetweenHits / comboCounter) && comboCounter < maxComboStack)
        {
            // Check if end cooldown
            if (timeSinceLastAttack >= currentCooldown)
            {
                // Speed up hits
                currentCooldown = Mathf.Max(currentCooldown / speedBonus, 0.1f);
                comboCounter++;
               
            }
         
        }
        else
        {
            // Reset combo
            currentCooldown = comboCooldown;
            comboCounter = 1;
            
        }
        
        lastAttackTime = Time.time;
        
        // Store combo data
        context.CustomData["ComboCounter"] = comboCounter;
        context.CustomData["CurrentCooldown"] = currentCooldown;
        
        // Temp Anim
        if (context.Animator != null)
        {
            bool isSecondAttack = (comboCounter % 2 == 0);
            context.Animator.SetBool(secondHitBoolParam, isSecondAttack);
            context.Animator.SetTrigger(normalAnimTrigger);
            Debug.Log($"[ComboComponent] Animation: {normalAnimTrigger}, Second={isSecondAttack}");
        }
        
        // Play VFX
        if (comboCounter % 2 == 0 && secondaryVFX != null)
        {
            GameObject.Instantiate(secondaryVFX, context.Owner.position, context.Owner.rotation);
        }
        else if (primaryVFX != null)
        {
            GameObject.Instantiate(primaryVFX, context.Owner.position, context.Owner.rotation);
        }
        
        // Activate hitbox
        ActivateHitbox(context);
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