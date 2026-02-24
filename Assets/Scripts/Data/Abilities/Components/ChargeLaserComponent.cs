using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "ScriptableObjects/Components/ChargeLaser")]
public class ChargeLaserComponent : PartComponent
{
    [Header("Projectile")]
    public GameObject orbPrefab;

    [Header("Charge Settings")]
    [Tooltip("Time to reach full charge (100%).")]
    public float fullChargeTimeSeconds = 1f;

    [Header("Distance (Fixed)")]
    [Tooltip("Projectile always travels this distance now (charge does NOT change it).")]
    public float fixedProjectileRange = 5f;

    [Header("Scale (charge => bigger)")]
    public float minScale = 1f;
    public float maxScale = 2.5f;

    [Header("Damage (charge => higher)")]
    [Tooltip("Damage at 0% charge.")]
    public float minDamage = 2f;

    [Tooltip("Damage at 100% charge.")]
    public float maxDamage = 10f;

    [Header("Speed (charge => slower)")]
    [Tooltip("Speed multiplier at 0% charge. 1 = normal.")]
    public float minSpeedMultiplier = 10f;

    [Tooltip("Speed multiplier at 100% charge. Example: 0.5 = half speed.")]
    public float maxSpeedMultiplier = 2f;

    [Header("Spawn Point")]
    public string spawnPointName = "SpawnPoint";

    [Header("Debug")]
    public bool debugLogs = false;

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
        if (context.Owner == null || orbPrefab == null)
            return;

        Transform spawnPoint = FindSpawnPoint(context.Owner);

        float chargePercent = Mathf.Clamp01(chargeTime / Mathf.Max(0.0001f, fullChargeTimeSeconds));

        float scale = Mathf.Lerp(minScale, maxScale, chargePercent);
        float damage = Mathf.Lerp(minDamage, maxDamage, chargePercent);

        // More charge => slower
        float speedMultiplier = Mathf.Lerp(minSpeedMultiplier, maxSpeedMultiplier, chargePercent);

        GameObject instance = GameObject.Instantiate(
            orbPrefab,
            spawnPoint.position,
            Quaternion.LookRotation(spawnPoint.forward)
        );

        instance.transform.localScale = Vector3.one * scale;

        // Grab projectile component (root or children)
        Component projectile = instance.GetComponent<Projectile>();
        if (projectile == null)
            projectile = instance.GetComponentInChildren<Projectile>();

        // Always fixed distance now
        Ray shotRay = new Ray(spawnPoint.position, spawnPoint.forward);

        if (projectile != null)
        {
            // Distance is fixed
            ((Projectile)projectile).FollowRay(shotRay, fixedProjectileRange);

            // Try to push damage + speed onto the projectile (best-effort)
            bool dmgApplied = TrySetProjectileDamage(projectile, damage);
            bool spdApplied = TrySetProjectileSpeed(projectile, speedMultiplier);

            if (debugLogs)
            {
                Debug.Log($"[ChargeLaser] charge={chargePercent:F2} scale={scale:F2} dmg={damage:F1} speedMul={speedMultiplier:F2} " +
                          $"(damageApplied={dmgApplied}, speedApplied={spdApplied})");
            }
        }
        else
        {
            // Fallback: at least orient the prefab correctly
            instance.transform.rotation = Quaternion.LookRotation(spawnPoint.forward);

            if (debugLogs)
                Debug.LogWarning("[ChargeLaser] No Projectile component found on orbPrefab (root or children). FollowRay not called.");
        }
    }

    private Transform FindSpawnPoint(Transform owner)
    {
        Transform spawnPoint = owner.Find(spawnPointName);
        return spawnPoint != null ? spawnPoint : owner;
    }

    // -------------------------
    // Best-effort hook helpers
    // -------------------------

    private bool TrySetProjectileDamage(Component projectile, float damage)
    {
        // Methods to try
        if (TryInvokeMethod(projectile, "SetDamage", damage))
            return true;
        if (TryInvokeMethod(projectile, "SetBaseDamage", damage))
            return true;
        if (TryInvokeMethod(projectile, "SetProjectileDamage", damage))
            return true;

        // Fields/properties to try
        if (TrySetMember(projectile, "damage", damage))
            return true;
        if (TrySetMember(projectile, "baseDamage", damage))
            return true;
        if (TrySetMember(projectile, "Damage", damage))
            return true;
        if (TrySetMember(projectile, "BaseDamage", damage))
            return true;

        return false;
    }

    private bool TrySetProjectileSpeed(Component projectile, float speedMultiplier)
    {
        // Methods to try
        if (TryInvokeMethod(projectile, "SetSpeedMultiplier", speedMultiplier))
            return true;
        if (TryInvokeMethod(projectile, "SetSpeed", speedMultiplier))
            return true; // sometimes interpreted as multiplier
        if (TryInvokeMethod(projectile, "SetMoveSpeedMultiplier", speedMultiplier))
            return true;

        // Fields/properties to try (multiplier style)
        if (TrySetMember(projectile, "speedMultiplier", speedMultiplier))
            return true;
        if (TrySetMember(projectile, "SpeedMultiplier", speedMultiplier))
            return true;

        // If the projectile uses absolute speed (not multiplier), you may prefer:
        // speed = baseSpeed * multiplier
        // We can still try setting common names directly to multiplier (won't help if it's absolute).
        if (TrySetMember(projectile, "speed", speedMultiplier))
            return true;
        if (TrySetMember(projectile, "moveSpeed", speedMultiplier))
            return true;
        if (TrySetMember(projectile, "Speed", speedMultiplier))
            return true;
        if (TrySetMember(projectile, "MoveSpeed", speedMultiplier))
            return true;

        return false;
    }

    private bool TryInvokeMethod(Component target, string methodName, float arg)
    {
        var t = target.GetType();
        var m = t.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (m == null)
            return false;

        var ps = m.GetParameters();
        if (ps.Length != 1)
            return false;

        // Accept float/int
        if (ps[0].ParameterType == typeof(float))
        {
            m.Invoke(target, new object[] { arg });
            return true;
        }
        if (ps[0].ParameterType == typeof(int))
        {
            m.Invoke(target, new object[] { Mathf.RoundToInt(arg) });
            return true;
        }

        return false;
    }

    private bool TrySetMember(Component target, string memberName, float value)
    {
        var t = target.GetType();

        // Field
        var f = t.GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (f != null)
        {
            if (f.FieldType == typeof(float))
            {
                f.SetValue(target, value);
                return true;
            }
            if (f.FieldType == typeof(int))
            {
                f.SetValue(target, Mathf.RoundToInt(value));
                return true;
            }
        }

        // Property
        var p = t.GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (p != null && p.CanWrite)
        {
            if (p.PropertyType == typeof(float))
            {
                p.SetValue(target, value);
                return true;
            }
            if (p.PropertyType == typeof(int))
            {
                p.SetValue(target, Mathf.RoundToInt(value));
                return true;
            }
        }

        return false;
    }
}
