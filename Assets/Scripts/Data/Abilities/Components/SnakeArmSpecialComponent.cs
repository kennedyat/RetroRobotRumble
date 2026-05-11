using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "ScriptableObjects/Components/SnakeArmSpecial")]
public class SnakeArmSpecialComponent : PartComponent
{
    [Header("Stun Settings")]
    [Tooltip("Duration enemies are stunned")]
    public float stunDuration = 2f;

    [Tooltip("How long the hitbox stays active")]
    public float hitboxActiveDuration = 0.1f;

    [Header("Damage Settings")]
    [Tooltip("Damage dealt to enemies hit")]
    public float stunDamage = 15f;

    // Track stunned enemies: Dictionary<Enemy, StunData>
    private class StunData
    {
        public float timeRemaining;
        public NavMeshAgent navAgent;
        public RigidbodyConstraints originalConstraints;
        public bool wasNavAgentEnabled;
    }

    public override void Initialize(PartContext context)
    {
        // Initialize stun tracking dictionary
        if (!context.CustomData.ContainsKey("StunnedEnemies"))
        {
            context.CustomData["StunnedEnemies"] = new Dictionary<Enemy, StunData>();
        }

        // Set InternalCooldown for ability usage
        if (context.partInstance != null)
        {
            context.partInstance.InternalCooldown = 0.1f;
        }
    }

    public override void OnExecute(PartContext context)
    {
        HitBox specialHitbox = context.HitBox;

        if (specialHitbox == null)
        {
            Debug.LogError("[SnakeArmSpecial] Special hitbox is null! Make sure the arm prefab has a hitbox assigned to specialHitBox in ComboArmBehavior.");
            return;
        }

        // Set up hit callback
        specialHitbox.OnHit = (Collider target) => OnSpecialHitboxHit(target, context);

        // Enable the hitbox directly (bypass HitBoxManager for instant activation)
        specialHitbox.EnableFrame(hitboxActiveDuration);

        Debug.Log($"[SnakeArmSpecial] Activated special hitbox for {hitboxActiveDuration} seconds");
    }

    private void OnSpecialHitboxHit(Collider target, PartContext context)
    {
        if (!target.CompareTag("Enemy"))
            return;

        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy == null)
            return;

        Debug.Log($"[SnakeArmSpecial] Hit {target.name} - applying stun!");

        // Deal damage
        enemy.DealDamage((int)stunDamage);

        // Apply minimal knockback
        if (knockbackForce > 0 && context.Owner != null)
        {
            Rigidbody enemyRb = target.GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                Vector3 knockbackDir = (target.transform.position - context.Owner.position).normalized;
                enemyRb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
            }
        }

        // Apply stun effect
        enemy.InflictStun(stunDuration);

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

    public override void OnUpdate(PartContext context, float deltaTime)
    {
        // do nothing
    }
}
