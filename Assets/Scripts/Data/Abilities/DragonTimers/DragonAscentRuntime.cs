using System.Collections;
using UnityEngine;

public class DragonAscensionRuntime : MonoBehaviour
{
    [Header("Config")]
    public float chargeTime = 1f;
    public float airTimeMax = 3f;
    public float riseHeight = 8f;
    public float riseSpeed = 12f;
    public float diveSpeed = 30f;

    [Header("AOE")]
    public float landingHitboxDuration = 0.25f;
    public float landingDamage = 50f;

    [Header("Fire Zone")]
    public GameObject fireZonePrefab;
    public float fireZoneLifetime = 4f;

    private bool inProgress = false;
    private bool inAir = false;

    private PartContext ctx;
    private Coroutine routine;

    public void StartUltimate(PartContext context, float charge, float airMax)
    {
        if (inProgress)
            return;

        ctx = context;
        chargeTime = charge;
        airTimeMax = airMax;

        routine = StartCoroutine(UltimateRoutine());
    }

    private IEnumerator UltimateRoutine()
    {
        inProgress = true;
        yield return new WaitForSeconds(chargeTime);

        inAir = true;
        Vector3 start = transform.position;
        Vector3 target = start + Vector3.up * riseHeight;

        while ((transform.position - target).sqrMagnitude > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, riseSpeed * Time.deltaTime);
            yield return null;
        }
        yield return new WaitForSeconds(airTimeMax);

        Vector3 groundTarget = new Vector3(transform.position.x, start.y, transform.position.z);

        while (transform.position.y > start.y + 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, groundTarget, diveSpeed * Time.deltaTime);
            yield return null;
        }

        if (ctx != null && ctx.HitBox != null && ctx.hitBoxManager != null)
        {
            HitBox box = ctx.HitBox;
            ctx.hitBoxManager.SetHitBox(box);
            HitBoxManager.duration = landingHitboxDuration;

            box.OnHit = (Collider targetCol) =>
            {
                Enemy e = targetCol.GetComponent<Enemy>();
                if (e != null)
                {
                    e.DealDamage(Mathf.RoundToInt(landingDamage));
                }
            };
        }

        if (fireZonePrefab != null)
        {
            GameObject zone = Instantiate(
                fireZonePrefab,
                new Vector3(transform.position.x, 0.05f, transform.position.z),
                Quaternion.identity
            );
            Destroy(zone, fireZoneLifetime);
        }

        inAir = false;
        inProgress = false;
    }
}
