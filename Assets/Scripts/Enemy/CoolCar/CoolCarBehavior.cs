using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CoolCarBehavior : Enemy
{
    [Header("Attacking")]
    [SerializeField, Tooltip("The collider attached to this car, used to prevent it from winding up in a wall.")]
    GameObject windUpCollider;
    [SerializeField, Tooltip("Time in seconds the car spends winding up before attacking.")]
    float windUpTime;
    [SerializeField, Tooltip("Distance the car winds backward over windUpTime seconds.")]
    float windUpDistance;
    [SerializeField, Tooltip("Speed of the car as it dashes towards the player.")]
    float attackDashSpeed;
    [SerializeField, Tooltip("Maximum distance the car can dash forward before spinning out")]
    float maxDashDistance;
    [SerializeField, Tooltip("Time the car is stunned when hits something.")]
    float stunPeriod;
    [SerializeField, Tooltip("The distance the player will be knocked back when it hits the car.")]
    float knockbackDistance;
    [SerializeField, Tooltip("Knockback distance is multiplied if the car crashes into the player instead of the player running into the car.")]
    float knockbackMultiplier;
    bool crashed = false;

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

    protected override void DeathState()
    {
        base.DeathState();

        // AUDIO: the car is dead, play a death sound
        crashed = true;
    }

    public override void InflictStun(float time)
    {
        base.InflictStun(time);

        crashed = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.drag = 10;
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (currentState == EnemyState.Death || currentState == EnemyState.Stunned)
            return;

        int otherLayer = other.gameObject.layer;

        if (otherLayer == playerLayer)
        {
            other.GetComponent<PlayerHealth>().TakeDamage(attackDamage);

            // inflict a knockback on the player
            Vector3 forceVector = Vector3.Normalize(other.transform.position - transform.position);

            // make the knockback stronger depending on whether the car was attacking or the player just ran into it for fun
            // for now the player can only run into the car "for fun" when the car is stunned and not attacking
            float attackMultiplier = currentState == EnemyState.Attacking ? knockbackMultiplier : 1.0f;
            other.GetComponent<Rigidbody>().AddForce(attackMultiplier * knockbackDistance * forceVector, ForceMode.VelocityChange);
        }
        // keeping it separate to make it clear
        else if (otherLayer == enemyLayer && currentState == EnemyState.Attacking) // only allow this when the cars are attacking
        {
            // per Daniel the designer, damage the other enemy
            other.GetComponent<Enemy>().DealDamage(attackDamage);

            // inflict a knockback in the same way
            // this time there is no multiplier, just a constant
            Vector3 forceVector = Vector3.Normalize(other.transform.position - transform.position);
            other.GetComponent<Rigidbody>().AddForce(0.25f * knockbackDistance * forceVector, ForceMode.VelocityChange);
        }

        if (attackStarted && currentState == EnemyState.Attacking) // enemy hit something while attacking, stun it and play audio
        {
            // these are separated because of audio
            if (otherLayer == playerLayer)
            {
                crashed = true;

                // AUDIO: we hit the player
            }
            else if (otherLayer == levelLayer)
            {
                crashed = true;

                // AUDIO: we crashed into something
            }
            else if (otherLayer == enemyLayer)
            {
                crashed = true;

                // AUDIO: the car hit another enemy
            }
        }

        if (attackStarted && currentState == EnemyState.Channeling && !windUpCollider.GetComponent<CollisionChecker>().Clear)
        {
            crashed = true;
            rb.velocity = Vector3.zero;
        }
    }

    IEnumerator AttackSequence()
    {
        // navigate towards the player
        currentState = EnemyState.Chasing;
        while (!LineOfSight() || !WithinDistance())
        {
            navMeshAgent.SetDestination(player.position);

            yield return null;
        }

        // update state
        currentState = EnemyState.Channeling;

        // remove navigation
        navMeshAgent.ResetPath();

        // remove all drag
        rb.drag = 0;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        FacePlayer();

        // 1/2: dash backwards
        // first store the position of the backwards dash
        Vector3 backwardsPos = -transform.forward;

        // AUDIO: the car is winding up, play a wind-up sound
        // note: it should match the duration of windUpTime
        rb.velocity = backwardsPos * (windUpDistance / windUpTime);
        yield return new WaitForSeconds(windUpTime);

        // stun check before we charge forward
        if (currentState == EnemyState.Stunned)
            yield break;

        crashed = false;
        navMeshAgent.enabled = false;

        // update state
        currentState = EnemyState.Attacking;

        // 2/2: dash towards the player direction and go forward without stopping
        // AUDIO: the car is dashing forward after winding up, idk what sound matches lol
        float dashTime = maxDashDistance / attackDashSpeed;
        rb.velocity = transform.forward * attackDashSpeed;

        float t = 0;
        while (t < dashTime)
        {
            if (crashed)
                break;

            t += Time.deltaTime;
            yield return null;
        }

        // reset velocity
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.drag = 10;

        // update the state again
        currentState = EnemyState.Stunned;

        // the car hit something, make it wait before doing anything else
        yield return new WaitForSeconds(stunPeriod);

        // reset variables
        currentState = EnemyState.Channeling;
        crashed = false;
        attackStarted = false;
        navMeshAgent.enabled = true;
    }
}