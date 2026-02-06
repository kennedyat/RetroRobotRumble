using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Rand = UnityEngine.Random;

public class FinalBoss : Enemy
{
    #region Attack Variables
    // the melees are all even, the ranges are all odd, gungnir is first 4, trishula is last 4
    public enum P1_Attacks { Gungnir_M1 = 0, Gungnir_R1, Gungnir_M2, Gungnir_R2, Trishula_M1, Trishula_R1, Trishula_M2, Trishula_R2, NONE }
    public enum P2_Attacks { Omega_GM = 0, Omega_GR, Omega_TM, Omega_TR, OMEGA1, OMEGA2, OMEGA3, NONE }

    [Header("Attacks")]
    [SerializeField, Tooltip("DO NOT CHANGE THE ORDER OR ANY REFERENCES HERE, YOU CAN MODIFY THE SCRIPTABLE OBJECTS BUT NOT THEIR ORDER HERE")]
    FB_P1AttackData[] P1_attackDatas = new FB_P1AttackData[8];
    [SerializeField, Tooltip("DO NOT CHANGE THE ORDER OR ANY REFERENCES HERE, YOU CAN MODIFY THE SCRIPTABLE OBJECTS BUT NOT THEIR ORDER HERE")]
    FB_P2AttackData[] P2_attackDatas = new FB_P2AttackData[7];
    [SerializeField, Tooltip("Used for GM1 to lerp back to the middle")]
    FB_LerpMid fB_LerpMid;
    P1_Attacks P1_currentAttack;
    P2_Attacks P2_currentAttack;
    Queue<P1_Attacks> attackQueue = new();
    HashSet<P1_Attacks> attackSet = new();

    bool isAttacking = false;
    bool isPhase2 = false;
    bool chargeStunned = false;
    /// <summary>
    /// Any coroutines running concurrently with the current attack need to be tracked and stopped when this enemy dies.
    /// </summary>
    Coroutine concurrentCoroutine;
    Coroutine currentPhaseCoroutine;
    #endregion

    #region Other Variables
    int maxHealth;
    [Header("Time Variables")]
    [SerializeField, Tooltip("The waiting time between attacks")]
    float waitTime = 4.0f;
    [SerializeField, Tooltip("How much to multiply waitTime by in phase 2")]
    float waitTimeMultiplier = 0.5f;
    [SerializeField, Tooltip("Length of the phase 1 to phase 2 cutscene")]
    float phaseTransitionTime;

    [Header("Dashing")]
    [SerializeField, Tooltip("How far this enemy dashes")]
    float dashDistance = 5f;
    [SerializeField, Tooltip("How long it takes to complete a dash")]
    float dashDuration = 0.2f;

    [Header("References")]
    [SerializeField, Tooltip("A prefab that is instantiated on top of the player, telling Bentley if his attacks hit the player")]
    GameObject FB_playerCollider;
    // after we instantiate a collider, use this one for the reference
    FB_PlayerCollider playerCollider;

    [SerializeField, Tooltip("The sphere retical that will be instantiated for some abilities")]
    GameObject sphereReticle;
    [SerializeField, Tooltip("The line reticle that is ATTACHED to Bentley (not instantiated)")]
    GameObject lineReticle;
    [SerializeField, Tooltip("Where projectiles are instantiated")]
    Transform firePoint;
    [SerializeField, Tooltip("TEMPORARY text on Bentley's face for debugging and other use")]
    TextMeshPro TEMP_text;

    [Header("Debug")]
    [SerializeField, Tooltip("Use this to force what Bentley's attack will be, to debug. Leave NONE for no forced attack")]
    P1_Attacks forceAttackP1 = P1_Attacks.NONE;
    [SerializeField, Tooltip("Use this to force Bentley to enter phase 2 and start only using this attack. Leave NONE to test phase 1 and not force any attack")]
    P2_Attacks forceAttackP2 = P2_Attacks.NONE;
    [SerializeField, Tooltip("Whether or not to render debug colliders used in several melee and ranged abilities")]
    bool renderDebugColliders = true;
    [SerializeField, Tooltip("Skip the phase transition cutscene")]
    bool skipPhaseTransition = true;
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

        if (forceAttackP2 == P2_Attacks.NONE)
        {
            currentPhaseCoroutine = StartCoroutine(BentleyPhase1());
        }
        else
        {
            currentPhaseCoroutine = StartCoroutine(BentleyPhase2());
        }
    }

    protected void Update()
    {
        // if he's not attacking, face the player
        if (!isAttacking)
        {
            transform.LookAt(SetY(player.position, transform.position.y));
        }
    }
    #endregion

    #region P1 Attack Logic
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
        P1_Attacks lastElement = (P1_Attacks)random;
        attackQueue.Enqueue(lastElement);
        attackSet.Add(lastElement);

        // depending on this first element, add the second, third, and fourth
        for (int i = 0; i < 3; i++)
        {
            // dummy assignment to avoid "unassigned variable" error
            P1_Attacks nextElement = P1_Attacks.Gungnir_M1;

            // which arm?
            if ((int)lastElement <= 3)
            {
                // gungnir, so we need to add trishula
                // last one was ranged or melee?
                if ((int)lastElement % 2 == 0)
                {
                    // melee, so we need to add ranged
                    // is 1 or 2 already there?
                    if (attackSet.Contains(P1_Attacks.Trishula_R1))
                    {
                        nextElement = P1_Attacks.Trishula_R2;
                    }
                    else if (attackSet.Contains(P1_Attacks.Trishula_R2))
                    {
                        nextElement = P1_Attacks.Trishula_R1;
                    }
                    else
                    {
                        // neither are there so pick a random one
                        nextElement = Rand.value > 0.5f ? P1_Attacks.Trishula_R1 : P1_Attacks.Trishula_R2;
                    }
                }
                else
                {
                    // ranged, so we need to add melee
                    // is 1 or 2 already there?
                    if (attackSet.Contains(P1_Attacks.Trishula_M1))
                    {
                        nextElement = P1_Attacks.Trishula_M2;
                    }
                    else if (attackSet.Contains(P1_Attacks.Trishula_M2))
                    {
                        nextElement = P1_Attacks.Trishula_M1;
                    }
                    else
                    {
                        // neither are there so pick a random one
                        nextElement = Rand.value > 0.5f ? P1_Attacks.Trishula_M1 : P1_Attacks.Trishula_M2;
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
                    if (attackSet.Contains(P1_Attacks.Gungnir_R1))
                    {
                        nextElement = P1_Attacks.Gungnir_R2;
                    }
                    else if (attackSet.Contains(P1_Attacks.Gungnir_R2))
                    {
                        nextElement = P1_Attacks.Gungnir_R1;
                    }
                    else
                    {
                        // neither are there so pick a random one
                        nextElement = Rand.value > 0.5f ? P1_Attacks.Gungnir_R1 : P1_Attacks.Gungnir_R2;
                    }
                }
                else
                {
                    // ranged, so we need to add melee
                    // is 1 or 2 already there?
                    if (attackSet.Contains(P1_Attacks.Gungnir_M1))
                    {
                        nextElement = P1_Attacks.Gungnir_M2;
                    }
                    else if (attackSet.Contains(P1_Attacks.Gungnir_M2))
                    {
                        nextElement = P1_Attacks.Gungnir_M1;
                    }
                    else
                    {
                        // neither are there so pick a random one
                        nextElement = Rand.value > 0.5f ? P1_Attacks.Gungnir_M1 : P1_Attacks.Gungnir_M2;
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
            P1_Attacks t = attackQueue.Dequeue();
            // dummy assignment to avoid "unassigned variable" error
            P1_Attacks nextElement = P1_Attacks.Gungnir_M1;

            // which arm?
            if ((int)t <= 3)
            {
                // gungnir, so add gungnir but flip melee/range
                // last one was ranged or melee?
                if ((int)t % 2 == 0)
                {
                    // melee, so we need to add ranged
                    // is 1 or 2 already there?
                    if (attackSet.Contains(P1_Attacks.Gungnir_R1))
                    {
                        nextElement = P1_Attacks.Gungnir_R2;
                    }
                    else if (attackSet.Contains(P1_Attacks.Gungnir_R2))
                    {
                        nextElement = P1_Attacks.Gungnir_R1;
                    }
                    else
                    {
                        // neither are there so pick a random one
                        nextElement = Rand.value > 0.5f ? P1_Attacks.Gungnir_R1 : P1_Attacks.Gungnir_R2;
                    }
                }
                else
                {
                    // ranged, so we need to add melee
                    // is 1 or 2 already there?
                    if (attackSet.Contains(P1_Attacks.Gungnir_M1))
                    {
                        nextElement = P1_Attacks.Gungnir_M2;
                    }
                    else if (attackSet.Contains(P1_Attacks.Gungnir_M2))
                    {
                        nextElement = P1_Attacks.Gungnir_M1;
                    }
                    else
                    {
                        // neither are there so pick a random one
                        nextElement = Rand.value > 0.5f ? P1_Attacks.Gungnir_M1 : P1_Attacks.Gungnir_M2;
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
                    if (attackSet.Contains(P1_Attacks.Trishula_R1))
                    {
                        nextElement = P1_Attacks.Trishula_R2;
                    }
                    else if (attackSet.Contains(P1_Attacks.Trishula_R2))
                    {
                        nextElement = P1_Attacks.Trishula_R1;
                    }
                    else
                    {
                        // neither are there so pick a random one
                        nextElement = Rand.value > 0.5f ? P1_Attacks.Trishula_R1 : P1_Attacks.Trishula_R2;
                    }
                }
                else
                {
                    // ranged, so we need to add melee
                    // is 1 or 2 already there?
                    if (attackSet.Contains(P1_Attacks.Trishula_M1))
                    {
                        nextElement = P1_Attacks.Trishula_M2;
                    }
                    else if (attackSet.Contains(P1_Attacks.Trishula_M2))
                    {
                        nextElement = P1_Attacks.Trishula_M1;
                    }
                    else
                    {
                        // neither are there so pick a random one
                        nextElement = Rand.value > 0.5f ? P1_Attacks.Trishula_M1 : P1_Attacks.Trishula_M2;
                    }
                }
            }

            attackQueue.Enqueue(nextElement);
            attackSet.Add(nextElement);
        }

        // debug, uncomment if needed
        DebugPrintQueue();
    }

    void DebugPrintQueue()
    {
        Debug.Assert(attackQueue.Count == 4);
        string message = "";
        for (int i = 0; i < 4; i++)
        {
            P1_Attacks t = attackQueue.Dequeue();
            message += t.ToString() + " ";
            attackQueue.Enqueue(t);
        }
        Debug.Log(message);
    }

    /// <summary>
    /// Returns the corresponding attack range for a given attack, by searching through attackDatas
    /// </summary>
    /// <param name="type">The attack to get the range for</param>
    /// <returns>The range of that specific attack</returns>
    float EnumToAttackRange(P1_Attacks type)
    {
        return P1_attackDatas[(int)type].attackRange;
    }
    #endregion

    #region P1 Attacks
    IEnumerator BentleyPhase1()
    {
        FillQueue();

        while (!isPhase2)
        {
            // 1/6: pick the attack
            if (forceAttackP1 == P1_Attacks.NONE)
            {
                P1_Attacks t = attackQueue.Dequeue();
                attackQueue.Enqueue(t);
                P1_currentAttack = P1_attackDatas[(int)t].attackType;
            }
            else
            {
                P1_currentAttack = forceAttackP1;
            }

            // 2/6: get into range for the attack, using movePosition
            while (Vector3.Distance(SetY(transform.position, 0), SetY(player.position, 0)) > EnumToAttackRange(P1_currentAttack))
            {
                // no obstacles so straight pathfind
                Vector3 toPlayer = player.position - transform.position;
                toPlayer = moveSpeed * SetY(toPlayer, 0).normalized;

                rb.MovePosition(rb.position + toPlayer * Time.deltaTime);

                transform.LookAt(SetY(player.position, transform.position.y));
                yield return null;
            }

            transform.LookAt(SetY(player.position, transform.position.y));
            yield return null;

            // 3/6: execute that attack
            isAttacking = true;
            yield return EnumToAttack(P1_currentAttack);
            isAttacking = false;

            // 4/6: check for feedback, did we hit, cuz if we did the attack sequence needs to change
            if (playerCollider.playerTookDamage)
            {
                Debug.Log("the player has been hit by an attack! now shuffling queue");
                ShuffleQueue();
                playerCollider.playerTookDamage = false;
            }

            // 5/6: wait the wait period
            yield return new WaitForSeconds(waitTime);

            // 6/6: repeat
        }
    }

    /// <summary>
    /// Calls the appropriate attack coroutine with the type given
    /// </summary>
    /// <param name="t">The attack to call</param>
    /// <returns></returns>
    IEnumerator EnumToAttack(P1_Attacks t)
    {
        FB_P1AttackData data = P1_attackDatas[(int)t];

        switch (t)
        {
            case P1_Attacks.Gungnir_M1:
                yield return GungnirM1((Gungnir_M1)data);
                break;
            case P1_Attacks.Gungnir_R1:
                yield return GungnirR1((Gungnir_R1)data);
                break;
            case P1_Attacks.Gungnir_M2:
                yield return GungnirM2((Gungnir_M2)data);
                break;
            case P1_Attacks.Gungnir_R2:
                yield return GungnirR2((Gungnir_R2)data);
                break;
            case P1_Attacks.Trishula_M1:
                yield return TrishulaM1((Trishula_M1)data);
                break;
            case P1_Attacks.Trishula_R1:
                yield return TrishulaR1((Trishula_R1)data);
                break;
            case P1_Attacks.Trishula_M2:
                yield return TrishulaM2((Trishula_M2)data);
                break;
            case P1_Attacks.Trishula_R2:
                yield return TrishulaR2((Trishula_R2)data);
                break;
        }
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
        // for now temporary text change
        TEMP_text.text = "channel";
        while (t < channelTime)
        {
            if (t < channelTime - trackingLetGo)
            {
                // look at the player while tracking
                transform.LookAt(SetY(player.position, transform.position.y));
            }

            yield return null;
            t += Time.deltaTime;
        }
        TEMP_text.text = "I SEE YOU";
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

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            float angle = Mathf.Lerp(
                0f,
                totalDegrees * direction,
                t
            );

            transform.rotation = start * Quaternion.Euler(0, angle, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Final snap (now actually correct)
        transform.rotation = start * Quaternion.Euler(0, totalDegrees * direction, 0);
    }

    IEnumerator GungnirR1(Gungnir_R1 data)
    {
        // 8 second tracking laser
        // spawn the reticle (or rather make it appear)
        lineReticle.SetActive(true);
        lineReticle.GetComponent<LineReticle>().Init(data.laserRange, data.channelTime, data.laserWidth);

        // then channel and track
        yield return AnimationTrackingSequence(data.channelTime, data.trackingLetGo);

        // instantiate a laser hitbox
        GameObject reference = Instantiate(data.projectilePrefab, transform);
        reference.GetComponent<FB_Hitbox>().Init(data.damage, data.laserWidth, data.laserRange, playerLayer, renderDebugColliders);

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

            yield return null;
            t += Time.deltaTime;
        }

        Destroy(reference);
    }

    IEnumerator GungnirR2(Gungnir_R2 data)
    {
        // instantly shoot the player and burn the ground
        // instantiate a collider in advance and use the same one for all attacks
        GameObject collider = Instantiate(data.projectilePrefab, transform);
        collider.GetComponent<FB_Hitbox>().Init(data.damage, data.laserWidth, data.laserRange, playerLayer, renderDebugColliders);
        collider.SetActive(false);
        for (int i = 0; i < data.attackCount; i++)
        {
            // set the reticle
            lineReticle.SetActive(true);
            lineReticle.GetComponent<LineReticle>().Init(data.laserRange, data.channelTime, data.laserWidth);

            // track the player until the let go period
            yield return AnimationTrackingSequence(data.channelTime, data.trackingLetGo);

            // fire the projectile
            collider.SetActive(true);

            // leave the burning area behind
            GameObject burn = Instantiate(data.burnArea, collider.transform.position, collider.transform.rotation);
            burn.GetComponent<FB_BurnArea>().Init(data.burnDamage, 1f, playerLayer, data.laserWidth, data.laserRange, data.burnDuration);

            // wait
            yield return new WaitForSeconds(data.delayBetweenLasers);

            // deactivate the laser
            collider.SetActive(false);
        }

        Destroy(collider);
    }

    IEnumerator GungnirM1(Gungnir_M1 data)
    {
        // charge forward a few times
        for (int i = 0; i < data.chargeCount; i++)
        {
            // set the reticle
            lineReticle.SetActive(true);
            lineReticle.GetComponent<LineReticle>().Init(-1, data.channelTime, transform.localScale.x, true);

            yield return AnimationTrackingSequence(data.channelTime, data.trackingLetGo);
            // use the same trick as the car, where it will keep going forward until
            // it is stunned, with that being controlled by a separate collision function
            // first zero everything out
            chargeStunned = false;

            // go forward until we cant 
            while (!chargeStunned)
            {
                yield return null;
                rb.MovePosition(rb.position + data.chargeSpeed * Time.deltaTime * transform.forward);
            }

            yield return new WaitForSeconds(data.recoveryTime);
        }

        // return back to the middle
        yield return LerpMid();
    }

    IEnumerator GungnirM2(Gungnir_M2 data)
    {
        // basically samus final smash
        // first jump up
        yield return AnimationTrackingSequence(data.channelTime, 0);

        // then apply force to our y pos to make us untargetable
        // for now he just teleports below
        float ySnapshot = transform.position.y;

        // and teleport to that position
        transform.position = player.position + Vector3.up * data.jumpHeight;

        // then for 10 seconds
        float shotDelay = data.duration / data.beamCount;
        for (int i = 0; i < data.beamCount; i++)
        {
            // shoot a shot straight down
            // 1/6: generate a random position inside the circle centered around playerPosSnapshot
            // use polar coordinates, so generate a random angle and random distance
            float rAngle = Rand.Range(0, 360f) * Mathf.Deg2Rad;
            float rDistance = Mathf.Sqrt(Rand.value) * data.radiusAroundPlayer;

            // 2/6: convert polar to cartesian
            float xPos = rDistance * Mathf.Cos(rAngle) + player.position.x;
            float zPos = rDistance * Mathf.Sin(rAngle) + player.position.z;
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
            yield return new WaitForSeconds(shotDelay);
        }

        // set the collider scale to be a bit wider
        float origScale = box.size.x;
        box.size = new Vector3(origScale * data.crashScale, box.size.y, origScale * data.crashScale);

        // set the reticle for crash channel
        SphereReticle crashIndicator = Instantiate(sphereReticle, new Vector3(transform.position.x, 0.05f, transform.position.z), Quaternion.identity).GetComponent<SphereReticle>();
        crashIndicator.Init(data.crashChannel, transform.localScale.x * data.crashScale);

        // now track the player location and prepare to land
        float t = 0;
        while (t < data.crashChannel)
        {
            if (t < data.crashChannel - data.trackingLetGo)
            {
                transform.position = player.position + Vector3.up * data.jumpHeight;
                crashIndicator.transform.position = new Vector3(transform.position.x, 0.05f, transform.position.z);
            }

            t += Time.deltaTime;
            yield return null;
        }

        // then land, probably by lerping again
        t = 0;
        while (t < data.crashSpeed)
        {
            rb.MovePosition(transform.position + data.jumpHeight * t * Vector3.down / data.crashSpeed);

            t += Time.deltaTime;
            yield return null;
        }
        // collision controlled by trigger
        transform.position = new Vector3(transform.position.x, ySnapshot, transform.position.z);

        // then set the collider back to normal
        yield return null;
        box.size = new Vector3(origScale, box.size.y, origScale);
    }

    IEnumerator TrishulaR1(Trishula_R1 data)
    {
        // shoot 8 shots, rotate each shot
        float totalDuration = data.projectileCount * data.shotDelay;
        Quaternion startRotation = transform.rotation * Quaternion.Euler(0, -data.totalDegRotation / 2f % 180, 0);
        int direction = 1;
        for (int i = 0; i < data.attackCount; i++)
        {
            // rotation is controlled by RotateSequence
            concurrentCoroutine = StartCoroutine(RotateSequence(startRotation, data.totalDegRotation, totalDuration, direction));
            yield return null;
            for (int j = 0; j < data.projectileCount; j++)
            {
                // 1/2: shoot a shot, instantiated slightly forward
                GameObject reference = Instantiate(data.projectilePrefab, firePoint.position, Quaternion.identity);
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
        concurrentCoroutine = StartCoroutine(RotateSequence(startRotation, data.totalDegRotation, totalDuration));
        yield return null;
        for (int i = 0; i < data.projectileCount; i++)
        {
            // 1/3: shoot
            GameObject reference = Instantiate(data.projectilePrefab, firePoint.position, Quaternion.identity);
            reference.transform.localScale = Vector3.one * data.projectileScale;
            reference.GetComponent<FB_SplitProj>().Init(transform.forward, data.projectileSpeed, data.damage, data.splitDistance,
                data.splitCount, data.splitProjLifetime, data.splitProjScale, data.splitProjDamage, data.splitProjSpeed,
                    playerLayer, levelLayer, pattern, player);

            // 2/3: change the split pattern to alternate
            pattern = pattern == FB_SplitProj.SplitPattern.Cross ? FB_SplitProj.SplitPattern.X : FB_SplitProj.SplitPattern.Cross;

            // 3/3: wait for delay seconds
            yield return new WaitForSeconds(data.shotDelay);
        }
    }

    IEnumerator TrishulaM1(Trishula_M1 data)
    {
        // pantheon tap q
        // in the future may be controlled by animation instead
        // but for now lets do this manually with a line reticle and collider
        lineReticle.SetActive(true);
        lineReticle.GetComponent<LineReticle>().Init(data.stabLength, data.channelTime, data.stabWidth);

        // wait
        yield return new WaitForSeconds(data.channelTime);

        // then spawn the collider
        GameObject rc = Instantiate(data.projectilePrefab, transform);
        rc.GetComponent<FB_Hitbox>().Init(data.damage, data.stabWidth, data.stabLength, playerLayer, renderDebugColliders);

        // recovery time
        yield return new WaitForSeconds(data.recoveryTime);

        Destroy(rc);
    }

    IEnumerator TrishulaM2(Trishula_M2 data)
    {
        // darius q
        // in the future may be controlled by animation instead
        // but for now lets do this manually with a sphere reticle and collider
        GameObject sr = Instantiate(sphereReticle, transform);
        sr.transform.localPosition = Vector3.down;
        sr.GetComponent<SphereReticle>().Init(data.channelTime, 2 * data.sweepRange / transform.localScale.x);

        // wait
        yield return new WaitForSeconds(data.channelTime);

        // then spawn the collider
        GameObject sc = Instantiate(data.projectilePrefab, transform);
        sc.GetComponent<FB_Hitbox>().Init(data.damage, 2 * data.sweepRange, 2 * data.sweepRange, playerLayer, renderDebugColliders);

        // recovery time
        yield return new WaitForSeconds(data.recoveryTime);

        Destroy(sc);
    }

    IEnumerator LerpMid(float time = -1)
    {
        P1_currentAttack = P1_Attacks.NONE;
        P2_currentAttack = P2_Attacks.NONE;
        TEMP_text.text = "lerp mid";
        Vector3 startPos = transform.position;
        Vector3 endPos = SetY(fB_LerpMid.midLocation, transform.position.y);
        time = time == -1 ? fB_LerpMid.lerpTime : time;
        float t = 0;
        while (t < time)
        {
            transform.position = Vector3.Lerp(startPos, endPos, t / time);

            t += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
        TEMP_text.text = "I SEE YOU";
    }
    #endregion

    #region P2 Attack Logic
    void CleanupPhase1()
    {
        // stop rotate coroutine, but NOT the attack coroutine, because at this point
        // attackCoroutine = phase 2
        if (concurrentCoroutine != null)
            StopCoroutine(concurrentCoroutine);

        // in case we have a melee attack
        P1_currentAttack = P1_Attacks.NONE;

        // delete any projectiles
        GameObject[] delete = GameObject.FindGameObjectsWithTag("FB_DestroyOnPhase2");
        for (int i = 0; i < delete.Length; i++)
        {
            Destroy(delete[i]);
        }

        // in case any attack was active
        lineReticle.SetActive(false);

        // reset health to max and other variables
        health = 999999999; // to make him invulnerable (sure)
        isPhase2 = true;
    }

    IEnumerator BentleyPhase2()
    {
        CleanupPhase1();
        if (!skipPhaseTransition)
        {
            // go back to the middle slowly
            StartCoroutine(LerpMid(phaseTransitionTime));

            // while thats happening delete a bunch of stuff
            float t = 0;
            while (t < phaseTransitionTime)
            {
                CleanupPhase1();

                t += Time.deltaTime;
                yield return null;
            }
        }

        // health set to max health
        health = maxHealth;

        // then start attacking in a similar way
        int normalAttackCount = 0;
        List<P2_Attacks> omegaAttacks = new()
        {
            P2_Attacks.OMEGA1,
            P2_Attacks.OMEGA2,
            P2_Attacks.OMEGA3
        };

        while (health > 0)
        {
            // pick the next attack depending on how many normal attacks there were
            if (forceAttackP2 == P2_Attacks.NONE)
            {
                if (normalAttackCount == 2)
                {
                    // omega attack, pick a random one
                    P2_currentAttack = omegaAttacks[Rand.Range(0, omegaAttacks.Count)];
                    omegaAttacks.Remove(P2_currentAttack);

                    // if we removed the last one, refresh the list
                    if (omegaAttacks.Count == 0)
                    {
                        omegaAttacks.Add(P2_Attacks.OMEGA1);
                        omegaAttacks.Add(P2_Attacks.OMEGA2);
                        omegaAttacks.Add(P2_Attacks.OMEGA3);
                    }

                    // reset the counter
                    normalAttackCount = 0;
                }
                else
                {
                    // pick a regular attack
                    // TODO: awaiting design specs for this
                    P2_currentAttack = P2_Attacks.NONE; // dummy assignment to avoid compiler errors

                    normalAttackCount++;
                }
            }
            else
            {
                // take the forced attack
                P2_currentAttack = forceAttackP2;
            }

            // then get in range for that attack
            while (Vector3.Distance(SetY(transform.position, 0), SetY(player.position, 0)) > EnumToAttackRange(this.P2_currentAttack))
            {
                // no obstacles so straight pathfind
                Vector3 toPlayer = player.position - transform.position;
                toPlayer = moveSpeed * SetY(toPlayer, 0).normalized;

                rb.MovePosition(rb.position + toPlayer * Time.deltaTime);

                transform.LookAt(SetY(player.position, transform.position.y));
                yield return null;
            }

            // dash?? if so steal the elite ranged code!

            // execute that attack
            isAttacking = true;
            yield return EnumToAttack(P2_currentAttack);
            isAttacking = false;

            // wait some time
            yield return new WaitForSeconds(waitTime * waitTimeMultiplier);

            // repeat
        }
    }

    float EnumToAttackRange(P2_Attacks t)
    {
        return P2_attackDatas[(int)t].attackRange;
    }

    IEnumerator EnumToAttack(P2_Attacks t)
    {
        FB_P2AttackData data = P2_attackDatas[(int)t];

        switch (t)
        {
            case P2_Attacks.Omega_GM:
                yield return OmegaGM((Omega_GM)data);
                break;

            case P2_Attacks.Omega_GR:
                yield return OmegaGR((Omega_GR)data);
                break;

            case P2_Attacks.Omega_TM:
                yield return OmegaTM((Omega_TM)data);
                break;

            case P2_Attacks.Omega_TR:
                yield return OmegaTR((Omega_TR)data);
                break;

            case P2_Attacks.OMEGA1:
                yield return Omega1((OMEGA_1)data);
                break;

            case P2_Attacks.OMEGA2:
                yield return Omega2((OMEGA_2)data);
                break;

            case P2_Attacks.OMEGA3:
                yield return Omega3((OMEGA_3)data);
                break;
        }
    }
    #endregion

    #region P2 Attacks
    IEnumerator OmegaGM(Omega_GM data)
    {
        // lance charge a few times, and while this is happening, shots fall from the sky like GM2
        // the shots falling from the sky will be handled separately
        concurrentCoroutine = StartCoroutine(OmegaGMProjectiles(data));
        for (int i = 0; i < data.chargeCount; i++)
        {
            // set the reticle
            lineReticle.SetActive(true);
            lineReticle.GetComponent<LineReticle>().Init(-1, data.channelTime, transform.localScale.x, true);

            yield return AnimationTrackingSequence(data.channelTime, data.trackingLetGo);
            // use the same trick as the car, where it will keep going forward until
            // it is stunned, with that being controlled by a separate collision function
            // first zero everything out
            chargeStunned = false;

            // go forward until we cant 
            while (!chargeStunned)
            {
                yield return null;
                rb.MovePosition(rb.position + data.chargeSpeed * Time.deltaTime * transform.forward);
            }

            yield return new WaitForSeconds(data.recoveryTime);
        }

        // stop the projectiles
        StopCoroutine(concurrentCoroutine);

        // return back to the middle
        yield return LerpMid();
    }

    IEnumerator OmegaGMProjectiles(Omega_GM data)
    {
        // basically just get the player position and spawn projectiles forever
        while (true)
        {
            // copy paste from GM2
            // 1/6: generate a random position inside the circle centered around playerPosSnapshot
            // use polar coordinates, so generate a random angle and random distance
            float rAngle = Rand.Range(0, 360f) * Mathf.Deg2Rad;
            float rDistance = Mathf.Sqrt(Rand.value) * data.radiusAroundPlayer;

            // 2/6: convert polar to cartesian
            float xPos = rDistance * Mathf.Cos(rAngle) + player.position.x;
            float zPos = rDistance * Mathf.Sin(rAngle) + player.position.z;
            Vector3 projPos = new(xPos, data.projectileHeight, zPos);

            // 3/6: calculate the speed the projectile needs to travel to hit the ground in the specified amount of time
            float velocity = data.projectileHeight / data.shotTravelTime;

            // 4/5: fire the projectile from a height such that it takes some time to fall
            GameObject reference = Instantiate(data.projectilePrefab, projPos, Quaternion.Euler(-90, 0, 0));
            reference.GetComponent<FinalBossProj>().Init(Vector3.down, velocity, data.shotTravelTime, data.shotDamage, playerLayer, levelLayer);
            reference.transform.localScale = Vector3.one * data.projectileScale;

            // 5/6: instantiate a retical below the projectile we just instantiated
            SphereReticle sr = Instantiate(sphereReticle, new Vector3(projPos.x, 0.05f, projPos.z), Quaternion.identity).GetComponent<SphereReticle>();
            sr.Init(data.shotTravelTime, data.projectileScale);

            // 6/6: wait
            yield return new WaitForSeconds(data.shotDelay);
        }
    }

    IEnumerator OmegaGR(Omega_GR data)
    {
        // fire GR2 5 times, then fire the big beam which has actually 8 of them
        // GR2 copied code
        GameObject collider = Instantiate(data.projectilePrefab, transform);
        collider.GetComponent<FB_Hitbox>().Init(data.burnLaserDamage, data.burnLaserWidth, data.burnLaserLength, playerLayer, renderDebugColliders);
        collider.SetActive(false);
        for (int i = 0; i < data.attackCount; i++)
        {
            // set the reticle
            lineReticle.SetActive(true);
            lineReticle.GetComponent<LineReticle>().Init(data.burnLaserLength, data.burnChannelTime, data.burnLaserWidth);

            // track the player until the let go period
            yield return AnimationTrackingSequence(data.burnChannelTime, data.trackingLetGo);

            // fire the projectile
            collider.SetActive(true);

            // leave the burning area behind
            GameObject burn = Instantiate(data.burnArea, collider.transform.position, collider.transform.rotation);
            burn.GetComponent<FB_BurnArea>().Init(data.burnDamage, 1f, playerLayer, data.burnLaserWidth, data.burnLaserLength, data.burnDuration);

            // wait
            yield return new WaitForSeconds(data.delayBetweenLasers);

            // deactivate the laser
            collider.SetActive(false);
        }
        Destroy(collider);

        // fire a star of 8 tracking beams
        // after channeling of course
        GameObject sr = Instantiate(sphereReticle, transform);
        sr.transform.localPosition = Vector3.down;
        sr.GetComponent<SphereReticle>().Init(data.starLaserChannel, data.starLaserLength * 2 / transform.localScale.x);

        yield return new WaitForSeconds(data.starLaserChannel);

        float degBetweenBeams = 360f / data.starLaserCount;
        for (int i = 0; i < data.starLaserCount; i++)
        {
            // math
            Quaternion rotation = Quaternion.Euler(0, i * degBetweenBeams, 0);
            Vector3 offset = rotation * Vector3.forward * data.starLaserLength / 2;

            GameObject laser = Instantiate(data.starLaserPrefab, transform.position + offset, rotation, transform);
            laser.GetComponent<FB_Hitbox>().Init(data.starLaserDamage, data.starLaserWidth, data.starLaserLength, playerLayer, renderDebugColliders);

            // so we don't have to destroy them later
            Destroy(laser, data.duration);
        }

        // then just call the rotate sequence and wait
        concurrentCoroutine = StartCoroutine(RotateSequence(transform.rotation, data.totalDegRotation, data.duration));
        yield return new WaitForSeconds(data.duration);

        // clean up
        if (concurrentCoroutine != null)
            StopCoroutine(concurrentCoroutine);
    }

    IEnumerator OmegaTM(Omega_TM data)
    {
        // stab 3 times, then sweep
        // spawn the collider here because we reuse it 3 times
        GameObject rc = Instantiate(data.stabHitbox, transform);
        rc.GetComponent<FB_Hitbox>().Init(data.stabDamage, data.stabWidth, data.stabLength, playerLayer, renderDebugColliders);
        rc.SetActive(false);

        for (int i = 0; i < data.stabTimes.Length; i++)
        {
            // turn to face the player
            transform.LookAt(SetY(player.position, transform.position.y));

            // copied from TM1
            lineReticle.SetActive(true);
            lineReticle.GetComponent<LineReticle>().Init(data.stabLength, data.stabTimes[i].windup, data.stabWidth);

            // wait
            yield return new WaitForSeconds(data.stabTimes[i].windup);

            // then spawn the collider (or rather re enable it)
            rc.SetActive(true);

            // recovery time
            yield return new WaitForSeconds(data.stabTimes[i].recovery);
            rc.SetActive(false);
        }
        Destroy(rc);

        // sweep
        GameObject sc = Instantiate(data.sweepHitbox, transform);
        sc.GetComponent<FB_Hitbox>().Init(data.sweepDamage, 2 * data.sweepRadius, 2 * data.sweepRadius, playerLayer, renderDebugColliders);
        sc.SetActive(false);
        for (int i = 0; i < data.sweepTimes.Length; i++) // for loop for one iteration bruh
        {
            // turn to face the player
            transform.LookAt(SetY(player.position, transform.position.y));

            // copied from TM2
            GameObject sr = Instantiate(sphereReticle, transform);
            sr.GetComponent<SphereReticle>().Init(data.sweepTimes[i].windup, data.sweepRadius / transform.localScale.x);

            // wait
            yield return new WaitForSeconds(data.sweepTimes[i].windup);

            // then spawn the collider
            sc.SetActive(true);

            // recovery time
            yield return new WaitForSeconds(data.sweepTimes[i].recovery);
            sc.SetActive(false);
        }
        Destroy(sc);
    }

    IEnumerator OmegaTR(Omega_TR data)
    {
        // fire split shots all around
        // copied from trishula R2
        // shoot shots that split into smaller shots (configured in the FB_SplitProj class)
        float totalDuration = data.projectileCount * data.shotDelay;
        Quaternion startRotation = transform.rotation * Quaternion.Euler(0, -data.totalDegRotation / 2f % 180, 0);

        // pick a random starting pattern
        FB_SplitProj.SplitPattern pattern = Rand.value < 0.5f ? FB_SplitProj.SplitPattern.Cross : FB_SplitProj.SplitPattern.X;

        // rotation is controlled by RotateSequence
        concurrentCoroutine = StartCoroutine(RotateSequence(startRotation, data.totalDegRotation, totalDuration));
        yield return null;
        for (int i = 0; i < data.projectileCount; i++)
        {
            // 1/3: shoot
            GameObject reference = Instantiate(data.projectilePrefab, firePoint.position, Quaternion.identity);
            reference.transform.localScale = Vector3.one * data.projectileScale;
            reference.GetComponent<FB_SplitProj>().Init(transform.forward, data.projSpeed, data.damage, data.projSplitDistance,
                data.splitCount, data.splitProjLifetime, data.splitProjScale, data.splitProjDamage, data.splitProjSpeed,
                    playerLayer, levelLayer, pattern, player);

            // 2/3: change the split pattern to alternate
            pattern = pattern == FB_SplitProj.SplitPattern.Cross ? FB_SplitProj.SplitPattern.X : FB_SplitProj.SplitPattern.Cross;

            // 3/3: wait for delay seconds
            yield return new WaitForSeconds(data.shotDelay);
        }
    }

    IEnumerator Omega1(OMEGA_1 data)
    {
        yield return null;
    }

    IEnumerator Omega2(OMEGA_2 data)
    {
        yield return null;
    }

    IEnumerator Omega3(OMEGA_3 data)
    {
        yield return null;
    }
    #endregion

    #region Other Enemy Functions
    protected override void DeathState()
    {
        base.DeathState();

        StopCoroutine(concurrentCoroutine);
        StopCoroutine(currentPhaseCoroutine);
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
                // we should probably have something more special for when the final boss dies but whatever

                ImpulseSource.GenerateImpulseWithForce(DeathScreenshakeForce);
                StartCoroutine(nameof(DeathHitstop));
                //Boom plays INSTEAD of hitEffect. Once we have a VFX for boom instead of UI, use .Play instead of coroutine. 
                StartCoroutine(nameof(ShowBoom));
            }
            else
            {
                // initiate revive sequence for phase 2
                StopCoroutine(currentPhaseCoroutine);
                currentPhaseCoroutine = StartCoroutine(BentleyPhase2());
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
        // use this for any melee damage
        // the lance charge attack
        if (P1_currentAttack == P1_Attacks.Gungnir_M1)
        {
            int otherLayer = other.gameObject.layer;

            if (otherLayer == playerLayer)
            {
                chargeStunned = true;
                Gungnir_M1 data = (Gungnir_M1)P1_attackDatas[(int)P1_Attacks.Gungnir_M1];
                other.GetComponent<PlayerHealth>().TakeDamage(data.damage);
            }
            if (otherLayer == levelLayer)
            {
                chargeStunned = true;
            }
        }

        // samus final smash attack
        else if (P1_currentAttack == P1_Attacks.Gungnir_M2)
        {
            int otherLayer = other.gameObject.layer;

            if (otherLayer == playerLayer)
            {
                Gungnir_M2 data = (Gungnir_M2)P1_attackDatas[(int)P1_Attacks.Gungnir_M2];
                other.GetComponent<PlayerHealth>().TakeDamage(data.damage);
            }
        }

        // phase 2 lance charge attack
        else if (P2_currentAttack == P2_Attacks.Omega_GM)
        {
            int otherLayer = other.gameObject.layer;

            if (otherLayer == playerLayer)
            {
                chargeStunned = true;
                Omega_GM data = (Omega_GM)P2_attackDatas[(int)P2_Attacks.Omega_GM];
                other.GetComponent<PlayerHealth>().TakeDamage(data.chargeDamage);
            }
            if (otherLayer == levelLayer)
            {
                chargeStunned = true;
            }
        }
    }
    #endregion
}