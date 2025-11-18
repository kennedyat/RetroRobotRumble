using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "ScriptableObjects/Components/Charge")]
public class ChargeComponent : PartComponent
{
    [Header("Charge Stages")]
    public float[] chargeThresholds = { 0.5f, 1.2f, 2.2f };
    public float[] stageDamageMultipliers = { 1f, 2f, 3f };
    public float[] stageKnockbackMultipliers = { 1f, 2f, 4f };
    
    [Header("Charge VFX")]
    public VisualEffect chargeVFX;
    public float[] vfxAmountPerStage = { 8f, 16f, 32f, 64f };
    public string vfxAmountParameter = "Amount";
    
    [Header("Animation")]
    public string chargeBoolParameter = "isCharging";
    public string punchTrigger = "LocomotiveSpecial";
    
    [Header("Freeze Settings")]
    public bool freezeWhileCharging = true;
    
    public override void Initialize(PartContext context)
    {
        context.CustomData["wasPressed"] = false;
        context.CustomData["isCharging"] = false;
        context.CustomData["chargeTime"] = 0f;
        context.CustomData["currentStage"] = 0;
        context.CustomData["stageTriggered"] = new bool[3];
        
        if (chargeVFX != null)
        {
            chargeVFX.Stop();
            chargeVFX.SetFloat(vfxAmountParameter, vfxAmountPerStage[0]);
        }
    }
    
    public override void OnExecute(PartContext context)
    {
        Debug.Log($"[ChargeComponent] OnExecute called!");
    }
    
    public override void OnUpdate(PartContext context, float deltaTime)
    {
        InputAction inputAction = context.CustomData["InputAction"] as InputAction;
        bool pressing = inputAction != null && inputAction.ReadValue<float>() > 0.5f;
        
        bool wasPressed = (bool)context.CustomData["wasPressed"];
        bool isCharging = (bool)context.CustomData["isCharging"];
        float chargeTime = (float)context.CustomData["chargeTime"];
        int currentStage = (int)context.CustomData["currentStage"];
        bool[] stageTriggered = (bool[])context.CustomData["stageTriggered"];
        
        // Just pressed to start charging (change)
        if (pressing && !wasPressed && !isCharging)
        {
            isCharging = true;
            chargeTime = 0f;
            currentStage = 0;
            stageTriggered = new bool[3];
            
            if (freezeWhileCharging && context.Rigidbody != null)
                context.Rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            
            if (context.Animator != null)
                context.Animator.SetBool(chargeBoolParameter, true);
            
            if (chargeVFX != null)
            {
                chargeVFX.Play();
                chargeVFX.SetFloat(vfxAmountParameter, vfxAmountPerStage[1]);
            }
        }
        
        // Charge while hold
        if (pressing && isCharging)
        {
            chargeTime += deltaTime;
            
            // Check for stages
            for (int i = 0; i < chargeThresholds.Length; i++)
            {
                if (!stageTriggered[i] && chargeTime >= chargeThresholds[i])
                {
                    currentStage = i + 1;
                    stageTriggered[i] = true;
                    
                    if (chargeVFX != null)
                        chargeVFX.SetFloat(vfxAmountParameter, vfxAmountPerStage[currentStage]);
                }
            }
        }
        
        // Release
        if (!pressing && wasPressed && isCharging)
        {
            if (freezeWhileCharging && context.Rigidbody != null)
                context.Rigidbody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            
            if (context.Animator != null)
            {
                context.Animator.SetBool(chargeBoolParameter, false);
                context.Animator.SetTrigger(punchTrigger);
            }
            
            // Calculate damage/knockback based on stage
            float damageMultiplier = currentStage > 0 ? stageDamageMultipliers[currentStage - 1] : 1f;
            float knockbackMultiplier = currentStage > 0 ? stageKnockbackMultipliers[currentStage - 1] : 1f;
            
            float finalDamage = baseDamage * damageMultiplier;
            float originalKnockback = knockbackForce;
            knockbackForce = knockbackForce * knockbackMultiplier;
            
            ActivateHitbox(context, customDamage: finalDamage);
            
            knockbackForce = originalKnockback;
            
            if (chargeVFX != null)
                chargeVFX.SetFloat(vfxAmountParameter, vfxAmountPerStage[3]);
            
            isCharging = false;
            chargeTime = 0f;
            currentStage = 0;
        }
        
        // Store state
        context.CustomData["wasPressed"] = pressing;
        context.CustomData["isCharging"] = isCharging;
        context.CustomData["chargeTime"] = chargeTime;
        context.CustomData["currentStage"] = currentStage;
        context.CustomData["stageTriggered"] = stageTriggered;
        
        // Reset VFX when idle
        if (!isCharging && chargeTime == 0f && chargeVFX != null)
            chargeVFX.SetFloat(vfxAmountParameter, vfxAmountPerStage[0]);
    }

}