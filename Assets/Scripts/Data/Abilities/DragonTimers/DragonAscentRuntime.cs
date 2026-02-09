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

    [Tooltip("Scale applied to the spawned instance.")]
    public Vector3 fireZoneScale = Vector3.one;

    private bool inProgress = false;

    private PartContext ctx;
    private Coroutine routine;

    private GameObject zoneInstance;

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

        Vector3 start = transform.position;
        Vector3 apex = start + Vector3.up * riseHeight;

        while ((transform.position - apex).sqrMagnitude > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, apex, riseSpeed * Time.deltaTime);
            yield return null;
        }

        if (fireZonePrefab != null && zoneInstance == null)
        {
            Vector3 groundPos = new Vector3(transform.position.x, 0.05f, transform.position.z);
            zoneInstance = Instantiate(fireZonePrefab, groundPos, Quaternion.identity);
            zoneInstance.transform.localScale = fireZoneScale;

            FireZoneDOT dot = zoneInstance.GetComponentInChildren<FireZoneDOT>();
            if (dot != null)
            {
                dot.SetActiveDamage(false); 
                //dot.ClearInside();       
            }
        }

        float t = 0f;
        while (t < airTimeMax)
        {
            t += Time.deltaTime;

            if (zoneInstance != null)
            {
                zoneInstance.transform.position = new Vector3(transform.position.x, 0.05f, transform.position.z);
            }

            yield return null;
        }

        Vector3 groundTarget = new Vector3(transform.position.x, start.y, transform.position.z);
        while (transform.position.y > start.y + 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, groundTarget, diveSpeed * Time.deltaTime);

            if (zoneInstance != null)
            {
                zoneInstance.transform.position = new Vector3(transform.position.x, 0.05f, transform.position.z);
            }

            yield return null;
        }

        if (ctx != null && ctx.HitBox != null)
        {
            HitBox box = ctx.HitBox;

            box.OnHit = (Collider targetCol) =>
            {
                Enemy e = targetCol.GetComponent<Enemy>();
                if (e != null)
                    e.DealDamage(Mathf.RoundToInt(landingDamage));
            };

            if (ctx.hitBoxManager != null)
            {
                ctx.hitBoxManager.SetHitBox(box);
                HitBoxManager.duration = landingHitboxDuration;
            }

            box.EnableFrame(landingHitboxDuration);
        }

        if (zoneInstance != null)
        {
            FireZoneDOT dot = zoneInstance.GetComponentInChildren<FireZoneDOT>();
            if (dot != null)
            {
                dot.SetActiveDamage(true);
                dot.RefreshInside();
            }

            Destroy(zoneInstance, fireZoneLifetime);
            zoneInstance = null;
        }

        inProgress = false;
    }
}
