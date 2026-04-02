using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Assets.Scripts.Combat.Robot;

[CreateAssetMenu(menuName = "ScriptableObjects/Components/LocomotiveSpecial")]
public class LocomotiveSpecialComponent : PartComponent
{
    [Header("Charge Settings")]
    [Tooltip("Fixed charge duration before auto-punch (in seconds)")]
    public float chargeDuration = 1f;
    
    [Tooltip("Maximum damage that can be mitigated to reach 3x multiplier")]
    public float maxMitigationDamage = 100f;
    
    [Header("Damage Scaling")]
    [Tooltip("Maximum damage multiplier (at max mitigation)")]
    public float maxDamageMultiplier = 3f;
    
    [Header("Punch Settings")]
    [Tooltip("How long the punch hitbox stays active")]
    public float punchDuration = 0.3f;
    
    [Header("Animation")]
    [Tooltip("Animation bool parameter for charging state")]
    public string chargeBoolParameter = "isCharging";
    
    [Tooltip("Animation trigger for punch")]
    public string punchTrigger = "LocomotiveSpecial";
    
    // Damage interceptor component
    private LocomotiveDamageInterceptor interceptorComponent;
    // Rotation lock component
    private LocomotiveRotationLock rotationLock;
    
    public override void Initialize(PartContext context)
    {
        context.CustomData["wasPressed"] = false;
        context.CustomData["isCharging"] = false;
        context.CustomData["chargeStartTime"] = 0f;
        context.CustomData["mitigatedDamage"] = 0f;
        context.CustomData["originalRigidbodyConstraints"] = RigidbodyConstraints.None;
        context.CustomData["originalRotation"] = Quaternion.identity;
        context.CustomData["combatRobot"] = null;
        
        // Store original rigidbody constraints
        if (context.Rigidbody != null)
        {
            context.CustomData["originalRigidbodyConstraints"] = context.Rigidbody.constraints;
        }
        
        // Find and store CombatRobot component for rotation control
        CombatRobot combatRobot = null;
        if (context.Owner != null)
        {
            combatRobot = context.Owner.GetComponent<CombatRobot>();
            if (combatRobot == null)
            {
                combatRobot = context.Owner.GetComponentInParent<CombatRobot>();
            }
            context.CustomData["combatRobot"] = combatRobot;
        }
        
        // Create rotation lock component
        if (context.Owner != null)
        {
            rotationLock = context.Owner.GetComponent<LocomotiveRotationLock>();
            if (rotationLock == null)
            {
                rotationLock = context.Owner.gameObject.AddComponent<LocomotiveRotationLock>();
            }
            if (combatRobot != null)
            {
                rotationLock.Initialize(combatRobot);
            }
        }
        
        // Create damage interceptor component if needed
        if (context.Owner != null)
        {
            interceptorComponent = context.Owner.GetComponent<LocomotiveDamageInterceptor>();
            if (interceptorComponent == null)
            {
                interceptorComponent = context.Owner.gameObject.AddComponent<LocomotiveDamageInterceptor>();
            }
            interceptorComponent.Initialize(this, context);
        }
    }
    
    public override void OnExecute(PartContext context)
    {
        // OnExecute is called when the ability is first triggered
        // We'll handle the actual charge/punch logic in OnUpdate based on input
        Debug.Log("[LocomotiveSpecial] OnExecute called");
    }
    
    public override void OnUpdate(PartContext context, float deltaTime)
    {
        if (context.Owner == null || context.Rigidbody == null)
            return;
        
        // Get input action
        InputAction inputAction = context.CustomData.ContainsKey("InputAction") 
            ? context.CustomData["InputAction"] as InputAction 
            : null;
        
        bool pressing = inputAction != null && inputAction.ReadValue<float>() > 0.5f;
        
        bool wasPressed = (bool)context.CustomData["wasPressed"];
        bool isCharging = (bool)context.CustomData["isCharging"];
        float chargeStartTime = (float)context.CustomData["chargeStartTime"];
        float mitigatedDamage = (float)context.CustomData["mitigatedDamage"];
        
        // Just pressed to start charging
        if (pressing && !wasPressed && !isCharging)
        {
            isCharging = true;
            mitigatedDamage = 0f;
            chargeStartTime = Time.time;
            
            // Freeze position and rotation completely (prevents movement and rotation)
            context.Rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            
            // Store original rotation and lock it
            Quaternion lockedRotation = context.Owner.rotation;
            context.CustomData["originalRotation"] = lockedRotation;
            
            // Lock rotation using helper component (prevents mouse rotation)
            if (rotationLock != null)
            {
                rotationLock.LockRotation(lockedRotation);
            }
            
            // Start intercepting damage
            StartDamageInterception(context);
            
            if (context.Animator != null)
            {
                context.Animator.SetBool(chargeBoolParameter, true);
            }
            
            Debug.Log($"[LocomotiveSpecial] Started charging for {chargeDuration} seconds");
        }
        
        // During charge - maintain frozen state and check for auto-punch
        if (isCharging)
        {
            // Keep rotation frozen (prevent direction change/mouse rotation)
            context.Rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            
            // Rotation lock is handled by LocomotiveRotationLock in LateUpdate
            // This ensures it runs after all input processing
            
            context.partInstance.ChangeState(PartState.Active);
            
            // Check if charge duration has elapsed
            if (Time.time - chargeStartTime >= chargeDuration)
            {
                // Auto-punch after charge duration
                ExecutePunch(context, mitigatedDamage);
                
                // Reset state
                isCharging = false;
                mitigatedDamage = 0f;
                context.CustomData["isCharging"] = false;
                context.CustomData["mitigatedDamage"] = 0f;
            }
        }
        
        // Store state
        context.CustomData["wasPressed"] = pressing;
        context.CustomData["isCharging"] = isCharging;
        context.CustomData["chargeStartTime"] = chargeStartTime;
        context.CustomData["mitigatedDamage"] = mitigatedDamage;
    }
    
    private void ExecutePunch(PartContext context, float mitigatedDamage)
    {
        // Unlock rotation FIRST (before restoring constraints)
        // This ensures rotation is free before we restore movement
        if (rotationLock != null)
        {
            rotationLock.UnlockRotation();
            Debug.Log("[LocomotiveSpecial] Rotation unlocked");
        }
        
        // Stop intercepting damage
        StopDamageInterception();
        
        // Calculate damage multiplier based on mitigated damage
        float damageMultiplier = CalculateDamageMultiplier(mitigatedDamage);
        float finalDamage = baseDamage * damageMultiplier;
        
        Debug.Log($"[LocomotiveSpecial] Auto-punch after {chargeDuration}s - Mitigated: {mitigatedDamage:F2}, Multiplier: {damageMultiplier:F2}x, Final Damage: {finalDamage:F2}");
        
        // Restore movement and rotation constraints
        RigidbodyConstraints originalConstraints = (RigidbodyConstraints)context.CustomData["originalRigidbodyConstraints"];
        context.Rigidbody.constraints = originalConstraints;
        
        // Clear any accumulated rotation values to start fresh
        CombatRobot combatRobot = context.CustomData["combatRobot"] as CombatRobot;
        if (combatRobot != null)
        {
            combatRobot.yawDelta = 0f;
            combatRobot.yawRotationalVelocity = 0f;
            Debug.Log("[LocomotiveSpecial] Cleared rotation values, rotation should be free now");
        }
        
        // Activate punch hitbox
        HitBox specialHitbox = context.HitBox;
        if (specialHitbox != null)
        {
            specialHitbox.OnHit = (Collider target) => OnPunchHit(target, finalDamage, knockbackForce, context);
            specialHitbox.EnableFrame(punchDuration);
            Debug.Log($"[LocomotiveSpecial] Activated punch hitbox for {punchDuration} seconds with {finalDamage} damage");
        }
        else
        {
            Debug.LogWarning("[LocomotiveSpecial] Special hitbox is null! Cannot activate punch.");
        }
        
        // Play punch animation
        if (context.Animator != null)
        {
            context.Animator.SetBool(chargeBoolParameter, false);
            context.Animator.SetTrigger(punchTrigger);
        }
    }
    
    private float CalculateDamageMultiplier(float mitigatedDamage)
    {
        // Linear scaling from 1x (0 damage) to maxDamageMultiplier (maxMitigationDamage)
        float mitigationPercent = Mathf.Clamp01(mitigatedDamage / maxMitigationDamage);
        return Mathf.Lerp(1f, maxDamageMultiplier, mitigationPercent);
    }
    
    private void StartDamageInterception(PartContext context)
    {
        if (interceptorComponent != null)
        {
            interceptorComponent.SetIntercepting(true);
            Debug.Log("[LocomotiveSpecial] Started damage interception");
        }
    }
    
    private void StopDamageInterception()
    {
        if (interceptorComponent != null)
        {
            interceptorComponent.SetIntercepting(false);
            Debug.Log("[LocomotiveSpecial] Stopped damage interception");
        }
    }
    
    // Called by LocomotiveDamageInterceptor
    public bool TryMitigateDamage(PartContext context, float damageAmount, Vector3 damageSourcePosition)
    {
        if (context.Owner == null)
            return false;
        
        // Check if damage is coming from front 180 degrees
        Vector3 playerPosition = context.Owner.position;
        Vector3 playerForward = context.Owner.forward;
        Vector3 toDamageSource = (damageSourcePosition - playerPosition).normalized;
        
        // Calculate angle between player forward and damage source direction
        float angle = Vector3.Angle(playerForward, toDamageSource);
        
        // Front 180 degrees means angle <= 90 degrees (half of 180)
        if (angle <= 90f)
        {
            // Mitigate this damage
            float mitigatedDamage = (float)context.CustomData["mitigatedDamage"];
            mitigatedDamage += damageAmount;
            context.CustomData["mitigatedDamage"] = mitigatedDamage;
            
            Debug.Log($"[LocomotiveSpecial] Mitigated {damageAmount} damage from front (angle: {angle:F1}°). Total mitigated: {mitigatedDamage:F2}");
            
            return true; // Damage was mitigated
        }
        
        return false; // Damage not from front, don't mitigate
    }
    
    private void OnPunchHit(Collider target, float damage, float knockback, PartContext context)
    {
        if (!target.CompareTag("Enemy"))
            return;
        
        // Deal damage
        var enemy = target.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.DealDamage((int)damage);
        }
        
        // Apply knockback
        if (knockback > 0 && context.Owner != null)
        {
            var enemyRb = target.GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                Vector3 knockbackDir = (target.transform.position - context.Owner.position).normalized;
                enemyRb.AddForce(knockbackDir * knockback, ForceMode.Impulse);
            }
        }
        
        // Play hit sound and VFX
        if (hitSound != null && context.Owner != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, target.transform.position);
        }
        
        if (hitVFX != null)
        {
            GameObject.Instantiate(hitVFX, target.transform.position, Quaternion.identity);
        }
        
        Debug.Log($"[LocomotiveSpecial] Punched {target.name} for {damage} damage");
    }
}
