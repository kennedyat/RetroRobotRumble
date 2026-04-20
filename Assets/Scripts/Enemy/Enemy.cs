using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;
using DG.Tweening;
using TMPro;
using Cinemachine;
using UnityEngine.AI;
using System.Reflection;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CinemachineImpulseSource))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider))]
public class Enemy : MonoBehaviour
{
    #region Variables/References
    protected enum EnemyState { Chasing = 0, Channeling, Attacking, CloseEnough, DashingForward, DashingTangent, Stunned, Death }
    protected enum EnemyPriority { FinalBoss = 0, EliteMelee, EliteRanged, SpinningShredder, MonochromeMilitia, CoolCar, SpikyStego }

    [Header("References")]
    [SerializeField, Tooltip("A reference to the player's position")]
    protected Transform player;
    [SerializeField, Tooltip("A reference to this enemy's rigidbody, used for movements")]
    protected Rigidbody rb;
    [SerializeField, Tooltip("The NavMeshAgent attached to this enemy, used for pathfinding")]
    protected NavMeshAgent navMeshAgent;
    [SerializeField, Tooltip("Animator for the enemy, used to switch between animations")]
    protected Animator enemyAnimator;
    [SerializeField, Tooltip("The colldier attached to this enemy")]
    protected Collider col;
    [SerializeField, Tooltip("Line reticle that is instantiated for some enemies and some attacks")]
    protected GameObject lineReticle;
    [SerializeField, Tooltip("Sphere reticle that is instantiated for some enemies and some attacks")]
    protected GameObject sphereReticle;

    [Header("General Enemy Stats")]
    [SerializeField] EnemyPriority type;
    [SerializeField, Tooltip("Move speed of this enemy")]
    protected float moveSpeed;
    [SerializeField, Tooltip("The health of this enemy")]
    protected int health;
    public int GetHealth() { return health; }
    [SerializeField, Tooltip("The damage this enemy deals with whatever it attacks with")]
    protected int attackDamage;
    [SerializeField, Tooltip("The range this enemy needs to be within to initiate its attack")]
    protected float attackRange;
    [SerializeField, Tooltip("For the enemy spawner, the amount of points it needs to spawn this enemy")]
    protected int spawnCost;
    public int GetSpawnCost() { return spawnCost; }
    [SerializeField, Tooltip("This enemy will try to stay this far from other enemies")]
    protected float separationDistance = .75f;

    [Header("Health UI")]
    [SerializeField] protected GameObject EnemyCanvas;
    [SerializeField] protected Slider TEMP_EnemyHPBar;
    [SerializeField] protected VisualEffect hitEffect;
    [SerializeField] protected GameObject TEMPBoom;
    [SerializeField] protected GameObject TEMPDamageNumber;
    [SerializeField] protected float duration;

    [Header("Combat Feel")]
    [SerializeField] protected CinemachineImpulseSource ImpulseSource;
    [SerializeField] protected float DefaultScreenshakeForce = 0.05f;
    [SerializeField] protected float DeathScreenshakeForce = 0.2f;
    //Hitstop should be called once per activation! This keeps track of that
    protected bool IsHitstop = false;
    [SerializeField] protected float GlobalHitstopTime = 0.02f;
    [SerializeField] protected float DeathHitstopTime = 0.08f;

    [Header("Raycasting")]
    [SerializeField, Tooltip("DO NOT TOUCH THIS UNLESS YOU KNOW WHAT IT DOES")]
    protected float raycastVerticalOffset = 1.3f;
    [SerializeField, Tooltip("DO NOT TOUCH THIS UNLESS YOU KNOW WHAT IT DOES")]
    protected float LOS_Width = 1;

    [Header("Debug - Enemy Parent")]
    [SerializeField] protected EnemyState currentState;
    [SerializeField] protected bool lineOfSightRays = false;

    // internal variables
    protected Coroutine logicCoroutine;
    protected Coroutine attackCoroutine;

    protected Coroutine stunCoroutine;
    protected float stunTimer;
    protected bool attackStarted;
    public GameObject HitStopManagerObject;
    private HitStopManager HSMScript;

    // for layers
    protected static int enemyLayer = -1, playerLayer = -1, levelLayer = -1;
    #endregion

    protected virtual void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        rb = GetComponent<Rigidbody>();
        enemyAnimator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        ImpulseSource = GetComponent<CinemachineImpulseSource>();
        col = GetComponent<Collider>();
        


        TEMP_EnemyHPBar.maxValue = health;
        TEMP_EnemyHPBar.value = health;
        DOTween.Init();

        navMeshAgent.speed = moveSpeed;
        navMeshAgent.autoBraking = false;
        // allow it to instantly get up to speed
        navMeshAgent.acceleration = 1000;
        // and turn really fast
        navMeshAgent.angularSpeed = 360;
        // separation from other enemies and obstacles
        navMeshAgent.radius = separationDistance;
        navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
        navMeshAgent.avoidancePriority = (int)type;


        HitStopManagerObject = GameObject.Find("CombatFeelManager");
        HSMScript = (HitStopManager)HitStopManagerObject.GetComponent(typeof(HitStopManager));


        if (enemyLayer == -1)
        {
            enemyLayer = LayerMask.NameToLayer("Enemy");
            playerLayer = LayerMask.NameToLayer("Player");
            levelLayer = LayerMask.NameToLayer("Level");
        }
    }

    #region Damage/Hitstop
    /// <summary>
    /// Deals damage to this enemy, shows VFX, and destroys it if it has <= 0 health left
    /// </summary>
    /// <param name="damageToDeal">How much damage to deal</param>
    /// <returns>The amount of damage dealt, in case it was modified by damage amplification or resistance</returns>
    public virtual void DealDamage(int damageToDeal)
    {
        // prevent further input
        if (health <= 0)
            return;

        int realDamage = damageToDeal;
        bool crit = false;

        if (StickerBehavior.Instance != null)
        {
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

        if (BarkManager.Instance != null)
            BarkManager.Instance.StartBark("Fleck_Happy", "Enemy_Upset");
        health -= realDamage;

        // also show some effects
        StartCoroutine(ShowDamageNumbers(realDamage, crit));

        // destroy when we have no health left
        if (health <= 0)
        {
            DeathState();
            ImpulseSource.GenerateImpulseWithForce(DeathScreenshakeForce);
            HSMScript.DeathhitStopinitiator(0.08f);
            //StartCoroutine(nameof(DeathHitstop));
            //Boom plays INSTEAD of hitEffect. Once we have a VFX for boom instead of UI, use .Play instead of coroutine. 
            StartCoroutine(nameof(ShowBoom));
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

    /// <summary>
    /// Called once when enemies die. Stops attacking and logic coroutines, while keeping the enemy still.
    /// </summary>
    protected virtual void DeathState()
    {
        // hold the enemy in place again
        rb.constraints = RigidbodyConstraints.FreezeAll;
        navMeshAgent.enabled = false;
        col.enabled = false;
        currentState = EnemyState.Death;

        // stop every coroutine
        if (logicCoroutine != null)
            StopCoroutine(logicCoroutine);

        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);

        if (stunCoroutine != null)
            StopCoroutine(stunCoroutine);
    }

    protected IEnumerator ShowBoom()
    {
        TEMPBoom.SetActive(true);
        yield return new WaitForSecondsRealtime(2.0f);
        TEMPBoom.SetActive(false);
        this.DOKill();
        Destroy(gameObject);
    }

    protected IEnumerator ShowDamageNumbers(int incomingDamage, bool crit)
    {
        yield return new WaitForSecondsRealtime(0.1f);
        GameObject DamageNumberCopy = Instantiate(TEMPDamageNumber, EnemyCanvas.transform, false);
        DamageNumber reference = DamageNumberCopy.GetComponent<DamageNumber>();
        reference.duration = duration;
        reference.SetDamage(incomingDamage, crit);
        reference.ShowNumber();
        yield return new WaitForSecondsRealtime(duration);
        Destroy(DamageNumberCopy);
    }

    //This hitstop is called for every hit
    protected IEnumerator GlobalHitstop()
    {
        Time.timeScale = 0.0f;
        yield return new WaitForSecondsRealtime(GlobalHitstopTime);
        Time.timeScale = 1.0f;
    }

    //This hitstop is called when the enemy dies, punchier
    protected IEnumerator DeathHitstop()
    {
        Time.timeScale = 0.0f;
        yield return new WaitForSecondsRealtime(DeathHitstopTime);
        Time.timeScale = 1.0f;
    }
    #endregion

    #region Other Combat
    /// <summary>
    /// Inflict a stacking stun on this enemy. Calling it multiple times refreshes the stun time, it doesn't add.
    /// </summary>
    /// <param name="time">The time to set the stun. Does nothing if the current stun time > value passed</param>
    public virtual void InflictStun(float time)
    {
        // stuns do not stack, they instead refresh duration
        // so dont let a smaller stun overwrite a larger stun
        if (stunTimer > time)
            return;

        // similar to death state, hold the enemy in place
        currentState = EnemyState.Stunned;

        // disable navigation and stop all velocity
        navMeshAgent.enabled = false;
        if (!rb.isKinematic)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // if there is an attack, stop it
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackStarted = false;
        }

        // re enable after some time
        // structured like this to allow for stuns to extend time
        if (stunCoroutine == null)
        {
            stunCoroutine = StartCoroutine(ReEnable());
        }
    }

    protected virtual IEnumerator ReEnable()
    {
        // this way when stunTimer is reset, the stuns stack
        while (stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;
            yield return null;
        }

        // re enable navigation
        navMeshAgent.enabled = true;

        // assign to channeling so as to not lock in stunned forever
        currentState = EnemyState.Channeling;

        stunCoroutine = null;
    }

    /// <summary>
    /// For channelTime seconds, suspends execution while always facing the player, until the last letGo seconds. 
    /// Sets states to channeling and attacking at start and end, respectively.
    /// After this routine ends, should check if this enemy is stunned, and if it is, break out.
    /// </summary>
    /// <param name="channelTime">The amount of time to face the player</param>
    /// <param name="trackingLetGo">The amount of time to let go of tracking at the end</param>
    /// <param name="facePlayer">Whether or not to constantly face the player during channelTime</param>
    /// <param name="animation">The animation to play (none by default)</param>
    protected virtual IEnumerator AnimationTrackingSequence(float channelTime, float letGo, bool facePlayer = true, Animation anim = null)
    {
        // start the animation if one is provided
        if (anim != null)
        {
            // idk somehow start it though
        }

        // tracking sequence
        currentState = EnemyState.Channeling;
        float t = 0;
        while (t < channelTime)
        {
            if (currentState == EnemyState.Stunned)
                yield break;

            if (facePlayer && t < channelTime - letGo)
            {
                FacePlayer();
            }

            t += Time.deltaTime;
            yield return null;
        }
        currentState = EnemyState.Attacking;
    }

    /// <summary>
    /// Returns whether or not this enemy has an unobstructed line of sight to the player
    /// </summary>
    /// <param name="requireBoth">Set true if both left and right are required to be unobstructed</param>
    /// <returns>The result of the raycasts, unobstructed line of sight to the player?</returns>
    protected virtual bool LineOfSight(bool requireBoth = true)
    {
        Vector3 target = SetY(player.position, 0) + Vector3.up * raycastVerticalOffset;
        Vector3 origin = SetY(transform.position, 0) + Vector3.up * raycastVerticalOffset;

        // direction TO THE PLAYER
        Vector3 baseDir = (target - origin).normalized;

        // 2 raycasts because a singular central raycast causes weird things when turning
        Vector3 rightOffset = LOS_Width / 2 * Vector3.Cross(Vector3.up, baseDir);

        Vector3 left = origin - rightOffset;
        Vector3 right = origin + rightOffset;

        // set the layermask to level and player, as those are the only things to hit
        LayerMask mask = LayerMask.GetMask("Level", "Player");

        // raycast the left
        bool leftClear = !Physics.Raycast(left, baseDir, out RaycastHit hit, attackRange, mask);

        // if nothing was hit, then we are fine (ie left is clear)
        if (!leftClear)
        {
            // if we did hit something, check if it was the player, and if it is, we are clear
            if (hit.collider.gameObject.layer == playerLayer)
                leftClear = true;
        }

        // do the same on the right
        bool rightClear = !Physics.Raycast(right, baseDir, out hit, attackRange, mask);
        if (!rightClear)
        {
            if (hit.collider.gameObject.layer == playerLayer)
                rightClear = true;
        }

        if (lineOfSightRays)
        {
            Debug.DrawRay(left, baseDir * attackRange, Color.red);
            Debug.DrawRay(right, baseDir * attackRange, Color.green);
        }

        if (requireBoth)
            return leftClear && rightClear;
        else
            return leftClear || rightClear;
    }
    #endregion

    #region Helper Functions
    protected static Vector3 SetY(Vector3 input, float set)
    {
        input.y = set;
        return input;
    }

    protected virtual void FacePlayer()
    {
        transform.LookAt(SetY(player.position, transform.position.y));
    }

    protected virtual bool WithinDistance()
    {
        return Vector3.Distance(SetY(player.transform.position, 0), SetY(transform.position, 0)) <= attackRange;
    }

    protected virtual float DistanceToPlayer()
    {
        return Vector3.Distance(SetY(player.position, 0), SetY(transform.position, 0));
    }
    #endregion
}
