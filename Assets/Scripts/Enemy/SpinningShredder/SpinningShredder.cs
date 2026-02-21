using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rand = UnityEngine.Random;

public class SpinningShredder : Enemy
{
    [Header("References")]
    [SerializeField, Tooltip("Leave as empty (null) if this is not supposed to split")]
    GameObject splitPrefab;

    [Header("Attacking")]
    [SerializeField, Tooltip("Speed of the spinner as it charges forward")]
    float chargeSpeed = 4f;
    [SerializeField, Tooltip("Maximum random offset from the player")]
    float degOffset = 1f;
    [SerializeField, Tooltip("Knockback dealt to enemies and the player")]
    float knockbackStrength = 4f;
    [SerializeField, Tooltip("Knockback done to self when hitting something")]
    float selfKnockback = 2f;
    [SerializeField, Tooltip("Refractory period before this enemy can attack again")]
    float refractoryPeriod = 4f;

    [Header("Group Behavior")]
    [SerializeField, Tooltip("How far the spinners will try to stay apart from each other")]
    float separationDistance = 1f;
    [SerializeField, Tooltip("Spinners will try to stagger their attacks by this much")]
    float attackStagger = 1f;

    // for group behavior
    static List<SpinningShredder> shredders = new();
    static float nextAttackTime;
    static float attackTimeIncrement = 0.5f;
    bool crashed;

    protected override void Start()
    {
        base.Start();
        navMeshAgent.radius = separationDistance;

        // add this to the shredder list
        shredders.Add(this);
        attackTimeIncrement = attackStagger;

        StartCoroutine(AttackLogic());
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

            // prepare to attack
            currentState = EnemyState.Attacking;
            navMeshAgent.ResetPath();
            FacePlayer();

            // wait for group behavior clearance
            // if we are allowed to attack right now, skip all this mess
            if (Time.time >= nextAttackTime)
            {
                // but before skipping this mess, set the next attack time
                nextAttackTime = Time.time + attackTimeIncrement;
            }
            else
            {
                // random offset to make it slightly more fair
                float randomConst = Rand.Range(0, 0.05f);
                float thisAttackTime;
                do
                {
                    // in case the time updates while we are waiting
                    thisAttackTime = nextAttackTime + randomConst;
                    yield return null;
                } while (Time.time < thisAttackTime);

                // set the next attack time
                nextAttackTime = thisAttackTime + attackTimeIncrement;
            }

            // attack by charging forward
            // forward vector with a random degree offset
            Vector3 chargeVector = Quaternion.Euler(0, Rand.Range(-degOffset, degOffset), 0) * transform.forward;

            // calculate the time it would take to cover the whole distance
            float chargeTime = DistanceToPlayer() / chargeSpeed;

            // charge forward for that duration
            crashed = false;
            rb.velocity = chargeVector * chargeSpeed;
            rb.drag = 0;

            float t = 0;
            while (t < chargeTime)
            {
                if (crashed)
                    break;

                t += Time.deltaTime;
                yield return null;
            }

            rb.velocity = Vector3.zero;
            rb.drag = 10;
            rb.angularVelocity = Vector3.zero;
            currentState = EnemyState.CloseEnough;

            // wait
            yield return new WaitForSeconds(refractoryPeriod);
        }
    }

    protected override void DeathState()
    {
        base.DeathState();

        // remove this from the shredder list
        shredders.Remove(this);

        // if this is supposed to split, then do that
        if (splitPrefab != null)
        {
            // i will figure this out later bruh
        }
    }

    protected void Update()
    {
        if (currentState != EnemyState.Chasing)
            return;

        // separate this spinner from others around it
        foreach (var s in shredders)
        {
            if (s == this)
                continue;

            float dist = Vector3.Distance(SetY(transform.position, 0), SetY(s.transform.position, 0));

            if (dist < separationDistance)
            {
                Vector3 away = (s.transform.position - transform.position).normalized;
                s.rb.MovePosition(s.rb.position + Time.deltaTime * away);
            }
        }
    }

    protected void OnTriggerEnter(Collider other)
    {
        int otherLayer = other.gameObject.layer;

        if (currentState == EnemyState.Attacking)
        {
            crashed = true;
            if (otherLayer == playerLayer)
            {
                other.GetComponent<PlayerHealth>().TakeDamage(attackDamage);
                // supposed to knock back the player but KINEMATIC
            }
            else if (otherLayer == enemyLayer)
            {
                // knock back the other enemy
                Vector3 force = (other.transform.position - transform.position) * knockbackStrength;
                rb.drag = 10;
                other.attachedRigidbody.AddForce(force, ForceMode.Impulse);

                other.GetComponent<Enemy>().DealDamage(attackDamage);
            }
            else if (otherLayer == levelLayer)
            {
                // self knockback
                Vector3 force = (transform.position - other.transform.position) * selfKnockback;
                other.attachedRigidbody.AddForce(force, ForceMode.Impulse);
            }
        }
    }
}
