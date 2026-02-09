using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteMelee : Enemy
{
    #region Variables
    public enum EliteMeleeState { Chasing = 0, Chasing_Dashing, Chasing_TangentialDash, CloseEnough, Attacking, Death }
    public enum AttackType { Light1 = 0, Light2, Heavy1, Heavy2, NONE }
    AttackType currentAttack;
    EliteMeleeState nextDash;
    Queue<AttackType> attackQueue = new();

    [Header("References")]
    [SerializeField, Tooltip("Hitbox used for Light 1 attack")]
    GameObject L1_hitbox;
    [SerializeField, Tooltip("Hitbox used for Light 2 attack")]
    GameObject L2_hitbox;
    [SerializeField, Tooltip("Hitbox used for Heavy 2 attack")]
    GameObject H2_hitbox;

    [Header("Dash Settings")]
    [SerializeField, Tooltip("If after attacking, the player is still in range of attack, how long we should wait before attacking again")]
    float attackWaitTime = 3f;
    [SerializeField, Tooltip("Close enough dash range")]
    float dashRange = 6f;
    [SerializeField, Tooltip("How far this enemy dashes")]
    float dashDistance = 4f;
    [SerializeField, Tooltip("How long it takes to complete a dash")]
    float dashDuration = 0.2f;
    [SerializeField, Tooltip("The cooldown of dashes")]
    float dashCooldown = 2.0f;

    [Header("Attack Datas")]
    [SerializeField, Tooltip("DO NOT CHANGE THE ORDER OR ANY REFERENCES HERE, YOU CAN MODIFY THE SCRIPTABLE OBJECTS BUT NOT THEIR ORDER HERE")]
    EliteMeleeAttackData[] data = new EliteMeleeAttackData[4];

    [Header("Debug")]
    [SerializeField] EliteMeleeState currentState;
    [SerializeField, Tooltip("Use this to force what the attack will be")]
    AttackType forceAttack = AttackType.NONE;
    bool H1_stunned = false;
    #endregion

    protected override void Start()
    {
        base.Start();

        // choose 2 random attacks
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

        // random first dash
        nextDash = Random.value < 0.5f ? EliteMeleeState.Chasing : EliteMeleeState.Chasing_TangentialDash;

        attackCoroutine = StartCoroutine(AttackSequence());
    }

    #region Attacking Logic
    IEnumerator AttackSequence()
    {
        while (true)
        {
            // get the attack range
            if (forceAttack == AttackType.NONE)
            {
                currentAttack = attackQueue.Dequeue();
                attackQueue.Enqueue(currentAttack);
            }
            else
            {
                currentAttack = forceAttack;
            }
            attackRange = EnumToAttackRange(currentAttack);

            // a bit different than elite ranged
            // wait until LOS and within distance OF DASH RANGE
            currentState = EliteMeleeState.Chasing;
            while (!LineOfSight() || !WithinDashDistance()) // DEMORGANS LAW!!!
            {
                navMeshAgent.SetDestination(player.position);

                yield return null;
            }

            // then start the dashing every 2 seconds, until we are within range of the player
            currentState = EliteMeleeState.Chasing_Dashing;
            float t = 0;
            while (!LineOfSight() || !WithinDistance())
            {
                if (t >= dashCooldown)
                {
                    // dash, reset cooldown, and switch dash type
                    StartCoroutine(DecideDash(nextDash));
                    nextDash = nextDash == EliteMeleeState.Chasing_TangentialDash ?
                        EliteMeleeState.Chasing : EliteMeleeState.Chasing_TangentialDash;
                    t = 0;
                }
                else
                {
                    navMeshAgent.SetDestination(player.position);
                    t += Time.deltaTime;
                }

                yield return null;
            }
            FacePlayer();

            // stop navigation
            navMeshAgent.ResetPath();

            // perform the attack
            currentState = EliteMeleeState.Attacking;
            yield return EnumToAttack(currentAttack);

            // dash depending on the distance to the player
            currentState = GetState();
            yield return DecideDash(currentState);

            // repeat (implicitly)
        }
    }

    bool WithinDashDistance()
    {
        return Vector3.Distance(SetY(player.transform.position, 0), SetY(transform.position, 0)) <= dashRange;
    }

    float EnumToAttackRange(AttackType t)
    {
        return data[(int)t].attackRange;
    }

    EliteMeleeState GetState()
    {
        float distToPlayer = Vector3.Distance(SetY(player.position, 0), SetY(transform.position, 0));

        // too far
        if (distToPlayer > dashRange)
        {
            return EliteMeleeState.Chasing;
        }
        // close enough range
        else if (attackRange < distToPlayer && distToPlayer <= dashRange)
        {
            return EliteMeleeState.CloseEnough;
        }
        // well within range
        else // if distToPlayer <= attackRange
        {
            return EliteMeleeState.Attacking;
        }
    }
    #endregion

    #region Attacks
    IEnumerator EnumToAttack(AttackType t)
    {
        EliteMeleeAttackData data = this.data[(int)t];

        switch (t)
        {
            case AttackType.Light1:
                yield return StartCoroutine(Light1((L1_EliteMelee)data));
                break;
            case AttackType.Light2:
                yield return StartCoroutine(Light2((L2_EliteMelee)data));
                break;
            case AttackType.Heavy1:
                yield return StartCoroutine(Heavy1((H1_EliteMelee)data));
                break;
            case AttackType.Heavy2:
                yield return StartCoroutine(Heavy2((H2_EliteMelee)data));
                break;
        }
    }
    IEnumerator Light1(L1_EliteMelee data)
    {
        // pantheon tap q
        // wind up
        yield return new WaitForSeconds(data.channelTime);

        // make the box appear
        L1_hitbox.SetActive(true);
        L1_hitbox.GetComponent<EM_L1Hitbox>().Init(data.damage, data.width, data.length, playerLayer, true);

        // wait some time
        yield return new WaitForSeconds(data.duration);

        // deactivate hitbox
        L1_hitbox.SetActive(false);

        // refractory period
        yield return new WaitForSeconds(data.recoveryTime);
    }

    IEnumerator Light2(L2_EliteMelee data)
    {
        // darius q
        // recycled from Light1
        // wind up
        yield return new WaitForSeconds(data.channelTime);

        // make the box appear
        L2_hitbox.SetActive(true);
        L2_hitbox.GetComponent<EM_L2Hitbox>().Init(data.damage, 2 * data.radius, playerLayer, true);

        // wait some time
        yield return new WaitForSeconds(data.duration);

        // deactivate hitbox
        L2_hitbox.SetActive(false);

        // refractory period
        yield return new WaitForSeconds(data.recoveryTime);
    }

    IEnumerator Heavy1(H1_EliteMelee data)
    {
        // cool car dash forward
        yield return new WaitForSeconds(data.channelTime);
        H1_stunned = false;

        // go forward until we cant
        // SPEED = DISTANCE OVER TIME WE LOVE MATHEMATIC
        float dashSpeed = data.dashDistance / data.dashTime;
        float t = 0;
        while (!H1_stunned || t < data.dashTime)
        {
            t += Time.deltaTime;
            yield return null;

            if (!H1_stunned)
            {
                rb.MovePosition(rb.position + dashSpeed * Time.deltaTime * transform.forward);
            }
        }
    }

    IEnumerator Heavy2(H2_EliteMelee data)
    {
        // garen e SPIN TO WIN BABY
        // recycled from Light1 again
        // wind up
        yield return new WaitForSeconds(data.channelTime);

        // make the box appear
        H2_hitbox.SetActive(true);
        EM_H2Hitbox hitbox = H2_hitbox.GetComponent<EM_H2Hitbox>();
        int damagePerTick = (int)(data.damage * data.damageTickRate / data.duration);
        hitbox.Init(2 * data.radius, damagePerTick, data.damageTickRate, playerLayer, true);

        // DIFFERENT: set navigation towards the player over the duration, while we haven't damaged the player
        navMeshAgent.speed = data.spinMoveSpeed;
        float t = 0;
        while (t < data.duration)
        {
            if (hitbox.HasDamagedPlayer)
            {
                navMeshAgent.ResetPath();
            }
            else
            {
                navMeshAgent.SetDestination(player.position);
            }

            t += Time.deltaTime;
            yield return null;
        }

        // deactivate hitbox
        H2_hitbox.SetActive(false);

        // refractory period
        navMeshAgent.speed = moveSpeed;
        yield return new WaitForSeconds(data.recoveryTime);
    }
    #endregion

    #region Dashing
    IEnumerator DecideDash(EliteMeleeState dashType)
    {
        // a lot of recycled code from elite ranged
        // vector straight to the player
        Vector3 toPlayer = (player.position - transform.position).normalized;
        Vector3 dashTarget = Vector3.zero; // dummy assignment

        switch (dashType)
        {
            case EliteMeleeState.Chasing:
                // dash straight to the player
                dashTarget = transform.position + toPlayer * dashDistance;
                break;

            case EliteMeleeState.CloseEnough:
                // same as above code
                dashTarget = transform.position + toPlayer * dashDistance;
                break;

            case EliteMeleeState.Chasing_TangentialDash:
                // dash tangent to the player
                // pick a random direction
                Vector3 tangent = Vector3.Cross(toPlayer, Vector3.up).normalized;
                if (Random.value < 0.5f)
                    tangent = -tangent;
                dashTarget = transform.position + tangent * dashDistance;
                break;

            case EliteMeleeState.Attacking:
                // attack immediately and do not dash
                yield return new WaitForSeconds(attackWaitTime);
                yield break;
        }

        // execute in separate coroutine
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

    #region Enemy Functions
    protected override void DeathState()
    {
        base.DeathState();

        currentState = EliteMeleeState.Death;

        // disable all the melee hitboxes
        H2_hitbox.SetActive(false);
        L1_hitbox.SetActive(false);
        L2_hitbox.SetActive(false);
    }

    protected void OnTriggerEnter(Collider other)
    {
        // dash forward attack
        if (currentAttack == AttackType.Heavy1)
        {
            if (other.gameObject.layer == playerLayer)
            {
                H1_stunned = true;
                other.GetComponent<PlayerHealth>().TakeDamage(data[(int)AttackType.Heavy1].damage);
            }
            else if (other.gameObject.layer == levelLayer)
            {
                H1_stunned = true;
            }
        }
    }
    #endregion
}