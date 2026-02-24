using System.Collections;
using UnityEngine;

public class FallingPillar : MonoBehaviour
{
    [Header("Damage / Hits")]
    [SerializeField] private int hitsToFall = 3;

    [Header("Fall Direction")]
    [Tooltip("Local-space direction the pillar should fall toward (e.g. Vector3.forward).")]
    [SerializeField] private Vector3 localFallDirection = Vector3.forward;

    [Header("Fall Motion")]
    [SerializeField] private float fallDuration = 0.6f;
    [SerializeField] private float fallAngleDegrees = 90f;

    [Header("Projectile Handling")]
    [SerializeField] private bool destroyProjectileOnHit = true;

    private int hitsRemaining;
    private bool hasFallen;

    private void Awake()
    {
        hitsRemaining = Mathf.Max(1, hitsToFall);

        if (localFallDirection.sqrMagnitude < 0.0001f)
            localFallDirection = Vector3.forward;

        localFallDirection.Normalize();

        Debug.Log($"[FallingPillar] Initialized on {gameObject.name} | HitsToFall = {hitsRemaining}");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[FallingPillar] Trigger entered by {other.name}");

        if (hasFallen)
        {
            Debug.Log("[FallingPillar] Already fallen — ignoring hit.");
            return;
        }

        if (!other.CompareTag("PlayerProjectile"))
        {
            Debug.Log($"[FallingPillar] Ignored object (wrong tag): {other.tag}");
            return;
        }

        hitsRemaining--;
        Debug.Log($"[FallingPillar] Hit by PlayerProjectile! Hits remaining: {hitsRemaining}");

        if (destroyProjectileOnHit)
        {
            Debug.Log($"[FallingPillar] Destroying projectile: {other.name}");
            Destroy(other.gameObject);
        }

        if (hitsRemaining <= 0)
        {
            Debug.Log("[FallingPillar] Hit threshold reached — starting fall.");
            StartCoroutine(FallRoutine());
        }
    }

    private IEnumerator FallRoutine()
    {
        hasFallen = true;

        Vector3 worldFallDir = transform.TransformDirection(localFallDirection).normalized;
        Vector3 axis = Vector3.Cross(Vector3.up, worldFallDir);

        if (axis.sqrMagnitude < 0.0001f)
        {
            axis = transform.right;
            Debug.LogWarning("[FallingPillar] Fall direction parallel to Up — using fallback axis.");
        }

        axis.Normalize();

        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.AngleAxis(fallAngleDegrees, axis) * startRot;

        Debug.Log($"[FallingPillar] Falling around axis {axis} over {fallDuration} seconds.");

        float t = 0f;
        while (t < fallDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(t / fallDuration);

            // Smoothstep
            alpha = alpha * alpha * (3f - 2f * alpha);

            transform.rotation = Quaternion.Slerp(startRot, endRot, alpha);
            yield return null;
        }

        transform.rotation = endRot;

        Debug.Log("[FallingPillar] Fall complete.");
    }
}
