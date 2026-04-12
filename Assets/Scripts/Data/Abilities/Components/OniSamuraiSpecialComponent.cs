using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Components/OniSamuraiSpecial")]
public class OniSamuraiSpecialComponent : PartComponent
{
    [Header("Dash Settings")]
    [Tooltip("Distance the player dashes forward")]
    public float dashDistance = 5f;

    [Tooltip("Duration of the dash")]
    public float dashDuration = 0.3f;

    [Header("Hitbox Settings")]
    [Tooltip("Prefab for the fixed-position hitbox (should have HitBox component)")]
    public GameObject hitboxPrefab;

    [Tooltip("Size of the hitbox (if creating dynamically)")]
    public Vector3 hitboxSize = new Vector3(3f, 2f, 3f);

    [Header("Damage Settings")]
    [Tooltip("Damage dealt by the special hitbox")]
    public float specialDamage = 20f;

    [Tooltip("Knockback force applied by the special hitbox")]
    public float specialKnockback = 15f;

    public override void Initialize(PartContext context)
    {
        context.CustomData["IsDashing"] = false;
        context.CustomData["DashTimeRemaining"] = 0f;
        context.CustomData["DashStartPosition"] = Vector3.zero;
        context.CustomData["DashTargetPosition"] = Vector3.zero;
        context.CustomData["DashStartRotation"] = Quaternion.identity;
        context.CustomData["ActiveHitboxObject"] = null;
        context.CustomData["HitboxStartTime"] = 0f;
    }

    public override void OnExecute(PartContext context)
    {
        if (context.Owner == null || context.Rigidbody == null)
        {
            Debug.LogError("[OniSamuraiSpecial] Owner or Rigidbody is null!");
            return;
        }

        // Calculate dash direction (player's forward direction)
        Vector3 dashDirection = context.Owner.forward;

        // Calculate target position
        Vector3 startPosition = context.Owner.position;
        Vector3 targetPosition = startPosition + dashDirection * dashDistance;

        // Store dash state and starting rotation
        context.CustomData["IsDashing"] = true;
        context.CustomData["DashTimeRemaining"] = dashDuration;
        context.CustomData["DashStartPosition"] = startPosition;
        context.CustomData["DashTargetPosition"] = targetPosition;
        context.CustomData["DashStartRotation"] = context.Owner.rotation;

        // Spawn hitbox immediately at starting position with starting rotation
        SpawnFixedHitbox(context, startPosition, context.Owner.rotation);

        Debug.Log($"[OniSamuraiSpecial] Starting dash from {startPosition} to {targetPosition}, spawning hitbox at start");
    }

    public override void OnUpdate(PartContext context, float deltaTime)
    {
        bool isDashing = (bool)context.CustomData["IsDashing"];
        float dashTimeRemaining = (float)context.CustomData["DashTimeRemaining"];
        GameObject activeHitboxObject = context.CustomData["ActiveHitboxObject"] as GameObject;

        // Handle dash movement
        if (isDashing && dashTimeRemaining > 0)
        {
            if (context.Rigidbody == null || context.Owner == null)
            {
                context.CustomData["IsDashing"] = false;
                dashTimeRemaining = 0f;
                return;
            }

            Vector3 startPos = (Vector3)context.CustomData["DashStartPosition"];
            Vector3 targetPos = (Vector3)context.CustomData["DashTargetPosition"];

            // Calculate progress (0 to 1)
            float progress = 1f - (dashTimeRemaining / dashDuration);

            // Interpolate position, only if there isn't anything in the way
            LayerMask mask = LayerMask.GetMask("Level");
            Transform player = context.Owner.parent.transform;
            CapsuleCollider cap = context.Owner.parent.GetComponent<CapsuleCollider>();
            float dashSpeed = Vector3.Distance(startPos, targetPos) / dashDuration;
            if (!Physics.SphereCast(cap.bounds.center, cap.radius * 1.05f, player.forward, out RaycastHit hit, dashSpeed * deltaTime, mask))
            {
                Vector3 currentPosition = Vector3.Lerp(startPos, targetPos, progress);
                context.Rigidbody.MovePosition(currentPosition);
                dashTimeRemaining -= deltaTime;
                context.CustomData["DashTimeRemaining"] = dashTimeRemaining;
            }
            else
            {
                dashTimeRemaining = 0;
            }

            // When dash completes, just mark it as done (hitbox already spawned)
            if (dashTimeRemaining <= 0)
            {
                context.CustomData["IsDashing"] = false;
            }
        }

        // Handle hitbox expiration
        if (activeHitboxObject != null)
        {
            float hitboxStartTime = (float)context.CustomData["HitboxStartTime"];
            if (Time.time - hitboxStartTime >= hitboxDuration)
            {
                DestroyFixedHitbox(context);
            }
        }
    }

    private void SpawnFixedHitbox(PartContext context, Vector3 position, Quaternion rotation)
    {
        // Check if there's already an active hitbox
        GameObject existingHitbox = context.CustomData["ActiveHitboxObject"] as GameObject;
        if (existingHitbox != null)
        {
            DestroyFixedHitbox(context);
        }

        GameObject hitboxObject = null;

        // Use prefab if provided, otherwise create dynamically
        if (hitboxPrefab != null)
        {
            hitboxObject = GameObject.Instantiate(hitboxPrefab, position, rotation);
            Debug.Log($"[OniSamuraiSpecial] Spawned hitbox from prefab at {position} with rotation {rotation.eulerAngles}");
        }
        else
        {
            // Create hitbox dynamically
            hitboxObject = new GameObject("OniSamuraiSpecialHitbox");
            hitboxObject.transform.position = position;
            hitboxObject.transform.rotation = rotation;

            // Add BoxCollider
            BoxCollider collider = hitboxObject.AddComponent<BoxCollider>();
            collider.size = hitboxSize;
            collider.isTrigger = true;

            // Add MeshRenderer and MeshFilter for visualization
            MeshFilter meshFilter = hitboxObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = hitboxObject.AddComponent<MeshRenderer>();

            // Create a cube mesh
            GameObject tempCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            meshFilter.mesh = tempCube.GetComponent<MeshFilter>().mesh;
            Destroy(tempCube);

            // Add HitBox component
            HitBox hitBox = hitboxObject.AddComponent<HitBox>();

            Debug.Log($"[OniSamuraiSpecial] Created dynamic hitbox at {position}");
        }

        // Get or add HitBox component
        HitBox hitBoxComponent = hitboxObject.GetComponent<HitBox>();
        if (hitBoxComponent == null)
        {
            hitBoxComponent = hitboxObject.AddComponent<HitBox>();
        }

        // Set up hit callback
        hitBoxComponent.OnHit = (Collider target) => OnSpecialHitboxHit(target, context);

        // Enable the hitbox for the duration
        hitBoxComponent.EnableFrame(hitboxDuration);

        // Store reference
        context.CustomData["ActiveHitboxObject"] = hitboxObject;
        context.CustomData["HitboxStartTime"] = Time.time;

        Debug.Log($"[OniSamuraiSpecial] Activated fixed hitbox at {position} for {hitboxDuration} seconds");
    }

    private void DestroyFixedHitbox(PartContext context)
    {
        GameObject hitboxObject = context.CustomData["ActiveHitboxObject"] as GameObject;
        if (hitboxObject != null)
        {
            HitBox hitBox = hitboxObject.GetComponent<HitBox>();
            if (hitBox != null)
            {
                hitBox.DisableFrame();
            }

            GameObject.Destroy(hitboxObject);
            context.CustomData["ActiveHitboxObject"] = null;
            context.CustomData["HitboxStartTime"] = 0f;

            Debug.Log("[OniSamuraiSpecial] Destroyed fixed hitbox");
        }
    }

    private void OnSpecialHitboxHit(Collider target, PartContext context)
    {
        if (!target.CompareTag("Enemy"))
            return;

        // Deal damage
        var enemy = target.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.DealDamage((int)specialDamage);
        }

        // Apply knockback (away from hitbox center)
        if (specialKnockback > 0 && context.CustomData["ActiveHitboxObject"] != null)
        {
            GameObject hitboxObject = context.CustomData["ActiveHitboxObject"] as GameObject;
            Vector3 hitboxPosition = hitboxObject.transform.position;

            var enemyRb = target.GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                Vector3 knockbackDirection = (target.transform.position - hitboxPosition).normalized;
                // If enemy is at center, push upward
                if (knockbackDirection.magnitude < 0.1f)
                {
                    knockbackDirection = Vector3.up;
                }

                enemyRb.AddForce(knockbackDirection * specialKnockback, ForceMode.Impulse);
            }
        }

        // Call base for sound and VFX
        base.OnHitboxHit(target, specialDamage, specialKnockback, context);

        Debug.Log($"[OniSamuraiSpecial] Hit enemy {target.name} for {specialDamage} damage");
    }
}
