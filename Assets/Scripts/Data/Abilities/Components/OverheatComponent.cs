using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Components/Overheat")]
public class OverheatComponent : PartComponent
{
    [Header("Heat Settings")]
    public float maxHeat = 100f;
    public float heatPerShot = 3f;
    public float cooldownDelay = 0.5f; // Delay before heat starts cooling
    public float cooldownRate = 40f; // Heat per second
    public float overheatLockoutTime = 5f;
    
    private float currentHeat;
    private float timeSinceLastShot;
    private bool overheated;
    private float overheatTimer;
    
    public override void Initialize(PartContext context)
    {
        currentHeat = 0f;
        timeSinceLastShot = 999f;
        overheated = false;
        overheatTimer = 0f;
    }
    
    public override void OnExecute(PartContext context)
    {
        // Nothing needed
    }
    
    public override void OnUpdate(PartContext context, float deltaTime)
    {
        // Handle overheat lockout
        if (overheated)
        {
            overheatTimer -= deltaTime;
            
            if (overheatTimer <= 0)
            {
                overheated = false;
                currentHeat = 0f;
                Debug.Log("[Overheat] Lockout ended!");
            }
            
            // Block firing while overheated
            context.CustomData["RapidFireBlocked"] = true;
            context.CustomData["Overheated"] = true;
            context.CustomData["OverheatTimer"] = overheatTimer;
            context.CustomData["CurrentHeat"] = currentHeat;
            context.CustomData["HeatPercent"] = 1f;
            return;
        }
        
        // Check if another component fired a shot
        bool shotFired = context.CustomData.ContainsKey("RapidFireShot") 
            && (bool)context.CustomData["RapidFireShot"];
        
        if (shotFired)
        {
            currentHeat += heatPerShot;
            timeSinceLastShot = 0f;
            Debug.Log($"[Overheat] Heat added! Current: {currentHeat:F1}/{maxHeat}");
        }
        
        // Handle heat cooldown
        timeSinceLastShot += deltaTime;
        
        if (timeSinceLastShot >= cooldownDelay)
        {
            currentHeat -= cooldownRate * deltaTime;
            currentHeat = Mathf.Max(0, currentHeat);
        }
        
        // Check for overheat
        if (currentHeat >= maxHeat)
        {
            overheated = true;
            overheatTimer = overheatLockoutTime;
            Debug.Log("[Overheat] OVERHEATED!");
        }
        
        // Store state
        context.CustomData["RapidFireBlocked"] = false;
        context.CustomData["Overheated"] = false;
        context.CustomData["CurrentHeat"] = currentHeat;
        context.CustomData["HeatPercent"] = currentHeat / maxHeat;
    }
}