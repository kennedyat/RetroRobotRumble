using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "ScriptableObjects/Components/Rapid Fire")]
public class RapidFireComponent : PartComponent
{
    [Header("Projectile")]
    public GameObject projectilePrefab;
    public float projectileRange = 100f;
    public string spawnPointName = "SpawnPoint";
    
    [Header("Fire Rate")]
    public float initialShotsPerSecond = 3f;
    public float maxShotsPerSecond = 8f;
    public float rampUpTime = 1.75f;
    public float rampDownTime = 1f;
    
    [Header("Spread")]
    public float minSpread = 10f;
    public float maxSpread = 45f;
    
    private float timeUntilNextShot;
    private float currentRampup;
    
    public override void Initialize(PartContext context)
    {
        timeUntilNextShot = 0f;
        currentRampup = 0f;
    }
    
    public override void OnExecute(PartContext context)
    {
        Debug.Log("[RapidFire] Button pressed!");
    }
    
    public override void OnUpdate(PartContext context, float deltaTime)
    {
        // Check if firing (from input or other component saying we can fire)
        InputAction inputAction = context.CustomData.ContainsKey("InputAction") 
            ? context.CustomData["InputAction"] as InputAction 
            : null;
        
        bool pressing = inputAction != null && inputAction.ReadValue<float>() > 0.5f;
        
        // Check if blocked by overheat. Slightly hardcoded
        bool blocked = context.CustomData.ContainsKey("RapidFireBlocked") 
            && (bool)context.CustomData["RapidFireBlocked"];
        
        if (blocked)
        {
            // ramp down when blocked
            currentRampup -= deltaTime / rampDownTime;
            currentRampup = Mathf.Max(0, currentRampup);
            context.CustomData["RapidFireRampup"] = currentRampup;
            return;
        }
        
        // Handle firing
        if (pressing && timeUntilNextShot <= 0)
        {
            Shoot(context);
            
            // Signal that we fired --> overheat hard code again
            context.CustomData["RapidFireShot"] = true;
            
            float shotsPerSecond = Mathf.Lerp(initialShotsPerSecond, maxShotsPerSecond, currentRampup);
            timeUntilNextShot = 1f / shotsPerSecond;
        }
        else
        {
            context.CustomData["RapidFireShot"] = false;
        }
        
        //  rampup/rampdown
        if (pressing)
        {
            currentRampup += deltaTime / rampUpTime;
        }
        else
        {
            currentRampup -= deltaTime / rampDownTime;
        }
        currentRampup = Mathf.Clamp01(currentRampup);
        
        // Update shot timer
        if (timeUntilNextShot > 0)
        {
            timeUntilNextShot -= deltaTime;
        }
        
        // Store state for UI and other stuff
        context.CustomData["RapidFireRampup"] = currentRampup;
        context.CustomData["IsFiring"] = pressing && !blocked;
    }
    
    private void Shoot(PartContext context)
    {
        if (context.Owner == null || projectilePrefab == null)
        {
            Debug.LogError("[RapidFire] Owner or projectile prefab is null!");
            return;
        }
        
        Transform spawnPoint = context.Owner.Find(spawnPointName);
        if (spawnPoint == null)
        {
            spawnPoint = context.Owner;
        }
        
       
        float spreadDegrees = Mathf.Lerp(minSpread, maxSpread, currentRampup);
        
        Quaternion randomRotation = Quaternion.AngleAxis(
            Random.Range(-spreadDegrees / 2f, spreadDegrees / 2f), 
            Vector3.up
        );
        Vector3 direction = randomRotation * spawnPoint.forward;
        
        var instance = GameObject.Instantiate(projectilePrefab);
        var projectile = instance.GetComponent<Projectile>();
        
        if (projectile != null)
        {
            Ray shotRay = new Ray(spawnPoint.position, direction);
            projectile.FollowRay(shotRay, projectileRange);
        }
        else
        {
            instance.transform.position = spawnPoint.position;
            instance.transform.rotation = Quaternion.LookRotation(direction);
        }
        
       
    }
}