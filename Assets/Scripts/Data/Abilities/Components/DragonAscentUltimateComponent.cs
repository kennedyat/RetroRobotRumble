using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Parts/Dragon/Draconic Ascension Ultimate")]
public class DraconicAscensionUltimateComponent : PartComponent
{
    [Header("Ultimate Settings")]
    public float chargeTime = 1f;
    public float airTimeMax = 3f;

    [Header("Movement Tuning")]
    public float riseHeight = 8f;
    public float riseSpeed = 12f;
    public float diveSpeed = 30f;
    [Tooltip("Extra acceleration applied while diving so the landing feels heavier.")]
    public float diveAcceleration = 20f;
    [Tooltip("Horizontal movement speed while airborne during the ultimate.")]
    public float airControlSpeed = 8f;

    [Header("Camera")]
    [Tooltip("Extra downward pitch applied to the combat camera target while airborne.")]
    public float airborneCameraPitch = 18f;
    [Tooltip("Pushes the combat camera target farther along its current line to widen the view.")]
    public float airborneCameraDistanceOffset = 0f;
    [Tooltip("Small local offset added after pitch, mainly for fine tuning.")]
    public Vector3 airborneCameraOffset = Vector3.zero;
    public float airborneCameraBlendSpeed = 6f;
    public float airborneCameraRestoreTime = 0.2f;

    [Header("Landing Damage")]
    public float landingDamage = 50f;
    public float landingHitboxDuration = 0.25f;

    [Header("Landing AOE Hitbox Size (BoxCollider)")]
    [Tooltip("Smaller default than before. Tune this in inspector.")]
    public Vector3 landingHitboxSize = new Vector3(4f, 2f, 4f);

    [Tooltip("Center offset of the landing hitbox relative to its transform.")]
    public Vector3 landingHitboxCenter = new Vector3(0f, 0.5f, 0f);

    [Header("Fire Zone")]
    public GameObject fireZonePrefab;
    public float fireZoneLifetime = 4f;

    [Tooltip("Scales the spawned fire zone instance (NOT the prefab asset).")]
    public Vector3 fireZoneScale = new Vector3(1.5f, 1f, 1.5f);


    private DragonAscensionRuntime runtime;

    [SerializeField] private AK.Wwise.Event takeoffSFX;
    [SerializeField] private AK.Wwise.Event diveHitSFX;

    public override void Initialize(PartContext context)
    
    {
        Transform host = context.Rigidbody != null ? context.Rigidbody.transform : context.Owner;
        if (host == null)
        {
            Debug.LogWarning("[DraconicAscensionUltimateComponent] No valid host transform (Owner/Rigidbody).");
            return;
        }

        runtime = host.GetComponent<DragonAscensionRuntime>();
        if (runtime == null)
            runtime = host.gameObject.AddComponent<DragonAscensionRuntime>();


        runtime.chargeTime = chargeTime;
        runtime.airTimeMax = airTimeMax;
        runtime.riseHeight = riseHeight;
        runtime.riseSpeed = riseSpeed;
        runtime.diveSpeed = diveSpeed;
        runtime.diveAcceleration = diveAcceleration;
        runtime.airControlSpeed = airControlSpeed;
        runtime.airborneCameraPitch = airborneCameraPitch;
        runtime.airborneCameraDistanceOffset = airborneCameraDistanceOffset;
        runtime.airborneCameraOffset = airborneCameraOffset;
        runtime.airborneCameraBlendSpeed = airborneCameraBlendSpeed;
        runtime.airborneCameraRestoreTime = airborneCameraRestoreTime;

        runtime.fireZonePrefab = fireZonePrefab;
        runtime.fireZoneLifetime = fireZoneLifetime;
        runtime.fireZoneScale = fireZoneScale;

        runtime.landingDamage = landingDamage;
        runtime.landingHitboxDuration = landingHitboxDuration;

        if (context.HitBox != null)
        {
            BoxCollider bc = context.HitBox.GetComponent<BoxCollider>();
            if (bc != null)
            {
                bc.isTrigger = true;
                bc.size = landingHitboxSize;
                bc.center = landingHitboxCenter;
            }
            else
            {
                Debug.LogWarning("[DraconicAscensionUltimateComponent] HitBox has no BoxCollider.");
            }
        }
        else
        {
            Debug.LogWarning("[DraconicAscensionUltimateComponent] context.HitBox is NULL (ultimateHitbox not wired?).");
        }
    }

    public override void OnExecute(PartContext context)
    {
        if (runtime == null)
        {
            Debug.LogWarning("[DraconicAscensionUltimateComponent] Runtime missing; did Initialize run?");
            return;
        }

        runtime.StartUltimate(context, chargeTime, airTimeMax);
    }

    public override void OnUpdate(PartContext context, float deltaTime) { }
}
            
