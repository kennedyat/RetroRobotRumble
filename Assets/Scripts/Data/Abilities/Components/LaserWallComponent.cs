using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "ScriptableObjects/Components/LargeAssLaser")]
public class LaserWallComponent : PartComponent
{

    [Header("Timing")]
    public float cooldownSeconds = 95f;
    public float chargeSeconds = 2f;
    public float fireSeconds = 3f;

    [Header("Laser Settings")]
    public GameObject tracerPrefab;
    public float laserRange = 10f;

    [Header("Damage")]
    public int damagePerTick = 2;

    [Header("Tick Rate")]
    public float refreshRate = 0.1f;

    [Header("Visual")]
    [Tooltip("Beam thickness used for both damage area and tracer scale.")]
    public float laserThickness = 1f;

    [Tooltip("Spawn the beam slightly above ground so it doesn't clip. Optional.")]
    public float originHeightOffset = 0.8f;

    private const string KEY_WAS_PRESSED = "LW_wasPressed";
    private const string KEY_PHASE = "LW_phase";
    private const string KEY_PHASE_T = "LW_phaseTimer";
    private const string KEY_COOLDOWN = "LW_cooldownRemaining";
    private const string KEY_TICK_T = "LW_tickTimer";
    private const string KEY_TRACER = "LW_tracerInstance";

    private enum Phase { Ready = 0, Charging = 1, Firing = 2, Cooldown = 3 }

    public override void Initialize(PartContext context)
    {
        context.CustomData[KEY_WAS_PRESSED] = false;
        context.CustomData[KEY_PHASE] = (int)Phase.Ready;
        context.CustomData[KEY_PHASE_T] = 0f;
        context.CustomData[KEY_COOLDOWN] = 0f;   
        context.CustomData[KEY_TICK_T] = 0f;
        context.CustomData[KEY_TRACER] = null;
    }

    public override void OnExecute(PartContext context)
    {
        Debug.Log("[LaserWall] OnExecute called!");
    }

    public override void OnUpdate(PartContext context, float deltaTime)
    {
        InputAction inputAction = context.CustomData["InputAction"] as InputAction;
        bool pressing = inputAction != null && inputAction.ReadValue<float>() > 0.5f;

        bool wasPressed = (bool)context.CustomData[KEY_WAS_PRESSED];
        Phase phase = (Phase)(int)context.CustomData[KEY_PHASE];

        float phaseTimer = (float)context.CustomData[KEY_PHASE_T];
        float cooldownRemaining = (float)context.CustomData[KEY_COOLDOWN];
        float tickTimer = (float)context.CustomData[KEY_TICK_T];

        if (phase == Phase.Cooldown)
        {
            cooldownRemaining -= deltaTime;
            if (cooldownRemaining <= 0f)
            {
                cooldownRemaining = 0f;
                phase = Phase.Ready;
                phaseTimer = 0f;
            }
        }

        if (pressing && !wasPressed && phase == Phase.Ready)
        {
            phase = Phase.Charging;
            phaseTimer = 0f;
            tickTimer = 0f;

            Debug.Log("[LaserWall] Started CHARGE.");
        }

        if (phase == Phase.Charging)
        {
            phaseTimer += deltaTime;


            if (phaseTimer >= chargeSeconds)
            {
                phase = Phase.Firing;
                phaseTimer = 0f;
                tickTimer = refreshRate; 
                EnsureTracer(context);    

                Debug.Log("[LaserWall] Charge complete. Started FIRING.");
            }
        }
        else if (phase == Phase.Firing)
        {
            phaseTimer += deltaTime;
            tickTimer += deltaTime;

            UpdateTracer(context);

            if (tickTimer >= refreshRate)
            {
                tickTimer = 0f;
                FireDamageTick(context);
            }

            if (phaseTimer >= fireSeconds)
            {
                CleanupTracer(context);

                phase = Phase.Cooldown;
                phaseTimer = 0f;
                cooldownRemaining = cooldownSeconds;

                Debug.Log("[LaserWall] Firing finished. Cooldown started.");
            }
        }

        context.CustomData[KEY_WAS_PRESSED] = pressing;
        context.CustomData[KEY_PHASE] = (int)phase;
        context.CustomData[KEY_PHASE_T] = phaseTimer;
        context.CustomData[KEY_COOLDOWN] = cooldownRemaining;
        context.CustomData[KEY_TICK_T] = tickTimer;

        context.CustomData["LaserWallPhase"] = (int)phase;
        context.CustomData["LaserWallCooldown"] = cooldownRemaining;
        context.CustomData["LaserWallChargePct"] = (phase == Phase.Charging && chargeSeconds > 0f) ? Mathf.Clamp01(phaseTimer / chargeSeconds) : 0f;
        context.CustomData["LaserWallFirePct"] = (phase == Phase.Firing && fireSeconds > 0f) ? Mathf.Clamp01(phaseTimer / fireSeconds) : 0f;
    }

    private void FireDamageTick(PartContext context)
    {
        if (context.Owner == null)
            return;

        Vector3 origin = GetOrigin(context);
        Vector3 direction = context.Owner.forward.normalized;
        Quaternion orientation = Quaternion.LookRotation(direction);

        float beamWidth = laserThickness;
        float beamHeight = laserThickness;

        Vector3 halfExtents = new Vector3(beamWidth * 0.5f, beamHeight * 0.5f, 0.05f);

        RaycastHit[] hits = Physics.BoxCastAll(
            origin,
            halfExtents,
            direction,
            orientation,
            laserRange,
            ~0,
            QueryTriggerInteraction.Collide
        );

        if (hits == null || hits.Length == 0)
            return;

        HashSet<Enemy> damagedThisTick = new HashSet<Enemy>();

        for (int i = 0; i < hits.Length; i++)
        {
            Collider c = hits[i].collider;
            if (c == null)
                continue;

            if (!c.CompareTag("Enemy"))
                continue;

            Enemy e = c.GetComponentInParent<Enemy>();
            if (e == null)
                continue;

            if (damagedThisTick.Add(e))
            {
                e.DealDamage(damagePerTick);
            }
        }
    }

    private void EnsureTracer(PartContext context)
    {
        if (context.Owner == null || tracerPrefab == null)
            return;

        GameObject existing = context.CustomData[KEY_TRACER] as GameObject;

        if (existing != null)
            return;

        var tracer = GameObject.Instantiate(tracerPrefab);
        context.CustomData[KEY_TRACER] = tracer;

        DisableAutoDestroyScripts(tracer);

        UpdateTracer(context);
    }

    private void DisableAutoDestroyScripts(GameObject root)
    {
        var auto = root.GetComponent<Tracer>();
        if (auto != null)
            auto.enabled = false;

        var autos = root.GetComponentsInChildren<Tracer>(true);
        for (int i = 0; i < autos.Length; i++)
            autos[i].enabled = false;
    }

    private void UpdateTracer(PartContext context)
    {
        GameObject tracer = context.CustomData[KEY_TRACER] as GameObject;
        if (tracer == null || context.Owner == null)
            return;

        Vector3 origin = GetOrigin(context);
        Vector3 direction = context.Owner.forward.normalized;

        Vector3 endPoint = origin + direction * laserRange;
        if (Physics.Raycast(new Ray(origin, direction), out RaycastHit hitInfo, laserRange, ~0, QueryTriggerInteraction.Ignore))
        {
            endPoint = hitInfo.point;
        }

        float distance = Vector3.Distance(origin, endPoint);

        tracer.transform.position = origin;
        tracer.transform.rotation = Quaternion.LookRotation(direction);

        float beamWidth = laserThickness;
        float beamHeight = laserThickness;

        tracer.transform.localScale = new Vector3(beamWidth, beamHeight, distance);
    }

    private void CleanupTracer(PartContext context)
    {
        GameObject tracer = context.CustomData[KEY_TRACER] as GameObject;
        context.CustomData[KEY_TRACER] = null;

        if (tracer != null)
            GameObject.Destroy(tracer);
    }

    private Vector3 GetOrigin(PartContext context)
    {
        Vector3 origin = context.Owner.position;
        origin.y += originHeightOffset;
        return origin;
    }
}
