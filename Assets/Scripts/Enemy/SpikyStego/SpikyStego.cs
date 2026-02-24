using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rand = UnityEngine.Random;

public class SpikyStego : Enemy
{
    [Header("References")]
    [SerializeField] Transform firePoint;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] GameObject hazardPrefab;

    [Header("Attacking")]
    [SerializeField, Tooltip("The amount of projectiles fired per attack")]
    int attackCount = 6;
    [SerializeField, Tooltip("The amount of time to wait after each individual attack")]
    float shotDelay = .25f;
    [SerializeField, Tooltip("The scale of each attack, separate from the scale of hazards left behind")]
    float projectileScale = 2f;
    [SerializeField, Tooltip("The radius that determines the maximmum distance from the projectile landing spot and the player")]
    float distanceRadius = 3f;
    [SerializeField, Tooltip("The amount of time projectiles spend in the air (ie. attack delay)")]
    float attackDuration = 1.5f;
    [SerializeField, Tooltip("The maximum height that projectiles reach while traveling")]
    float projMaxHeight = 5f;
    [SerializeField, Tooltip("The amount of time to wait after an attack has been completed")]
    float attackCooldown = 3f;

    [Header("Hazards")]
    [SerializeField, Tooltip("The damage that hazards do when hitting the player")]
    int hazardDamage = 5;
    [SerializeField, Tooltip("The radius of the hazard, separate from the radius of the projectile itself")]
    float hazardRadius = 3f;
    [SerializeField, Tooltip("The radius of the hazard left on death")]
    float deathHazardRadius = 4f;
    [SerializeField, Tooltip("The radius of the hazard left on death")]
    int deathHazardDamage = 8;
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

            // snapshot the player pos
            Vector3 snapshotPlayerPos = player.position;

            // repeat 6 times (or whatever the variable is)
            for (int i = 0; i < attackCount; i++)
            {
                // pick random spot to fire (copied from fb)
                // use polar coordinates, so generate a random angle and random distance
                float rAngle = Rand.Range(0, 360f) * Mathf.Deg2Rad;
                float rDistance = Mathf.Sqrt(Rand.value) * distanceRadius;

                // convert polar to cartesian
                float xPos = rDistance * Mathf.Cos(rAngle) + snapshotPlayerPos.x;
                float zPos = rDistance * Mathf.Sin(rAngle) + snapshotPlayerPos.z;
                Vector3 projPos = new(xPos, 0, zPos);

                // fire
                GameObject reference = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
                reference.GetComponent<ST_Proj>().Init(attackDamage, projMaxHeight, attackDuration, projectileScale, playerLayer,
                    projPos, hazardDamage, 2 * hazardRadius, 2 * hazardRadius, maxHazardDuration, this);

                // reticle
                GameObject sr = Instantiate(sphereReticle, projPos, Quaternion.identity);
                sr.GetComponent<SphereReticle>().Init(attackDuration, projectileScale / 2);

                // wait
                yield return new WaitForSeconds(shotDelay);
            }

            // wait again
            yield return new WaitForSeconds(attackCooldown);
        }
    }

    protected override void DeathState()
    {
        base.DeathState();

        // leave behind a hazard where it dies
        Instantiate(hazardPrefab, transform.position, Quaternion.identity).GetComponent<ST_Hazard>().Init(
            deathHazardDamage, playerLayer, 2 * deathHazardRadius, 2 * deathHazardRadius, maxHazardDuration);
    }

    // called by projectiles
    public void AddToHazardList(GameObject g)
    {
        hazards.Add(g);
    }
}