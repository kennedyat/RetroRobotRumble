using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Rand = UnityEngine.Random;

public class SpinningShredder : Enemy
{
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

    [Header("Splitting")]
    [SerializeField, Tooltip("This must be CHECKED if this is the mini spinner")]
    bool isSplitting = false;
    [SerializeField, Tooltip("Leave as empty (null) if this is not supposed to split")]
    GameObject splitPrefab;
    [SerializeField, Tooltip("The distance that the spinners try to split from the original")]
    float splitDistance = 1.5f;
    [SerializeField, Tooltip("The time it takes for spinners to split from the original")]
    float splitTime = .5f;
    [SerializeField, Tooltip("The height that the spinners jump when they split")]
    float splitHeight = 3f;
    [SerializeField, Tooltip("The delay between spinners spawning out when splitting. Note: edit this in MINI spinner")]
    float splitDelay = 0.15f;

    [Header("Group Behavior")]
    [SerializeField, Tooltip("Spinners will try to stagger their attacks by this much")]
    float attackStagger = 1f;

    // for group behavior
    static List<SpinningShredder> spinners;
    static float nextAttackTime;
    static Transform enemyParent;
    bool crashed;

    protected override void Start()
    {
        base.Start();

        // add this to the list
        if (spinners == null)
            spinners = new();
        spinners.Add(this);

        // to spawn in the little guys
        if (enemyParent == null)
            enemyParent = GameObject.Find("EnemyParent").transform;

        // prevent these guys from attacking if they just split
        if (splitPrefab != null)
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
            navMeshAgent.ResetPath();

            // wait for group behavior clearance
            // if we are allowed to attack right now, skip all this mess
            FacePlayer();
            float scheduledTime;
            if (Time.time >= nextAttackTime)
            {
                scheduledTime = Time.time;
            }
            else
            {
                // random offset to make it slightly more fair
                float randomConst = Rand.Range(0, 0.08f);

                // set the next attack time
                scheduledTime = nextAttackTime + randomConst;
            }

            // reserve the next slot
            nextAttackTime = scheduledTime + attackStagger;

            // wait until scheduled time, and stay near the player while waiting
            while (Time.time < scheduledTime)
            {
                if (!LineOfSight() || !WithinDistance())
                    navMeshAgent.SetDestination(player.position);
                else
                    navMeshAgent.ResetPath();
                yield return null;
            }
            navMeshAgent.ResetPath();

            // attack by charging forward
            // forward vector with a random degree offset
            currentState = EnemyState.Attacking;
            FacePlayer();
            Vector3 chargeVector = Quaternion.Euler(0, Rand.Range(-degOffset, degOffset), 0) * transform.forward;

            // calculate the time it would take to cover the whole distance
            float chargeTime = DistanceToPlayer() / chargeSpeed;

            // charge forward for that duration
            crashed = false;

            // also disable navmesh
            navMeshAgent.enabled = false;

            float t = 0;
            while (t < chargeTime)
            {
                rb.velocity = chargeVector * chargeSpeed;

                if (crashed)
                    break;

                t += Time.deltaTime;
                yield return null;
            }

            // reset velocity
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            currentState = EnemyState.CloseEnough;

            // wait
            yield return new WaitForSeconds(refractoryPeriod);

            // navmesh needs to be enabled again
            navMeshAgent.enabled = true;
        }
    }

    protected override void DeathState()
    {
        base.DeathState();

        // remove this from the shredder list
        spinners.Remove(this);

        // if this is supposed to split, then do that (otherwise dont and just die)
        if (splitPrefab != null)
        {
            // this assumes that the 3 spinners spawn at an even 120 degrees from each other
            // and the first one spawns directly opposite the player
            FacePlayer();
            Vector3 spawnDir = -transform.forward;
            for (int i = 0; i < 3; i++)
            {
                GameObject mini = Instantiate(splitPrefab, transform.position, Quaternion.LookRotation(spawnDir), enemyParent);
                mini.GetComponent<SpinningShredder>().SplitInitializer(splitDistance, splitTime, splitHeight, i);
                spawnDir = Quaternion.Euler(0, 120, 0) * spawnDir;
            }
        }
    }

    void SplitInitializer(float d, float t, float height, int num)
    {
        StartCoroutine(Split(d, t, height, num));
    }

    IEnumerator Split(float d, float time, float height, int num)
    {
        // really dumb and cringe wait until statement but we have to wait for start to call and get the rb
        yield return new WaitUntil(() => rb != null);

        // for now, disable the collider so we dont take damage and to prevent other weird issues
        col.enabled = false;

        // lerp to the destination
        Vector3 start = transform.position;
        Vector3 dest = start + transform.forward * d;
        float t = 0;
        while (t < 1f)
        {
            // also add the parabolic offset
            Vector3 lerpPos = Vector3.Lerp(start, dest, t);

            float arc = 4 * height * t * (1 - t);
            lerpPos.y += arc;
            transform.position = lerpPos;

            t += Time.deltaTime / time;
            yield return null;
        }

        // then start attacking
        col.enabled = true;
        isSplitting = false;
        StartCoroutine(AttackLogic());
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (currentState == EnemyState.Death || currentState == EnemyState.Stunned)
            return;

        int otherLayer = other.gameObject.layer;

        if (otherLayer == playerLayer)
        {
            other.GetComponent<PlayerHealth>().TakeDamage(attackDamage);
            // supposed to knock back the player but KINEMATIC                
        }
        else if (otherLayer == enemyLayer && currentState == EnemyState.Attacking)
        {
            // knock back the other enemy
            Vector3 force = (other.transform.position - transform.position) * knockbackStrength;
            other.attachedRigidbody.AddForce(force, ForceMode.Impulse);

            other.GetComponent<Enemy>().DealDamage(attackDamage);
        }
        else if (otherLayer == levelLayer)
        {
            // self knockback
            Vector3 force = (transform.position - other.transform.position) * selfKnockback;
            rb.AddForce(force, ForceMode.Impulse);
        }

        // set crashed. copy paste from car
        if (attackStarted && currentState == EnemyState.Attacking)
        {
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
    }

    public override void DealDamage(int damageToDeal)
    {
        if (isSplitting)
            return;

        base.DealDamage(damageToDeal);
    }
}
