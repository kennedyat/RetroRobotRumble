using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "ScriptableObjects/Components/LocomotiveChargeDash")]
public class LocomotiveChargeDashComponent : PartComponent
{
    [Header("Charge Settings")]
    [Tooltip("Time to reach full charge (1 second)")]
    public float fullChargeTime = 1f;
    
    [Header("Dash Settings")]
    [Tooltip("Minimum dash distance (at 0% charge)")]
    public float minDashDistance = 2f;
    
    [Tooltip("Maximum dash distance (at 100% charge)")]
    public float maxDashDistance = 8f;
    
    [Tooltip("Dash speed (how fast the dash moves)")]
    public float dashSpeed = 20f;
    
    [Header("Knockback Settings")]
    [Tooltip("Knockback force applied sideways to enemies")]
    public float sidewaysKnockbackForce = 10f;
    
    [Header("Animation")]
    [Tooltip("Animation bool parameter for charging state")]
    public string chargeBoolParameter = "isCharging";
    
    [Tooltip("Animation trigger for dash")]
    public string dashTrigger = "LocomotiveNormal";

    [Header("Cooldown")]
    [Tooltip("Cooldown after a dash completes before the next charge/dash can start.")]
    [Min(0f)]
    public float cooldownAfterDash = 0.35f;
    
    public override void Initialize(PartContext context)
    {
        context.CustomData["wasPressed"] = false;
        context.CustomData["isCharging"] = false;
        context.CustomData["isDashing"] = false;
        context.CustomData["chargeTime"] = 0f;
        context.CustomData["dashTimeRemaining"] = 0f;
        context.CustomData["dashDirection"] = Vector3.zero;
        context.CustomData["dashStartPosition"] = Vector3.zero;
        context.CustomData["originalRigidbodyConstraints"] = RigidbodyConstraints.None;
        
        // Store original rigidbody constraints
        if (context.Rigidbody != null)
        {
            context.CustomData["originalRigidbodyConstraints"] = context.Rigidbody.constraints;
        }
    }
    
    public override void OnExecute(PartContext context)
    {
        // Execute() always applies PartInstance cooldown + Active state. This arm drives combat from OnUpdate
        // via polled input, so clear that window here and return to Ready — real cooldown is only applied
        // after dash ends (OverrideCooldown). Without this, charge could ignore dash-end cooldown entirely.
        if (context.partInstance != null)
        {
            context.partInstance.OverrideCooldown(0f);
            context.partInstance.ChangeState(PartState.Ready);
        }

        Debug.Log("[LocomotiveChargeDash] OnExecute called");
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
        bool isDashing = (bool)context.CustomData["isDashing"];
        float chargeTime = (float)context.CustomData["chargeTime"];
        float dashTimeRemaining = (float)context.CustomData["dashTimeRemaining"];
        
        // Handle dashing state (priority over charging)
        if (isDashing && dashTimeRemaining > 0)
        {
            Vector3 dashDirection = (Vector3)context.CustomData["dashDirection"];
            Vector3 dashStartPosition = (Vector3)context.CustomData["dashStartPosition"];
            
            // Calculate dash progress
            float dashDuration = CalculateDashDuration(chargeTime);
            float progress = 1f - (dashTimeRemaining / dashDuration);
            
            // Calculate target position
            float dashDistance = CalculateDashDistance(chargeTime);
            Vector3 targetPosition = dashStartPosition + dashDirection * dashDistance;
            
            // Interpolate position
            Vector3 currentPosition = Vector3.Lerp(dashStartPosition, targetPosition, progress);
            context.Rigidbody.MovePosition(currentPosition);
            
            dashTimeRemaining -= deltaTime;
            context.CustomData["dashTimeRemaining"] = dashTimeRemaining;
            
            // When dash completes, restore movement
            if (dashTimeRemaining <= 0)
            {
                isDashing = false;
                context.CustomData["isDashing"] = false;
                
                // Disable hitbox when dash completes
                HitBox hitbox = context.HitBox;
                if (hitbox != null)
                {
                    hitbox.DisableFrame();
                    Debug.Log("[LocomotiveChargeDash] Disabled hitbox after dash");
                }
                
                // Restore original rigidbody constraints
                RigidbodyConstraints originalConstraints = (RigidbodyConstraints)context.CustomData["originalRigidbodyConstraints"];
                context.Rigidbody.constraints = originalConstraints;
                
                // Reset charge state
                isCharging = false;
                chargeTime = 0f;
                context.CustomData["isCharging"] = false;
                context.CustomData["chargeTime"] = 0f;
                
                if (context.Animator != null)
                {
                    context.Animator.SetBool(chargeBoolParameter, false);
                }

                if (context.partInstance != null)
                    context.partInstance.OverrideCooldown(cooldownAfterDash);
                
                Debug.Log("[LocomotiveChargeDash] Dash completed");
            }
            
            // Store state
            context.CustomData["wasPressed"] = pressing;
            return;
        }
        
        // Just pressed to start charging — must respect PartInstance cooldown (set after dash completes).
        if (pressing && !wasPressed && !isCharging && !isDashing)
        {
            bool canStartCharge = context.partInstance == null || context.partInstance.CanUse;
            if (canStartCharge)
            {
                isCharging = true;
                chargeTime = 0f;

                // Freeze position (X and Z) but allow rotation and Y movement
                // Note: Rotation is NOT frozen so player can change direction freely
                context.Rigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;

                if (context.Animator != null)
                {
                    context.Animator.SetBool(chargeBoolParameter, true);
                }

                Debug.Log("[LocomotiveChargeDash] Started charging");
            }
        }
        
        // Charge while holding
        if (pressing && isCharging && !isDashing)
        {
            chargeTime += deltaTime;
            
            // Cap charge time at full charge
            chargeTime = Mathf.Clamp(chargeTime, 0f, fullChargeTime);
            
            // Allow rotation while charging (update forward direction)
            // Rotation is handled by player controller, we just don't freeze it
            
            context.partInstance.ChangeState(PartState.Active);
        }
        
        // Release to dash - handles both normal release and long hold cases
        // Check if button was released while charging (works even if held past max charge time)
        if (!pressing && isCharging && !isDashing && (wasPressed || chargeTime > 0f))
        {
            // Ensure chargeTime is clamped (in case it somehow exceeded max)
            chargeTime = Mathf.Clamp(chargeTime, 0f, fullChargeTime);
            
            // Calculate dash direction (player's forward direction at release)
            Vector3 dashDirection = context.Owner.forward;
            Vector3 dashStartPosition = context.Owner.position;
            
            // Calculate dash distance based on charge
            float dashDistance = CalculateDashDistance(chargeTime);
            float dashDuration = CalculateDashDuration(chargeTime);
            
            Debug.Log($"[LocomotiveChargeDash] Release detected - chargeTime: {chargeTime:F2}, dashDistance: {dashDistance:F2}, dashDuration: {dashDuration:F2}");
            
            // Start dashing
            isDashing = true;
            isCharging = false;
            dashTimeRemaining = dashDuration;
            
            // Store dash state
            context.CustomData["isDashing"] = true;
            context.CustomData["isCharging"] = false;
            context.CustomData["dashTimeRemaining"] = dashTimeRemaining;
            context.CustomData["dashDirection"] = dashDirection;
            context.CustomData["dashStartPosition"] = dashStartPosition;
            
            // Unfreeze position for dashing (but keep rotation frozen)
            context.Rigidbody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            
            // Activate hitbox immediately when dash starts
            HitBox hitbox = context.HitBox;
            if (hitbox != null)
            {
                hitbox.OnHit = (Collider target) => OnHitboxHit(target, baseDamage, knockbackForce, context);
                hitbox.EnableFrame(dashDuration);
                Debug.Log($"[LocomotiveChargeDash] Activated hitbox for {dashDuration:F2} seconds");
            }
            else
            {
                Debug.LogWarning("[LocomotiveChargeDash] Hitbox is null, cannot activate!");
            }
            
            // Play dash animation
            if (context.Animator != null)
            {
                context.Animator.SetBool(chargeBoolParameter, false);
                context.Animator.SetTrigger(dashTrigger);
            }
            
            Debug.Log($"[LocomotiveChargeDash] Released at {chargeTime:F2}s charge, dashing {dashDistance:F2} units");
        }
        
        // Store state
        context.CustomData["wasPressed"] = pressing;
        context.CustomData["isCharging"] = isCharging;
        context.CustomData["chargeTime"] = chargeTime;
    }
    
    private float CalculateDashDistance(float chargeTime)
    {
        float chargePercent = Mathf.Clamp01(chargeTime / fullChargeTime);
        return Mathf.Lerp(minDashDistance, maxDashDistance, chargePercent);
    }
    
    private float CalculateDashDuration(float chargeTime)
    {
        float dashDistance = CalculateDashDistance(chargeTime);
        return dashDistance / dashSpeed;
    }
    
    protected override void OnHitboxHit(Collider target, float damage, float knockback, PartContext context)
    {
        if (!target.CompareTag("Enemy"))
            return;
        
        // Deal damage
        var enemy = target.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.DealDamage((int)damage);
        }
        
        // Apply sideways knockback (perpendicular to dash direction)
        if (sidewaysKnockbackForce > 0 && context.Owner != null)
        {
            Vector3 dashDirection = (Vector3)context.CustomData["dashDirection"];
            
            // Calculate perpendicular direction (sideways)
            // Use cross product with up vector to get a perpendicular direction
            Vector3 sidewaysDirection = Vector3.Cross(dashDirection, Vector3.up).normalized;
            
            // Randomly choose left or right (or use a consistent method)
            // For now, we'll use a simple method: alternate based on enemy position
            Vector3 toEnemy = (target.transform.position - context.Owner.position).normalized;
            float dot = Vector3.Dot(sidewaysDirection, toEnemy);
            
            // If enemy is on the "negative" side, flip direction
            if (dot < 0)
            {
                sidewaysDirection = -sidewaysDirection;
            }
            
            var enemyRb = target.GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                enemyRb.AddForce(sidewaysDirection * sidewaysKnockbackForce, ForceMode.Impulse);
                Debug.Log($"[LocomotiveChargeDash] Knocked {target.name} sideways with force {sidewaysKnockbackForce}");
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
        
        Debug.Log($"[LocomotiveChargeDash] Hit {target.name} for {damage} damage");
    }
}
