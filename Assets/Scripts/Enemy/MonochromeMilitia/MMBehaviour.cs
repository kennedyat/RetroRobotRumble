using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Combat.Prototype;
using UnityEngine;

public class MMBehaviour : Enemy
{
    public enum MMState { Chasing = 0, Shooting, Death }

    [Header("References")]
    [SerializeField, Tooltip("The projectile to be shot")] 
    GameObject projectilePrefab;
    [SerializeField, Tooltip("Where projectiles appear/are instantiated")] 
    Transform firePoint;

    [Header("Group Behavior")]
    [SerializeField, Tooltip("MM will try to space themselves out according to this distance")] 
    float minDistanceBetweenUnits = 2f;

    [Header("Attack Settings")]
    [SerializeField, Tooltip("The cooldown in seconds between attacks")] 
    float attackCooldown = 1.2f;
    [SerializeField, Tooltip("Each bullet fires with a random projectile spread")]
    float projectileSpread = 1.5f;

    [Header("Projectile Settings")]
    [SerializeField, Tooltip("The speed of the projectiles")]
    float projectileSpeed = 25f;
    [SerializeField, Tooltip("How long in seconds a projectile can continue before it is destroyed")]
    float projectileLifetime = 3f;

    private bool canShoot = true;
    public static List<MMBehaviour> allMilitia;

    [Header("Debug")]
    [SerializeField] MMState currentState = MMState.Chasing;

    protected override void Start()
    {
        base.Start();
        if (allMilitia == null) allMilitia = new List<MMBehaviour>();
        allMilitia.Add(this);
    }

    protected void FixedUpdate()
    {
        if (Terminate()) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > attackRange)
        {
            currentState = MMState.Chasing;
            MoveTowardPlayer();
        }
        else
        {
            currentState = MMState.Shooting;
            rb.velocity = Vector3.zero;

            if (canShoot)
            {
                StartCoroutine(ShootRoutine());
            }
        }
    }

    protected override void DeathState()
    {
        currentState = MMState.Death;
        StopCoroutine(ShootRoutine());
        rb.constraints = RigidbodyConstraints.FreezeAll;

        // also remove this gameobject
        allMilitia.Remove(this);
    }

    void MoveTowardPlayer()
    {
        Vector3 toPlayer = (player.position - transform.position).normalized;
        Vector3 separation = Vector3.zero;

        foreach (var ally in allMilitia)
        {
            if (ally != this)
            {
                Vector3 toAlly = transform.position - ally.transform.position;
                float dist = toAlly.magnitude;
                if (dist < minDistanceBetweenUnits && dist > 0.001f)
                {
                    separation += toAlly.normalized * (minDistanceBetweenUnits - dist);
                }
            }
        }

        float separationStrength = 2.5f;
        Vector3 moveDir = (toPlayer + separation * separationStrength).normalized;
        rb.MovePosition(rb.position + moveSpeed * Time.fixedDeltaTime * moveDir);

        Vector3 flatDir = new Vector3(toPlayer.x, 0f, toPlayer.z);
        if (flatDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(flatDir, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * 5f));
        }
    }

    IEnumerator ShootRoutine()
    {
        canShoot = false;

        Vector3 targetPos = SetY(player.position, firePoint.position.y);
        targetPos += new Vector3(
            Random.Range(-projectileSpread, projectileSpread),
            0, 
            Random.Range(-projectileSpread, projectileSpread)
        );

        Vector3 direction = (targetPos - firePoint.position).normalized;
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(direction));

        MMProjectiles projScript = proj.GetComponent<MMProjectiles>();
        if (projScript != null)
        {
            projScript.Init(direction, projectileSpeed, projectileLifetime, playerLayer, levelLayer);
        }

        yield return new WaitForSeconds(attackCooldown);
        canShoot = true;
    }
}
