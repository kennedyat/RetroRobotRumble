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
    
    [Tooltip("Knockback force (minimal, as this is a CC ability)")]
    public float knockbackForce = 1f;
    
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
        ApplyStun(enemy, context);
        
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
        // Update stun timers
        UpdateStuns(context, deltaTime);
    }
    
    private void ApplyStun(Enemy enemy, PartContext context)
    {
        Dictionary<Enemy, StunData> stunnedDict = context.CustomData["StunnedEnemies"] as Dictionary<Enemy, StunData>;
        
        if (stunnedDict == null)
        {
            stunnedDict = new Dictionary<Enemy, StunData>();
            context.CustomData["StunnedEnemies"] = stunnedDict;
        }
        
        // If enemy is already stunned, refresh the duration
        if (stunnedDict.ContainsKey(enemy))
        {
            StunData data = stunnedDict[enemy];
            data.timeRemaining = stunDuration;
        }
        else
        {
            // Create new stun data
            StunData data = new StunData
            {
                timeRemaining = stunDuration,
                navAgent = enemy.GetComponent<NavMeshAgent>(),
                originalConstraints = RigidbodyConstraints.None,
                wasNavAgentEnabled = false
            };
            
            // Store original state
            Rigidbody rb = enemy.GetComponent<Rigidbody>();
            if (rb != null)
            {
                data.originalConstraints = rb.constraints;
            }
            
            if (data.navAgent != null)
            {
                data.wasNavAgentEnabled = data.navAgent.enabled;
            }
            
            stunnedDict[enemy] = data;
            
            // Apply stun immediately
            if (rb != null)
            {
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }
            
            if (data.navAgent != null)
            {
                data.navAgent.enabled = false;
            }
        }
    }
    
    private void UpdateStuns(PartContext context, float deltaTime)
    {
        Dictionary<Enemy, StunData> stunnedDict = context.CustomData["StunnedEnemies"] as Dictionary<Enemy, StunData>;
        
        if (stunnedDict == null || stunnedDict.Count == 0)
            return;
        
        // Create a list of enemies to remove (unstunned or destroyed)
        List<Enemy> toRemove = new List<Enemy>();
        
        foreach (var kvp in stunnedDict)
        {
            Enemy enemy = kvp.Key;
            StunData data = kvp.Value;
            
            // Check if enemy is destroyed
            if (enemy == null)
            {
                toRemove.Add(enemy);
                continue;
            }
            
            // Update stun timer
            data.timeRemaining -= deltaTime;
            
            // If stun expired, restore enemy
            if (data.timeRemaining <= 0)
            {
                RestoreEnemy(enemy, data);
                toRemove.Add(enemy);
            }
        }
        
        // Remove expired/destroyed enemies
        foreach (Enemy enemy in toRemove)
        {
            stunnedDict.Remove(enemy);
        }
    }
    
    private void RestoreEnemy(Enemy enemy, StunData data)
    {
        if (enemy == null)
            return;
        
        // Restore rigidbody constraints
        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.constraints = data.originalConstraints;
        }
        
        // Restore navmesh agent
        if (data.navAgent != null)
        {
            data.navAgent.enabled = data.wasNavAgentEnabled;
        }
        
        Debug.Log($"[SnakeArmSpecial] Restored {enemy.name} from stun");
    }
}
