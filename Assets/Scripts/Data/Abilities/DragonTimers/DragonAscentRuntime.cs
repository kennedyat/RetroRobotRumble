using System.Collections;
using UnityEngine;
using Assets.Scripts.Combat.Robot;

public class DragonAscensionRuntime : MonoBehaviour
{
    [Header("Config")]
    public float chargeTime = 1f;
    public float airTimeMax = 3f;
    public float riseHeight = 8f;
    public float riseSpeed = 12f;
    public float diveSpeed = 30f;
    public float diveAcceleration = 20f;
    public float airControlSpeed = 8f;
    public float airborneCameraPitch = 18f;
    public float airborneCameraDistanceOffset = 0f;
    public Vector3 airborneCameraOffset = Vector3.zero;
    public float airborneCameraBlendSpeed = 6f;
    public float airborneCameraRestoreTime = 0.2f;

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
    private CombatRobot combatRobot;
    private RigidbodyConstraints originalConstraints;
    private bool cachedConstraints;
    private Vector3 cachedAirborneInput;
    private Transform cameraTarget;
    private Vector3 cameraTargetDefaultLocalPosition;
    private Quaternion cameraTargetDefaultLocalRotation;
    private bool cameraTargetCached;
    private bool airborneCameraActive;

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
        cachedAirborneInput = Vector3.zero;
        SetScriptedMovementLock(true);
        SetVerticalMotionEnabled(true);

        try
        {
            yield return new WaitForSeconds(chargeTime);
            airborneCameraActive = true;

            Rigidbody rb = ctx != null ? ctx.Rigidbody : null;
            Vector3 start = rb != null ? rb.position : transform.position;
            Vector3 apex = start + Vector3.up * riseHeight;

            while ((GetCurrentPosition() - apex).sqrMagnitude > 0.05f)
            {
                Vector3 currentPosition = GetCurrentPosition();
                Vector3 nextPosition = Vector3.MoveTowards(currentPosition, apex, riseSpeed * Time.fixedDeltaTime);
                nextPosition += GetAirControlDelta(Time.fixedDeltaTime);
                MoveTo(nextPosition);
                yield return new WaitForFixedUpdate();
            }

            if (fireZonePrefab != null && zoneInstance == null)
            {
                Vector3 currentPosition = GetCurrentPosition();
                Vector3 groundPos = new Vector3(currentPosition.x, 0.05f, currentPosition.z);
                zoneInstance = Instantiate(fireZonePrefab, groundPos, Quaternion.identity);
                zoneInstance.transform.localScale = fireZoneScale;

                FireZoneDOT dot = zoneInstance.GetComponentInChildren<FireZoneDOT>();
                if (dot != null)
                {
                    dot.SetActiveDamage(false);
                }
            }

            float t = 0f;
            while (t < airTimeMax)
            {
                t += Time.deltaTime;
                Vector3 currentPosition = GetCurrentPosition();
                Vector3 adjustedPosition = currentPosition + GetAirControlDelta(Time.deltaTime);
                MoveTo(adjustedPosition);

                if (zoneInstance != null)
                {
                    zoneInstance.transform.position = new Vector3(adjustedPosition.x, 0.05f, adjustedPosition.z);
                }

                yield return null;
            }

            Vector3 positionBeforeDive = GetCurrentPosition();
            Vector3 groundTarget = new Vector3(positionBeforeDive.x, start.y, positionBeforeDive.z);
            float currentDiveSpeed = diveSpeed;
            while (GetCurrentPosition().y > start.y + 0.05f)
            {
                currentDiveSpeed += diveAcceleration * Time.fixedDeltaTime;
                Vector3 currentPosition = GetCurrentPosition();
                Vector3 nextPosition = Vector3.MoveTowards(currentPosition, groundTarget, currentDiveSpeed * Time.fixedDeltaTime);
                nextPosition += GetAirControlDelta(Time.fixedDeltaTime);
                MoveTo(nextPosition);

                if (zoneInstance != null)
                {
                    zoneInstance.transform.position = new Vector3(nextPosition.x, 0.05f, nextPosition.z);
                }

                yield return new WaitForFixedUpdate();
            }

            if (ctx != null && ctx.HitBox != null)
            {
                HitBox box = ctx.HitBox;

                box.OnHit = (Collider targetCol) =>
                {
                    Enemy e = targetCol.GetComponent<Enemy>();
                    if (e != null)
                        e.DealDamage(Mathf.RoundToInt(landingDamage), true);
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

            airborneCameraActive = false;
            float cameraRestoreTimer = 0f;
            while (cameraRestoreTimer < airborneCameraRestoreTime)
            {
                cameraRestoreTimer += Time.deltaTime;
                yield return null;
            }
        }
        finally
        {
            airborneCameraActive = false;
            SetAirborneCameraActive(false, true, 0f);
            SetVerticalMotionEnabled(false);
            SetScriptedMovementLock(false);
            routine = null;
            inProgress = false;
        }
    }

    private void MoveTo(Vector3 targetPosition)
    {
        if (ctx != null && ctx.Rigidbody != null)
        {
            ctx.Rigidbody.MovePosition(targetPosition);
            return;
        }

        transform.position = targetPosition;
    }

    private Vector3 GetCurrentPosition()
    {
        if (ctx != null && ctx.Rigidbody != null)
        {
            return ctx.Rigidbody.position;
        }

        return transform.position;
    }

    private void SetScriptedMovementLock(bool isLocked)
    {
        if (combatRobot == null)
        {
            combatRobot = GetComponent<CombatRobot>();
        }

        if (combatRobot == null)
        {
            return;
        }

        combatRobot.ScriptedMovementLock = isLocked;
        combatRobot.worldspaceMoveInput = Vector3.zero;
        combatRobot.remainingDistance = 0f;
        combatRobot.dashCooldown = 0f;
        combatRobot.yawDelta = 0f;
        combatRobot.yawRotationalVelocity = 0f;
    }

    private void Update()
    {
        if (!inProgress)
        {
            return;
        }

        if (combatRobot == null)
        {
            combatRobot = GetComponent<CombatRobot>();
        }

        if (combatRobot != null)
        {
            cachedAirborneInput = combatRobot.worldspaceMoveInput;
            cachedAirborneInput.y = 0f;
            cachedAirborneInput = Vector3.ClampMagnitude(cachedAirborneInput, 1f);
        }

        SetAirborneCameraActive(airborneCameraActive, false, Time.deltaTime);
    }

    private Vector3 GetAirControlDelta(float deltaTime)
    {
        if (cachedAirborneInput.sqrMagnitude <= 0.0001f || airControlSpeed <= 0f)
        {
            return Vector3.zero;
        }

        return cachedAirborneInput * (airControlSpeed * deltaTime);
    }

    private void SetAirborneCameraActive(bool active, bool snap, float deltaTime)
    {
        if (!TryCacheCameraTarget())
        {
            return;
        }

        Vector3 targetLocalPosition = cameraTargetDefaultLocalPosition;
        Quaternion targetLocalRotation = cameraTargetDefaultLocalRotation;

        if (active)
        {
            Quaternion pitchRotation = Quaternion.AngleAxis(airborneCameraPitch, Vector3.right);
            Vector3 pitchedLocalPosition = pitchRotation * cameraTargetDefaultLocalPosition;
            Vector3 distanceDirection = pitchedLocalPosition.sqrMagnitude > 0.0001f
                ? pitchedLocalPosition.normalized
                : Vector3.forward;

            targetLocalPosition = pitchedLocalPosition
                + (distanceDirection * airborneCameraDistanceOffset)
                + airborneCameraOffset;
            targetLocalRotation = cameraTargetDefaultLocalRotation * Quaternion.Euler(airborneCameraPitch, 0f, 0f);
        }

        if (snap)
        {
            cameraTarget.localPosition = targetLocalPosition;
            cameraTarget.localRotation = targetLocalRotation;
            return;
        }

        float blend = 1f - Mathf.Exp(-airborneCameraBlendSpeed * deltaTime);
        cameraTarget.localPosition = Vector3.Lerp(cameraTarget.localPosition, targetLocalPosition, blend);
        cameraTarget.localRotation = Quaternion.Slerp(cameraTarget.localRotation, targetLocalRotation, blend);
    }

    private bool TryCacheCameraTarget()
    {
        if (cameraTargetCached)
        {
            return cameraTarget != null;
        }

        cameraTargetCached = true;
        cameraTarget = FindChildRecursive(transform, "Camera Target");
        if (cameraTarget == null)
        {
            return false;
        }

        cameraTargetDefaultLocalPosition = cameraTarget.localPosition;
        cameraTargetDefaultLocalRotation = cameraTarget.localRotation;
        return true;
    }

    private Transform FindChildRecursive(Transform root, string childName)
    {
        if (root == null)
        {
            return null;
        }

        if (root.name == childName)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindChildRecursive(root.GetChild(i), childName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private void SetVerticalMotionEnabled(bool enabled)
    {
        if (ctx == null || ctx.Rigidbody == null)
        {
            return;
        }

        if (!cachedConstraints)
        {
            originalConstraints = ctx.Rigidbody.constraints;
            cachedConstraints = true;
        }

        if (enabled)
        {
            ctx.Rigidbody.constraints = originalConstraints & ~RigidbodyConstraints.FreezePositionY;
            return;
        }

        ctx.Rigidbody.constraints = originalConstraints;
        cachedConstraints = false;
    }

    private void OnDisable()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }

        airborneCameraActive = false;
        SetAirborneCameraActive(false, true, 0f);
        SetVerticalMotionEnabled(false);
        SetScriptedMovementLock(false);
        inProgress = false;
    }
}
