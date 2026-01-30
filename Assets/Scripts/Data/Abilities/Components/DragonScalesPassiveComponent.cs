using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Parts/Dragon/Draconic Scales Passive")]
public class DragonScalesPassiveComponent : PartComponent
{
    [Header("Passive Settings")]
    public float cooldownSeconds = 90f;
    public float lingerSeconds = 1.5f;
    public float cooldownReductionPerHit = 1f;

    private DragonScaleRuntime runtime;

    public override void Initialize(PartContext context)
    {
        if (context.Owner == null)
            return;

        PlayerHealth ph = context.Owner.GetComponentInParent<PlayerHealth>();
        if (ph == null)
        {
            Debug.LogWarning("[DragonScalesPassiveComponent] No PlayerHealth found on owner.");
            return;
        }

        runtime = context.Owner.GetComponent<DragonScaleRuntime>();
        if (runtime == null)
            runtime = context.Owner.gameObject.AddComponent<DragonScaleRuntime>();
        runtime.Initialize(ph, cooldownSeconds, lingerSeconds);

        CombatEvents.OnOwnerHitEnemy -= OnOwnerHitEnemy;
        CombatEvents.OnOwnerHitEnemy += OnOwnerHitEnemy;
    }

    public override void OnExecute(PartContext context)
    {
    }

    public override void OnUpdate(PartContext context, float deltaTime)
    {
        if (runtime != null)
        {
            runtime.Tick(deltaTime);
        }
    }

    private void OnOwnerHitEnemy(Transform owner)
    {
        if (runtime == null)
            return;
        if (owner != runtime.transform)
            return;
        runtime.ReduceCooldown(cooldownReductionPerHit);
    }
}
