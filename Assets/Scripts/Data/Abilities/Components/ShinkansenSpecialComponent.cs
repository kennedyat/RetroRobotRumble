using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Components/ShinkansenSpecial")]
public class ShinkansenSpecialComponent : PartComponent
{
    [Header("Uppercut Settings")]
    [Tooltip("Base damage for the uppercut")]
    public float baseDamage = 20f;

    [Tooltip("How long the hitbox stays active")]
    public float hitboxActiveDuration = 0.2f;

    [Tooltip("Knockback force (upward for uppercut)")]
    public float knockbackForce = 8f;

    [Header("Animation")]
    [Tooltip("Animation trigger name (optional)")]
    public string animationTrigger = "ShinkansenSpecial";

    // Note: Cooldown is set in the PartComponentData asset (15 seconds)
    // Stacks system will be implemented later

    public override void Initialize(PartContext context)
    {
        // Initialize state
        context.CustomData["IsAttacking"] = false;

        // Note: Cooldown is handled by PartInstance using PartComponentData.cooldown
        // Set to 15 seconds in the asset
    }

    public override void OnExecute(PartContext context)
    {
        HitBox specialHitbox = context.HitBox;

        if (specialHitbox == null)
        {
            Debug.LogError("[ShinkansenSpecial] Special hitbox is null! Make sure the arm prefab has a hitbox assigned to specialHitBox in ComboArmBehavior.");
            return;
        }

        // Set up hit callback
        specialHitbox.OnHit = (Collider target) => OnSpecialHitboxHit(target, context);

        // Enable the hitbox directly (bypass HitBoxManager for instant activation)
        specialHitbox.EnableFrame(hitboxActiveDuration);

        // Trigger animation if available
        if (context.Animator != null && !string.IsNullOrEmpty(animationTrigger))
        {
            context.Animator.SetTrigger(animationTrigger);
        }

        context.CustomData["IsAttacking"] = true;

        Debug.Log($"[ShinkansenSpecial] Activated uppercut hitbox for {hitboxActiveDuration} seconds");
    }

    public override void OnUpdate(PartContext context, float deltaTime)
    {
        bool isAttacking = (bool)context.CustomData["IsAttacking"];

        // Reset attacking state after hitbox duration
        if (isAttacking)
        {
            // Check if hitbox is still active
            HitBox specialHitbox = context.HitBox;
            if (specialHitbox != null && !specialHitbox.isActive)
            {
                context.CustomData["IsAttacking"] = false;
            }
        }
    }

    private void OnSpecialHitboxHit(Collider target, PartContext context)
    {
        if (!target.CompareTag("Enemy"))
            return;

        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy == null)
            return;

        // Calculate damage (base damage for now, stacks will be added later)
        float damage = baseDamage;
        // TODO: Add stack-based damage calculation when stacks system is implemented
        // float damage = baseDamage + (stackCount * perStackDamage);

        Debug.Log($"[ShinkansenSpecial] Uppercut hit {target.name} for {damage} damage");

        // Deal damage
        enemy.DealDamage((int)damage);

        // Apply upward knockback (uppercut effect)
        if (knockbackForce > 0 && context.Owner != null)
        {
            Rigidbody enemyRb = target.GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                // Upward knockback direction (up + slightly forward)
                Vector3 knockbackDir = Vector3.up + (target.transform.position - context.Owner.position).normalized * 0.3f;
                knockbackDir.Normalize();
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

        // TODO: Execute enemies below 5% health at max stacks (12 stacks)
        // This will be implemented when stacks system is added
    }
}
