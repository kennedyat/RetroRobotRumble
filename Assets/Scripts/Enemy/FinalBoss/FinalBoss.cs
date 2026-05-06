using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Rand = UnityEngine.Random;
using DG.Tweening;

public class FinalBoss : Enemy
{
    #region Attack Variables
    // the melees are all even, the ranges are all odd, gungnir is first 4, trishula is last 4
    public enum P1_AttackType { Gungnir_M1 = 0, Gungnir_R1, Gungnir_M2, Gungnir_R2, Trishula_M1, Trishula_R1, Trishula_M2, Trishula_R2, NONE }
    public enum P2_AttackType { Omega_GM = 0, Omega_GR, Omega_TM, Omega_TR, OMEGA1, OMEGA2, PHASE2ONLY, NONE }

    // for phase 1, given an attack, return its complement, used in reshuffling the queue
    Dictionary<P1_AttackType, P1_AttackType> typeToComplement = new()
    {
        {P1_AttackType.Gungnir_M1, P1_AttackType.Trishula_M2},
        {P1_AttackType.Gungnir_R1, P1_AttackType.Trishula_R2},
        {P1_AttackType.Gungnir_M2, P1_AttackType.Trishula_M1},
        {P1_AttackType.Gungnir_R2, P1_AttackType.Trishula_R1},
        {P1_AttackType.Trishula_M1, P1_AttackType.Gungnir_M2},
        {P1_AttackType.Trishula_R1, P1_AttackType.Gungnir_R2},
        {P1_AttackType.Trishula_M2, P1_AttackType.Gungnir_M1},
        {P1_AttackType.Trishula_R2, P1_AttackType.Gungnir_R1},
    };

    [Header("Attacks")]
    [SerializeField, Tooltip("DO NOT CHANGE THE ORDER OR ANY REFERENCES HERE, YOU CAN MODIFY THE SCRIPTABLE OBJECTS BUT NOT THEIR ORDER HERE")]
    FB_P1AttackData[] P1_attackDatas = new FB_P1AttackData[8];
    [SerializeField, Tooltip("DO NOT CHANGE THE ORDER OR ANY REFERENCES HERE, YOU CAN MODIFY THE SCRIPTABLE OBJECTS BUT NOT THEIR ORDER HERE")]
    FB_P2AttackData[] P2_attackDatas = new FB_P2AttackData[6];
    [SerializeField, Tooltip("Used for GM1 to lerp back to the middle")]
    FB_LerpMid fB_LerpMid;
    P1_AttackType P1_currentAttack;
    P2_AttackType P2_currentAttack;
    Queue<P1_AttackType> P1_attackQueue = new();
    Queue<P2_AttackType> P2_attackQueue = new();

    bool isAttacking = false;
    public bool isPhase2 = false;
    bool collisionTrigger = false;

    Coroutine concurrentCoroutine;
    List<GameObject> O2_sectors;
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
    [SerializeField, Tooltip("How long it takes to recover from a dash")]
    float dashRecovery = 0.2f;
    [SerializeField, Tooltip("The amount of damage dealt by the fire which Bentley leaves behind when dashing")]
    int dashFireDamage = 5;

    [Header("References")]
    [SerializeField, Tooltip("A prefab that is instantiated on top of the player, telling Bentley if his attacks hit the player")]
    GameObject FB_playerCollider;
    // after we instantiate a collider, use this one for the reference
    FB_PlayerCollider playerCollider;
    [SerializeField] RectTransform roundInfoText;

    [SerializeField, Tooltip("Where projectiles are instantiated")]
    Transform firePoint;
    [SerializeField, Tooltip("The main directional light, turned off for OMEGA 1's darkness shroud")]
    GameObject O1_light;

    [Header("Debug")]
    [SerializeField, Tooltip("Use this to force what Bentley's attack will be, to debug. \nNONE = no forced attack")]
    P1_AttackType forceAttackP1 = P1_AttackType.NONE;
    [SerializeField, Tooltip("Use this to force Bentley to enter phase 2 and start only using this attack. \nPHASE2ONLY = skips to phase 2, but no forced attack. \nNONE = phase 1, no forced attack. ")]
    P2_AttackType forceAttackP2 = P2_AttackType.NONE;
    [SerializeField, Tooltip("Whether or not to render colliders used in several melee and ranged abilities")]
    bool renderColliders = true;
    [SerializeField, Tooltip("Whether or not to have reticles expand outward. False = reticles will be static lines/circles")]
    bool expandReticles = true;
    [SerializeField, Tooltip("Skip the phase transition cutscene")]
    bool skipPhaseTransition = false;
    [SerializeField, Tooltip("Whether or not to log the attack queue when it is initialized and reshuffled")]
    bool logQueueOrders = false;
    #endregion

    #region Unity Functions
    protected override void Start()
    {
        base.Start();

        // spawn the player collider
        playerCollider = Instantiate(FB_playerCollider, player).GetComponent<FB_PlayerCollider>();
        playerCollider.transform.localPosition = Vector3.zero;

        maxHealth = health;

        if (forceAttackP2 == P2_AttackType.NONE)
        {
            attackCoroutine = StartCoroutine(BentleyPhase1());
        }
        else
        {
            attackCoroutine = StartCoroutine(BentleyPhase2());
        }
    }

    protected void Update()
    {
        // if he's not attacking, face the player
        if (!isAttacking)
        {
            FacePlayer();
        }
    }
    #endregion

    #region P1 Attack Logic
    void FillQueueP1()
    {
        /* rules: 
        * no same attack range (melee, range) back to back
        * no same arm (gungnir, trishula) back to back
        * 1 or 2, pick random for first, then pick the other one
        * the queue will only ever have 4 elements in it until the player is damaged
        */
        // assume the queue is EMPTY, so clear it to be safe
        P1_attackQueue.Clear();
        HashSet<P1_AttackType> P1_attackSet = new();

        // add a random element to the queue
        int random = Rand.Range(0, 8);
        P1_AttackType lastElement = (P1_AttackType)random;
        P1_attackQueue.Enqueue(lastElement);
        P1_attackSet.Add(lastElement);

        // depending on this first element, add the second, third, and fourth
        for (int i = 0; i < 3; i++)
        {
            P1_AttackType nextElement;

            // which arm?
            if ((int)lastElement <= 3)
            {
                // gungnir, so we need to add trishula
                // last one was ranged or melee?
                if ((int)lastElement % 2 == 0)
                {
                    // melee, so we need to add ranged
                    // is 1 or 2 already there?
                    if (P1_attackSet.Contains(P1_AttackType.Trishula_R1))
                    {
                        nextElement = P1_AttackType.Trishula_R2;
                    }
                    else if (P1_attackSet.Contains(P1_AttackType.Trishula_R2))
                    {
                        nextElement = P1_AttackType.Trishula_R1;
                    }
                    else
                    {
                        // neither are there so pick a random one
                        nextElement = Rand.value > 0.5f ? P1_AttackType.Trishula_R1 : P1_AttackType.Trishula_R2;
                    }
                }
                else
                {
                    // ranged, so we need to add melee
                    // is 1 or 2 already there?
                    if (P1_attackSet.Contains(P1_AttackType.Trishula_M1))
                    {
                        nextElement = P1_AttackType.Trishula_M2;
                    }
                    else if (P1_attackSet.Contains(P1_AttackType.Trishula_M2))
                    {
                        nextElement = P1_AttackType.Trishula_M1;
                    }
                    else
                    {
                        // neither are there so pick a random one
                        nextElement = Rand.value > 0.5f ? P1_AttackType.Trishula_M1 : P1_AttackType.Trishula_M2;
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
                    if (P1_attackSet.Contains(P1_AttackType.Gungnir_R1))
                    {
                        nextElement = P1_AttackType.Gungnir_R2;
                    }
                    else if (P1_attackSet.Contains(P1_AttackType.Gungnir_R2))
                    {
                        nextElement = P1_AttackType.Gungnir_R1;
                    }
                    else
                    {
                        // neither are there so pick a random one
                        nextElement = Rand.value > 0.5f ? P1_AttackType.Gungnir_R1 : P1_AttackType.Gungnir_R2;
                    }
                }
                else
                {
                    // ranged, so we need to add melee
                    // is 1 or 2 already there?
                    if (P1_attackSet.Contains(P1_AttackType.Gungnir_M1))
                    {
                        nextElement = P1_AttackType.Gungnir_M2;
                    }
                    else if (P1_attackSet.Contains(P1_AttackType.Gungnir_M2))
                    {
                        nextElement = P1_AttackType.Gungnir_M1;
                    }
                    else
                    {
                        // neither are there so pick a random one
                        nextElement = Rand.value > 0.5f ? P1_AttackType.Gungnir_M1 : P1_AttackType.Gungnir_M2;
                    }
                }
            }

            // update queue, set, and last element
            P1_attackSet.Add(nextElement);
            P1_attackQueue.Enqueue(nextElement);
            lastElement = nextElement;
        }

        if (logQueueOrders)
            DebugPrintQueueP1();
    }

    void ShuffleQueueP1()
    {
        // take the first element in the queue, and add its opposite arm and opposite number
        // do that 4 times

        for (int i = 0; i < 4; i++)
        {
            P1_AttackType t = P1_attackQueue.Dequeue();

            P1_attackQueue.Enqueue(typeToComplement[t]);
        }

        if (logQueueOrders)
            DebugPrintQueueP1();
    }

    void DebugPrintQueueP1()
    {
        Debug.Assert(P1_attackQueue.Count == 4);
        string message = "";
        for (int i = 0; i < 4; i++)
        {
            P1_AttackType t = P1_attackQueue.Dequeue();
            message += t.ToString() + " ";
            P1_attackQueue.Enqueue(t);
        }
        Debug.Log("New Queue Order: " + message);
    }

    float EnumToAttackRange(P1_AttackType type)
    {
        return P1_attackDatas[(int)type].attackRange;
    }

    float EnumToCloseRange(P1_AttackType type)
    {
        return P1_attackDatas[(int)type].tooCloseRange;
    }
    #endregion

    #region P1 Attacks
    IEnumerator BentleyPhase1()
    {
        FillQueueP1();

        while (!isPhase2)
        {
            // 1/6: pick the attack
            if (forceAttackP1 == P1_AttackType.NONE)
            {
                P1_currentAttack = P1_attackQueue.Dequeue();
                P1_attackQueue.Enqueue(P1_currentAttack);
            }
            else
            {
                P1_currentAttack = forceAttackP1;
            }

            // 2/6: get into range for the attack by dashing around
            yield return DashLogic(EnumToAttackRange(P1_currentAttack), EnumToCloseRange(P1_currentAttack));

            transform.LookAt(SetY(player.position, transform.position.y));
            yield return null;

            // 3/6: execute that attack
            isAttacking = true;
            yield return EnumToAttack(P1_currentAttack);
            isAttacking = false;

            // 4/6: check for feedback, did we hit, cuz if we did the attack sequence needs to change
            if (playerCollider.playerTookDamage)
            {
                ShuffleQueueP1();
                playerCollider.playerTookDamage = false;
            }

            // 5/6: wait the wait period
            yield return new WaitForSeconds(waitTime);

            // 6/6: repeat
        }
    }

    IEnumerator EnumToAttack(P1_AttackType t)
    {
        FB_P1AttackData data = P1_attackDatas[(int)t];
        BarkManager.Instance?.PlayBark(GetP1BarkTrigger(t), "Final Boss");

        switch (t)
        {
            case P1_AttackType.Gungnir_M1:
                yield return GungnirM1((Gungnir_M1)data);
                break;
            case P1_AttackType.Gungnir_R1:
                yield return GungnirR1((Gungnir_R1)data);
                break;
            case P1_AttackType.Gungnir_M2:
                yield return GungnirM2((Gungnir_M2)data);
                break;
            case P1_AttackType.Gungnir_R2:
                yield return GungnirR2((Gungnir_R2)data);
                break;
            case P1_AttackType.Trishula_M1:
                yield return TrishulaM1((Trishula_M1)data);
                break;
            case P1_AttackType.Trishula_R1:
                yield return TrishulaR1((Trishula_R1)data);
                break;
            case P1_AttackType.Trishula_M2:
                yield return TrishulaM2((Trishula_M2)data);
                break;
            case P1_AttackType.Trishula_R2:
                yield return TrishulaR2((Trishula_R2)data);
                break;
        }
    }

    protected override IEnumerator AnimationTrackingSequence(float channelTime, float trackingLetGo, bool facePlayer = true, Animation animation = null)
    {
        float t = 0;

        // animation.play or whatever it is, but do it here to only call it once
        // for now temporary text change
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

            float angle = Mathf.Lerp(0f, totalDegrees * direction, t);

            transform.rotation = start * Quaternion.Euler(0, angle, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // snap to the end
        transform.rotation = start * Quaternion.Euler(0, totalDegrees * direction, 0);
    }

    IEnumerator GungnirR1(Gungnir_R1 data)
    {
        // big tracking laser
        // spawn the reticle
        GameObject lr = Instantiate(lineReticle, transform);
        lr.GetComponent<LineReticle>().Init(data.laserRange, data.channelTime, data.laserWidth, true, expandReticles);

        // then channel and track
        yield return AnimationTrackingSequence(data.channelTime, data.trackingLetGo);

        // instantiate a laser hitbox
        GameObject reference = Instantiate(data.projectilePrefab, transform);
        reference.GetComponent<FB_DotHitbox>().Init(data.damage, data.damageTickRate, data.laserWidth, data.laserRange, playerLayer, renderColliders);

        float t = 0;
        while (t < data.duration)
        {
            // bentley tracks the player, rotating his laser at a certain speed to try to catch up to the player
            Vector3 toPlayer = SetY(player.position - transform.position, 0);

            if (toPlayer.sqrMagnitude >= 0.001f)
            {
                // get the angle that we need to rotate, and depending on angular distance, we will rotate a bit faster
                Quaternion playerRotation = Quaternion.LookRotation(toPlayer);
                float angle = Quaternion.Angle(playerRotation, transform.rotation);

                // for every degree that we need to rotate, rotate a bit (5%) faster exponentially
                float factor = 1f + data.rotationSpeedFactor / 100;
                float rSpeed = Mathf.Pow(factor, (int)angle);

                // don't let it go too slow, so clamp with the rotation speed base
                rSpeed = Mathf.Max(rSpeed, data.rotationSpeedBase);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, playerRotation, rSpeed * Time.deltaTime);
            }

            t += Time.deltaTime;
            yield return null;
        }

        Destroy(reference);
    }

    IEnumerator GungnirR2(Gungnir_R2 data)
    {
        // instantly shoot the player and burn the ground
        // instantiate a collider in advance and use the same one for all attacks
        GameObject collider = Instantiate(data.projectilePrefab, transform);
        collider.GetComponent<FB_Hitbox>().Init(data.damage, data.laserWidth, data.laserRange, playerLayer, renderColliders);
        collider.SetActive(false);
        for (int i = 0; i < data.attackCount; i++)
        {
            // set the reticle
            GameObject lr = Instantiate(lineReticle, transform);
            lr.GetComponent<LineReticle>().Init(data.laserRange, data.channelTime, data.laserWidth, true, expandReticles);

            // track the player until the let go period
            yield return AnimationTrackingSequence(data.channelTime, data.trackingLetGo);

            // fire the projectile
            collider.SetActive(true);

            // leave the burning area behind
            GameObject burn = Instantiate(data.burnArea, collider.transform.position, collider.transform.rotation);
            burn.GetComponent<FB_BurnArea>().Init(data.burnDamage, data.burnCooldown, playerLayer, data.laserWidth, data.laserRange, data.burnDuration);

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
            GameObject lr = Instantiate(lineReticle, transform);
            lr.GetComponent<LineReticle>().Init(-1, data.channelTime, transform.localScale.x, true, expandReticles);

            yield return AnimationTrackingSequence(data.channelTime, data.trackingLetGo);

            // use the same trick as the car, where it will keep going forward until
            // it is stunned, with that being controlled by a separate collision function
            // first zero everything out
            collisionTrigger = false;

            // go forward until we cant 
            while (!collisionTrigger)
            {
                rb.MovePosition(rb.position + data.chargeSpeed * Time.deltaTime * transform.forward);
                yield return null;
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
            reference.GetComponent<FB_Proj>().Init(Vector3.down, velocity, data.shotTravelTime, data.damage, playerLayer, levelLayer);
            reference.transform.localScale = Vector3.one * data.projectileScale;

            // 5/6: instantiate a reticle below the projectile we just instantiated
            SphereReticle sr = Instantiate(sphereReticle, SetY(projPos, 0), Quaternion.identity).GetComponent<SphereReticle>();
            sr.Init(data.shotTravelTime, data.projectileScale / 2, expandReticles);

            // 6/6: wait
            yield return new WaitForSeconds(shotDelay);
        }

        // set the collider scale to be a bit wider
        float origScale = ((BoxCollider)col).size.x;
        ((BoxCollider)col).size = new Vector3(origScale * data.crashScale, ((BoxCollider)col).size.y, origScale * data.crashScale);

        // set the reticle for crash channel
        SphereReticle crashIndicator = Instantiate(sphereReticle, new Vector3(transform.position.x, 0.05f, transform.position.z), Quaternion.identity).GetComponent<SphereReticle>();
        crashIndicator.Init(data.crashChannel, transform.localScale.x * data.crashScale, expandReticles);

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
        ((BoxCollider)col).size = new Vector3(origScale, ((BoxCollider)col).size.y, origScale);
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
                reference.GetComponent<FB_Proj>().Init(transform.forward, data.projectileSpeed, data.projLifetime, data.damage, playerLayer, levelLayer);

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
        GameObject lr = Instantiate(lineReticle, transform);
        lr.GetComponent<LineReticle>().Init(data.stabLength, data.channelTime, data.stabWidth, true, expandReticles);

        // wait
        yield return new WaitForSeconds(data.channelTime);

        // then spawn the collider
        GameObject rc = Instantiate(data.projectilePrefab, transform);
        rc.GetComponent<FB_Hitbox>().Init(data.damage, data.stabWidth, data.stabLength, playerLayer, renderColliders);

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
        sr.GetComponent<SphereReticle>().Init(data.channelTime, data.sweepRadius / transform.localScale.x, expandReticles);

        // wait
        yield return new WaitForSeconds(data.channelTime);

        // then spawn the collider
        GameObject sc = Instantiate(data.projectilePrefab, transform);
        sc.GetComponent<FB_Hitbox>().Init(data.damage, 2 * data.sweepRadius, 2 * data.sweepRadius, playerLayer, renderColliders);

        // recovery time
        yield return new WaitForSeconds(data.recoveryTime);

        Destroy(sc);
    }

    IEnumerator LerpMid(float time = -1)
    {
        P1_currentAttack = P1_AttackType.NONE;
        P2_currentAttack = P2_AttackType.NONE;
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
        P1_currentAttack = P1_AttackType.NONE;

        // delete any projectiles
        GameObject[] delete = GameObject.FindGameObjectsWithTag("FB_DestroyOnPhase2");
        for (int i = 0; i < delete.Length; i++)
        {
            Destroy(delete[i]);
        }

        // reset health to max and other variables
        health = 999999999; // to make him invulnerable (sure)
        isPhase2 = true;
    }

    IEnumerator BentleyPhase2()
    {
        Debug.Log("Bentley: phase 2 starting");
        BarkManager.Instance?.PlayBark("Start Phase 2", "Final Boss");
        StartCoroutine(UpdateRoundInfoText());
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
        FillQueueP2();

        // for omega attacks, always start with OMEGA1, then alternate
        int normalAttackCount = 0;
        Queue<P2_AttackType> omegaQueue = new();
        omegaQueue.Enqueue(P2_AttackType.OMEGA1);
        omegaQueue.Enqueue(P2_AttackType.OMEGA2);

        while (health > 0)
        {
            // pick the next attack depending on how many normal attacks there were
            if (forceAttackP2 == P2_AttackType.NONE || forceAttackP2 == P2_AttackType.PHASE2ONLY)
            {
                if (normalAttackCount == 2)
                {
                    // omega attack, pick the next one
                    P2_currentAttack = omegaQueue.Dequeue();
                    omegaQueue.Enqueue(P2_currentAttack);

                    // reset the counter
                    normalAttackCount = 0;
                }
                else
                {
                    // pick a regular attack from the queue
                    P2_currentAttack = P2_attackQueue.Dequeue();
                    P2_attackQueue.Enqueue(P2_currentAttack);
                    normalAttackCount++;
                }
            }
            else
            {
                // take the forced attack
                P2_currentAttack = forceAttackP2;
            }

            // then get in range for that attack
            yield return DashLogic(EnumToAttackRange(P2_currentAttack), EnumToCloseRange(P2_currentAttack));

            // execute that attack
            isAttacking = true;
            yield return EnumToAttack(P2_currentAttack);
            isAttacking = false;

            // check for a hit, and if we do, shuffle
            // but ONLY for non OMEGA attacks
            if (playerCollider.playerTookDamage && (int)P2_currentAttack <= 3)
            {
                ShuffleQueueP2();
                playerCollider.playerTookDamage = false;
            }

            // wait some time
            yield return new WaitForSeconds(waitTime * waitTimeMultiplier);

            // repeat
        }
    }

    void FillQueueP2()
    {
        // pick a random first attack
        int random = Rand.Range(0, 4);
        P2_AttackType firstAttack = (P2_AttackType)random;

        // the complement to this first random one is 3 minus random
        // so GM gives us TR, TM gives us GR, etc.
        int complement = 3 - random;
        P2_AttackType secondAttack = (P2_AttackType)complement;

        // add them to the queue
        P2_attackQueue.Enqueue(firstAttack);
        P2_attackQueue.Enqueue(secondAttack);

        // debug
        if (logQueueOrders)
            DebugPrintQueueP2();
    }

    void ShuffleQueueP2()
    {
        // get one of the attacks (it doesn't matter which)
        P2_AttackType top = P2_attackQueue.Dequeue();
        P2_attackQueue.Dequeue(); // clear the second element as well

        // check if the number is divisible by 3 (0 and 3)
        if ((int)top % 3 == 0)
        {
            // just did GM and TR, so now do GR and TM
            int random = Rand.value < 0.5f ? 1 : 2;
            P2_attackQueue.Enqueue((P2_AttackType)random);
            P2_attackQueue.Enqueue((P2_AttackType)(3 - random));
        }
        else
        {
            // just did GR and TM, so now do GM and TR
            int random = Rand.value < 0.5f ? 0 : 3;
            P2_attackQueue.Enqueue((P2_AttackType)random);
            P2_attackQueue.Enqueue((P2_AttackType)(3 - random));
        }

        // debug
        if (logQueueOrders)
            DebugPrintQueueP2();
    }

    void DebugPrintQueueP2()
    {
        Debug.Assert(P2_attackQueue.Count == 2);
        string message = "";
        for (int i = 0; i < 2; i++)
        {
            P2_AttackType t = P2_attackQueue.Dequeue();
            message += t.ToString() + " ";
            P2_attackQueue.Enqueue(t);
        }
        Debug.Log("New Queue Order: " + message);
    }

    float EnumToAttackRange(P2_AttackType t)
    {
        return P2_attackDatas[(int)t].attackRange;
    }

    float EnumToCloseRange(P2_AttackType t)
    {
        return P2_attackDatas[(int)t].tooCloseRange;
    }

    IEnumerator EnumToAttack(P2_AttackType t)
    {
        FB_P2AttackData data = P2_attackDatas[(int)t];
        BarkManager.Instance?.PlayBark(GetP2BarkTrigger(t), "Final Boss");

        switch (t)
        {
            case P2_AttackType.Omega_GM:
                yield return OmegaGM((Omega_GM)data);
                break;

            case P2_AttackType.Omega_GR:
                yield return OmegaGR((Omega_GR)data);
                break;

            case P2_AttackType.Omega_TM:
                yield return OmegaTM((Omega_TM)data);
                break;

            case P2_AttackType.Omega_TR:
                yield return OmegaTR((Omega_TR)data);
                break;

            case P2_AttackType.OMEGA1:
                yield return Omega1((OMEGA_1)data);
                break;

            case P2_AttackType.OMEGA2:
                yield return Omega2((OMEGA_2)data);
                break;
        }
    }

    string GetP1BarkTrigger(P1_AttackType attackType)
    {
        switch (attackType)
        {
            case P1_AttackType.Gungnir_M1:
                return "Lance Straight Charge";
            case P1_AttackType.Gungnir_R1:
                return "Lance Tracking Laser";
            case P1_AttackType.Gungnir_M2:
                return "Lance Crash Down";
            case P1_AttackType.Gungnir_R2:
                return "Lance Big Laser";
            case P1_AttackType.Trishula_M1:
                return "Shotgun Fire";
            case P1_AttackType.Trishula_R1:
                return "Shotgun Panic";
            case P1_AttackType.Trishula_M2:
                return "Trident Stab";
            case P1_AttackType.Trishula_R2:
                return "Trident Sweep";
            default:
                return "";
        }
    }

    string GetP2BarkTrigger(P2_AttackType attackType)
    {
        switch (attackType)
        {
            case P2_AttackType.Omega_GM:
                return "Lance Omega Melee";
            case P2_AttackType.Omega_GR:
                return "Lance Omega Ranged";
            case P2_AttackType.Omega_TM:
                return "Trident Omega";
            case P2_AttackType.Omega_TR:
                return "Shotgun Omega";
            case P2_AttackType.OMEGA1:
                return "Omega 1";
            case P2_AttackType.OMEGA2:
                return "Omega 2";
            default:
                return "";
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
            GameObject lr = Instantiate(lineReticle, transform);
            lr.GetComponent<LineReticle>().Init(-1, data.channelTime, transform.localScale.x, true, expandReticles);

            yield return AnimationTrackingSequence(data.channelTime, data.trackingLetGo);
            // use the same trick as the car, where it will keep going forward until
            // it is stunned, with that being controlled by a separate collision function
            // first zero everything out
            collisionTrigger = false;

            // go forward until we cant 
            while (!collisionTrigger)
            {
                rb.MovePosition(rb.position + data.chargeSpeed * Time.deltaTime * transform.forward);
                yield return null;
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
            reference.GetComponent<FB_Proj>().Init(Vector3.down, velocity, data.shotTravelTime, data.shotDamage, playerLayer, levelLayer);
            reference.transform.localScale = Vector3.one * data.projectileScale;

            // 5/6: instantiate a retical below the projectile we just instantiated
            SphereReticle sr = Instantiate(sphereReticle, SetY(projPos, 0), Quaternion.identity).GetComponent<SphereReticle>();
            sr.Init(data.shotTravelTime, data.projectileScale / 2, expandReticles);

            // 6/6: wait
            yield return new WaitForSeconds(data.shotDelay);
        }
    }

    IEnumerator OmegaGR(Omega_GR data)
    {
        // fire GR2 5 times, then fire the big beam which has actually 8 of them
        // GR2 copied code
        GameObject collider = Instantiate(data.projectilePrefab, transform);
        collider.GetComponent<FB_Hitbox>().Init(data.burnLaserDamage, data.burnLaserWidth, data.burnLaserLength, playerLayer, renderColliders);
        collider.SetActive(false);
        for (int i = 0; i < data.attackCount; i++)
        {
            // set the reticle
            GameObject lr = Instantiate(lineReticle, transform);
            lr.GetComponent<LineReticle>().Init(data.burnLaserLength, data.burnChannelTime, data.burnLaserWidth, true, expandReticles);

            // track the player until the let go period
            yield return AnimationTrackingSequence(data.burnChannelTime, data.trackingLetGo);

            // fire the projectile
            collider.SetActive(true);

            // leave the burning area behind
            GameObject burn = Instantiate(data.burnArea, collider.transform.position, collider.transform.rotation);
            burn.GetComponent<FB_BurnArea>().Init(data.burnDamage, data.burnCooldown, playerLayer, data.burnLaserWidth, data.burnLaserLength, data.burnDuration);

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
        sr.GetComponent<SphereReticle>().Init(data.starLaserChannel, data.starLaserLength / transform.localScale.x, expandReticles);

        yield return new WaitForSeconds(data.starLaserChannel);

        float degBetweenBeams = 360f / data.starLaserCount;
        for (int i = 0; i < data.starLaserCount; i++)
        {
            // math
            Quaternion rotation = Quaternion.Euler(0, i * degBetweenBeams, 0);
            Vector3 offset = rotation * Vector3.forward * data.starLaserLength / 2;

            GameObject laser = Instantiate(data.starLaserPrefab, transform.position + offset, rotation, transform);
            laser.GetComponent<FB_Hitbox>().Init(data.starLaserDamage, data.starLaserWidth, data.starLaserLength, playerLayer, renderColliders);

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
        rc.GetComponent<FB_Hitbox>().Init(data.stabDamage, data.stabWidth, data.stabLength, playerLayer, renderColliders);
        rc.SetActive(false);

        for (int i = 0; i < data.stabTimes.Length; i++)
        {
            // turn to face the player
            transform.LookAt(SetY(player.position, transform.position.y));

            // copied from TM1
            GameObject lr = Instantiate(lineReticle, transform);
            lr.GetComponent<LineReticle>().Init(data.stabLength, data.stabTimes[i].windup, data.stabWidth, true, expandReticles);

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
        sc.GetComponent<FB_Hitbox>().Init(data.sweepDamage, 2 * data.sweepRadius, 2 * data.sweepRadius, playerLayer, renderColliders);
        sc.SetActive(false);
        for (int i = 0; i < data.sweepTimes.Length; i++) // for loop for one iteration bruh
        {
            // turn to face the player
            transform.LookAt(SetY(player.position, transform.position.y));

            // copied from TM2
            GameObject sr = Instantiate(sphereReticle, transform);
            sr.GetComponent<SphereReticle>().Init(data.sweepTimes[i].windup, data.sweepRadius / transform.localScale.x, expandReticles);

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
        // put a circle in a random area, and make everything else dark for some time
        float x = Rand.Range(data.xBounds.negative, data.xBounds.positive);
        float z = Rand.Range(data.zBounds.negative, data.zBounds.positive);
        Vector3 safePos = new(x, 0, z);
        SphereReticle sr = Instantiate(sphereReticle, safePos, Quaternion.identity).GetComponent<SphereReticle>();
        sr.Init(data.safetyTime, data.safeSpotRadius, expandReticles);

        // shroud the arena in darkness by disabling the directional light
        O1_light.SetActive(false);

        // instantiate the spotlight above the safe zone
        GameObject spot = Instantiate(data.projectilePrefab, new Vector3(x, 5, z), Quaternion.Euler(90, 0, 0));
        spot.GetComponent<FB_Spotlight>().Init(data.safeSpotRadius, data.safetyTime);

        // wait the time
        yield return new WaitForSeconds(data.safetyTime);

        // deal damage to the player depending on how close they are to the middle
        float t = 0;
        bool damagedPlayer = false;
        while (t < data.laserDuration)
        {
            if (Vector3.Distance(SetY(player.position, 0), SetY(safePos, 0)) > data.safeSpotRadius && !damagedPlayer)
            {
                player.GetComponent<PlayerHealth>().TakeDamage(data.damage);
                damagedPlayer = true;
            }

            t += Time.deltaTime;
            yield return null;
        }

        O1_light.SetActive(true);
    }

    IEnumerator Omega2(OMEGA_2 data)
    {
        // if the arena has not been partitioned, do it now
        if (O2_sectors == null)
        {
            O2_sectors = new();
            for (int i = 0; i < data.partitionCount; i++)
            {
                GameObject reference = Instantiate(data.projectilePrefab, Vector3.zero, Quaternion.Euler(0, i * 360 / data.partitionCount, 0));
                reference.transform.localScale = new Vector3(data.partitionRadius, 1, data.partitionRadius);
                O2_sectors.Add(reference);
            }
        }

        // the arena is pre-partitioned and all we have to do here is activate them randomly
        for (int i = 0; i < data.pattern.Count; i++)
        {
            // first make a copy of the list (cuz assignment operator sucks)
            List<GameObject> partitionCopy = new();
            foreach (GameObject g in O2_sectors)
            {
                g.SetActive(true);
                partitionCopy.Add(g);
            }

            // pick the safe partitions
            for (int j = 0; j < data.pattern[i]; j++)
            {
                // pick one of the partitions that are left to be SAFE
                int random = Rand.Range(0, partitionCopy.Count);
                GameObject p = partitionCopy[random];
                partitionCopy.Remove(p);

                // then highlight it for the player to see
                p.GetComponent<FB_Sector>().Init(true);
            }

            // for the rest of the sectors, highlight them as NOT SAFE
            foreach (GameObject g in partitionCopy)
            {
                g.GetComponent<FB_Sector>().Init(false);
            }

            // then wait
            yield return new WaitForSeconds(data.explosionDelay);

            // explode all the stuff, which is done inside the partition

            // wait a bit so its not spammy
            yield return new WaitForSeconds(data.recoveryTime);

            // the partitions deactivate themselves in recoveryTime / 2 seconds
        }
    }
    #endregion

    #region Other Enemy Functions
    protected override void DeathState()
    {
        base.DeathState();

        StopCoroutine(concurrentCoroutine);
        StopCoroutine(attackCoroutine);
    }

    IEnumerator DashLogic(float attackRange, float tooCloseRange)
    {
        int forwardDashCount = Rand.Range(0, 2);

        // dash until we are within range
        float distToPlayer = Vector3.Distance(SetY(transform.position, 0), SetY(player.position, 0));
        while (distToPlayer < tooCloseRange || attackRange < distToPlayer)
        {
            Vector3 toPlayer = (player.position - transform.position).normalized;
            Vector3 dashTarget;

            // too close?
            if (distToPlayer < tooCloseRange)
            {
                dashTarget = transform.position - toPlayer * dashDistance;
            }
            // too far
            else
            {
                if (forwardDashCount == 2)
                {
                    // tangent dash
                    Vector3 tangent = Vector3.Cross(toPlayer, Vector3.up).normalized;
                    if (Rand.value < 0.5f)
                        tangent = -tangent;
                    dashTarget = transform.position + tangent * dashDistance;

                    forwardDashCount = 0;
                }
                else
                {
                    // forward dash
                    dashTarget = transform.position + toPlayer * dashDistance;

                    forwardDashCount++;
                }
            }

            // dash
            yield return DashSequence(dashTarget);

            // recovery period
            yield return new WaitForSeconds(dashRecovery);

            distToPlayer = Vector3.Distance(SetY(transform.position, 0), SetY(player.position, 0));
        }
        yield return null;
    }

    IEnumerator DashSequence(Vector3 target)
    {
        // pre dash configuration
        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
        Vector3 dir = (target - rb.position).normalized;
        Vector3 velocityVector = dir * (dashDistance / dashDuration);

        // set velocity every frame
        float t = 0;
        while (t < dashDuration / 2)
        {
            rb.velocity = velocityVector;
            t += Time.deltaTime;
            yield return null;
        }

        // fire area!
        Gungnir_R2 data = (Gungnir_R2)P1_attackDatas[(int)P1_AttackType.Gungnir_R2];
        GameObject fireArea = Instantiate(data.burnArea, SetY(transform.position, 0), Quaternion.LookRotation(rb.velocity));
        fireArea.GetComponent<FB_BurnArea>().Init(dashFireDamage, 1, playerLayer, transform.localScale.x, dashDistance, 5);

        t = 0;
        while (t < dashDuration / 2)
        {
            rb.velocity = velocityVector;
            t += Time.deltaTime;
            yield return null;
        }

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
    }

    public override void DealDamage(int damageToDeal, bool wasAnotherEnemy)
    {
        if (health <= 0)
            return;

        int realDamage = damageToDeal;
        bool crit = false;

        // use real damage for ult charge
        if (player != null)
        {
            CombatPartManager manager = player.GetComponent<CombatPartManager>();

            if (manager != null)
            {
                float ultPoints = realDamage;
                ultPoints += realDamage * (StickerBehavior.Instance.GetUltimateChargeBonus() / 100f);

                manager.AddUltimatePoints(ultPoints);
            }
        }

        // stickers
        if (StickerBehavior.Instance != null)
        {
            // stickers: attack damage buff
            float rawAddedDamage = realDamage * (StickerBehavior.Instance.GetAttackDamageBonus() / 100f);
            int adjustedAddedDamage = Mathf.CeilToInt(rawAddedDamage);
            realDamage += adjustedAddedDamage;

            // stickers: crit chance buff
            int critRoll = Random.Range(0, 100);
            if (critRoll < StickerBehavior.Instance.GetCritChanceBonus())
            {
                crit = true;
                realDamage *= 2;
            }

            // stickers: lifesteal
            float rawHealing = realDamage * (StickerBehavior.Instance.GetLifestealBonus() / 100f);
            int adjustedHealing = Mathf.CeilToInt(rawHealing);
            if (player != null)
            {
                if (player.GetComponent<PlayerHealth>() != null)
                {
                    player.GetComponent<PlayerHealth>().AddHealing(adjustedHealing);
                }
            }
        }

        bool willDie = health - realDamage <= 0;
        BarkManager.Instance?.PlayBark(willDie ? "Enemy Defeated" : "Enemy Take Damage", "Final Boss");

        health -= realDamage;

        // also show some effects
        hitEffect.Play();
        StartCoroutine(ShowDamageNumbers(realDamage, crit));

        // destroy when we have no health left
        if (health <= 0)
        {
            if (isPhase2)
            {
                ImpulseSource.GenerateImpulseWithForce(DeathScreenshakeForce);
                HSMScript.DeathhitStopinitiator(0.2f);

                //Boom plays INSTEAD of hitEffect. Once we have a VFX for boom instead of UI, use .Play instead of coroutine. 
                StartCoroutine(nameof(ShowBoom));
                Debug.Log("Bentley: dead");
            }
            else
            {
                // initiate revive sequence for phase 2
                StopCoroutine(attackCoroutine);
                attackCoroutine = StartCoroutine(BentleyPhase2());
            }
        }
        // these "normal" effects should only play if the enemy isn't dead from that attack.
        else
        {
            // hit VFX
            hitEffect.Play();
            // also play screenshake
            ImpulseSource.GenerateImpulseWithForce(DefaultScreenshakeForce);
        }

        // and update the health bar to match
        TEMP_EnemyHPBar.value = health;
    }

    IEnumerator UpdateRoundInfoText()
    {
        float delayDuration = 0.25f;

        roundInfoText.transform.DOScale(1.25f, delayDuration).SetEase(Ease.OutQuint);
        yield return new WaitForSeconds(delayDuration * 2);

        roundInfoText.GetComponent<TextMeshProUGUI>().text = "// Final Round >> Phase 2";
        yield return new WaitForSeconds(delayDuration * 2);

        roundInfoText.transform.DOScale(0.8f, delayDuration).SetEase(Ease.OutQuint);
        yield return null;
    }

    public override void InflictStun(float time)
    {
        // bentley is not stunnable!
        return;
    }

    protected override IEnumerator ReEnable()
    {
        // see above
        yield return null;
    }

    protected void OnTriggerEnter(Collider other)
    {
        int otherLayer = other.gameObject.layer;

        // the lance charge attack
        if (P1_currentAttack == P1_AttackType.Gungnir_M1)
        {
            if (otherLayer == playerLayer)
            {
                collisionTrigger = true;
                Gungnir_M1 data = (Gungnir_M1)P1_attackDatas[(int)P1_AttackType.Gungnir_M1];
                other.GetComponent<PlayerHealth>().TakeDamage(data.damage);
            }
            if (otherLayer == levelLayer)
            {
                collisionTrigger = true;
            }
        }

        // samus final smash attack
        else if (P1_currentAttack == P1_AttackType.Gungnir_M2)
        {
            if (otherLayer == playerLayer)
            {
                Gungnir_M2 data = (Gungnir_M2)P1_attackDatas[(int)P1_AttackType.Gungnir_M2];
                other.GetComponent<PlayerHealth>().TakeDamage(data.damage);
            }
        }

        // phase 2 lance charge attack
        else if (P2_currentAttack == P2_AttackType.Omega_GM)
        {
            if (otherLayer == playerLayer)
            {
                collisionTrigger = true;
                Omega_GM data = (Omega_GM)P2_attackDatas[(int)P2_AttackType.Omega_GM];
                other.GetComponent<PlayerHealth>().TakeDamage(data.chargeDamage);
            }
            if (otherLayer == levelLayer)
            {
                collisionTrigger = true;
            }
        }
    }
    #endregion
}
