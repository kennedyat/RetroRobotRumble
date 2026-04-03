using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Components/SnakeArmSweep")]
public class SnakeArmSweepComponent : PartComponent
{
    [Header("Sweep Settings")]
    [Tooltip("Angle of the sweep cone (60-80 degrees recommended)")]
    [Range(30f, 120f)]
    public float sweepAngle = 70f;
    
    [Tooltip("Range of the sweep attack (between melee and ranged)")]
    public float sweepRange = 4f;
    
    [Tooltip("How long the hitboxes stay active")]
    public float sweepDuration = 0.15f;
    
    [Header("Main Hitbox Damage")]
    [Tooltip("Damage dealt by the main sweep hitbox")]
    public float mainDamage = 12f;
    
    [Tooltip("Knockback from the main sweep")]
    public float mainKnockback = 3f;
    
    [Header("Tip Hitbox (Poison)")]
    [Tooltip("Damage dealt by the tip hitbox (initial hit)")]
    public float tipDamage = 8f;
    
    [Tooltip("Knockback from the tip hitbox")]
    public float tipKnockback = 2f;
    
    [Tooltip("Poison damage per tick")]
    public float poisonDamagePerTick = 2f;
    
    [Tooltip("Total poison duration in seconds")]
    public float poisonDuration = 3f;
    
    [Tooltip("Time between poison ticks")]
    public float poisonTickInterval = 0.5f;
    
    [Tooltip("VFX to spawn when poison is applied (tip hit)")]
    public GameObject poisonVFX;
    
    [Header("Animation")]
    [Tooltip("Animation trigger name (optional)")]
    public string animationTrigger = "SnakeSweep";
    
    private class PoisonData
    {
        public float timeRemaining;
        public float nextTickTime;
        public int totalTicks;
        public int currentTick;
    }
    
    public override void Initialize(PartContext context)
    {
        // Try to get hitboxes from ComboArmBehavior first (stored in CustomData)
        HitBox mainHitbox = null;
        HitBox tipHitbox = null;
        
        // Check if ComboArmBehavior has set up hitboxes in CustomData
        if (context.CustomData.ContainsKey("ComboHitBox1") && context.CustomData.ContainsKey("ComboHitBox2"))
        {
            // Using ComboArmBehavior: hitbox1 = main, hitbox2 = tip
            mainHitbox = context.CustomData["ComboHitBox1"] as HitBox;
            tipHitbox = context.CustomData["ComboHitBox2"] as HitBox;
            
            Debug.Log($"[SnakeArmSweep] Using ComboArmBehavior hitboxes - Main: {(mainHitbox != null ? mainHitbox.name : "NULL")}, Tip: {(tipHitbox != null ? tipHitbox.name : "NULL")}");
        }
        else
        {
            // Fallback: Using ArmBehavior - find hitboxes manually
            mainHitbox = context.HitBox;
            
            // Get the special hitbox from ArmBehavior to exclude it from search
            HitBox specialHitbox = null;
            if (context.Owner != null)
            {
                ArmBehavior armBehavior = context.Owner.GetComponent<ArmBehavior>();
                if (armBehavior != null && armBehavior.specialHitBox != null)
                {
                    specialHitbox = armBehavior.specialHitBox.GetComponent<HitBox>();
                }
            }
            
            // Try to find tip hitbox by searching children of Owner (recursively)
            if (context.Owner != null)
            {
                // Search recursively for a child named "TipHitBox" or "SnakeTipHitBox"
                HitBox[] allHitboxes = context.Owner.GetComponentsInChildren<HitBox>();
                
                foreach (HitBox hb in allHitboxes)
                {
                    // Skip main and special hitboxes
                    if (hb == mainHitbox || hb == specialHitbox)
                        continue;
                    
                    // First, try to find by name
                    string hitboxName = hb.gameObject.name;
                    if (hitboxName == "TipHitBox" || hitboxName == "SnakeTipHitBox")
                    {
                        tipHitbox = hb;
                        break;
                    }
                }
                
                // If not found by name, use the first HitBox that's not main or special
                if (tipHitbox == null)
                {
                    foreach (HitBox hb in allHitboxes)
                    {
                        if (hb != mainHitbox && hb != specialHitbox)
                        {
                            tipHitbox = hb;
                            break;
                        }
                    }
                }
            }
            
            Debug.Log($"[SnakeArmSweep] Using ArmBehavior fallback - Main: {(mainHitbox != null ? mainHitbox.name : "NULL")}, Tip: {(tipHitbox != null ? tipHitbox.name : "NULL")}");
        }
        
        // Store both hitboxes in CustomData
        context.CustomData["MainHitbox"] = mainHitbox;
        context.CustomData["TipHitbox"] = tipHitbox;
        context.CustomData["IsAttacking"] = false;
        context.CustomData["AttackStartTime"] = 0f;
        
        // Initialize poison tracking
        if (!context.CustomData.ContainsKey("PoisonedEnemies"))
        {
            context.CustomData["PoisonedEnemies"] = new Dictionary<Enemy, PoisonData>();
        }
        
        // Set InternalCooldown for fast attacks
        if (context.partInstance != null)
        {
            context.partInstance.InternalCooldown = 0.1f;
        }
        
        Debug.Log($"[SnakeArmSweep] Initialized - MainHitbox: {(mainHitbox != null ? mainHitbox.name : "NULL")}, TipHitbox: {(tipHitbox != null ? tipHitbox.name : "NULL")}");
    }
    
    public override void OnExecute(PartContext context)
    {
        HitBox mainHitbox = context.CustomData["MainHitbox"] as HitBox;
        HitBox tipHitbox = context.CustomData["TipHitbox"] as HitBox;
        
        if (mainHitbox == null)
        {
            Debug.LogError("[SnakeArmSweep] Main hitbox is null! Make sure the arm prefab has a hitbox assigned to normalHitBox in ArmBehavior.");
            return;
        }
        
        // Trigger animation if available
        if (context.Animator != null && !string.IsNullOrEmpty(animationTrigger))
        {
            context.Animator.SetTrigger(animationTrigger);
        }
        
        // Enable both hitboxes simultaneously by calling EnableFrame() directly
        // This bypasses HitBoxManager which only supports one hitbox at a time
        
        // Set up main hitbox callback
        mainHitbox.OnHit = (Collider target) => OnMainHitboxHit(target, context);
        mainHitbox.EnableFrame(sweepDuration);
        
        // Set up tip hitbox callback (if it exists)
        if (tipHitbox != null)
        {
            tipHitbox.OnHit = (Collider target) => OnTipHitboxHit(target, context);
            tipHitbox.EnableFrame(sweepDuration);
        }
        
        context.CustomData["IsAttacking"] = true;
        context.CustomData["AttackStartTime"] = Time.time;
        
        Debug.Log($"[SnakeArmSweep] Sweep attack executed - MainHitbox: {mainHitbox.name}, TipHitbox: {(tipHitbox != null ? tipHitbox.name : "NULL")}");
    }
    
    public override void OnUpdate(PartContext context, float deltaTime)
    {
        // Check if attack is complete
        bool isAttacking = (bool)context.CustomData["IsAttacking"];
        float attackStartTime = (float)context.CustomData["AttackStartTime"];
        
        if (isAttacking && Time.time - attackStartTime >= sweepDuration)
        {
            context.CustomData["IsAttacking"] = false;
            
            // Reset InternalCooldown after attack completes
            if (context.partInstance != null)
            {
                context.partInstance.InternalCooldown = 0.1f;
            }
        }
        
        // Update poison effects
        UpdatePoisonEffects(context, deltaTime);
    }
    
    private void OnMainHitboxHit(Collider target, PartContext context)
    {
        if (!target.CompareTag("Enemy"))
            return;
        
        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy == null)
            return;
        
        Debug.Log($"[SnakeArmSweep] Main hitbox hit {target.name} for {mainDamage} damage");
        
        // Deal damage
        enemy.DealDamage((int)mainDamage);
        
        // Apply knockback
        if (mainKnockback > 0 && context.Owner != null)
        {
            Rigidbody enemyRb = target.GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                Vector3 knockbackDir = (target.transform.position - context.Owner.position).normalized;
                enemyRb.AddForce(knockbackDir * mainKnockback, ForceMode.Impulse);
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
    
    private void OnTipHitboxHit(Collider target, PartContext context)
    {
        if (!target.CompareTag("Enemy"))
            return;
        
        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy == null)
            return;
        
        Debug.Log($"[SnakeArmSweep] Tip hitbox hit {target.name} - applying poison!");
        
        // Deal initial tip damage
        enemy.DealDamage((int)tipDamage);
        
        // Apply knockback
        if (tipKnockback > 0 && context.Owner != null)
        {
            Rigidbody enemyRb = target.GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                Vector3 knockbackDir = (target.transform.position - context.Owner.position).normalized;
                enemyRb.AddForce(knockbackDir * tipKnockback, ForceMode.Impulse);
            }
        }
        
        // Apply poison effect
        ApplyPoison(enemy, context);
        
        // Play poison VFX
        if (poisonVFX != null)
        {
            GameObject.Instantiate(poisonVFX, target.transform.position, Quaternion.identity);
        }
        
        // Play hit sound
        if (hitSound != null && context.Owner != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, target.transform.position);
        }
    }
    
    private void ApplyPoison(Enemy enemy, PartContext context)
    {
        Dictionary<Enemy, PoisonData> poisonedDict = context.CustomData["PoisonedEnemies"] as Dictionary<Enemy, PoisonData>;
        
        if (poisonedDict == null)
        {
            poisonedDict = new Dictionary<Enemy, PoisonData>();
            context.CustomData["PoisonedEnemies"] = poisonedDict;
        }
        
        // If enemy is already poisoned, refresh the duration
        if (poisonedDict.ContainsKey(enemy))
        {
            PoisonData data = poisonedDict[enemy];
            data.timeRemaining = poisonDuration;
            data.currentTick = 0; // Reset tick counter
        }
        else
        {
            // Create new poison data
            PoisonData data = new PoisonData
            {
                timeRemaining = poisonDuration,
                nextTickTime = Time.time + poisonTickInterval,
                totalTicks = Mathf.CeilToInt(poisonDuration / poisonTickInterval),
                currentTick = 0
            };
            poisonedDict[enemy] = data;
        }
    }
    
    private void UpdatePoisonEffects(PartContext context, float deltaTime)
    {
        Dictionary<Enemy, PoisonData> poisonedDict = context.CustomData["PoisonedEnemies"] as Dictionary<Enemy, PoisonData>;
        
        if (poisonedDict == null || poisonedDict.Count == 0)
            return;
        
        // Create a list of enemies to remove (dead or expired)
        List<Enemy> toRemove = new List<Enemy>();
        
        foreach (var kvp in poisonedDict)
        {
            Enemy enemy = kvp.Key;
            PoisonData data = kvp.Value;
            
            // Check if enemy is destroyed
            if (enemy == null)
            {
                toRemove.Add(enemy);
                continue;
            }
            
            // Update poison timer
            data.timeRemaining -= deltaTime;
            
            // Check if it's time for a poison tick
            if (Time.time >= data.nextTickTime && data.currentTick < data.totalTicks)
            {
                // Deal poison damage
                enemy.DealDamage((int)poisonDamagePerTick);
                data.currentTick++;
                data.nextTickTime = Time.time + poisonTickInterval;
                
                Debug.Log($"[SnakeArmSweep] Poison tick {data.currentTick}/{data.totalTicks} on {enemy.name} - {poisonDamagePerTick} damage");
            }
            
            // Remove if poison expired
            if (data.timeRemaining <= 0)
            {
                toRemove.Add(enemy);
            }
        }
        
        // Remove expired/dead enemies
        foreach (Enemy enemy in toRemove)
        {
            poisonedDict.Remove(enemy);
        }
    }
}
