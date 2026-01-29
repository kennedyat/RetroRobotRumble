using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteRanged : Enemy
{
    #region Variables
    public enum EliteRangedState { Chasing = 0, Chasing_TangentialDash, Shooting, Retreating, Death }
    enum AttackType { Light1 = 0, Light2, Heavy1, Heavy2 }

    Queue<AttackType> attackQueue = new();

    [Header("References")]
    [SerializeField, Tooltip("The projectile to be shot")] 
    GameObject projectilePrefab;
    [SerializeField, Tooltip("Where projectiles appear/are instantiated")] 
    Transform firePoint;

    [Header("Movement Settings")]
    [SerializeField, Tooltip("How close the player needs to be for this enemy to start retreating")]
    float retreatRange = 2f;
    [SerializeField, Tooltip("I (Kevin) do not know what this does, ask Alex!")]
    float rotationSpeed = 8f;

    [Header("Attack Settings")]
    [SerializeField, Tooltip("The cooldown in seconds between attacks")] 
    float fireInterval = 1.5f;
    [SerializeField, Tooltip("The speed of the projectile fired")]
    float projectileSpeed = 20f;
    [SerializeField, Tooltip("How long the projectile lasts")]
    float projectileLifetime = 5f;

    [Header("Dash Settings")]
    [SerializeField, Tooltip("How far this enemy dashes")] 
    float dashRange = 5f;
    [SerializeField, Tooltip("How far this enemy dashes")] 
    float dashDistance = 5f;
    [SerializeField, Tooltip("How long it takes to complete a dash")] 
    float dashDuration = 0.2f;
    [SerializeField, Tooltip("Time between dashes. Elite enemies dash 'off cooldown'")] 
    float dashCooldown = 5f;

    [Header("Debug")]
    [SerializeField] EliteRangedState currentState = EliteRangedState.Chasing;
    [SerializeField] bool justDashed = false;

    private bool isDashing = false;
    #endregion

    protected override void Start()
    {
        base.Start();
        
        // temporary assert statements
        Debug.Assert(attackRange > dashRange, "Error: dash range must be strictly less than attack range");
        Debug.Assert(attackRange > retreatRange, "Error: retreat range must be strictly less than attack range");
        Debug.Assert(dashRange > retreatRange, "Error: retreat range must be strictly less than dash range");

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
    #region Attack Logic
    IEnumerator AttackSequence()
    {
        while (true)
        {
            // walk to the player until we have line of sight and within range
            while (!LineOfSight() || !WithinDistance())
            {
                navMeshAgent.SetDestination(player.position);

                yield return new WaitForEndOfFrame();
            }

            // execute the top attack in the queue ONE TIME
            AttackType top = attackQueue.Dequeue();
            yield return StartCoroutine(EnumToAttack(top));
            attackQueue.Enqueue(top);

            // decide where the player is and dash appropriately
            EliteRangedState state = GetState();
            yield return StartCoroutine(DetermineDash(state));

            // repeat (implicitly)
        }
    }

    IEnumerator EnumToAttack(AttackType t)
    {
        switch (t)
        {
            case AttackType.Light1:
                yield return StartCoroutine(Light1());
                break;

            case AttackType.Light2:
                yield return StartCoroutine(Light2());
                break;
            case AttackType.Heavy1:
                yield return StartCoroutine(Heavy1());
                break;
            case AttackType.Heavy2:
                yield return StartCoroutine(Heavy2());
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
        // within tangential dash distance
        else if (dashDistance <= distToPlayer && distToPlayer <= attackRange) 
        {
            return EliteRangedState.Chasing_TangentialDash;
        }
        // panic range
        else if (retreatRange <= distToPlayer && distToPlayer < dashDistance)
        {
            return EliteRangedState.Retreating;
        }

        // it should NEVER get here
        return EliteRangedState.Chasing;
    }
    #endregion
    
    #region Enemy Functions
    protected override void DeathState()
    {
        base.DeathState();
        currentState = EliteRangedState.Death;
        StopAllCoroutines();
    }

    public override int DealDamage(int damageToDeal)
    {
        // if this enemy is dashing, it is invulnerable and we cannot deal damage
        if (isDashing) damageToDeal = 0;

        // set to zero to still show effects
        return base.DealDamage(damageToDeal);
    }
    #endregion

    #region Dashing
    IEnumerator DetermineDash(EliteRangedState dashType)
    {
        // vector straight to the player
        Vector3 toPlayer = (player.position - transform.position).normalized;
        Vector3 dashTarget = Vector3.zero;

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
    IEnumerator Light1()
    {
        // 3 quick shots towards the player
        yield return null;
    }

    IEnumerator Light2()
    {
        // a bomb that explodes in a small circle
        yield return null;
    }

    IEnumerator Heavy1()
    {
        // slow moving projectile
        yield return null;
    }

    IEnumerator Heavy2()
    {
        // 3 second tracking laser
        yield return null;
    }
    #endregion
}
