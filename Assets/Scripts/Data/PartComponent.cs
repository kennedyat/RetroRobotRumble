using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class PartComponent : ScriptableObject
{
    
    [Header("Hitbox Settings")]
  
   
    
    [Tooltip("How long the hitbox stays active when this component executes")]
    public float hitboxDuration = 0.2f;
    
    [Tooltip("Enable the hitbox when this component executes")]
    public bool useHitbox = true;
    
    [Header("Damage Settings")]
    public float baseDamage = 10f;
    public float knockbackForce = 5f;
    
    [Header("Effects (Optional)")]
    public AudioClip hitSound;
    public GameObject hitVFX;
    
    public abstract void Initialize(PartContext context);
    public abstract void OnExecute(PartContext context);
    public abstract void OnUpdate(PartContext context, float deltaTime);
    
   //Helper func to activate hitbox within component
    protected void ActivateHitbox(PartContext context, float? customDuration = null, float? customDamage = null, float? customKnockback = null)
    {
        HitBox box = context.HitBox;
        if (!useHitbox || box == null)
        {
            if (useHitbox && box == null)
                Debug.LogWarning($"[{GetType().Name}] useHitbox is true but no hitbox assigned!");
            return;
        }
        float knockback = customKnockback ?? knockbackForce;
        float duration = customDuration ?? hitboxDuration;
        float damage = customDamage ?? baseDamage;
               
        // Enable the hitbox
        context.hitBoxManager.SetHitBox(box);
        HitBoxManager.duration = duration;
        //box.EnableFrame(duration);
                
        // Set up hit callback
        box.OnHit = (Collider target) => OnHitboxHit(target, damage, knockback, context);
    }

        //General on hit behavior. Can overide    
       protected virtual void OnHitboxHit(Collider target, float damage, float knockback, PartContext context)
    {
        Debug.Log($"[{GetType().Name}] Hit {target.name} for {damage} damage!");
        
        //TO DO:Apply Damage

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
        
        // Play hit sound
        if (hitSound != null && context.Owner != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, target.transform.position);
        }
        
        // Spawn hit VFX
        if (hitVFX != null)
        {
            GameObject.Instantiate(hitVFX, target.transform.position, Quaternion.identity);
        }
    }
}
