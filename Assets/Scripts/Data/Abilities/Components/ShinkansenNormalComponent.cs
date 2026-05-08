using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "ScriptableObjects/Components/ShinkansenNormal")]
public class ShinkansenNormalComponent : PartComponent
{
    [Header("Combo Settings")]

    [Tooltip("Damage multiplier for third attack (1.5 = 50% more damage)")]
    public float thirdAttackDamageMultiplier = 1.5f;

    [Tooltip("Input window duration in seconds (0.2s recommended)")]
    public float inputWindowDuration = 0.2f;

    [Tooltip("Attack duration (how long hitbox stays active)")]
    public float attackDuration = 0.15f;

    [Header("Cooldown")]
    [Tooltip("Internal cooldown used between hits inside one combo string")]
    [Min(0f)]
    public float comboStepInternalCooldown = 0.1f;

    [Tooltip("Internal cooldown applied after the 3rd hit before the next combo can start")]
    [Min(0f)]
    public float comboResetInternalCooldown = 0.6f;

    [Header("Third Attack Special")]
    [Tooltip("Distance player moves forward on third attack")]
    public float forwardMovementDistance = 0.5f;

    [Tooltip("Duration of forward movement on third attack")]
    public float forwardMovementDuration = 0.1f;

    [Header("Animation")]
    [Tooltip("Animation trigger name")]
    public string animationTrigger = "ShinkansenNormal";

    [Tooltip("Animation bool parameter for alternating attacks")]
    public string secondHitBoolParam = "Second";

    public override void Initialize(PartContext context)
    {
        // Initialize combo state
        context.CustomData["ComboCounter"] = 0; // 0, 1, or 2 (resets to 0 after 3rd attack)
        context.CustomData["IsAttacking"] = false;
        context.CustomData["AttackStartTime"] = 0f;
        context.CustomData["InputWindowStartTime"] = 0f;
        context.CustomData["InInputWindow"] = false;
        context.CustomData["IsFinisherAttack"] = false;
        context.CustomData["IsMovingForward"] = false;
        context.CustomData["ForwardMovementStartTime"] = 0f;
        context.CustomData["ForwardMovementStartPosition"] = Vector3.zero;

        // Get hitboxes from ComboArmBehavior
        HitBox hitbox1 = null;
        HitBox hitbox2 = null;

        if (context.CustomData.ContainsKey("ComboHitBox1"))
        {
            hitbox1 = context.CustomData["ComboHitBox1"] as HitBox;
        }
        if (context.CustomData.ContainsKey("ComboHitBox2"))
        {
            hitbox2 = context.CustomData["ComboHitBox2"] as HitBox;
        }

        context.CustomData["Hitbox1"] = hitbox1;
        context.CustomData["Hitbox2"] = hitbox2;

        // Set starting cooldown used for regular combo chaining.
        if (context.partInstance != null)
        {
            context.partInstance.InternalCooldown = comboStepInternalCooldown;
        }

        Debug.Log($"[ShinkansenNormal] Initialized - Hitbox1: {(hitbox1 != null ? hitbox1.name : "NULL")}, Hitbox2: {(hitbox2 != null ? hitbox2.name : "NULL")}");
    }

    public override void OnExecute(PartContext context)
    {
        int comboCounter = (int)context.CustomData["ComboCounter"];
        bool isAttacking = (bool)context.CustomData["IsAttacking"];
        bool inInputWindow = (bool)context.CustomData["InInputWindow"];
        float inputWindowStartTime = (float)context.CustomData["InputWindowStartTime"];

        // Check if we're in the input window
        if (isAttacking && inInputWindow)
        {
            float timeInWindow = Time.time - inputWindowStartTime;
            if (timeInWindow <= inputWindowDuration)
            {
                // Still in window, increment counter (this input will trigger next attack)
                comboCounter++;
                Debug.Log($"[ShinkansenNormal] Input during window - Counter incremented to: {comboCounter}");
            }
            else
            {
                // Window expired, reset counter and start new combo
                comboCounter = 0;
                Debug.Log($"[ShinkansenNormal] Input window expired - Counter reset to 0, starting new combo");
            }
        }
        else if (!isAttacking)
        {
            // Not attacking, start new combo (counter starts at 0)
            comboCounter = 0;
            Debug.Log($"[ShinkansenNormal] Starting new combo - Counter: 0");
        }
        else
        {
            // Still mid-swing before input window — rapid clicks should advance the combo instead of
            // snapping back to hit 1 (which forces Hitbox1 for every swing).
            int prev = comboCounter;
            comboCounter = Mathf.Min(comboCounter + 1, 2);
            Debug.Log($"[ShinkansenNormal] Early chain input — Counter {prev} -> {comboCounter}");
        }

        // Execute attack based on counter
        // If counter = 2, this is the special third attack
        ExecuteAttack(context, comboCounter);

        // After executing attack, if it was the special third attack, reset counter
        // Otherwise, counter stays as is (will be incremented if input during window)
        if (comboCounter == 2)
        {
            comboCounter = 0;
            Debug.Log($"[ShinkansenNormal] Special third attack executed - Counter reset to 0");
        }

        // Store updated counter
        context.CustomData["ComboCounter"] = comboCounter;
    }

    private void ExecuteAttack(PartContext context, int comboCounter)
    {
        HitBox hitbox1 = context.CustomData["Hitbox1"] as HitBox;
        HitBox hitbox2 = context.CustomData["Hitbox2"] as HitBox;

        // Determine which hitbox to use and damage
        // comboCounter = 0: First attack (normal)
        // comboCounter = 1: Second attack (normal) 
        // comboCounter = 2: Third attack (special - larger hitbox, more damage, forward movement)
        HitBox activeHitbox = null;
        float damage = baseDamage;
        bool isThirdAttack = (comboCounter == 2);

        if (isThirdAttack)
        {
            // Third attack - use hitbox2 (larger), more damage, forward movement
            activeHitbox = hitbox2;
            damage = baseDamage * thirdAttackDamageMultiplier;
        }
        else
        {
            // First or second attack - use hitbox1
            activeHitbox = hitbox1;
            damage = baseDamage;
        }

        if (activeHitbox == null)
        {
            Debug.LogError($"[ShinkansenNormal] Active hitbox is NULL for combo counter {comboCounter}!");
            return;
        }

        // Set up hitbox callback
        activeHitbox.OnHit = (Collider target) => OnHitboxHit(target, damage, context);

        // Activate hitbox
        activeHitbox.EnableFrame(attackDuration);

        // Trigger animation
        if (context.Animator != null)
        {
            // Alternate between left and right for first two attacks
            if (comboCounter == 0 || comboCounter == 1)
            {
                bool isSecond = (comboCounter == 1);
                context.Animator.SetBool(secondHitBoolParam, isSecond);
            }
            context.Animator.SetTrigger(animationTrigger);
        }

        // Handle third attack special behavior
        if (isThirdAttack)
        {
            // Start forward movement
            StartForwardMovement(context);
        }

        if (context.partInstance != null)
        {
            float nextCooldown = isThirdAttack ? comboResetInternalCooldown : comboStepInternalCooldown;
            context.partInstance.OverrideCooldown(nextCooldown);
        }

        // Set attack state
        context.CustomData["IsAttacking"] = true;
        context.CustomData["IsFinisherAttack"] = isThirdAttack;
        context.CustomData["AttackStartTime"] = Time.time;

        // Input window will start after attack duration (handled in OnUpdate)
        // For third attack, no input window needed since counter resets
        if (!isThirdAttack)
        {
            context.CustomData["InInputWindow"] = false; // Will be set to true in OnUpdate after attack duration
        }
        else
        {
            // Third attack - no input window, counter will reset
            context.CustomData["InInputWindow"] = false;
        }

        Debug.Log($"[ShinkansenNormal] Attack executed - Counter: {comboCounter}, Damage: {damage}, Hitbox: {activeHitbox.name}, ThirdAttack: {isThirdAttack}");
    }

    private void StartForwardMovement(PartContext context)
    {
        if (context.Owner == null || context.Rigidbody == null)
            return;

        context.CustomData["IsMovingForward"] = true;
        context.CustomData["ForwardMovementStartTime"] = Time.time;
        context.CustomData["ForwardMovementStartPosition"] = context.Owner.position;

        Debug.Log($"[ShinkansenNormal] Starting forward movement for third attack");
    }

    public override void OnUpdate(PartContext context, float deltaTime)
    {
        bool isAttacking = (bool)context.CustomData["IsAttacking"];
        bool isFinisherAttack = (bool)context.CustomData["IsFinisherAttack"];
        float attackStartTime = (float)context.CustomData["AttackStartTime"];
        bool inInputWindow = (bool)context.CustomData["InInputWindow"];
        float inputWindowStartTime = (float)context.CustomData["InputWindowStartTime"];
        bool isMovingForward = (bool)context.CustomData["IsMovingForward"];
        float forwardMovementStartTime = (float)context.CustomData["ForwardMovementStartTime"];
        Vector3 forwardMovementStartPosition = (Vector3)context.CustomData["ForwardMovementStartPosition"];

        // Handle forward movement for third attack
        if (isMovingForward)
        {
            float elapsed = Time.time - forwardMovementStartTime;
            if (elapsed < forwardMovementDuration)
            {
                // Move forward
                float progress = elapsed / forwardMovementDuration;

                LayerMask mask = LayerMask.GetMask("Level", "Enemy");
                Transform player = context.Owner.parent.transform;
                CapsuleCollider cap = context.Owner.parent.GetComponent<CapsuleCollider>();
                Vector3 targetPosition = forwardMovementStartPosition + context.Owner.forward * forwardMovementDistance;
                float dashSpeed = Vector3.Distance(forwardMovementStartPosition, targetPosition);
                if (Physics.SphereCast(cap.bounds.center, cap.radius * 1.05f, player.forward, out RaycastHit hit, dashSpeed * deltaTime, mask))
                {
                    targetPosition = forwardMovementStartPosition + context.Owner.forward * forwardMovementDistance;
                    Vector3 currentPosition = Vector3.Lerp(forwardMovementStartPosition, targetPosition, progress);

                    if (context.Rigidbody != null)
                    {
                        context.Rigidbody.MovePosition(currentPosition);
                    }
                }
            }
            else
            {
                // Forward movement complete
                context.CustomData["IsMovingForward"] = false;
            }
        }

        // Check if attack is complete
        if (isAttacking)
        {
            float attackElapsed = Time.time - attackStartTime;

            // Third hit: do not open chain window, just finish attack and wait for finisher cooldown.
            if (isFinisherAttack)
            {
                if (attackElapsed >= attackDuration)
                {
                    context.CustomData["IsAttacking"] = false;
                    context.CustomData["InInputWindow"] = false;
                    context.CustomData["IsFinisherAttack"] = false;
                }
                return;
            }

            // Input window starts after attack duration
            if (attackElapsed >= attackDuration)
            {
                // Attack complete, now in input window
                if (!inInputWindow)
                {
                    // Start input window
                    context.CustomData["InInputWindow"] = true;
                    context.CustomData["InputWindowStartTime"] = Time.time;
                    inInputWindow = true;
                    inputWindowStartTime = Time.time;
                }

                // Check if input window has expired
                if (inInputWindow)
                {
                    float windowElapsed = Time.time - inputWindowStartTime;

                    if (windowElapsed >= inputWindowDuration)
                    {
                        // Input window expired, reset counter
                        context.CustomData["ComboCounter"] = 0;
                        context.CustomData["InInputWindow"] = false;
                        context.CustomData["IsAttacking"] = false;
                        Debug.Log($"[ShinkansenNormal] Input window expired - Counter reset to 0");
                    }
                    // If window hasn't expired, keep waiting for input (stay in attacking state)
                }
            }
        }

    }

    private void OnHitboxHit(Collider target, float damage, PartContext context)
    {
        if (!target.CompareTag("Enemy"))
            return;

        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy == null)
            return;

        Debug.Log($"[ShinkansenNormal] Hit {target.name} for {damage} damage");

        // Deal damage
        enemy.DealDamage((int)damage);

        // Apply knockback
        if (knockbackForce > 0 && context.Owner != null)
        {
            Rigidbody enemyRb = target.GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                Vector3 knockbackDir = (target.transform.position - context.Owner.position).normalized;
                enemyRb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
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

    }
}
