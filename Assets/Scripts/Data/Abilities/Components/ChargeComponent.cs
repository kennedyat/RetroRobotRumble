using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

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
     

        if (chargeVFX != null)
            chargeVFX.Stop();
    }
    
    // Called once when the button is first pressed
    public override void OnExecute(PartContext context)
    {
        if (freezeWhileCharging && context.Rigidbody != null)
            context.Rigidbody.constraints = RigidbodyConstraints.FreezeAll;

        if (context.Animator != null)
            context.Animator.SetBool(chargeBoolParameter, true);

        if (chargeVFX != null)
            chargeVFX.Play();
    }

    // Called every frame while button is held
    public override void OnHeld(PartContext context, float heldDuration, float deltaTime)
    {
        for (int i = 0; i < chargeThresholds.Length; i++)
        {
            if (heldDuration >= chargeThresholds[i] && chargeVFX != null)
                chargeVFX.SetFloat(vfxAmountParameter, vfxAmountPerStage[i + 1]); //vfx placeholder
        }
    }

    // Called once when button is released
    public override void OnReleased(PartContext context, float heldDuration)
    {
     
        int stage = 0;
        for (int i = chargeThresholds.Length - 1; i >= 0; i--)
        {
            if (heldDuration >= chargeThresholds[i]) 
            { 
                stage = i + 1; 
                break; 
            }
        }

        float damage    = baseDamage      * (stage > 0 ? stageDamageMultipliers[stage - 1]   : 1f);
        float knockback = knockbackForce  * (stage > 0 ? stageKnockbackMultipliers[stage - 1] : 1f);

        if (freezeWhileCharging && context.Rigidbody != null)
            context.Rigidbody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

        if (context.Animator != null)
        {
            context.Animator.SetBool(chargeBoolParameter, false);
            context.Animator.SetTrigger(punchTrigger);
        }

        if (chargeVFX != null)
            chargeVFX.Stop();
        Debug.Log($"[ChargeComponent] Charge ended");
        ActivateHitbox(context, customDamage: damage, customKnockback: knockback);
    }

    public override void OnUpdate(PartContext context, float deltaTime) { }
}