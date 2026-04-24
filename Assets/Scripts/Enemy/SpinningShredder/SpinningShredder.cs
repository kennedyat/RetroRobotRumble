using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    [SerializeField, Tooltip("Leave as empty (null) if this is not supposed to split")]
    GameObject splitPrefab;
    [SerializeField, Tooltip("The distance that the spinners try to split from the original")]
    float splitDistance = 1.5f;
    [SerializeField, Tooltip("The time it takes for spinners to split from the original")]
    float splitTime = .5f;
    [SerializeField, Tooltip("The height that the spinners jump when they split")]
    float splitHeight = 3f;

    [Header("Group Behavior")]
    [SerializeField, Tooltip("Spinners will try to stagger their attacks by this much")]
    float attackStagger = 1f;

    [SerializeField] AK.Wwise.Event PlaySpinningDashSFX;
    [SerializeField] AK.Wwise.Event PlaySpinningMovingSFX;
    [SerializeField] AK.Wwise.Event PlaySpinningAlarmSFX;
    [SerializeField] AK.Wwise.Event PlaySpinningSplitSFX;
    [SerializeField] AK.Wwise.Event PlaySpinningBaybladeSFX;

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
            bool isPlayingMoveSound = false;

            while (!LineOfSight() || !WithinDistance())
            {
                navMeshAgent.SetDestination(player.position);

                if (!isPlayingMoveSound)
                {
                    PlaySpinningMovingSFX.Post(gameObject); // AUDIO: SPINNING CASUAL MOVING AROUND SFX
                    isPlayingMoveSound = true;
                }
                yield return null;

            }

            isPlayingMoveSound = false; // reset when chase ends
            
            // prepare to attack
            navMeshAgent.ResetPath();

            // wait for group behavior clearance
            // if we are allowed to attack right now, skip all this mess
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

            PlaySpinningAlarmSFX.Post(gameObject); // AUDIO: SPINNING SHREDDER ALARM SFX

            // wait until scheduled time
            yield return new WaitUntil(() => Time.time >= scheduledTime);

            // attack by charging forward
            // forward vector with a random degree offset
            currentState = EnemyState.Attacking;
            FacePlayer();
            Vector3 chargeVector = Quaternion.Euler(0, Rand.Range(-degOffset, degOffset), 0) * transform.forward;

            // calculate the time it would take to cover the whole distance
            float chargeTime = DistanceToPlayer() / chargeSpeed;

            // charge forward for that duration
            crashed = false;

            PlaySpinningDashSFX.Post(gameObject); // AUDIO: SPINNING SHREDDER DASH SFX

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
                mini.GetComponent<SpinningShredder>().SplitInitializer(splitDistance, splitTime, splitHeight);
                spawnDir = Quaternion.Euler(0, 120, 0) * spawnDir;
            }
        }
    }

    void SplitInitializer(float d, float t, float height)
    {
        StartCoroutine(Split(d, t, height));
        PlaySpinningSplitSFX.Post(gameObject); // AUDIO: SPINNING SHREDDER SPLIT SFX
    }

    IEnumerator Split(float d, float time, float height)
    {
        // really dumb and cringe wait until statement but we have to wait for start to call and get the rb
        yield return new WaitUntil(() => rb != null);

        // for now, disable the collider so we dont take damage
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
        StartCoroutine(AttackLogic());

        PlaySpinningBaybladeSFX.Post(gameObject); // AUDIO: SPINNING SHREDDER (small bayblade) ATTACK SFX
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
                other.attachedRigidbody.AddForce(force, ForceMode.Impulse);

                other.GetComponent<Enemy>().DealDamage(attackDamage);
            }
            else if (otherLayer == levelLayer)
            {
                // self knockback
                Vector3 force = (transform.position - other.transform.position) * selfKnockback;
                rb.AddForce(force, ForceMode.Impulse);
            }
        }
    }
}
