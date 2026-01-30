using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteRanged : Enemy
{
    #region Variables
    public enum EliteRangedState { Chasing = 0, Chasing_TangentialDash, Attacking, Retreating, Death }
    public enum AttackType { Light1 = 0, Light2, Heavy1, Heavy2, NONE }
    Queue<AttackType> attackQueue = new();

    [Header("References")]
    [SerializeField, Tooltip("Projectile prefab used for L1 and H1")]
    GameObject projectilePrefab;
    [SerializeField, Tooltip("Where projectiles appear/are instantiated")] 
    Transform firePoint;

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

    [Header("Attacks")]
    [SerializeField, Tooltip("DO NOT CHANGE THE ORDER OR ANY REFERENCES HERE, YOU CAN MODIFY THE SCRIPTABLE OBJECTS BUT NOT THEIR ORDER HERE")]
    EliteRangedAttackData[] data = new EliteRangedAttackData[4];

    [Header("Debug")]
    [SerializeField] EliteRangedState currentState = EliteRangedState.Chasing;
    [SerializeField, Tooltip("Use this to force what the elite enemies will always use")]
    AttackType forceAttack = AttackType.NONE;
    private bool isDashing = false;
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

        StartCoroutine(AttackSequence());
    }
    protected void Update()
    {
        Terminate();
    }

    #region Attack Logic
    IEnumerator AttackSequence()
    {
        while (true)
        {
            // get the next attack
            AttackType nextAttack;
            if (forceAttack == AttackType.NONE)
            {
                nextAttack = attackQueue.Dequeue();
                attackQueue.Enqueue(nextAttack);
            }
            else
            {
                nextAttack = forceAttack;
            }

            // set the range, used in LOS and WithinDistance functions
            attackRange = data[(int)nextAttack].attackRange;

            // walk to the player until we have line of sight and within range
            currentState = EliteRangedState.Chasing;
            while (!LineOfSight() || !WithinDistance()) // DEMORGANS LAW!!!
            {
                navMeshAgent.SetDestination(player.position);

                yield return new WaitForEndOfFrame();
            }
            transform.LookAt(SetY(player.position, transform.position.y));

            // stop navigation
            navMeshAgent.ResetPath();

            // execute the top attack in the queue ONE TIME, or the forced attack
            currentState = EliteRangedState.Attacking;
            yield return StartCoroutine(EnumToAttack(nextAttack));

            // decide where the player is and dash appropriately
            currentState = GetState();
            yield return StartCoroutine(DetermineDash(currentState));

            // repeat (implicitly)
        }
    }

    IEnumerator EnumToAttack(AttackType t)
    {
        EliteRangedAttackData data = this.data[(int)t];
        switch (t)
        {
            case AttackType.Light1:
                yield return StartCoroutine(Light1((EliteRanged_L1)data));
                break;

            case AttackType.Light2:
                yield return StartCoroutine(Light2((EliteRanged_L2)data));
                break;

            case AttackType.Heavy1:
                yield return StartCoroutine(Heavy1((EliteRanged_H1)data));
                break;

            case AttackType.Heavy2:
                yield return StartCoroutine(Heavy2((EliteRanged_H2)data));
                break;
        }
    }
    
    EliteRangedState GetState()
    {
        float distToPlayer = Vector3.Distance(SetY(player.position, 0), SetY(transform.position, 0));

        // outside of attack range
        if (distToPlayer > attackRange) 
        {
            return EliteRangedState.Chasing;
        }
        // within attack range already
        else if (dashDistance < distToPlayer && distToPlayer <= attackRange)
        {
            return EliteRangedState.Attacking;
        }
        // within tangential dash distance
        else if (retreatRange < distToPlayer && distToPlayer <= dashDistance) 
        {
            return EliteRangedState.Chasing_TangentialDash;
        }
        // panic range
        else // implicit distToPlayer <= retreatRange
        {
            return EliteRangedState.Retreating;
        }
    }
    #endregion

    #region Enemy Functions
    protected override void DeathState()
    {
        base.DeathState();
        currentState = EliteRangedState.Death;
        
        // this thankfully stops all the other coroutines
        // because they all are called from this one
        StopCoroutine(AttackSequence());
    }
    #endregion

    #region Dashing
    IEnumerator DetermineDash(EliteRangedState dashType)
    {
        // vector straight to the player
        Vector3 toPlayer = (player.position - transform.position).normalized;
        Vector3 dashTarget = Vector3.zero; // dummy assignment

        switch (dashType)
        {
            case EliteRangedState.Chasing:
                // dash straight to the player
                dashTarget = transform.position + toPlayer * dashDistance;
                break;

            case EliteRangedState.Chasing_TangentialDash:
                // dash tangent to the player
                // pick a random direction
                Vector3 tangent = Vector3.Cross(toPlayer, Vector3.up).normalized;
                if (Random.value < 0.5f) tangent = -tangent;
                dashTarget = transform.position + tangent * dashDistance;
                break;

            case EliteRangedState.Attacking:
                // wait 3 seconds and dont dash
                yield return new WaitForSeconds(attackWaitTime);
                yield break;

            case EliteRangedState.Retreating:
                // dash away from the player
                dashTarget = transform.position - toPlayer * dashDistance;
                break;
        }

        // execute this dash in a separate coroutine
        yield return StartCoroutine(DashSequence(dashTarget));
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

        isDashing = true;
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;

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

            reference.GetComponent<REEProjectiles>().Init(data.projectileSpeed, data.damage, 
                data.projectileLifetime, data.projectileScale, playerLayer, levelLayer);
            
            yield return new WaitForSeconds(data.projectileDelay);
        }
    }

    IEnumerator Light2(EliteRanged_L2 data)
    {
        // a bomb that explodes in a small circle
        yield return null;
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
                transform.LookAt(SetY(player.position, transform.position.y));
            }

            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        // 2/2: fire a very slow projectile
        GameObject reference = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        reference.GetComponent<REEProjectiles>().Init(data.projectileSpeed, data.damage, data.projectileLifetime, data.projectileScale, playerLayer, levelLayer);
    }

    IEnumerator Heavy2(EliteRanged_H2 data)
    {
        // 3 second tracking laser
        yield return null;
    }
    #endregion
}
