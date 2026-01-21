using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rand = UnityEngine.Random;

public class FinalBoss : Enemy
{
    #region Attack Variables
    // the melees are all even, the ranges are all odd, gungnir is first 4, trishula is last 4
    public enum AttackTypes { Gungnir_M1 = 0, Gungnir_R1, Gungnir_M2, Gungnir_R2, Trishula_M1, Trishula_R1, Trishula_M2, Trishula_R2, NONE }

    [Header("Attacks")]
    [SerializeField, Tooltip("DO NOT CHANGE THE ORDER OR ANY REFERENCES HERE, YOU CAN MODIFY THE SCRIPTABLE OBJECTS BUT NOT THEIR ORDER HERE")]
    FinalBossAttackData[] attackDatas = new FinalBossAttackData[8];
    AttackTypes currentAttack;
    Queue<AttackTypes> attackQueue = new();
    HashSet<AttackTypes> attackSet = new();
    #endregion

    #region Other Variables
    int maxHealth;
    [SerializeField, Tooltip("The waiting time between attacks")]
    float waitTime = 4.0f;
    [SerializeField, Tooltip("How much to multiply waitTime by in phase 2")]
    float waitTimeMultiplier = 0.5f;
    [SerializeField, Tooltip("How far in front of Bentley projectiles spawn")]
    float forwardMultiplier= 1.5f;

    bool isAttacking = false;
    bool isPhase2 = false;
    Vector3 forwardPos;

    [Header("References")]
    [SerializeField, Tooltip("dummy for now")] 
    GameObject TEMP_projectilePrefab;

    [Header("Debug")]
    [SerializeField, Tooltip("Use this to force what Bentley's attack will be, to debug")] 
    AttackTypes forceAttack = AttackTypes.NONE;
    #endregion

    #region Unity Functions
    protected override void Start()
    {
        base.Start();

        maxHealth = health;
        FillQueue();
        StartCoroutine(BentleySequence());
    }

    protected void Update()
    {
        // if he's not attacking, face the player
        if (!isAttacking)
        {
            transform.LookAt(SetY(player.position, transform.position.y));
        }
        forwardPos = transform.position + transform.forward * forwardMultiplier;
    }
    #endregion

    #region Bentley Logic
    IEnumerator BentleySequence()
    {
        while (true)
        {
            // 1/6: pick the attack
            if (forceAttack == AttackTypes.NONE)
            {
                AttackTypes t = attackQueue.Dequeue();
                attackQueue.Enqueue(t);
                currentAttack = attackDatas[(int)t].attackType;
            }
            else
            {
                currentAttack = forceAttack;
            }
            
            // 2/6: get into range for the attack, using velocity
            // reset velocity and zero out drag, this will be done after this as well (except drag)
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.drag = 0;
            while (Vector3.Distance(SetY(transform.position, 0), SetY(player.position, 0)) > GetAttackRange(currentAttack))
            {
                // no obstacles so straight pathfind
                Vector3 toPlayer = player.position - transform.position;
                toPlayer = moveSpeed * SetY(toPlayer, 0).normalized;

                rb.velocity = toPlayer;

                yield return new WaitForEndOfFrame();
            }
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.drag = 10;

            // 3/6: execute that attack
            isAttacking = true;
            yield return StartCoroutine(TypeToAttack(currentAttack));
            isAttacking = false;

            // 4/6: check for feedback, did we hit, cuz if we did the attack sequence needs to change
            // if hit call ShuffleQueue();

            // 5/6: wait the wait period
            if (isPhase2) yield return new WaitForSeconds(waitTime * waitTimeMultiplier);
            else yield return new WaitForSeconds(waitTime);

            // 6/6: repeat
        }
    }

    void FillQueue()
    {
        /* rules: 
        * no same attack range (melee, range) back to back
        * no same arm (gungnir, trishula) back to back
        * 1 or 2, pick random for first, then pick the other one
        * the queue will only ever have 4 elements in it until the player is damaged
        */
        // assume the queue is EMPTY, so clear it to be safe
        attackQueue.Clear();
        attackSet.Clear();

        // add a random element to the queue
        int random = Rand.Range(0, 8);
        AttackTypes lastElement = (AttackTypes)random;
        attackQueue.Enqueue(lastElement);
        attackSet.Add(lastElement);
    
        // depending on this first element, add the second, third, and fourth
        for (int i = 0; i < 3; i++)
        {
            // dummy assignment
            AttackTypes nextElement = AttackTypes.Gungnir_M1;

            // which arm?
            if ((int)lastElement <= 3)
            {
                // gungnir, so we need to add trishula
                // last one was ranged or melee?
                if ((int)lastElement % 2 == 0)
                {
                    // melee, so we need to add ranged
                    // is 1 or 2 already there?
                    if (attackSet.Contains(AttackTypes.Trishula_R1))
                    {
                        nextElement = AttackTypes.Trishula_R2;
                    }
                    else if (attackSet.Contains(AttackTypes.Trishula_R2))
                    {
                        nextElement = AttackTypes.Trishula_R1;
                    }
                    else
                    {
                        // neither are there so pick a random one
                        nextElement = Rand.value > 0.5 ? AttackTypes.Trishula_R1 : AttackTypes.Trishula_R2;
                    }
                }
                else
                {
                    // ranged, so we need to add melee
                    // is 1 or 2 already there?
                    if (attackSet.Contains(AttackTypes.Trishula_M1))
                    {
                        nextElement = AttackTypes.Trishula_M2;
                    }
                    else if (attackSet.Contains(AttackTypes.Trishula_M2))
                    {
                        nextElement = AttackTypes.Trishula_M1;
                    }
                    else
                    {
                        // neither are there so pick a random one
                        nextElement = Rand.value > 0.5 ? AttackTypes.Trishula_M1 : AttackTypes.Trishula_M2;
                    }
                }
            }
            else
            {
                // trishula, so we need to add gungnir
                // last one was ranged or melee?
                if ((int)lastElement % 2 == 0)
                {
                    // melee, so we need to add ranged
                    // is 1 or 2 already there?
                    if (attackSet.Contains(AttackTypes.Gungnir_R1))
                    {
                        nextElement = AttackTypes.Gungnir_R2;
                    }
                    else if (attackSet.Contains(AttackTypes.Gungnir_R2))
                    {
                        nextElement = AttackTypes.Gungnir_R1;
                    }
                    else
                    {
                        // neither are there so pick a random one
                        nextElement = Rand.value > 0.5 ? AttackTypes.Gungnir_R1 : AttackTypes.Gungnir_R2;
                    }
                }
                else
                {
                    // ranged, so we need to add melee
                    // is 1 or 2 already there?
                    if (attackSet.Contains(AttackTypes.Gungnir_M1))
                    {
                        nextElement = AttackTypes.Gungnir_M2;
                    }
                    else if (attackSet.Contains(AttackTypes.Gungnir_M2))
                    {
                        nextElement = AttackTypes.Gungnir_M1;
                    }
                    else
                    {
                        // neither are there so pick a random one
                        nextElement = Rand.value > 0.5 ? AttackTypes.Gungnir_M1 : AttackTypes.Gungnir_M2;
                    }
                }
            }

            // update queue, set, and last element
            attackSet.Add(nextElement);
            attackQueue.Enqueue(nextElement);
            lastElement = nextElement;
        }

        // debug
        //DebugPrintQueue();
    }

    void ShuffleQueue()
    {
        // take the first element in the queue, and add its opposite, 1 or 2
        // do that 4 times

        attackSet.Clear();
        for (int i = 0; i < 4; i++)
        {
            AttackTypes t = attackQueue.Dequeue();
            // dummy assignment
            AttackTypes nextElement = AttackTypes.Gungnir_M1;

            // which arm?
            if ((int)t <= 3)
            {
                // gungnir, so add gungnir but flip melee/range
                // last one was ranged or melee?
                if ((int)t % 2 == 0)
                {
                    // melee, so we need to add ranged
                    // is 1 or 2 already there?
                    if (attackSet.Contains(AttackTypes.Gungnir_R1))
                    {
                        nextElement = AttackTypes.Gungnir_R2;
                    }
                    else if (attackSet.Contains(AttackTypes.Gungnir_R2))
                    {
                        nextElement = AttackTypes.Gungnir_R1;
                    }
                    else
                    {
                        // neither are there so pick a random one
                        nextElement = Rand.value > 0.5 ? AttackTypes.Gungnir_R1 : AttackTypes.Gungnir_R2;
                    }
                }
                else
                {
                    // ranged, so we need to add melee
                    // is 1 or 2 already there?
                    if (attackSet.Contains(AttackTypes.Gungnir_M1))
                    {
                        nextElement = AttackTypes.Gungnir_M2;
                    }
                    else if (attackSet.Contains(AttackTypes.Gungnir_M2))
                    {
                        nextElement = AttackTypes.Gungnir_M1;
                    }
                    else
                    {
                        // neither are there so pick a random one
                        nextElement = Rand.value > 0.5 ? AttackTypes.Gungnir_M1 : AttackTypes.Gungnir_M2;
                    }
                }
            }
            else
            {
                // trishula, so add trishula but flip melee/range
                // last one was ranged or melee?
                if ((int)t % 2 == 0)
                {
                    // melee, so we need to add ranged
                    // is 1 or 2 already there?
                    if (attackSet.Contains(AttackTypes.Trishula_R1))
                    {
                        nextElement = AttackTypes.Trishula_R2;
                    }
                    else if (attackSet.Contains(AttackTypes.Trishula_R2))
                    {
                        nextElement = AttackTypes.Trishula_R1;
                    }
                    else
                    {
                        // neither are there so pick a random one
                        nextElement = Rand.value > 0.5 ? AttackTypes.Trishula_R1 : AttackTypes.Trishula_R2;
                    }
                }
                else
                {
                    // ranged, so we need to add melee
                    // is 1 or 2 already there?
                    if (attackSet.Contains(AttackTypes.Trishula_M1))
                    {
                        nextElement = AttackTypes.Trishula_M2;
                    }
                    else if (attackSet.Contains(AttackTypes.Trishula_M2))
                    {
                        nextElement = AttackTypes.Trishula_M1;
                    }
                    else
                    {
                        // neither are there so pick a random one
                        nextElement = Rand.value > 0.5 ? AttackTypes.Trishula_M1 : AttackTypes.Trishula_M2;
                    }
                }
            }

            attackQueue.Enqueue(nextElement);
            attackSet.Add(nextElement);
        }

        // debug
        DebugPrintQueue();
    }

    void DebugPrintQueue()
    {
        Debug.Assert(attackQueue.Count == 4);
        for (int i = 0; i < 4; i++)
        {
            AttackTypes t = attackQueue.Dequeue();
            Debug.Log("element at position " + i + ": " + t.ToString());
            attackQueue.Enqueue(t);
        }
    }

    /// <summary>
    /// Returns the corresponding attack range for a given attack, by searching through attackDatas
    /// </summary>
    /// <param name="type">The attack to get the range for</param>
    /// <returns>The range of that specific attack</returns>
    float GetAttackRange(AttackTypes type)
    {
        return attackDatas[(int)type].attackRange;
    }
    #endregion

    #region Attacks
    /// <summary>
    /// Calls the appropriate attack coroutine with the type given
    /// </summary>
    /// <param name="t">The attack to call</param>
    /// <returns></returns>
    IEnumerator TypeToAttack(AttackTypes t)
    {
        switch (t)
        {
            case AttackTypes.Gungnir_M1:
                yield return StartCoroutine(GungnirM1((Gungnir_M1)EnumToSO(t)));
                break;
            case AttackTypes.Gungnir_R1:
                yield return StartCoroutine(GungnirR1((Gungnir_R1)EnumToSO(t)));
                break;
            case AttackTypes.Gungnir_M2:
                yield return StartCoroutine(GungnirM2((Gungnir_M2)EnumToSO(t)));
                break;
            case AttackTypes.Gungnir_R2:
                yield return StartCoroutine(GungnirR2((Gungnir_R2)EnumToSO(t)));
                break;
            case AttackTypes.Trishula_M1:
                yield return StartCoroutine(TrishulaM1((Trishula_M1)EnumToSO(t)));
                break;
            case AttackTypes.Trishula_R1:
                yield return StartCoroutine(TrishulaR1((Trishula_R1)EnumToSO(t)));
                break;
            case AttackTypes.Trishula_M2:
                yield return StartCoroutine(TrishulaM2((Trishula_M2)EnumToSO(t)));
                break;
            case AttackTypes.Trishula_R2:
                yield return StartCoroutine(TrishulaR2((Trishula_R2)EnumToSO(t)));
                break;
        }
        yield return null;
    }
    
    /// <summary>
    /// Returns the appropriate ScriptableObject with the corresponding attack values. NOTE: result needs to be casted
    /// </summary>
    /// <param name="t">The attack type</param>
    /// <returns>The scriptable object with the associated data</returns>
    FinalBossAttackData EnumToSO(AttackTypes t)
    {
        return attackDatas[(int)t];
    }
    
    IEnumerator GungnirR1(Gungnir_R1 data)
    {
        // 360 laser shot for 8 seconds

        // first channel and track
        float t = 0;
        while (t < data.channelTime)
        {
            if (t < data.channelTime - data.trackingLetGo)
            {
                // look at the player while tracking
            }

            yield return new WaitForEndOfFrame();
            t += Time.deltaTime;
        }

        // snapshot the current y rotation
        float snapshotYRotation = transform.eulerAngles.y;

        while (transform.eulerAngles.y <= snapshotYRotation + 360f)
        {
            // do some math and have the laser rotate around depending on the speed
            // clamped between data.minSpeed and data.maxSpeed
            // probably also have to do some weird cosine stuff to determine distance
        }
    }

    IEnumerator GungnirR2(Gungnir_R2 data)
    {
        // instantly shoot the player and burn the ground
        for (int i = 0; i < data.attackCount; i++)
        {
            // track the player until the let go period
            float t = 0;
            while (t < data.channelTime)
            {
                if (t < data.channelTime - data.trackingLetGo)
                {
                    // look at the player while charging up
                }

                yield return new WaitForEndOfFrame();
                t += Time.deltaTime;
            }
        }
        yield return null;
    }

    IEnumerator GungnirM1(Gungnir_M1 data)
    {
        // charge forward a few times
        for (int i = 0; i < data.chargeCount; i++)
        {
            float t = 0;
            while (t < data.channelTime)
            {
                // look at the player if its before the tracking let go
                if (t < data.channelTime - data.trackingLetGo)
                {
                    // look at the player while charging up
                }

                yield return new WaitForEndOfFrame();
                t += Time.deltaTime;
            }

            // use the same trick as the car, where it will keep going forward until
            // it is stunned, with that being controlled by a separate collision function

            yield return new WaitForSeconds(data.chargeDelay);
        }
    }

    IEnumerator GungnirM2(Gungnir_M2 data)
    {
        // basically samus final smash
        // first jump up
        yield return new WaitForSeconds(data.channelTime);

        // then apply force to our y pos to make us untargetable

        // then for 10 seconds
        float t = 0;
        float shotDelay = data.beamCount / data.duration;
        int i = 0;
        while (t < data.duration)
        {
            if (i * shotDelay <= t)
            {
                // shoot stuff
            }

            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        // now track the player location and prepare to land

        yield return new WaitForSeconds(data.crashChannel);
    }

    IEnumerator TrishulaR1(Trishula_R1 data)
    {
        // shoot 8 shots, rotate each shot
        float projectileRotation = data.totalDegRotation / data.projectileCount;
        for (int i = 0; i < data.attackCount; i++)
        {
            // rotate to the left so that the attack makes the player in the middle
            transform.LookAt(SetY(player.position, transform.position.y));
            transform.rotation = Quaternion.AngleAxis(transform.eulerAngles.y - data.totalDegRotation / 2 + projectileRotation / 2, Vector3.up);
            for (int j = 0; j < data.projectileCount; j++)
            {
                // 1/3: shoot a shot, instantiated slightly forward
                GameObject reference = Instantiate(data.projectilePrefab, forwardPos, Quaternion.identity);
                reference.GetComponent<FinalBossProj>().Init(transform.forward, data.projectileSpeed, data.projLifetime, data.damage, playerLayer, levelLayer);

                // 2/3: rotate
                transform.rotation = Quaternion.AngleAxis(transform.eulerAngles.y + projectileRotation, Vector3.up);

                // 3/3: wait for delay seconds
                yield return new WaitForSeconds(data.shotDelay);
            }
            yield return new WaitForSeconds(data.attackSequenceDelay);
        }
    }

    IEnumerator TrishulaR2(Trishula_R2 data)
    {
        // code mostly copied from TR1
        // shoot shots that split into smaller shots (configured in the FB_SplitProj class)
        float projectileRotation = data.totalDegRotation / data.projectileCount;

        // pick a random starting pattern
        FB_SplitProj.SplitPattern pattern = Rand.value < 0.5f ? FB_SplitProj.SplitPattern.Cross : FB_SplitProj.SplitPattern.X;

        // rotate to the left so that the attack makes the player in the middle
        transform.LookAt(SetY(player.position, transform.position.y));
        transform.rotation = Quaternion.AngleAxis(transform.eulerAngles.y - data.totalDegRotation / 2 + projectileRotation / 2, Vector3.up);
        for (int i = 0; i < data.projectileCount; i++)
        {
            // 1/4: shoot
            GameObject reference = Instantiate(data.splitProjPrefab, forwardPos, Quaternion.identity);
            reference.GetComponent<FB_SplitProj>().Init(transform.forward, playerLayer, levelLayer, pattern, player);

            // 2/4: rotate
            transform.rotation = Quaternion.AngleAxis(transform.eulerAngles.y + projectileRotation, Vector3.up);

            // 3/4: change the split pattern to alternate
            pattern = pattern == FB_SplitProj.SplitPattern.Cross ? FB_SplitProj.SplitPattern.X : FB_SplitProj.SplitPattern.Cross;

            // 4/4: wait for delay seconds
            yield return new WaitForSeconds(data.shotDelay);
        }
    }

    IEnumerator TrishulaM1(Trishula_M1 data)
    {
        // pantheon tap q
        // set the animation which should also set hitbox
        yield return new WaitForSeconds(data.channelTime);

        // recovery time
        yield return new WaitForSeconds(data.recoveryTime);
    }

    IEnumerator TrishulaM2(Trishula_M2 data)
    {
        // darius q
        // set the animation, which should also set hitbox
        yield return new WaitForSeconds(data.channelTime);
        
        // recovery time
        yield return new WaitForSeconds(data.recoveryTime);
    }
    #endregion

    #region Other Enemy Functions
    protected override void DeathState()
    {
        base.DeathState();

        // cuz theres 8 attacks and all of them are coroutines
        StopAllCoroutines();
    }

    public override int DealDamage(int damageToDeal)
    {
        // copy paste of original code in case we need to change/add effects
        int realDamage = damageToDeal;

        // insert any damage more calculations here
        // realDamage = damageToDeal * damageResist * damageMultiplier;

        // dont subtract for overkill damage
        if (BarkManager.Instance != null)
            BarkManager.Instance.StartBark("Fleck_Happy", "Enemy_Upset");
        health -= realDamage;

        // also show some effects
        hitEffect.Play();
        StartCoroutine(ShowDamageNumbers(realDamage));
        
        // destroy when we have no health left
        if (health <= 0)
        {
            if (isPhase2)
            {
                ImpulseSource.GenerateImpulseWithForce(DeathScreenshakeForce);
                StartCoroutine(nameof(DeathHitstop));
                //Boom plays INSTEAD of hitEffect. Once we have a VFX for boom instead of UI, use .Play instead of coroutine. 
                StartCoroutine(nameof(ShowBoom));
            }
            else
            {
                // initiate revive sequence for phase 2
                Debug.Log("phase 2!");
                isPhase2 = true;
                health = maxHealth;

                // probably also start a coroutine that prevents this function from letting bentley die
                // when he is in the process of reviving into stage 2
            }
        }
        // these "normal" effects should only play if the enemy isn't dead from that attack.
        else
        {
            // hit VFX
            hitEffect.Play();
            // also play screenshake
            ImpulseSource.GenerateImpulseWithForce(DefaultScreenshakeForce);
            // also hitstop
            StartCoroutine(nameof(GlobalHitstop));
        }

        // and update the health bar to match
        TEMP_EnemyHPBar.value = health;

        // use the return value if we need access to how much damage it did
        // like lifesteal calculations or damage trackers
        return realDamage;
    }
    #endregion
}