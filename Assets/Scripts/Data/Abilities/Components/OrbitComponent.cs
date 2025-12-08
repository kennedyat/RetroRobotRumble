using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Components/Orbit")]
public class OrbitComponent : PartComponent
{
    [Header("Orb Settings")]
    public GameObject orbPrefab;
    public int maxOrbs = 5;
    public float spawnInterval = 2f;
    
    [Header("Orbiting")]
    public float orbitRadius = 2f;
    public float orbitHeight = 1f;
    public float orbitSpeed = 90f;
    
    [Header("Auto-Fire")]
    public float detectionRadius = 12f;
    public float fireCooldown = 0.25f;
    
    private List<GameObject> activeOrbs = new List<GameObject>();
    private float spawnTimer;
    private float fireTimer;
    
    public override void Initialize(PartContext context)
    {
        activeOrbs.Clear();
        spawnTimer = 0f;
        fireTimer = 0f;
    }
    
    public override void OnExecute(PartContext context)
    {
        // Passive doesn't need execute - runs in OnUpdate
    }
    
    public override void OnUpdate(PartContext context, float deltaTime)
    {
        if (context.Owner == null) return;
        
        // Spawn orbs periodically
        spawnTimer += deltaTime;
        if (spawnTimer >= spawnInterval && activeOrbs.Count < maxOrbs)
        {
            SpawnOrb(context);
            spawnTimer = 0f;
        }
        
        // Update orb positions (orbit around player)
        UpdateOrbPositions(context.Owner);
        
        // Auto-fire at enemies
        fireTimer += deltaTime;
        if (fireTimer >= fireCooldown && activeOrbs.Count > 0)
        {
            Transform target = FindNearestEnemy(context.Owner.position);
            if (target != null)
            {
                FireOrb(target);
                fireTimer = 0f;
            }
        }
        
        // Store state for UI
        context.CustomData["OrbCount"] = activeOrbs.Count;
    }
    
    private void SpawnOrb(PartContext context)
    {
        if (orbPrefab == null)
        {
            Debug.LogError("[OrbitingPassive] No orb prefab assigned!");
            return;
        }
        
        GameObject orb = GameObject.Instantiate(orbPrefab, context.Owner.position, Quaternion.identity);
        
        // Disable homing until fired
        var homing = orb.GetComponent<EagleChassisPassiveProjectile>();
        if (homing != null)
        {
            homing.enabled = false;
        }
        
        activeOrbs.Add(orb);
        Debug.Log($"[OrbitingPassive] Spawned orb. Total: {activeOrbs.Count}");
    }
    
    private void UpdateOrbPositions(Transform owner)
    {
        // Remove destroyed orbs
        activeOrbs.RemoveAll(o => o == null);
        
        if (activeOrbs.Count == 0) return;
        
        float angleStep = 360f / activeOrbs.Count;
        float time = Time.time * orbitSpeed;
        
        for (int i = 0; i < activeOrbs.Count; i++)
        {
            var orb = activeOrbs[i];
            if (orb == null) continue;
            
            float angle = time + angleStep * i;
            float rad = angle * Mathf.Deg2Rad;
            
            Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * orbitRadius;
            Vector3 pos = owner.position + offset + Vector3.up * orbitHeight;
            
            orb.transform.position = pos;
            orb.transform.LookAt(owner);
        }
    }
    
    private void FireOrb(Transform target)
    {
        if (activeOrbs.Count == 0) return;
        
        GameObject orb = activeOrbs[0];
        activeOrbs.RemoveAt(0);
        
        var homing = orb.GetComponent<EagleChassisPassiveProjectile>();
        if (homing != null)
        {
            homing.enabled = true;
            homing.SetTarget(target);
        }
        
        Debug.Log($"[OrbitingPassive] Fired orb at {target.name}. Remaining: {activeOrbs.Count}");
    }
    
    private Transform FindNearestEnemy(Vector3 position)
    {
        Collider[] hits = Physics.OverlapSphere(position, detectionRadius);
        float bestDistSqr = Mathf.Infinity;
        Transform best = null;
        
        foreach (var c in hits)
        {
            if (c && c.CompareTag("Enemy"))
            {
                float d = (c.transform.position - position).sqrMagnitude;
                if (d < bestDistSqr)
                {
                    bestDistSqr = d;
                    best = c.transform;
                }
            }
        }
        
        return best;
    }
}