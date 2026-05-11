using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Components/Burst")]
public class BurstFireComponent : PartComponent
{
    [Header("Burst Settings")]
    public GameObject projectilePrefab;
    public int burstCount = 3;
    public float shotInterval = 0.15f;
    public float projectileSpeed = 30f;
    public float projectileLifetime = 6f;
    
    [Header("Spawn Points")]
    public string firePointName = "FirePoint";
    public string[] extraMuzzleNames; // Optional alternate spawn points
    public float spawnForwardOffset = 0.4f;
    
    private bool isBursting;
    private int shotsRemaining;
    private float nextShotTimer;
    private int currentMuzzleIndex;
    
    public override void Initialize(PartContext context)
    {
        isBursting = false;
        shotsRemaining = 0;
        nextShotTimer = 0f;
        currentMuzzleIndex = 0;
    }
    
    public override void OnExecute(PartContext context)
    {
        Debug.Log("[BurstFire] Starting burst!");
        
        // Start burst
        isBursting = true;
        shotsRemaining = burstCount;
        nextShotTimer = 0f;
        currentMuzzleIndex = 0;
        
        context.CustomData["IsBursting"] = true;
    }
    
    public override void OnUpdate(PartContext context, float deltaTime)
    {
        if (!isBursting) return;
        
        nextShotTimer -= deltaTime;
        
        if (nextShotTimer <= 0 && shotsRemaining > 0)
        {
            FireProjectile(context);
            shotsRemaining--;
            nextShotTimer = shotInterval;
            
            if (shotsRemaining <= 0)
            {
                // Burst complete
                isBursting = false;
                context.CustomData["IsBursting"] = false;
                Debug.Log("[BurstFire] Burst complete!");
            }
        }
        
        context.CustomData["BurstShotsRemaining"] = shotsRemaining;
    }
    
    private void FireProjectile(PartContext context)
    {
        if (projectilePrefab == null || context.Owner == null)
        {
            Debug.LogError("[BurstFire] Missing projectile prefab or owner!");
            return;
        }
        
        // Find spawn point
        Transform spawnPoint = GetSpawnPoint(context);
        
        Vector3 spawnPos = spawnPoint.position + spawnPoint.forward * spawnForwardOffset;
        Quaternion spawnRot = spawnPoint.rotation;
        
        // Instantiate projectile
        GameObject proj = GameObject.Instantiate(projectilePrefab, spawnPos, spawnRot);
        
        // Set up rigidbody
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = proj.AddComponent<Rigidbody>();
        }
        
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.velocity = spawnRot * Vector3.forward * projectileSpeed;
        
        // Auto-destroy after lifetime
        GameObject.Destroy(proj, projectileLifetime);
        
        Debug.Log($"[BurstFire] Fired projectile from {spawnPoint.name}");
    }
    
    private Transform GetSpawnPoint(PartContext context)
    {
        // Try to find named spawn point
        Transform spawnPoint = context.Owner.Find(firePointName);
        
        // Round-robin through extra muzzles if available
        if (extraMuzzleNames != null && extraMuzzleNames.Length > 0)
        {
            string muzzleName = extraMuzzleNames[currentMuzzleIndex % extraMuzzleNames.Length];
            Transform extraMuzzle = context.Owner.Find(muzzleName);
            
            if (extraMuzzle != null)
            {
                spawnPoint = extraMuzzle;
            }
            
            currentMuzzleIndex++;
        }
        
        // Fallback to owner if nothing found
        if (spawnPoint == null)
        {
            spawnPoint = context.Owner;
        }
        
        return spawnPoint;
    }
}