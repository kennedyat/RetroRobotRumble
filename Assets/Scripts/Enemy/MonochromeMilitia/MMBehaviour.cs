using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Combat.Prototype;
using UnityEngine;

public class MMBehaviour : Enemy
{
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
    [SerializeField, Tooltip("Each bullet fires with a random projectile spread in degrees")]
    float projectileSpread = 1.5f;

    [Header("Projectile Settings")]
    [SerializeField, Tooltip("The speed of the projectiles")]
    float projectileSpeed = 25f;
    [SerializeField, Tooltip("How long in seconds a projectile can continue before it is destroyed")]
    float projectileLifetime = 3f;

    public static List<MMBehaviour> allMilitia;

    protected override void Start()
    {
        base.Start();

        if (allMilitia == null)
            allMilitia = new List<MMBehaviour>();
        allMilitia.Add(this);

        // NOTE: this modifies min distance AND obstacle avoidance (how close it gets to walls)
        navMeshAgent.radius = minDistanceBetweenUnits;

        logicCoroutine = StartCoroutine(AttackLogic());
    }

    protected override void DeathState()
    {
        base.DeathState();

        // also remove this gameobject
        allMilitia.Remove(this);
    }

    IEnumerator AttackLogic()
    {
        while (currentState != EnemyState.Death)
        {
            yield return new WaitWhile(() => currentState == EnemyState.Stunned);
            // theres nothing complicated that takes time here, so thats why it looks so simple
            yield return AttackSequence();
        }
    }

    IEnumerator AttackSequence()
    {
        while (currentState != EnemyState.Stunned && currentState != EnemyState.Death)
        {
            // get in range of the player
            currentState = EnemyState.Chasing;
            while (!LineOfSight() || !WithinDistance())
            {
                navMeshAgent.SetDestination(player.position);

                yield return null;
            }

            // prepare to shoot
            currentState = EnemyState.Attacking;
            navMeshAgent.ResetPath();
            FacePlayer();

            // direction and random rotation
            Vector3 direction = (SetY(player.position, firePoint.position.y) - firePoint.position).normalized;
            float random = Random.Range(-projectileSpread, projectileSpread);
            direction = Quaternion.Euler(0, random, 0) * direction;

            // shoot proj and initialize the values
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(direction));
            MMProjectiles projScript = proj.GetComponent<MMProjectiles>();
            projScript.Init(direction, projectileSpeed, projectileLifetime, attackDamage, playerLayer, levelLayer);

            // wait again
            yield return new WaitForSeconds(attackCooldown);
        }
    }
}
