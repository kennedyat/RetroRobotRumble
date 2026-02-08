using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Parts/Dragon/Draconic Scales Passive")]
public class DragonScalesPassiveComponent : PartComponent
{
    [Header("Passive Settings")]
    public float cooldownSeconds = 90f;
    public float lingerSeconds = 1.5f;
    public float cooldownReductionPerHit = 1f;

    [Header("Debug")]
    public bool debugLogs = true;

    private DragonScaleRuntime runtime;
    private Transform expectedOwner;

    private float lastCooldown = -999f;

    public override void Initialize(PartContext context)
    {
        Transform host = context.Rigidbody != null ? context.Rigidbody.transform : context.Owner;
        if (host == null)
        {
            Debug.LogWarning("[DragonScalesPassiveComponent] No valid host transform (Owner/Rigidbody).");
            return;
        }

        expectedOwner = host;

        PlayerHealth ph = host.GetComponent<PlayerHealth>();
        if (ph == null)
            ph = host.GetComponentInParent<PlayerHealth>();

        if (ph == null)
        {
            Debug.LogWarning("[DragonScalesPassiveComponent] No PlayerHealth found on host or parents.");
            return;
        }

        runtime = host.GetComponent<DragonScaleRuntime>();
        if (runtime == null)
            runtime = host.gameObject.AddComponent<DragonScaleRuntime>();

        runtime.Initialize(ph, cooldownSeconds, lingerSeconds);

        CombatEvents.OnOwnerHitEnemy -= OnOwnerHitEnemy;
        CombatEvents.OnOwnerHitEnemy += OnOwnerHitEnemy;

        if (debugLogs)
        {
            Debug.Log($"[DragonScalesPassiveComponent] Initialized on {host.name}. Shield recharges in {cooldownSeconds}s.");
        }
    }

    public override void OnExecute(PartContext context)
    {
    }

    public override void OnUpdate(PartContext context, float deltaTime)
    {
        if (runtime == null)
            return;

        runtime.Tick(deltaTime);

        if (debugLogs)
        {
            float cd = runtime.RemainingCooldown;

            if (lastCooldown < -100f)
                lastCooldown = cd;

            if (lastCooldown > 0f && cd <= 0f)
            {
                Debug.Log("[DragonScalesPassiveComponent] Shield READY (invuln should be ON).");
            }

            if (cd > lastCooldown + 0.5f)
            {
                Debug.Log($"[DragonScalesPassiveComponent] Shield cycle reset. Cooldown restarted at ~{cd:F1}s.");
            }

            lastCooldown = cd;
        }
    }

    private void OnOwnerHitEnemy(Transform owner)
    {
        if (runtime == null || expectedOwner == null)
            return;

        if (owner != expectedOwner)
            return;

        runtime.ReduceCooldown(cooldownReductionPerHit);

        if (debugLogs)
        {
            Debug.Log($"[DragonScalesPassiveComponent] Reduced shield cooldown by {cooldownReductionPerHit:F1}s. Remaining: {runtime.RemainingCooldown:F1}s");
        }
    }
}
