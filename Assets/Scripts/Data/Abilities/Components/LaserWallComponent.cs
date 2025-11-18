using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "ScriptableObjects/Components/LargeAssLaser")]
public class LaserWallComponent : PartComponent
{
  
    [Header("Laser Settings")]
    public GameObject tracerPrefab;
    public float laserRange = 10f;

    [Header("Grid Pattern")]
    public int gridWidth = 7;
    public int gridHeight = 7;
    public float gridSpacing = 0.25f;
    
    [Header("Hold Settings")]
    public float tracerLifetime = 2f;
    public float refreshRate = 0.1f;

    public override void Initialize(PartContext context)
    {
        context.CustomData["wasPressed"] = false;
        context.CustomData["lastFireTime"] = -999f;
    }

    public override void OnExecute(PartContext context)
    {
       Debug.Log($"[LaserWall] OnExecute called!");
    }

    public override void OnUpdate(PartContext context, float deltaTime)
    {
        InputAction inputAction = context.CustomData["InputAction"] as InputAction;
        bool pressing = inputAction != null && inputAction.ReadValue<float>() > 0.5f;
        
        bool wasPressed = (bool)context.CustomData["wasPressed"];
        float lastFireTime = (float)context.CustomData["lastFireTime"];
        
        // Fire
        if (pressing && !wasPressed)
        {
            FireLaserWall(context);
            lastFireTime = Time.time;
        }
        
        //Fire while hold
        if (pressing && Time.time - lastFireTime >= refreshRate)
        {
            FireLaserWall(context);
            lastFireTime = Time.time;
        }
        
        context.CustomData["wasPressed"] = pressing;
        context.CustomData["lastFireTime"] = lastFireTime;
    }
    
    private void FireLaserWall(PartContext context)
    {
        if (context.Owner == null || tracerPrefab == null) return;

        int halfWidth = gridWidth / 2;

        for (int dx = -halfWidth; dx <= halfWidth; dx++)
        {
            for (int dy = 0; dy < gridHeight; dy++)
            {
                Vector3 offset = new Vector3(dx, dy, 0) * gridSpacing;
                Vector3 worldOffset = context.Owner.rotation * offset;
                Vector3 origin = context.Owner.position + worldOffset;
                Vector3 direction = context.Owner.forward;

                Ray ray = new Ray(origin, direction);
                bool hit = Physics.Raycast(ray, out RaycastHit hitInfo, laserRange);

                Vector3 endPoint = hit ? hitInfo.point : origin + direction * laserRange;
                float distance = Vector3.Distance(origin, endPoint);

                // Spawn tracer
                var tracer = GameObject.Instantiate(tracerPrefab);
                tracer.transform.position = origin;
                tracer.transform.LookAt(endPoint);
                tracer.transform.localScale = new Vector3(1, 1, distance);
                GameObject.Destroy(tracer, tracerLifetime);

                // Apply damage if hit enemy
                if (hit && hitInfo.collider.CompareTag("Enemy"))
                {
                    var enemy = hitInfo.collider.GetComponent<EnemyHealth>();
                    if (enemy != null)
                    {
                        // enemy.TakeDamage(baseDamage);
                    }

                    if (knockbackForce > 0)
                    {
                        var enemyRb = hitInfo.collider.GetComponent<Rigidbody>();
                        if (enemyRb != null)
                            enemyRb.AddForce(direction * knockbackForce, ForceMode.Impulse);
                    }
                }
            }
        }
    }
}
