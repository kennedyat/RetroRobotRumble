using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Components/OniSamuraiCombo")]
public class OniSamuraiComboComponent : PartComponent
{
    [Header("Combo Settings")]
    [Tooltip("Maximum combo hits (5-hit combo)")]
    public int maxComboHits = 5;
    
    [Tooltip("Time before combo resets if no input (seconds)")]
    public float comboResetTime = 0.15f;
    
    [Tooltip("Delay between each slash (seconds)")]
    public float slashDelay = 0.3f;

    [Header("Cooldown")]
    [Tooltip("Internal cooldown used between slashes inside one combo string")]
    [Min(0f)]
    public float comboStepInternalCooldown = 0.01f;

    [Tooltip("Internal cooldown applied after the final slash before a new combo starts")]
    [Min(0f)]
    public float comboResetInternalCooldown = 0.6f;
    
    [Tooltip("Attack range for all slashes")]
    public float attackRange = 3f;
    
    [Header("Combo Damage")]
    [Tooltip("Damage for slashes 1-2")]
    public float slash12Damage = 10f;
    
    [Tooltip("Damage for slashes 3-4")]
    public float slash34Damage = 13f;
    
    [Tooltip("Damage for slash 5 (final)")]
    public float slash5Damage = 13f;
    
    [Header("Combo Knockback")]
    [Tooltip("Knockback force for slashes 1-4")]
    public float normalKnockbackForce = 2f;
    
    [Tooltip("Large knockback force for slash 5")]
    public float finalKnockbackForce = 10f;
    
    [Header("Combo Angles")]
    [Tooltip("Angle for slashes 1-2 (half of total arc, so 50 = 100 degree arc)")]
    public float slash12Angle = 50f;
    
    [Tooltip("Angle for slashes 3-4 (half of total arc, so 90 = 180 degree arc)")]
    public float slash34Angle = 90f;
    
    [Header("Animation Triggers")]
    public string combo1Trigger = "OniSamuraiCombo1";
    public string combo2Trigger = "OniSamuraiCombo2";
    public string combo3Trigger = "OniSamuraiCombo3";
    public string combo4Trigger = "OniSamuraiCombo4";
    public string combo5Trigger = "OniSamuraiCombo5";
    
    public override void Initialize(PartContext context)
    {
        context.CustomData["ComboCount"] = 0;
        context.CustomData["BufferedInputs"] = 0;
        context.CustomData["SlashDelayTimer"] = 0f;
        context.CustomData["IsAttacking"] = false;
        context.CustomData["IsWaitingForNextSlash"] = false;
        context.CustomData["CurrentSlashDamage"] = 0f;
        context.CustomData["CurrentSlashKnockback"] = 0f;
        context.CustomData["CurrentSlashAngle"] = 0f;
        context.CustomData["QueuedSlash"] = 0;
        
        ApplyComboCooldown(context, isFinisher: false);
    }
    
    public override void OnExecute(PartContext context)
    {
        int comboCount = (int)context.CustomData["ComboCount"];
        bool isAttacking = (bool)context.CustomData["IsAttacking"];
        int bufferedInputs = (int)context.CustomData["BufferedInputs"];
        
        Debug.Log($"[OniSamuraiCombo] OnExecute START - Combo: {comboCount}, IsAttacking: {isAttacking}, BufferedInputs: {bufferedInputs}");

        // If we're mid-slash, buffer input and consume it in order later.
        // This prevents skipping directly to later combo stages when button is spammed quickly.
        if (isAttacking)
        {
            bufferedInputs = Mathf.Min(bufferedInputs + 1, maxComboHits);
            context.CustomData["BufferedInputs"] = bufferedInputs;
            ApplyComboCooldown(context, isFinisher: false);
            Debug.Log($"[OniSamuraiCombo] Buffered input. BufferedInputs: {bufferedInputs}");
            return;
        }
        
        // Increment combo (1-5), then reset to 1 after 5
        if (comboCount < maxComboHits)
        {
            comboCount++;
        }
        else
        {
            // Combo is at max, reset it back to 1
            comboCount = 1;
        }
        
        Debug.Log($"[OniSamuraiCombo] OnExecute - Combo set to {comboCount}");
        
        // Always start the slash - if already attacking, it will queue for the next one
        // The slash will start immediately if not attacking, or when current slash finishes
        StartSlash(context, comboCount);
        
        ApplyComboCooldown(context, isFinisher: comboCount == maxComboHits);
        
        // Store updated values
        context.CustomData["ComboCount"] = comboCount;
        
        Debug.Log($"[OniSamuraiCombo] OnExecute END - Final Combo: {comboCount}");
    }
    
    private void StartSlash(PartContext context, int comboCount)
    {
        bool isAttacking = (bool)context.CustomData["IsAttacking"];
        
        // If already attacking, queue this slash for when current one finishes
        if (isAttacking)
        {
            // Mark that we have a queued slash
            context.CustomData["QueuedSlash"] = comboCount;
            Debug.Log($"[OniSamuraiCombo] Queued slash {comboCount} - current attack in progress");
            return;
        }
        
        // Start the slash immediately
        context.CustomData["IsAttacking"] = true;
        context.CustomData["IsWaitingForNextSlash"] = false;
        context.CustomData["SlashDelayTimer"] = slashDelay;
        context.CustomData["HitboxActivated"] = false;
        context.CustomData["QueuedSlash"] = 0; // Clear any queued slash
        
        float damage = 0f;
        float knockback = 0f;
        float angle = 0f;
        
        // Set damage and parameters based on combo count
        switch (comboCount)
        {
            case 1:
            case 2:
                // Slash 1 & 2: ~100 degrees (50 degrees each side), 10 damage
                damage = slash12Damage;
                knockback = normalKnockbackForce;
                angle = slash12Angle;
                break;
            case 3:
            case 4:
                // Slash 3 & 4: 180 degrees (90 degrees each side), 13 damage
                damage = slash34Damage;
                knockback = normalKnockbackForce;
                angle = slash34Angle;
                break;
            case 5:
                // Slash 5: Full circle (360 degrees), large knockback
                damage = slash5Damage;
                knockback = finalKnockbackForce;
                angle = 180f; // Full circle (check all directions)
                break;
        }
        
        context.CustomData["CurrentSlashDamage"] = damage;
        context.CustomData["CurrentSlashKnockback"] = knockback;
        context.CustomData["CurrentSlashAngle"] = angle;
        
        // Play animation
        PlayAnimation(context, comboCount);
        
        // If slashDelay is 0, activate hitbox immediately
        if (slashDelay <= 0)
        {
            // Get the appropriate hitbox based on combo count
            HitBox selectedHitbox = GetHitboxForCombo(context, comboCount);
            
            Debug.Log($"[OniSamuraiCombo] StartSlash - Combo: {comboCount}, SelectedHitbox: {(selectedHitbox != null ? selectedHitbox.name : "NULL")}, slashDelay: {slashDelay}");
            
            if (selectedHitbox != null)
            {
                // Set the context's HitBox to the selected one BEFORE calling ActivateHitbox
                context.HitBox = selectedHitbox;
                
                Debug.Log($"[OniSamuraiCombo] StartSlash - Setting context.HitBox to {selectedHitbox.name} for combo {comboCount}");
                
                // Activate and enable hitbox directly
                ActivateHitbox(context, 
                    customDamage: damage,
                    customKnockback: knockback);
                
                // Verify the hitbox was set correctly
                if (context.hitBoxManager != null && HitBoxManager.currentHitbox != null)
                {
                    Debug.Log($"[OniSamuraiCombo] HitBoxManager.currentHitbox is now: {HitBoxManager.currentHitbox.name}");
                    
                    // Enable the hitbox directly through the manager
                    context.hitBoxManager.Enable();
                    Debug.Log($"[OniSamuraiCombo] Activated and enabled hitbox {GetHitboxName(comboCount)} ({selectedHitbox.name}) for combo {comboCount} immediately (slashDelay=0)");
                }
                else
                {
                    Debug.LogError($"[OniSamuraiCombo] HitBoxManager is NULL or currentHitbox is NULL! Cannot enable hitbox.");
                }
                
                context.CustomData["HitboxActivated"] = true;
                context.CustomData["ActiveHitbox"] = selectedHitbox;
                context.CustomData["HitboxStartTime"] = Time.time; // Track when hitbox was activated
            }
            else
            {
                Debug.LogError($"[OniSamuraiCombo] GetHitboxForCombo returned NULL for combo {comboCount}!");
            }
        }
    }
    
    public override void OnUpdate(PartContext context, float deltaTime)
    {
        int comboCount = (int)context.CustomData["ComboCount"];
        float slashDelayTimer = (float)context.CustomData["SlashDelayTimer"];
        bool isAttacking = (bool)context.CustomData["IsAttacking"];
        bool isWaitingForNextSlash = (bool)context.CustomData["IsWaitingForNextSlash"];
        
        // Check if special input was pressed (reset combo)
        // This is set by ArmBehavior when special input is detected
        if (context.CustomData.ContainsKey("SpecialInputPressed") && (bool)context.CustomData["SpecialInputPressed"])
        {
            comboCount = 0;
            isAttacking = false;
            isWaitingForNextSlash = false;
            slashDelayTimer = 0f;
            
            HitBox hitboxToDisable = context.CustomData.ContainsKey("ActiveHitbox") 
                ? context.CustomData["ActiveHitbox"] as HitBox 
                : context.HitBox;
            
            if (hitboxToDisable != null)
            {
                hitboxToDisable.DisableFrame();
            }
            
            context.CustomData["ActiveHitbox"] = null;
            context.CustomData["SpecialInputPressed"] = false;
        }
        
        // Handle slash delay and hitbox activation (no animation events needed)
        if (isAttacking)
        {
            bool hitboxActivated = context.CustomData.ContainsKey("HitboxActivated") && (bool)context.CustomData["HitboxActivated"];
            
            // If hitbox not yet activated, check if delay has passed
            if (!hitboxActivated)
            {
                // Count down the delay timer
                if (slashDelayTimer > 0)
                {
                    slashDelayTimer -= deltaTime;
                }
                
                // Activate hitbox when delay reaches 0 (or immediately if delay is 0)
                if (slashDelayTimer <= 0)
                {
                    // Get the appropriate hitbox based on combo count
                    HitBox selectedHitbox = GetHitboxForCombo(context, comboCount);
                    
                    if (selectedHitbox != null)
                    {
                        // Set the context's HitBox to the selected one BEFORE calling ActivateHitbox
                        context.HitBox = selectedHitbox;
                        
                        Debug.Log($"[OniSamuraiCombo] OnUpdate - Setting context.HitBox to {selectedHitbox.name} for combo {comboCount}");
                        
                        // Activate and enable hitbox directly (no animation events needed)
                        ActivateHitbox(context, 
                            customDamage: (float)context.CustomData["CurrentSlashDamage"],
                            customKnockback: (float)context.CustomData["CurrentSlashKnockback"]);
                        
                        // Verify the hitbox was set correctly
                        if (context.hitBoxManager != null && HitBoxManager.currentHitbox != null)
                        {
                            Debug.Log($"[OniSamuraiCombo] HitBoxManager.currentHitbox is now: {HitBoxManager.currentHitbox.name}");
                            
                            // Enable the hitbox directly through the manager
                            context.hitBoxManager.Enable();
                            Debug.Log($"[OniSamuraiCombo] Activated and enabled hitbox {GetHitboxName(comboCount)} ({selectedHitbox.name}) for combo {comboCount} (no animation)");
                        }
                        else
                        {
                            Debug.LogWarning($"[OniSamuraiCombo] HitBoxManager is null or currentHitbox is null! Cannot enable hitbox.");
                        }
                        
                        context.CustomData["HitboxActivated"] = true;
                        context.CustomData["ActiveHitbox"] = selectedHitbox; // Store which hitbox is active
                        context.CustomData["HitboxStartTime"] = Time.time; // Track when hitbox was activated
                    }
                    else
                    {
                        Debug.LogWarning($"[OniSamuraiCombo] No hitbox found for combo {comboCount}! Using default hitbox.");
                        // Fallback to default hitbox
                        ActivateHitbox(context, 
                            customDamage: (float)context.CustomData["CurrentSlashDamage"],
                            customKnockback: (float)context.CustomData["CurrentSlashKnockback"]);
                        
                        if (context.hitBoxManager != null)
                        {
                            context.hitBoxManager.Enable();
                        }
                        
                        context.CustomData["HitboxActivated"] = true;
                    }
                }
            }
        }
        
        // Auto-disable hitbox after it's been active for a bit (if no enemy was hit)
        // Check the active hitbox (stored in CustomData) or fallback to context.HitBox
        HitBox activeHitbox = context.CustomData.ContainsKey("ActiveHitbox") 
            ? context.CustomData["ActiveHitbox"] as HitBox 
            : context.HitBox;
        
        // Check if hitbox expired (was active but is now inactive)
        // OR if hitbox was never activated but enough time has passed (for slashDelay > 0 case)
        bool hitboxExpired = false;
        if (isAttacking && slashDelayTimer <= 0)
        {
            if (activeHitbox != null)
            {
                // Hitbox was active but is now inactive
                if (!activeHitbox.isActive && (bool)context.CustomData["HitboxActivated"])
                {
                    hitboxExpired = true;
                }
            }
            else if ((bool)context.CustomData["HitboxActivated"])
            {
                // Hitbox was marked as activated but is now null (shouldn't happen, but handle it)
                hitboxExpired = true;
            }
            // If slashDelay was 0, hitbox should have activated immediately, so check if it expired
            // Use a small timer to detect when hitbox duration (0.2s) has passed
            else if (slashDelay <= 0 && context.CustomData.ContainsKey("HitboxStartTime"))
            {
                float hitboxStartTime = (float)context.CustomData["HitboxStartTime"];
                if (Time.time - hitboxStartTime > hitboxDuration)
                {
                    hitboxExpired = true;
                }
            }
        }
        
        if (hitboxExpired)
        {
            // Hitbox expired, end this attack
            isAttacking = false;
            isWaitingForNextSlash = true;
            context.CustomData["HitboxActivated"] = false;
            context.CustomData["ActiveHitbox"] = null;
            context.CustomData["HitboxStartTime"] = null;
            
            ApplyComboCooldown(context, isFinisher: comboCount == maxComboHits);
            
            Debug.Log($"[OniSamuraiCombo] Slash {comboCount} finished (hitbox expired), ready for next input");
            
            // Consume one buffered input (if any) and continue combo sequentially.
            int bufferedInputs = (int)context.CustomData["BufferedInputs"];
            if (bufferedInputs > 0)
            {
                bufferedInputs--;
                context.CustomData["BufferedInputs"] = bufferedInputs;

                int nextCombo = comboCount < maxComboHits ? comboCount + 1 : 1;
                context.CustomData["ComboCount"] = nextCombo;
                StartSlash(context, nextCombo);
                Debug.Log($"[OniSamuraiCombo] Starting buffered slash {nextCombo}. Remaining buffered: {bufferedInputs}");
            }
        }
        
        // Store updated values
        context.CustomData["ComboCount"] = comboCount;
        context.CustomData["SlashDelayTimer"] = slashDelayTimer;
        context.CustomData["IsAttacking"] = isAttacking;
        context.CustomData["IsWaitingForNextSlash"] = isWaitingForNextSlash;
    }
    
    protected override void OnHitboxHit(Collider target, float damage, float knockback, PartContext context)
    {
        // Check if enemy is within angle and range
        if (!IsEnemyInAttackRange(target, context))
            return;
        
        int comboCount = (int)context.CustomData["ComboCount"];
        float currentSlashDamage = (float)context.CustomData["CurrentSlashDamage"];
        float currentSlashKnockback = (float)context.CustomData["CurrentSlashKnockback"];
        
        // Deal damage
        var enemy = target.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.DealDamage((int)currentSlashDamage);
        }
        
        // Apply knockback
        if (knockback > 0 && context.Owner != null)
        {
            var enemyRb = target.GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                Vector3 knockbackDirection;
                
                if (comboCount == 5)
                {
                    // Slash 5: Knockback away from player in all directions
                    knockbackDirection = (target.transform.position - context.Owner.position).normalized;
                }
                else
                {
                    // Slashes 1-4: Knockback in player's forward direction
                    knockbackDirection = context.Owner.forward;
                }
                
                enemyRb.AddForce(knockbackDirection * currentSlashKnockback, ForceMode.Impulse);
            }
        }
        
        // Call base to play hit sound and VFX
        base.OnHitboxHit(target, currentSlashDamage, currentSlashKnockback, context);
        
                // End this slash and prepare for next one
                float slashDelayTimer = (float)context.CustomData["SlashDelayTimer"];
                if (slashDelayTimer <= 0)
                {
                    context.CustomData["IsAttacking"] = false;
                    context.CustomData["IsWaitingForNextSlash"] = true;
                    context.CustomData["HitboxActivated"] = false;
                    
                    ApplyComboCooldown(context, isFinisher: comboCount == maxComboHits);
                    
                    // Disable the active hitbox (use stored reference or context.HitBox)
                    HitBox activeHitbox = context.CustomData.ContainsKey("ActiveHitbox") 
                        ? context.CustomData["ActiveHitbox"] as HitBox 
                        : context.HitBox;
                    
                    if (activeHitbox != null)
                    {
                        activeHitbox.DisableFrame();
                    }
                    
                    context.CustomData["ActiveHitbox"] = null; // Clear active hitbox reference
                    
                    // Consume one buffered input (if any) and continue combo sequentially.
                    int bufferedInputs = (int)context.CustomData["BufferedInputs"];
                    if (bufferedInputs > 0)
                    {
                        bufferedInputs--;
                        context.CustomData["BufferedInputs"] = bufferedInputs;

                        int nextCombo = comboCount < maxComboHits ? comboCount + 1 : 1;
                        context.CustomData["ComboCount"] = nextCombo;
                        StartSlash(context, nextCombo);
                        Debug.Log($"[OniSamuraiCombo] Starting buffered slash {nextCombo} after hit. Remaining buffered: {bufferedInputs}");
                    }
                    else
                    {
                        Debug.Log($"[OniSamuraiCombo] Slash {comboCount} completed, ready for next input");
                    }
                }
    }
    
    /// <summary>
    /// Gets the appropriate hitbox for the given combo count.
    /// Combo 1-2: Uses ComboHitBox1 (smaller angle)
    /// Combo 3-4: Uses ComboHitBox2 (wider angle)
    /// Combo 5: Uses ComboHitBox3 (full circle/spin)
    /// </summary>
    private HitBox GetHitboxForCombo(PartContext context, int comboCount)
    {
        Debug.Log($"[OniSamuraiCombo] GetHitboxForCombo - Combo: {comboCount}");
        
        // Check if ComboArmBehavior has set up the hitboxes in CustomData
        if (comboCount >= 1 && comboCount <= 2)
        {
            // Slashes 1-2: Use first hitbox (smaller angle)
            if (context.CustomData.ContainsKey("ComboHitBox1"))
            {
                HitBox hitbox = context.CustomData["ComboHitBox1"] as HitBox;
                Debug.Log($"[OniSamuraiCombo] Selected ComboHitBox1 for combo {comboCount}");
                return hitbox;
            }
            else
            {
                Debug.LogWarning($"[OniSamuraiCombo] ComboHitBox1 not found in CustomData for combo {comboCount}");
            }
        }
        else if (comboCount >= 3 && comboCount <= 4)
        {
            // Slashes 3-4: Use second hitbox (wider angle)
            if (context.CustomData.ContainsKey("ComboHitBox2"))
            {
                HitBox hitbox = context.CustomData["ComboHitBox2"] as HitBox;
                Debug.Log($"[OniSamuraiCombo] Selected ComboHitBox2 for combo {comboCount}");
                return hitbox;
            }
            else
            {
                Debug.LogWarning($"[OniSamuraiCombo] ComboHitBox2 not found in CustomData for combo {comboCount}");
            }
        }
        else if (comboCount == 5)
        {
            // Slash 5: Use third hitbox (full circle/spin)
            if (context.CustomData.ContainsKey("ComboHitBox3"))
            {
                HitBox hitbox = context.CustomData["ComboHitBox3"] as HitBox;
                if (hitbox != null)
                {
                    Debug.Log($"[OniSamuraiCombo] Selected ComboHitBox3 for combo {comboCount} - Hitbox: {hitbox.name}");
                    return hitbox;
                }
                else
                {
                    Debug.LogError($"[OniSamuraiCombo] ComboHitBox3 is NULL in CustomData! Make sure comboHitBox3 GameObject is assigned in ComboArmBehavior Inspector.");
                }
            }
            else
            {
                Debug.LogWarning($"[OniSamuraiCombo] ComboHitBox3 not found in CustomData for combo {comboCount}");
            }
        }
        
        // Fallback to default hitbox if combo hitboxes aren't available
        Debug.LogWarning($"[OniSamuraiCombo] Falling back to default hitbox for combo {comboCount}");
        return context.HitBox;
    }
    
    /// <summary>
    /// Gets a descriptive name for the hitbox being used (for debugging).
    /// </summary>
    private string GetHitboxName(int comboCount)
    {
        if (comboCount >= 1 && comboCount <= 2)
            return "ComboHitBox1 (Small Angle)";
        else if (comboCount >= 3 && comboCount <= 4)
            return "ComboHitBox2 (Wide Angle)";
        else if (comboCount == 5)
            return "ComboHitBox3 (Full Circle)";
        else
            return "Default";
    }
    
    private bool IsEnemyInAttackRange(Collider enemy, PartContext context)
    {
        if (context.Owner == null) return false;
        
        Vector3 playerPosition = context.Owner.position;
        Vector3 enemyPosition = enemy.transform.position;
        Vector3 playerForward = context.Owner.forward;
        
        // Check distance
        float distance = Vector3.Distance(playerPosition, enemyPosition);
        if (distance > attackRange)
            return false;
        
        int comboCount = (int)context.CustomData["ComboCount"];
        float currentSlashAngle = (float)context.CustomData["CurrentSlashAngle"];
        
        // For slash 5 (full circle), accept all enemies in range
        if (comboCount == 5)
            return true;
        
        // For other slashes, check angle
        Vector3 toEnemy = (enemyPosition - playerPosition).normalized;
        float angle = Vector3.Angle(playerForward, toEnemy);
        
        return angle <= currentSlashAngle;
    }
    
    private void PlayAnimation(PartContext context, int comboCount)
    {
        if (context.Animator == null)
            return;
        
        // Play appropriate animation based on combo count
        string triggerName = "";
        switch (comboCount)
        {
            case 1:
                triggerName = combo1Trigger;
                break;
            case 2:
                triggerName = combo2Trigger;
                break;
            case 3:
                triggerName = combo3Trigger;
                break;
            case 4:
                triggerName = combo4Trigger;
                break;
            case 5:
                triggerName = combo5Trigger;
                break;
        }
        
        if (!string.IsNullOrEmpty(triggerName))
        {
            context.Animator.SetTrigger(triggerName);
        }
    }

    private void ApplyComboCooldown(PartContext context, bool isFinisher)
    {
        if (context.partInstance == null)
            return;

        float cooldown = isFinisher ? comboResetInternalCooldown : comboStepInternalCooldown;
        context.partInstance.OverrideCooldown(cooldown);
    }
}
