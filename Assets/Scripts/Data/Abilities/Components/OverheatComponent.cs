using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Components/Overheat")]
public class OverheatComponent : PartComponent
{
    [Header("Special Projectile (Tiger Claw)")]
    [Tooltip("Use a special-only projectile prefab if you have one. If null, falls back to projectilePrefab.")]
    public GameObject specialProjectilePrefab;

    [Tooltip("Fallback projectile if specialProjectilePrefab is not set.")]
    public GameObject projectilePrefab;

    public float projectileRange = 100f;
    public string spawnPointName = "SpawnPoint";

    [Header("Special Duration")]
    public float specialDuration = 7f;

    [Header("Special Fire Rate (CONSTANT)")]
    public float shotsPerSecond = 8f;

    [Header("Special Spread")]
    public float minSpread = 10f;
    public float maxSpread = 55f;
    public float spreadGrowTime = 1.75f;

    [Header("Projectile Size")]
    [Tooltip("Scales the spawned projectile transform for special.")]
    public float projectileScaleMultiplier = 1.75f;

    public override void Initialize(PartContext context)
    {

    }

    public override void OnExecute(PartContext context)
    {
        if (context.Owner == null)
        {
            Debug.LogWarning("[TigerSpecial] context.Owner null.");
            return;
        }

        TigerSpecialRuntime rt = TigerSpecialRuntime.GetOrCreate(context.Owner);
        rt.StartSpecial(
            owner: context.Owner,
            spawnPointName: spawnPointName,
            prefab: (specialProjectilePrefab != null ? specialProjectilePrefab : projectilePrefab),
            range: projectileRange,
            duration: specialDuration,
            sps: shotsPerSecond,
            minSpread: minSpread,
            maxSpread: maxSpread,
            spreadGrowTime: spreadGrowTime,
            scaleMult: projectileScaleMultiplier
        );
    }

    public override void OnUpdate(PartContext context, float deltaTime)
    {

    }
}

/// <summary>
/// Runtime helper that lives on the arm/owner transform and executes the 7s special.
/// Also serves as a "lockout" flag so normals can't fire during special.
/// </summary>
public class TigerSpecialRuntime : MonoBehaviour
{
    private static readonly string RuntimeKey = "_TigerSpecialRuntimeAttached";

    private bool active;
    private Coroutine routine;
    private string spawnPointName;
    private GameObject prefab;
    private float range;
    private float duration;
    private float sps;
    private float minSpread;
    private float maxSpread;
    private float spreadGrowTime;
    private float scaleMult;

    public static TigerSpecialRuntime GetOrCreate(Transform owner)
    {
        TigerSpecialRuntime rt = owner.GetComponent<TigerSpecialRuntime>();
        if (rt == null)
            rt = owner.gameObject.AddComponent<TigerSpecialRuntime>();
        return rt;
    }

    public static bool IsSpecialActive(PartContext ctx)
    {
        if (ctx == null || ctx.Owner == null)
            return false;
        TigerSpecialRuntime rt = ctx.Owner.GetComponent<TigerSpecialRuntime>();
        return rt != null && rt.active;
    }

    public void StartSpecial(
        Transform owner,
        string spawnPointName,
        GameObject prefab,
        float range,
        float duration,
        float sps,
        float minSpread,
        float maxSpread,
        float spreadGrowTime,
        float scaleMult
    )
    {
        if (active)
            return;

        this.spawnPointName = spawnPointName;
        this.prefab = prefab;
        this.range = range;
        this.duration = duration;
        this.sps = Mathf.Max(0.1f, sps);
        this.minSpread = minSpread;
        this.maxSpread = maxSpread;
        this.spreadGrowTime = Mathf.Max(0.0001f, spreadGrowTime);
        this.scaleMult = Mathf.Max(0.01f, scaleMult);

        routine = StartCoroutine(SpecialRoutine(owner));
    }

    private IEnumerator SpecialRoutine(Transform owner)
    {
        active = true;

        float elapsed = 0f;
        float timeToNext = 0f;
        float spread01 = 0f;

        while (elapsed < duration)
        {
            float dt = Time.deltaTime;
            elapsed += dt;
            spread01 = Mathf.Clamp01(elapsed / spreadGrowTime);

            timeToNext -= dt;
            if (timeToNext <= 0f)
            {
                Fire(owner, spread01);
                timeToNext = 1f / sps;
            }

            yield return null;
        }

        active = false;
        routine = null;
    }

    private void Fire(Transform owner, float spread01)
    {
        if (owner == null || prefab == null)
            return;

        Transform sp = owner.Find(spawnPointName);
        if (sp == null)
            sp = owner;

        float spreadDeg = Mathf.Lerp(minSpread, maxSpread, spread01);

        Quaternion randomRotation = Quaternion.AngleAxis(
            Random.Range(-spreadDeg * 0.5f, spreadDeg * 0.5f),
            Vector3.up
        );

        Vector3 direction = randomRotation * sp.forward;

        GameObject instance = Instantiate(prefab);

        instance.transform.localScale *= scaleMult;

        Projectile projectile = instance.GetComponent<Projectile>();
        if (projectile != null)
        {
            Ray ray = new Ray(sp.position, direction);
            projectile.FollowRay(ray, range);
        }
        else
        {
            instance.transform.position = sp.position;
            instance.transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
