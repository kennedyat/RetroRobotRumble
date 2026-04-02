using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteMelee : Enemy
{
    #region Variables
    public enum AttackType { Light1 = 0, Light2, Heavy1, Heavy2, NONE }
    AttackType currentAttack;
    EnemyState nextDash;
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
    [SerializeField, Tooltip("Use this to force what the attack will be")]
    AttackType forceAttack = AttackType.NONE;
    [SerializeField] bool renderHitboxes = true;
    [SerializeField] bool expandReticles = true;
    bool H1_stunned = false;
    GameObject currentReticle;
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

        // so just delete 2
        list.RemoveAt(Random.Range(0, list.Count));
        list.RemoveAt(Random.Range(0, list.Count));
        attackQueue.Enqueue(list[0]);
        attackQueue.Enqueue(list[1]);

        // randomly shuffle to make it more fair
        if (Random.value < 0.5f)
        {
            attackQueue.Enqueue(attackQueue.Dequeue());
        }

        // random first dash
        nextDash = Random.value < 0.5f ? EnemyState.DashingForward : EnemyState.DashingTangent;

        logicCoroutine = StartCoroutine(AttackLogic());
    }

    protected void Update()
    {
        if (currentState == EnemyState.Stunned)
        {
            // already disabled in stunned function
            // but lets just be sure
            H2_hitbox.SetActive(false);
            L1_hitbox.SetActive(false);
            L2_hitbox.SetActive(false);
            Destroy(currentReticle);
        }
    }
    #region Attacking Logic
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
            float t = 0;
            while (!LineOfSight() || !WithinDistance())
            {
                if (WithinDashDistance())
                {
                    currentState = nextDash;
                    if (t >= dashCooldown)
                    {
                        // dash, reset cooldown, and switch dash type
                        StartCoroutine(DecideDash(nextDash));
                        nextDash = nextDash == EnemyState.DashingTangent ?
                            EnemyState.Chasing : EnemyState.DashingTangent;
                        t = 0;
                    }
                    else
                    {
                        navMeshAgent.SetDestination(player.position);
                        t += Time.deltaTime;
                    }
                }
                else
                {
                    currentState = EnemyState.Chasing;
                    navMeshAgent.SetDestination(player.position);
                }

                yield return null;
            }
            FacePlayer();

            // stop navigation
            navMeshAgent.ResetPath();

            // perform the attack
            currentState = EnemyState.Attacking;
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

    EnemyState GetState()
    {
        float distToPlayer = DistanceToPlayer();

        // too far
        if (distToPlayer > dashRange)
        {
            return EnemyState.Chasing;
        }
        // close enough range
        else if (attackRange < distToPlayer && distToPlayer <= dashRange)
        {
            return EnemyState.CloseEnough;
        }
        // well within range
        else // if distToPlayer <= attackRange
        {
            return EnemyState.Attacking;
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

        yield break;
    }
    IEnumerator Light1(L1_EliteMelee data)
    {
        // pantheon tap q
        // wind up and set the reticle
        currentReticle = Instantiate(lineReticle, transform);
        currentReticle.GetComponent<LineReticle>().Init(data.length, data.channelTime, data.width, expandReticles);

        yield return AnimationTrackingSequence(data.channelTime, data.trackingLetGo);
        if (currentState == EnemyState.Stunned)
            yield break;

        // make the box appear
        L1_hitbox.SetActive(true);
        L1_hitbox.GetComponent<EM_L1Hitbox>().Init(data.damage, data.width, data.length, playerLayer, renderHitboxes);

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
        // wind up and set the reticle
        currentReticle = Instantiate(sphereReticle, transform);
        currentReticle.GetComponent<SphereReticle>().Init(data.channelTime, data.radius, expandReticles);

        yield return AnimationTrackingSequence(data.channelTime, data.trackingLetGo);
        if (currentState == EnemyState.Stunned)
            yield break;

        // make the box appear
        L2_hitbox.SetActive(true);
        L2_hitbox.GetComponent<EM_L2Hitbox>().Init(data.damage, 2 * data.radius, playerLayer, renderHitboxes);

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
        currentReticle = Instantiate(lineReticle, transform);
        currentReticle.GetComponent<LineReticle>().Init(data.dashDistance, data.channelTime, transform.localScale.x, true, expandReticles);

        yield return AnimationTrackingSequence(data.channelTime, data.trackingLetGo);
        if (currentState == EnemyState.Stunned)
            yield break;

        H1_stunned = false;

        // go forward until we cant
        // SPEED = DISTANCE OVER TIME WE LOVE MATHEMATIC
        Vector3 forwardDash = transform.forward;
        float dashSpeed = data.dashDistance / data.duration;
        float t = 0;
        while (t < data.duration)
        {
            if (H1_stunned)
                break;

            rb.velocity = dashSpeed * forwardDash;

            t += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator Heavy2(H2_EliteMelee data)
    {
        // garen e SPIN TO WIN BABY
        // recycled from Light1 again
        // wind up and set the reticle
        currentReticle = Instantiate(sphereReticle, transform);
        currentReticle.GetComponent<SphereReticle>().Init(data.channelTime, data.radius, expandReticles);

        yield return AnimationTrackingSequence(data.channelTime, data.trackingLetGo);
        if (currentState == EnemyState.Stunned)
            yield break;

        // make the box appear
        H2_hitbox.SetActive(true);
        EM_H2Hitbox hitbox = H2_hitbox.GetComponent<EM_H2Hitbox>();
        int damagePerTick = (int)(data.damage * data.damageTickRate / data.duration);
        hitbox.Init(2 * data.radius, damagePerTick, data.damageTickRate, playerLayer, renderHitboxes);

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
    IEnumerator DecideDash(EnemyState dashType)
    {
        // a lot of recycled code from elite ranged
        // vector straight to the player
        Vector3 toPlayer = (player.position - transform.position).normalized;
        Vector3 dashTarget = Vector3.zero; // dummy assignment

        switch (dashType)
        {
            case EnemyState.Chasing:
                // dash straight to the player
                dashTarget = transform.position + toPlayer * dashDistance;
                break;

            case EnemyState.CloseEnough:
                // same as above code
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
        rb.angularVelocity = Vector3.zero;
        navMeshAgent.ResetPath();
        Vector3 dir = (target - rb.position).normalized;

        // set the velocity
        float t = 0;
        while (t < dashDuration)
        {
            rb.velocity = dir * (dashDistance / dashDuration);

            t += Time.deltaTime;
            yield return null;
        }

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
    #endregion

    #region Enemy Functions
    protected override void DeathState()
    {
        base.DeathState();

        // destroy all the melee hitboxes
        Destroy(H2_hitbox);
        Destroy(L1_hitbox);
        Destroy(L2_hitbox);

        if (currentReticle != null)
            Destroy(currentReticle);

        H1_stunned = true;
    }

    public override void InflictStun(float time)
    {
        base.InflictStun(time);

        // disable all the melee hitboxes like above
        H2_hitbox.SetActive(false);
        L1_hitbox.SetActive(false);
        L2_hitbox.SetActive(false);

        if (currentReticle != null)
            Destroy(currentReticle);

        // hold in place
        H1_stunned = true;
    }

    protected void OnTriggerEnter(Collider other)
    {
        // dash forward attack
        if (currentAttack == AttackType.Heavy1 && currentState == EnemyState.Attacking)
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