using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EagleChassis : MonoBehaviour
{
    // ---------------------- Passive Orb Settings ----------------------
    [Header("Passive Orb Settings")]
    public GameObject passiveProjectilePrefab;  // prefab with EagleChassisPassiveProjectile
    public Transform firePoint;
    public float orbInterval = 2f;
    public int maxOrbs = 5;

    // ---------------------- Orbit Visuals ----------------------
    [Header("Orbiting")]
    public float orbitRadius = 2f;
    public float orbitHeight = 1f;
    public float orbitSpeed = 90f;

    // ---------------------- Passive Auto-Fire ----------------------
    [Header("Passive Firing")]
    public float detectionRadius = 12f;
    public float fireCooldown = 0.25f;

    // ---------------------- Ultimate ----------------------
    [Header("Ultimate (Burst)")]
    public GameObject ultimateProjectilePrefab;
    public float ultimateSpeed = 30f;
    public float ultimateCooldown = 8f;
    public int ultimateShots = 3;
    public float ultimateShotInterval = 0.15f;
    public Transform[] extraMuzzles;
    public float ultimateSpawnForwardOffset = 0.4f;

    // ---------------------- Input (matches arm script style) ----------------------
    public PlayerInput.PlayerActions input_map;
    private PlayerInput _actions;
    private InputAction ultimateInput;

    // ---------------------- Runtime State ----------------------
    private readonly List<GameObject> _orbs = new List<GameObject>();
    private float _orbTimer;
    private float _fireTimer;
    private float _ultimateCooldownTimer;
    private bool _ultimateBurstRunning;

    [Header("Audio")]
    public AK.Wwise.Event Eagle_Passive_SFX;      
    public AK.Wwise.Event Eagle_Ultimate_SFX;     

    private void Start()
    {
        // Input wrapper like in your Locomotive_Revised
        _actions = new PlayerInput();
        input_map = _actions.Player;
        ultimateInput = input_map.Ultimate; // <-- Action must exist in your InputActions asset

        ultimateInput.performed += OnUltimatePerformed;
        input_map.Enable();
    }

    private void OnDestroy()
    {
        if (ultimateInput != null)
            ultimateInput.performed -= OnUltimatePerformed;
        _actions?.Dispose();
    }

    private void Update()
    {
        // Passive orb generation
        _orbTimer += Time.deltaTime;
        if (_orbTimer >= orbInterval)
        {
            _orbTimer = 0f;
            if (_orbs.Count < maxOrbs) SpawnOrb();
        }

        // Passive orb auto-fire
        _fireTimer += Time.deltaTime;
        if (_orbs.Count > 0 && _fireTimer >= fireCooldown)
        {
            Transform target = FindNearestEnemy();
            if (target != null)
            {
                FireOneOrbAt(target);
                _fireTimer = 0f;
            }
        }

        // Orbit visuals
        UpdateOrbPositions();

        // Ultimate cooldown
        if (_ultimateCooldownTimer > 0f)
            _ultimateCooldownTimer = Mathf.Max(0f, _ultimateCooldownTimer - Time.deltaTime);
    }

    // ---------------------- Input Callback ----------------------
    private void OnUltimatePerformed(InputAction.CallbackContext ctx)
    {
        TryCastUltimate();
    }

    public bool TryCastUltimate()
    {
        if (_ultimateBurstRunning || _ultimateCooldownTimer > 0f || !ultimateProjectilePrefab)
            return false;

        StartCoroutine(UltimateBurstRoutine());
        return true;
    }

    // ---------------------- Passive Orbs ----------------------
    private void SpawnOrb()
    {
        if (!passiveProjectilePrefab)
        {
            Debug.LogError("[EagleChassis] Missing passiveProjectilePrefab.");
            return;
        }

        GameObject orb = Instantiate(passiveProjectilePrefab, transform.position, Quaternion.identity);
        orb.transform.SetParent(null);

        var homing = orb.GetComponent<EagleChassisPassiveProjectile>();
        if (homing != null) homing.enabled = false;

        _orbs.Add(orb);
    }

    private void FireOneOrbAt(Transform target)
    {
        if (_orbs.Count == 0) return;

        GameObject orb = _orbs[0];
        _orbs.RemoveAt(0);

        var homing = orb.GetComponent<EagleChassisPassiveProjectile>();
        if (homing != null)
        {
            homing.enabled = true;
            homing.SetTarget(target);
        }

        if (firePoint)
            orb.transform.position = firePoint.position;

            //AUDIO EAGLE PASSIVE SFX
            Eagle_Passive_SFX.Post(gameObject);
    }

    private void UpdateOrbPositions()
    {
        if (_orbs.Count == 0) return;

        float angleStep = 360f / _orbs.Count;
        float time = Time.time * orbitSpeed;

        for (int i = 0; i < _orbs.Count; i++)
        {
            var orb = _orbs[i];
            if (!orb) continue;

            float angle = time + angleStep * i;
            float rad = angle * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * orbitRadius;
            Vector3 pos = transform.position + offset + Vector3.up * orbitHeight;

            orb.transform.position = pos;
            orb.transform.LookAt(transform);
        }
    }

    private Transform FindNearestEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);
        float bestDistSqr = Mathf.Infinity;
        Transform best = null;

        foreach (var c in hits)
        {
            if (c && c.CompareTag("Enemy"))
            {
                float d = (c.transform.position - transform.position).sqrMagnitude;
                if (d < bestDistSqr)
                {
                    bestDistSqr = d;
                    best = c.transform;
                }
            }
        }
        return best;
    }

    // ---------------------- Ultimate Burst ----------------------
    private IEnumerator UltimateBurstRoutine()
    {
        _ultimateBurstRunning = true;

        for (int i = 0; i < ultimateShots; i++)
        {
            FireOneUltimate(i);
            if (i < ultimateShots - 1 && ultimateShotInterval > 0f)
                yield return new WaitForSeconds(ultimateShotInterval);
        }

        _ultimateCooldownTimer = Mathf.Max(0f, ultimateCooldown);
        _ultimateBurstRunning = false;
    }

    private void FireOneUltimate(int shotIndex)
    {
        if (!ultimateProjectilePrefab) return;

        // Pick muzzle: round-robin if extra supplied
        Transform muzzle = firePoint ? firePoint : transform;
        if (extraMuzzles != null && extraMuzzles.Length > 0)
        {
            int idx = shotIndex % extraMuzzles.Length;
            if (extraMuzzles[idx]) muzzle = extraMuzzles[idx];
        }

        Vector3 spawnPos = muzzle.position + muzzle.forward * ultimateSpawnForwardOffset;
        Quaternion spawnRot = muzzle.rotation;

        GameObject proj = Instantiate(ultimateProjectilePrefab, spawnPos, spawnRot);
        proj.transform.SetParent(null);
        if (!proj.activeSelf) proj.SetActive(true);

        if (!proj.TryGetComponent<Rigidbody>(out var rb))
        {
            rb = proj.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.velocity = spawnRot * Vector3.forward * ultimateSpeed;

        // Cleanup if prefab doesn’t handle lifetime
        Destroy(proj, 6f);

        //AUDIO EAGLE ULTIMATE SFX
        Eagle_Ultimate_SFX.Post(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
