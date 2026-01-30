using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Parts/Dragon/Draconic Ascension Ultimate")]
public class DraconicAscensionUltimateComponent : PartComponent
{
    [Header("Ultimate Settings")]
    public float chargeTime = 1f;
    public float airTimeMax = 3f;

    [Header("Landing Damage")]
    public float landingDamage = 50f;
    public float landingHitboxDuration = 0.25f;

    [Header("Fire Zone")]
    public GameObject fireZonePrefab;
    public float fireZoneLifetime = 4f;

    private DragonAscensionRuntime runtime;

    public override void Initialize(PartContext context)
    {
        runtime = context.Owner.GetComponent<DragonAscensionRuntime>();
        if (runtime == null)
            runtime = context.Owner.gameObject.AddComponent<DragonAscensionRuntime>();

        runtime.fireZonePrefab = fireZonePrefab;
        runtime.fireZoneLifetime = fireZoneLifetime;
        runtime.landingDamage = landingDamage;
        runtime.landingHitboxDuration = landingHitboxDuration;
    }

    public override void OnExecute(PartContext context)
    {
        runtime.StartUltimate(context, chargeTime, airTimeMax);
    }

    public override void OnUpdate(PartContext context, float deltaTime) { }
}
