using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikyStego : Enemy
{
    [Header("References")]
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] GameObject hazardPrefab;

    [Header("Attacking")]
    [SerializeField, Tooltip("The amount of projectiles fired per attack")]
    int attackCount = 6;
    [SerializeField, Tooltip("The radius of each attack, separate from the radius of hazards left behind")]
    float projectileRadius = 2f;
    [SerializeField, Tooltip("The radius that determines the maximmum distance from the projectile landing spot and the player")]
    float distanceRadius = 3f;
    [SerializeField, Tooltip("The amount of time projectiles spend in the air (ie. attack delay)")]
    float attackDuration = 1.5f;
    [SerializeField, Tooltip("The amount of time to wait after an attack has been completed")]
    float attackCooldown = 3f;

    [Header("Hazards")]
    [SerializeField, Tooltip("The damage that hazards do")]
    int hazardDamage = 5;
    [SerializeField, Tooltip("The radius of the hazard, separate from the radius of the projectile itself")]
    float hazardRadius = 3f;
    [SerializeField, Tooltip("The radius of the hazard left on death")]
    float deathHazardRadius = 4f;
    [SerializeField, Tooltip("The maximum duratioon for a hazard. Note that they will be destroyed on the next attack")]
    float maxHazardDuration = 5f;

    // internal variables
    List<GameObject> hazards = new();

    protected override void Start()
    {
        base.Start();

        logicCoroutine = StartCoroutine(AttackLogic());
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
            while (!LineOfSight() || !WithinDistance())
            {
                navMeshAgent.SetDestination(player.position);
                yield return null;
            }

            // prepare to shoot
            currentState = EnemyState.Attacking;
            navMeshAgent.ResetPath();
            FacePlayer();

            // first delete all the remaining hazards
            for (int i = hazards.Count - 1; i >= 0; i--)
            {
                // copy, remove, destroy
                GameObject temp = hazards[i];
                hazards.RemoveAt(i);
                Destroy(temp);
            }

            // calculate the positions of the 6 projectiles

            // fire them (unclear if its all at the same time or separate)

            // wait for them to explode

            // store all the hazards inside the list

            // wait again
            yield return new WaitForSeconds(attackCooldown);
        }
    }

    protected override void DeathState()
    {
        base.DeathState();

        // leave behind hazards where it dies
    }
}
