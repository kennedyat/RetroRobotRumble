using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteRanged : Enemy
{
    public enum EliteRangedState { Chasing = 0, Chasing_TangentialDash, Shooting, Retreating, Death }
    enum AttackType { Light1 = 0, Light2, Heavy1, Heavy2 }

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
    float fireTimer = 0f;
    Queue<AttackType> attackSequence = new();

    protected override void Start()
    {
        base.Start();
        
        // temporary assert statements
        Debug.Assert(attackRange > dashRange, "Error: dash range must be strictly less than attack range");
        Debug.Assert(attackRange > retreatRange, "Error: retreat range must be strictly less than attack range");
        Debug.Assert(dashRange > retreatRange, "Error: retreat range must be strictly less than dash range");
    }

    protected void Update()
    {
        if (Terminate()) return;

        if (isDashing)
        {
            return;
        }
            
        // decide the state
        // in ATTACK range and clear line of sight
        float distance = Vector3.Distance(SetY(player.position, 0), SetY(transform.position, 0));

        // for debug purposes
        LineOfSight();

        // too far to attack
        if (distance > attackRange)
        {
            currentState = EliteRangedState.Chasing;
        }
        else
        {
            // in range to attack
            // but are we too close?
            if (distance <= retreatRange)
            {
                currentState = EliteRangedState.Retreating;
            }

            // dash range
            else if (retreatRange < distance && distance <= dashRange)
            {
                currentState = EliteRangedState.Chasing_TangentialDash;
            }

            // shooting range
            else if (dashRange < distance && distance <= attackRange)
            {
                // line of sight?
                if (LineOfSight())
                {
                    currentState = EliteRangedState.Shooting;
                }
                else
                {
                    currentState = EliteRangedState.Chasing;
                }
            }
        }

        if (currentState != EliteRangedState.Shooting)
        {
            fireTimer = 0;
        }

        // then decide what to do based on state
        switch (currentState)
        {
            case EliteRangedState.Chasing:
                // if we just attacked, perform one dash towards the player
                if (!justDashed)
                {
                    // dash towards the player
                    Dash(EliteRangedState.Chasing);
                    justDashed = true;
                }

                // set the destination
                navMeshAgent.SetDestination(player.position);
                break;
            
            case EliteRangedState.Chasing_TangentialDash:
                // make a dash tangent to the player if we just attacked
                if (!justDashed)
                {
                    Dash(EliteRangedState.Chasing_TangentialDash);
                    justDashed = true;
                }
                else
                {
                    // can't dash twice in a row, so regularly pathfind
                    navMeshAgent.SetDestination(player.position);
                }
                break;

            case EliteRangedState.Shooting:
                // fire a shot and remove navigation
                navMeshAgent.ResetPath();

                // for now will just fire one shot, later will upgrade to 4 abilities
                ProjectileTimer();
                //DecideNextAttack();
                break;

            case EliteRangedState.Retreating:
                // dash away from the player
                if (!justDashed)
                {
                    Dash(EliteRangedState.Retreating);
                    justDashed = true;
                }

                // if we're still in retreat range, try again
                if (Vector3.Distance(SetY(transform.position, 0), SetY(player.position, 0)) <= retreatRange)
                {
                    Dash(EliteRangedState.Retreating);
                }
                break;
        }
    }

    protected override void DeathState()
    {
        currentState = EliteRangedState.Death;
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    public override int DealDamage(int damageToDeal)
    {
        // if this enemy is dashing, it is invulnerable and we cannot deal damage
        if (isDashing) damageToDeal = 0;

        // set to zero to still show effects
        return base.DealDamage(damageToDeal);
    }

    void ProjectileTimer()
    {
        if (fireTimer <= 0)
        {
            FireProjectile();
            fireTimer = fireInterval;
            justDashed = false;
        }
        else
        {
            fireTimer -= Time.deltaTime;
        }
    }

    // this will probably be deleted due to new elite enemy design
    void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null || player == null)
            return;

        GameObject projObj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        REEProjectiles proj = projObj.GetComponent<REEProjectiles>();
            
        proj.Init(player, projectileSpeed, attackDamage, projectileLifetime, playerLayer, levelLayer);
    }

    void Dash(EliteRangedState dashType)
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
        StartCoroutine(DashSequence(dashTarget));
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

    void DecideNextAttack()
    {
        // move the front of the queue to the back and call the appropriate attack function
        AttackType next = attackSequence.Dequeue();

        switch (next)
        {
            case AttackType.Light1:
                Light1();
                break;
            
            case AttackType.Light2:
                Light2();
                break;

            case AttackType.Heavy1:
                Heavy1();
                break;

            case AttackType.Heavy2:
                Heavy2();
                break;
        }

        attackSequence.Enqueue(next);
    }

    void Light1()
    {
        
    }

    void Light2()
    {
        
    }

    void Heavy1()
    {
        
    }

    void Heavy2()
    {
        
    }
}
