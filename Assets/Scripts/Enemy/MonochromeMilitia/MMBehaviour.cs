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

    //private Animator MMAnimator;

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

    protected override void Start()
    {
        base.Start();

        //EnemyAnimator.SetTrigger("TrHop");

        //MMAnimator = GetComponent<Animator>();

        //MMAnimator.SetTrigger("TrHop");

        //enemyAnimator.SetTrigger("TrStandingStill");

        //enemyAnimator.SetTrigger("trShoot");

        // NOTE: this modifies min distance AND obstacle avoidance (how close it gets to walls)
        navMeshAgent.radius = minDistanceBetweenUnits;

        logicCoroutine = StartCoroutine(AttackLogic());
    }

    protected override void DeathState()
    {   
        base.DeathState();
        EnemyAnimator.SetTrigger("TrDestory");
    }

    IEnumerator AttackLogic()
    {
        while (currentState != EnemyState.Death)
        {
            yield return new WaitWhile(() => currentState == EnemyState.Stunned);
            attackCoroutine = StartCoroutine(AttackSequence());
            attackStarted = true;
            yield return new WaitWhile(() => attackStarted);
        }
    }

    IEnumerator AttackSequence()
    {
        while (currentState != EnemyState.Stunned && currentState != EnemyState.Death)
        {
            // get in range of the player
            currentState = EnemyState.Chasing;
            // I'll need to add a blend between the hop and shoot animation
            EnemyAnimator.SetTrigger("TrHop");

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
            EnemyAnimator.SetTrigger("TrShoot");
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(direction));
            MMProjectiles projScript = proj.GetComponent<MMProjectiles>();
            projScript.Init(direction, projectileSpeed, projectileLifetime, attackDamage, playerLayer, levelLayer);

            // wait again
            yield return new WaitForSeconds(attackCooldown);
        }
    }
}
