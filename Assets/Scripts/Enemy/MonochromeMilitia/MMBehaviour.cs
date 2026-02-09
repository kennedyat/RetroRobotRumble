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

        if (allMilitia == null)
            allMilitia = new List<MMBehaviour>();
        allMilitia.Add(this);

        // NOTE: this modifies min distance AND obstacle avoidance (how close it gets to walls)
        navMeshAgent.radius = minDistanceBetweenUnits;
    }

    protected void FixedUpdate()
    {
        if (currentState == MMState.Death)
            return;

        // NOTE: put LOS first to see the raycasts in editor (short circuiting)
        if (LineOfSight() && WithinDistance())
        {
            navMeshAgent.ResetPath();
            currentState = MMState.Shooting;
            rb.velocity = Vector3.zero;

            if (canShoot)
            {
                attackCoroutine = StartCoroutine(ShootRoutine());
            }
        }
        else
        {
            navMeshAgent.SetDestination(player.position);
            currentState = MMState.Chasing;
        }
    }

    protected override void DeathState()
    {
        base.DeathState();
        currentState = MMState.Death;

        // also remove this gameobject
        allMilitia.Remove(this);
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
        projScript.Init(direction, projectileSpeed, projectileLifetime, attackDamage, playerLayer, levelLayer);

        yield return new WaitForSeconds(attackCooldown);
        canShoot = true;
    }
}
