using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "ScriptableObjects/Components/Rapid Fire")]
public class RapidFireComponent : PartComponent
{
    [Header("Projectile")]
    public GameObject projectilePrefab;
    public float projectileRange = 100f;
    public string spawnPointName = "SpawnPoint";

    [Header("Fire Rate (CONSTANT)")]
    [Tooltip("Constant shots per second while holding.")]
    public float shotsPerSecond = 6f;

    [Header("Spread (grows with continuous fire)")]
    public float minSpread = 5f;
    public float maxSpread = 45f;

    [Tooltip("Seconds of holding fire to reach max spread.")]
    public float spreadGrowTime = 2.0f;

    [Tooltip("Seconds after releasing fire to return to min spread.")]
    public float spreadDecayTime = 0.75f;

    private float timeUntilNextShot;
    private float spread01;

    public override void Initialize(PartContext context)
    {
        timeUntilNextShot = 0f;
        spread01 = 0f;
    }

    public override void OnExecute(PartContext context)
    {

    }

    public override void OnUpdate(PartContext context, float deltaTime)
    {
        if (TigerSpecialRuntime.IsSpecialActive(context))
        {
            context.CustomData["IsFiring"] = false;
            context.CustomData["RapidFireRampup"] = 0f;
            context.CustomData["RapidFireShot"] = false;
            return;
        }

        InputAction inputAction = context.CustomData.ContainsKey("InputAction")
            ? context.CustomData["InputAction"] as InputAction
            : null;

        bool pressing = inputAction != null && inputAction.ReadValue<float>() > 0.5f;

        if (pressing)
        {
            float growDenom = Mathf.Max(0.0001f, spreadGrowTime);
            spread01 += deltaTime / growDenom;
        }
        else
        {
            float decayDenom = Mathf.Max(0.0001f, spreadDecayTime);
            spread01 -= deltaTime / decayDenom;
        }
        spread01 = Mathf.Clamp01(spread01);

        if (pressing)
        {
            context.partInstance.ChangeState(PartState.Active);

            timeUntilNextShot -= deltaTime;
            if (timeUntilNextShot <= 0f)
            {
                Shoot(context, spread01, projectilePrefab);
                context.CustomData["RapidFireShot"] = true;

                float sps = Mathf.Max(0.1f, shotsPerSecond);
                timeUntilNextShot = 1f / sps;
            }
            else
            {
                context.CustomData["RapidFireShot"] = false;
            }
        }
        else
        {
            timeUntilNextShot = Mathf.Max(0f, timeUntilNextShot - deltaTime);
            context.CustomData["RapidFireShot"] = false;
        }

        context.CustomData["RapidFireRampup"] = spread01;
        context.CustomData["IsFiring"] = pressing;
    }

    private void Shoot(PartContext context, float spreadRamp01, GameObject prefab)
    {
        if (context.Owner == null || prefab == null)
        {
            Debug.LogError("[RapidFire] Owner or projectile prefab is null!");
            return;
        }

        Transform spawnPoint = context.Owner.Find(spawnPointName);
        if (spawnPoint == null)
            spawnPoint = context.Owner;

        float spreadDegrees = Mathf.Lerp(minSpread, maxSpread, spreadRamp01);

        Quaternion randomRotation = Quaternion.AngleAxis(
            Random.Range(-spreadDegrees * 0.5f, spreadDegrees * 0.5f),
            Vector3.up
        );

        Vector3 direction = randomRotation * spawnPoint.forward;

        GameObject instance = GameObject.Instantiate(prefab);
        Projectile projectile = instance.GetComponent<Projectile>();

        if (projectile != null)
        {
            Ray shotRay = new Ray(spawnPoint.position, direction);
            projectile.FollowRay(shotRay, projectileRange);
        }
        else
        {
            instance.transform.position = spawnPoint.position;
            instance.transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
