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

    bool GM1_stunned = false;
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
    [SerializeField, Tooltip("A prefab that is instantiated on top of the player, telling Bentley if his attacks hit the player")] 
    GameObject FB_playerCollider;
    // after we instantiate a collider, use this one for the reference
    FB_PlayerCollider playerCollider;

    [SerializeField, Tooltip("Melee hitbox prefab used by Bentley")]
    GameObject FB_hitbox;
    [SerializeField, Tooltip("The sphere retical that will be instantiated for some abilities")]
    GameObject sphereReticle;
    [SerializeField, Tooltip("The line reticle that is ATTACHED to Bentley (not instantiated)")]
    GameObject lineReticle;

    [Header("Debug")]
    [SerializeField, Tooltip("Use this to force what Bentley's attack will be, to debug")] 
    AttackTypes forceAttack = AttackTypes.NONE;
    #endregion

    #region Unity Functions
    protected override void Start()
    {
        base.Start();

        // spawn the player collider
        playerCollider = Instantiate(FB_playerCollider, player).GetComponent<FB_PlayerCollider>();
        FB_playerCollider.transform.localPosition = Vector3.zero;
        FB_playerCollider.transform.localScale = Vector3.one * 1.1f;

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

                transform.LookAt(SetY(player.position, transform.position.y));
                yield return new WaitForEndOfFrame();
            }  
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.drag = 10;

            transform.LookAt(SetY(player.position, transform.position.y));
            yield return new WaitForEndOfFrame();

            // 3/6: execute that attack
            isAttacking = true;
            yield return StartCoroutine(TypeToAttack(currentAttack));

            // 4/6: check for feedback, did we hit, cuz if we did the attack sequence needs to change
            if (playerCollider.playerTookDamage)
            {
                Debug.Log("the player has been hit by an attack!");
                ShuffleQueue();
                playerCollider.playerTookDamage = false;
            }

            // 5/6: wait the wait period
            if (isPhase2) yield return new WaitForSeconds(waitTime * waitTimeMultiplier);
            else yield return new WaitForSeconds(waitTime);

            // 6/6: repeat
            isAttacking = false;
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
            // dummy assignment to avoid "unassigned variable" error
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
                        nextElement = Rand.value > 0.5f ? AttackTypes.Trishula_R1 : AttackTypes.Trishula_R2;
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
                        nextElement = Rand.value > 0.5f ? AttackTypes.Trishula_M1 : AttackTypes.Trishula_M2;
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
                        nextElement = Rand.value > 0.5f ? AttackTypes.Gungnir_R1 : AttackTypes.Gungnir_R2;
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
                        nextElement = Rand.value > 0.5f ? AttackTypes.Gungnir_M1 : AttackTypes.Gungnir_M2;
                    }
                }
            }

            // update queue, set, and last element
            attackSet.Add(nextElement);
            attackQueue.Enqueue(nextElement);
            lastElement = nextElement;
        }

        // debug, uncomment if needed
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
            // dummy assignment to avoid "unassigned variable" error
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
                        nextElement = Rand.value > 0.5f ? AttackTypes.Gungnir_R1 : AttackTypes.Gungnir_R2;
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
                        nextElement = Rand.value > 0.5f ? AttackTypes.Gungnir_M1 : AttackTypes.Gungnir_M2;
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
                        nextElement = Rand.value > 0.5f ? AttackTypes.Trishula_R1 : AttackTypes.Trishula_R2;
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
                        nextElement = Rand.value > 0.5f ? AttackTypes.Trishula_M1 : AttackTypes.Trishula_M2;
                    }
                }
            }

            attackQueue.Enqueue(nextElement);
            attackSet.Add(nextElement);
        }

        // debug, uncomment if needed
        //DebugPrintQueue();
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
    
    /// <summary>
    /// For channelTime seconds, suspends execution while always facing the player, until the last trackingLetGo seconds.
    /// </summary>
    /// <param name="channelTime">The amount of time to face the player</param>
    /// <param name="trackingLetGo">The amount of time to let go of tracking at the end</param>
    /// <param name="animation">The animation to play (null for now)</param>
    /// <returns></returns>
    IEnumerator AnimationTrackingSequence(float channelTime, float trackingLetGo, Animation animation = null)
    {
        float t = 0;

        // animation.play or whatever it is, but do it here to only call it once
        while (t < channelTime)
        {
            if (t < channelTime - trackingLetGo)
            {
                // look at the player while tracking
                transform.LookAt(SetY(player.position, transform.position.y));
            }

            yield return new WaitForEndOfFrame();
            t += Time.deltaTime;
        }
    }

    /// <summary>
    /// Rotates Bentley from the start Quaternion to the end Quaternion over duration seconds
    /// </summary>
    /// <param name="start">The start rotation</param>
    /// <param name="target">The end rotation</param>
    /// <param name="duration">the time between the two</param>
    /// <returns></returns>
    IEnumerator RotateSequence(Quaternion start, float totalDegrees, float duration, int direction = 1)
    {
        transform.rotation = start;
        
        float rotated = 0f;
        float speed = totalDegrees / duration;

        while (rotated < totalDegrees)
        {
            float step = speed * Time.deltaTime;
            transform.Rotate(Vector3.up, step * direction, Space.World);
            rotated += step;
            
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator GungnirR1(Gungnir_R1 data)
    {
        // 360 laser shot for 8 seconds
        // spawn the reticle (or rather make it appear)
        lineReticle.SetActive(true);
        lineReticle.GetComponent<LineReticle>().Init(data.laserRange, data.channelTime, data.laserWidth);

        // then channel and track
        yield return StartCoroutine(AnimationTrackingSequence(data.channelTime, data.trackingLetGo));

        // instantiate a laser hitbox
        GameObject reference = Instantiate(FB_hitbox, transform);
        reference.GetComponent<FB_Hitbox>().Init(data.damage, data.laserWidth, data.laserRange, playerLayer, true);

        float t = 0;
        while (t < data.duration)
        {
            // bentley tracks the player, rotating his laser at a certain speed
            // to try to catch up to the player
            Vector3 toPlayer = SetY(player.position - transform.position, 0);

            if (toPlayer.sqrMagnitude >= 0.001f)
            {
                // rotatetowards doesnt overshoot, so no need for fancy clamp functions or whatever
                Quaternion playerRotation = Quaternion.LookRotation(toPlayer);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, playerRotation, data.rotationSpeed * Time.deltaTime);
            }

            yield return new WaitForEndOfFrame();
            t += Time.deltaTime;
        }

        Destroy(reference);
    }

    IEnumerator GungnirR2(Gungnir_R2 data)
    {
        // instantly shoot the player and burn the ground
        for (int i = 0; i < data.attackCount; i++)
        {
            // track the player until the let go period
            yield return StartCoroutine(AnimationTrackingSequence(data.channelTime, data.trackingLetGo));

            // fire the projectile
        }
        yield return null;
    }

    IEnumerator GungnirM1(Gungnir_M1 data)
    {
        // charge forward a few times
        for (int i = 0; i < data.chargeCount; i++)
        {
            yield return StartCoroutine(AnimationTrackingSequence(data.channelTime, data.trackingLetGo));
            // use the same trick as the car, where it will keep going forward until
            // it is stunned, with that being controlled by a separate collision function
            // first zero everything out
            GM1_stunned = false;
            
            // go forward until we cant 
            while (!GM1_stunned)
            {
                rb.MovePosition(rb.position + data.chargeSpeed * Time.deltaTime * transform.forward);
                
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForSeconds(data.chargeDelay);
        }
    }

    IEnumerator GungnirM2(Gungnir_M2 data)
    {
        // basically samus final smash
        // first jump up
        yield return new WaitForSeconds(data.channelTime);

        // then apply force to our y pos to make us untargetable
        // for now he just teleports below
        float ySnapshot = transform.position.y;

        // snapshot player position
        Vector3 playerPosSnapshot = player.position;

        // and teleport to that position
        transform.position = player.position + Vector3.up * data.jumpHeight;

        // then for 10 seconds
        float shotDelay = data.duration / data.beamCount;
        int i = 0;
        while (i < data.beamCount)
        {
            // shot a shot straight down
            // 1/6: generate a random position inside the circle centered around playerPosSnapshot
            // use polar coordinates, so generate a random angle and random distance
            float rAngle = Rand.Range(0, 360f) * Mathf.Deg2Rad;
            float rDistance = Mathf.Sqrt(Rand.value) * data.radiusAroundPlayer;

            // 2/6: convert polar to cartesian
            float xPos = rDistance * Mathf.Cos(rAngle) + playerPosSnapshot.x;
            float zPos = rDistance * Mathf.Sin(rAngle) + playerPosSnapshot.z;
            Vector3 projPos = new(xPos, data.jumpHeight, zPos);

            // 3/6: calculate the speed the projectile needs to travel to hit the ground in the specified amount of time
            float velocity = data.jumpHeight / data.shotTravelTime;

            // 4/5: fire the projectile from a height such that it takes some time to fall
            GameObject reference = Instantiate(data.projectilePrefab, projPos, Quaternion.Euler(-90, 0, 0));
            reference.GetComponent<FinalBossProj>().Init(Vector3.down, velocity, data.shotTravelTime, data.damage, playerLayer, levelLayer);
            reference.transform.localScale = Vector3.one * data.projectileScale;

            // 5/6: instantiate a retical below the projectile we just instantiated
            SphereReticle sr = Instantiate(sphereReticle, new Vector3(projPos.x, 0.05f, projPos.z), Quaternion.identity).GetComponent<SphereReticle>();
            sr.Init(data.shotTravelTime, data.projectileScale);

            // 6/6: wait
            i++;
            yield return new WaitForSeconds(shotDelay);
        }

        // now track the player location and prepare to land
        float t = 0;
        while (t < data.crashChannel)
        {
            if (t < data.crashChannel - data.trackingLetGo)
            {
                transform.position = player.position + Vector3.up * data.jumpHeight;
            }
            
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        // then land, probably by lerping again
        t = 0;
        while (t < data.crashSpeed)
        {
            rb.MovePosition(transform.position + data.jumpHeight * t * Vector3.down / data.crashSpeed);

            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        // collision controlled by trigger
        transform.position = new Vector3(transform.position.x, ySnapshot, transform.position.z);
    }

    IEnumerator TrishulaR1(Trishula_R1 data)
    {
        // shoot 8 shots, rotate each shot
        float projectileRotation = data.totalDegRotation / data.projectileCount;
        float totalDuration = data.projectileCount * data.shotDelay;
        Quaternion startRotation = transform.rotation * Quaternion.Euler(0, -data.totalDegRotation / 2f + projectileRotation / 2f, 0);
        int direction = 1;
        for (int i = 0; i < data.attackCount; i++)
        {
            // rotation is controlled by RotateSequence
            StartCoroutine(RotateSequence(startRotation, data.totalDegRotation, totalDuration, direction));
            yield return new WaitForEndOfFrame();
            for (int j = 0; j < data.projectileCount; j++)
            {
                // 1/2: shoot a shot, instantiated slightly forward
                GameObject reference = Instantiate(data.projectilePrefab, forwardPos, Quaternion.identity);
                reference.GetComponent<FinalBossProj>().Init(transform.forward, data.projectileSpeed, data.projLifetime, data.damage, playerLayer, levelLayer);

                // 2/2: wait for delay seconds
                yield return new WaitForSeconds(data.shotDelay);
            }
            yield return new WaitForSeconds(data.attackSequenceDelay);
            startRotation = transform.rotation;
            direction *= -1;
        }
    }

    IEnumerator TrishulaR2(Trishula_R2 data)
    {
        // code mostly copied from TR1
        // shoot shots that split into smaller shots (configured in the FB_SplitProj class)
        float projectileRotation = data.totalDegRotation / data.projectileCount;
        float totalDuration = data.projectileCount * data.shotDelay;
        Quaternion startRotation = transform.rotation * Quaternion.Euler(0, -data.totalDegRotation / 2f + projectileRotation / 2f, 0);

        // pick a random starting pattern
        FB_SplitProj.SplitPattern pattern = Rand.value < 0.5f ? FB_SplitProj.SplitPattern.Cross : FB_SplitProj.SplitPattern.X;

        // rotation is controlled by RotateSequence
        StartCoroutine(RotateSequence(startRotation, data.totalDegRotation, totalDuration));
        yield return new WaitForEndOfFrame();
        for (int i = 0; i < data.projectileCount; i++)
        {
            // 1/3: shoot
            GameObject reference = Instantiate(data.splitProjPrefab, forwardPos, Quaternion.identity);
            reference.GetComponent<FB_SplitProj>().Init(transform.forward, playerLayer, levelLayer, pattern, player);

            // 2/3: change the split pattern to alternate
            pattern = pattern == FB_SplitProj.SplitPattern.Cross ? FB_SplitProj.SplitPattern.X : FB_SplitProj.SplitPattern.Cross;

            // 3/3: wait for delay seconds
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

    protected void OnTriggerEnter(Collider other)
    {
        // the lance charge attack
        if (currentAttack == AttackTypes.Gungnir_M1)
        {
            int otherLayer = other.gameObject.layer;

            if (otherLayer == playerLayer)
            {
                Debug.Log("player collision!");
                GM1_stunned = true;
                Gungnir_M1 data = (Gungnir_M1)EnumToSO(AttackTypes.Gungnir_M1);
                other.GetComponent<PlayerHealth>().TakeDamage(data.damage);
            }
            if (otherLayer == levelLayer)
            {
                GM1_stunned = true;
            }
        }

        // samus final smash attack
        else if (currentAttack == AttackTypes.Gungnir_M2)
        {
            int otherLayer = other.gameObject.layer;

            if (otherLayer == playerLayer)
            {
                Debug.Log("player collision!");
                Gungnir_M2 data = (Gungnir_M2)EnumToSO(AttackTypes.Gungnir_M2);
                other.GetComponent<PlayerHealth>().TakeDamage(data.damage);
            }
        }
    }

    #endregion
}