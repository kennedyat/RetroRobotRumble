using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteRanged : Enemy
{
    #region Variables
    public enum AttackType { Light1 = 0, Light2, Heavy1, Heavy2, NONE }
    AttackType currentAttack;
    Queue<AttackType> attackQueue = new();

    [Header("Attacks")]
    [SerializeField, Tooltip("DO NOT CHANGE THE ORDER OR ANY REFERENCES HERE, YOU CAN MODIFY THE SCRIPTABLE OBJECTS BUT NOT THEIR ORDER HERE")]
    EliteRangedAttackData[] data = new EliteRangedAttackData[4];

    [Header("References")]
    [SerializeField, Tooltip("Projectile prefab used for L1 and H1")]
    GameObject projectilePrefab;
    [SerializeField, Tooltip("Where projectiles appear/are instantiated")]
    Transform firePoint;
    [SerializeField, Tooltip("Sphere reticle used for L2")]
    GameObject sphereReticle;
    [SerializeField, Tooltip("Bomb prefab used for L2")]
    GameObject L2_bomb;
    [SerializeField, Tooltip("Laser attached to this enemy for H2")]
    GameObject H2_laser;

    [Header("Dash Settings")]
    [SerializeField, Tooltip("If after attacking, the player is still in range of attack, how long we should wait before attacking again")]
    float attackWaitTime = 3f;
    [SerializeField, Tooltip("How close the player needs to be for this enemy to start retreating")]
    float retreatRange = 2f;
    [SerializeField, Tooltip("Threshold for tangential dashes")]
    float dashRange = 5f;
    [SerializeField, Tooltip("How far this enemy dashes")]
    float dashDistance = 5f;
    [SerializeField, Tooltip("How long it takes to complete a dash")]
    float dashDuration = 0.2f;

    [Header("Debug")]
    [SerializeField, Tooltip("Use this to force what the elite enemies will always use")]
    AttackType forceAttack = AttackType.NONE;
    #endregion

    protected override void Start()
    {
        base.Start();

        // randomly select attack
        // from what i see its just pick 2 random, no repeat
        List<AttackType> list = new()
        {
            AttackType.Light1,
            AttackType.Light2,
            AttackType.Heavy1,
            AttackType.Heavy2
        };
        attackQueue.Enqueue(list[Random.Range(0, list.Count)]);
        list.Remove(attackQueue.Peek());
        attackQueue.Enqueue(list[Random.Range(0, list.Count)]);

        logicCoroutine = StartCoroutine(AttackLogic());
    }

    protected void Update()
    {
        if (currentState == EnemyState.Stunned || currentState == EnemyState.Death)
        {
            // already disabled in death and stunned functions
            // but lets just be sure
            H2_laser.SetActive(false);
        }
    }
    #region Attack Logic
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
        while (currentState != EnemyState.Stunned)
        {
            // get the next attack
            if (forceAttack == AttackType.NONE)
            {
                currentAttack = attackQueue.Dequeue();
                attackQueue.Enqueue(currentAttack);
            }
            else
            {
                currentAttack = forceAttack;
            }

            // set the range, used in LOS and WithinDistance functions
            attackRange = data[(int)currentAttack].attackRange;

            // walk to the player until we have line of sight and within range
            currentState = EnemyState.Chasing;
            while (!LineOfSight() || !WithinDistance()) // DEMORGANS LAW!!!
            {
                navMeshAgent.SetDestination(player.position);

                yield return null;
            }
            FacePlayer();

            // stop navigation
            navMeshAgent.ResetPath();

            // execute the top attack in the queue ONE TIME, or the forced attack
            currentState = EnemyState.Attacking;
            yield return EnumToAttack(currentAttack);

            // decide where the player is and dash appropriately
            currentState = GetState();
            yield return DetermineDash(currentState);

            // repeat (implicitly)
        }
    }

    IEnumerator EnumToAttack(AttackType t)
    {
        EliteRangedAttackData data = this.data[(int)t];
        switch (t)
        {
            case AttackType.Light1:
                yield return Light1((EliteRanged_L1)data);
                break;

            case AttackType.Light2:
                yield return Light2((EliteRanged_L2)data);
                break;

            case AttackType.Heavy1:
                yield return Heavy1((EliteRanged_H1)data);
                break;

            case AttackType.Heavy2:
                yield return Heavy2((EliteRanged_H2)data);
                break;
        }

        yield break;
    }

    EnemyState GetState()
    {
        float distToPlayer = DistanceToPlayer();

        // outside of attack range
        if (distToPlayer > attackRange)
        {
            return EnemyState.Chasing;
        }
        // within attack range already
        else if (dashRange < distToPlayer && distToPlayer <= attackRange)
        {
            return EnemyState.Attacking;
        }
        // within tangential dash distance
        else if (retreatRange < distToPlayer && distToPlayer <= dashRange)
        {
            return EnemyState.DashingTangent;
        }
        // panic range
        else // implicit distToPlayer <= retreatRange
        {
            return EnemyState.CloseEnough;
        }
    }
    #endregion

    #region Enemy Functions
    protected override void DeathState()
    {
        base.DeathState();
    }

    public override void InflictStun(float time, bool interruptAttacks = true)
    {
        base.InflictStun(time, interruptAttacks);

        // also disable the h2 laser
        H2_laser.SetActive(false);
    }

    protected override bool LineOfSight(bool requireBoth = true)
    {
        // the only override is to use the full width of the collider instead of half
        // because some of these projectiles can be very thicc
        Vector3 target = player.transform.position + Vector3.up * raycastVerticalOffset;
        Vector3 origin = transform.position + Vector3.up * raycastVerticalOffset;

        // direction TO THE PLAYER
        Vector3 baseDir = (target - origin).normalized;

        // 2 raycasts because a singular central raycast causes weird things when turning
        // local left/right offsets, placed according to the collider's width
        Vector3 rightOffset = box.bounds.extents.x * Vector3.Cross(Vector3.up, baseDir);

        Vector3 left = origin - rightOffset;
        Vector3 right = origin + rightOffset;

        bool leftClear = !Physics.Raycast(left, baseDir, out RaycastHit hit, attackRange, levelLayer);
        bool rightClear = !Physics.Raycast(right, baseDir, out hit, attackRange, levelLayer);

        Debug.DrawRay(left, baseDir * attackRange, Color.red);
        Debug.DrawRay(right, baseDir * attackRange, Color.green);
        if (requireBoth)
            return leftClear && rightClear;
        else
            return leftClear || rightClear;
    }
    #endregion

    #region Dashing
    IEnumerator DetermineDash(EnemyState dashType)
    {
        // vector straight to the player
        Vector3 toPlayer = (player.position - transform.position).normalized;
        Vector3 dashTarget = Vector3.zero; // dummy assignment

        switch (dashType)
        {
            case EnemyState.Chasing:
                // dash straight to the player
                dashTarget = transform.position + toPlayer * dashDistance;
                break;

            case EnemyState.DashingTangent:
                // dash tangent to the player
                // pick a random direction
                Vector3 tangent = Vector3.Cross(toPlayer, Vector3.up).normalized;
                if (Random.value < 0.5f)
                    tangent = -tangent;
                dashTarget = transform.position + tangent * dashDistance;
                break;

            case EnemyState.Attacking:
                // wait 3 seconds and dont dash
                yield return new WaitForSeconds(attackWaitTime);
                yield break;

            case EnemyState.CloseEnough:
                // dash away from the player
                dashTarget = transform.position - toPlayer * dashDistance;
                break;
        }

        // execute this dash in a separate coroutine
        yield return DashSequence(dashTarget);
    }

    IEnumerator DashSequence(Vector3 target)
    {
        // pre dash configuration
        rb.velocity = Vector3.zero;
        rb.drag = 0;
        navMeshAgent.ResetPath();

        // set the velocity
        Vector3 dir = (target - rb.position).normalized;
        rb.velocity = dir * (dashDistance / dashDuration);

        yield return new WaitForSeconds(dashDuration);

        rb.velocity = Vector3.zero;
        rb.drag = 10;
    }
    #endregion

    #region Attacks
    IEnumerator Light1(EliteRanged_L1 data)
    {
        // 3 quick shots towards the player
        for (int i = 0; i < data.projectileCount; i++)
        {
            // shoot a shot and wait a small duration
            GameObject reference = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

            // also rotate it a small random amount
            reference.transform.rotation *= Quaternion.Euler(
                0, Random.Range(-data.randomProjectileRotation, data.randomProjectileRotation), 0);

            reference.GetComponent<ER_BasicProj>().Init(data.projectileSpeed, data.damage,
                data.projectileLifetime, data.projectileScale, playerLayer, levelLayer);

            yield return new WaitForSeconds(data.projectileDelay);
        }
    }

    IEnumerator Light2(EliteRanged_L2 data)
    {
        // ziggs ultimate
        // get the player's position
        Vector3 playerPos = player.position;

        // summon a bomb
        GameObject bomb = Instantiate(L2_bomb, firePoint.position, firePoint.rotation);
        bomb.GetComponent<ER_BombProj>().Init(data.damage, data.bombMaxHeight, data.duration,
            data.bombSpinSpeed, data.projectileScale, data.explosionRadius, playerLayer, levelLayer, playerPos);

        // and the sphere reticle
        GameObject sr = Instantiate(sphereReticle, playerPos, Quaternion.identity);
        sr.GetComponent<SphereReticle>().Init(data.duration, data.explosionRadius);

        // that is really it, just pause execution here until the bomb is gone
        yield return new WaitForSeconds(data.duration);
    }

    IEnumerator Heavy1(EliteRanged_H1 data)
    {
        // slow moving projectile
        // 1/2: track the player while waiting
        float t = 0;
        while (t < data.channelTime)
        {
            if (t < data.channelTime - data.trackingLetGo)
            {
                FacePlayer();
            }

            t += Time.deltaTime;
            yield return null;
        }

        // 2/2: fire a very slow projectile
        GameObject reference = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        reference.GetComponent<ER_BasicProj>().Init(data.projectileSpeed, data.damage, data.projectileLifetime, data.projectileScale, playerLayer, levelLayer);
    }

    IEnumerator Heavy2(EliteRanged_H2 data)
    {
        // 3 second tracking laser
        // track the player while looking at them
        float t = 0;
        while (t < data.channelTime)
        {
            if (t < data.channelTime - data.trackingLetGo)
            {
                FacePlayer();
            }

            t += Time.deltaTime;
            yield return null;
        }

        // enable the laser over 3 seconds
        H2_laser.SetActive(true);
        H2_laser.GetComponent<ER_LaserProj>().Init(data.damage, data.tickRate, playerLayer);
        t = 0;
        while (t < data.duration)
        {
            // rotate towards the player at a certain speed (copied from FB code)
            Vector3 toPlayer = SetY(player.position - transform.position, 0);
            if (toPlayer.sqrMagnitude >= 0.001f)
            {
                // rotatetowards doesnt overshoot, so no need for fancy clamp functions or whatever
                Quaternion playerRotation = Quaternion.LookRotation(toPlayer);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, playerRotation, data.rotationSpeed * Time.deltaTime);
            }

            // raycast and find the length of the laser
            // use firepoint position + left and right based on the scale
            // mostly copied from baseline LOS function
            Vector3 origin = firePoint.position;

            // direction forward (now that we are facing the player or turning towards them)
            Vector3 baseDir = transform.forward;

            // 2 raycasts according to the specified width
            Vector3 rightOffset = data.laserWidth / 2f * Vector3.Cross(Vector3.up, baseDir);

            Vector3 left = origin - rightOffset;
            Vector3 right = origin + rightOffset;

            float distLeft = data.laserMaxLength;
            float distRight = data.laserMaxLength;
            LayerMask mask = LayerMask.GetMask("Level");
            if (Physics.Raycast(left, baseDir, out RaycastHit hitLeft, data.laserMaxLength, mask))
            {
                distLeft = hitLeft.distance;
            }

            if (Physics.Raycast(right, baseDir, out RaycastHit hitRight, data.laserMaxLength, mask))
            {
                distRight = hitRight.distance;
            }

            float laserLength = Mathf.Min(distLeft, distRight);


            Debug.DrawRay(left, baseDir * distLeft, Color.red);
            Debug.DrawRay(right, baseDir * distRight, Color.green);

            // set its length appropriately
            float scaleFactor = transform.localScale.x;
            H2_laser.transform.localScale = new Vector3(data.laserWidth / scaleFactor, 1.0f, laserLength / scaleFactor);
            H2_laser.transform.localPosition = new Vector3(0, firePoint.transform.position.y, laserLength / 2f);

            t += Time.deltaTime;
            yield return null;
        }
        H2_laser.SetActive(false);
    }
    #endregion
}
