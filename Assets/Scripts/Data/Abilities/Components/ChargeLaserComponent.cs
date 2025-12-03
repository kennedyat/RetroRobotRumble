using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "ScriptableObjects/Components/ChargeLaser")]
public class ChargeLaserComponent : PartComponent
{
 [Header("Projectile")]
    public GameObject orbPrefab;
    
    [Header("Charge Settings")]
    public float fullChargeTimeSeconds = 1f;
    public float minProjectileRange = 2.5f;
    public float maxProjectileRange = 5f;
    public float minScale = 1f;
    public float maxScale = 5f;
    
    [Header("Spawn Point")]
    public string spawnPointName = "SpawnPoint";
    
    public override void Initialize(PartContext context)
    {
        context.CustomData["wasPressed"] = false;
        context.CustomData["chargeTime"] = 0f;
        context.CustomData["isCharging"] = false;
    }
    
    public override void OnExecute(PartContext context)
    {
         Debug.Log($"[ChargeLaserComponent] OnExecute called!");
    }
    
    public override void OnUpdate(PartContext context, float deltaTime)
    {
        InputAction inputAction = context.CustomData["InputAction"] as InputAction;
        bool pressing = inputAction != null && inputAction.ReadValue<float>() > 0.5f;
        
        bool wasPressed = (bool)context.CustomData["wasPressed"];
        bool isCharging = (bool)context.CustomData["isCharging"];
        float chargeTime = (float)context.CustomData["chargeTime"];
        
        // Just pressed - start charging
        if (pressing && !wasPressed)
        {
            isCharging = true;
            chargeTime = 0f;
        }
        
        // While holding - charge up
        if (pressing && isCharging)
        {
            chargeTime += deltaTime;
        }
        
        // Just released - fire!
        if (!pressing && wasPressed && isCharging)
        {
            FireCharged(context, chargeTime);
            isCharging = false;
            chargeTime = 0f;
        }

        context.CustomData["wasPressed"] = pressing;
        context.CustomData["isCharging"] = isCharging;
        context.CustomData["chargeTime"] = chargeTime;
    }
    
    private void FireCharged(PartContext context, float chargeTime)
    {
        if (context.Owner == null || orbPrefab == null) return;
        
        Transform spawnPoint = FindSpawnPoint(context.Owner);
        
        float chargePercent = Mathf.Clamp01(chargeTime / fullChargeTimeSeconds);
        float scale = Mathf.Lerp(minScale, maxScale, chargePercent);
        float range = Mathf.Lerp(minProjectileRange, maxProjectileRange, chargePercent);
        
        GameObject instance = GameObject.Instantiate(
            orbPrefab, 
            spawnPoint.position, 
            Quaternion.LookRotation(spawnPoint.forward)
        );
        
        instance.transform.localScale = Vector3.one * scale;
        
        Projectile projectile = instance.GetComponent<Projectile>();
        if (projectile == null)
            projectile = instance.GetComponentInChildren<Projectile>();
        
        if (projectile != null)
        {
            Ray shotRay = new Ray(spawnPoint.position, spawnPoint.forward);
            projectile.FollowRay(shotRay, range);
        }
    }
    
    private Transform FindSpawnPoint(Transform owner)
    {       
        Transform spawnPoint = owner.Find(spawnPointName);
               
        return spawnPoint != null ? spawnPoint : owner;
    }
    
    
   
}
