using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Assets.Scripts.Combat.Robot;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "ScriptableObjects/Components/Fairy Legs")]
public class FairyLegs : PartComponent
{
    [Header("Trail")]
    [SerializeField] private float trailLifetime = 10f;
    [SerializeField] private float trailWidth = 1.5f;
    [SerializeField] private float trailHeight = 1f;
    [SerializeField] private float groundOffset = 0.05f;
    [SerializeField] private Material trailMaterial;
    [SerializeField] private Color trailColor = new Color(0.45f, 1f, 0.75f, 0.7f);

    [Header("Effects")]
    [SerializeField, Range(0f, 100f)] private float enemySlowPercent = 40f;
    [SerializeField, Range(0f, 100f)] private float playerMoveSpeedBonusPercent = 20f;
    [SerializeField] private float lingerDuration = 1f;

    public override void Initialize(PartContext context)
    {
        GetRuntime(context).Configure(this, context);
    }

    public override void OnExecute(PartContext context)
    {
        GetRuntime(context).TriggerDashAndArm();
    }

    public override void OnUpdate(PartContext context, float deltaTime)
    {
    }

    private FairyLegsRuntime GetRuntime(PartContext context)
    {
        const string runtimeKey = "FairyLegsRuntime";

        if (context.CustomData.TryGetValue(runtimeKey, out object existingRuntime) &&
            existingRuntime is FairyLegsRuntime cachedRuntime &&
            cachedRuntime != null)
        {
            return cachedRuntime;
        }

        GameObject ownerObject = context.Owner != null ? context.Owner.root.gameObject : null;
        FairyLegsRuntime runtime = ownerObject != null
            ? ownerObject.GetComponent<FairyLegsRuntime>()
            : null;

        if (runtime == null && ownerObject != null)
        {
            runtime = ownerObject.AddComponent<FairyLegsRuntime>();
        }

        context.CustomData[runtimeKey] = runtime;
        return runtime;
    }

    public float TrailLifetime => trailLifetime;
    public float TrailWidth => trailWidth;
    public float TrailHeight => trailHeight;
    public float GroundOffset => groundOffset;
    public Material TrailMaterial => trailMaterial;
    public Color TrailColor => trailColor;
    public float EnemySlowMultiplier => 1f - (enemySlowPercent / 100f);
    public float PlayerMoveSpeedBonusPercent => playerMoveSpeedBonusPercent;
    public float LingerDuration => lingerDuration;
}

public class FairyLegsRuntime : MonoBehaviour
{
    private FairyLegs config;
    private CombatRobot robot;
    private Transform playerRoot;
    private FairyLegsPlayerSpeedEffect playerSpeedEffect;

    private bool waitingForDash;
    private bool dashInProgress;
    private Vector3 dashStartPosition;

    private int playerTrailContacts;
    private float trailBuffExpiresAt;
    private float dashBuffExpiresAt;

    private readonly Dictionary<Enemy, int> enemyContactCounts = new Dictionary<Enemy, int>();
    private readonly Dictionary<Enemy, FairyLegsEnemySlowEffect> enemyEffects = new Dictionary<Enemy, FairyLegsEnemySlowEffect>();

    public void Configure(FairyLegs fairyLegs, PartContext context)
    {
        config = fairyLegs;
        playerRoot = context.Owner != null ? context.Owner.root : null;

        if (playerRoot != null && robot == null)
        {
            robot = playerRoot.GetComponent<CombatRobot>();
        }

        if (playerRoot != null && playerSpeedEffect == null)
        {
            playerSpeedEffect = playerRoot.GetComponent<FairyLegsPlayerSpeedEffect>();
            if (playerSpeedEffect == null)
            {
                playerSpeedEffect = playerRoot.gameObject.AddComponent<FairyLegsPlayerSpeedEffect>();
            }
        }

        if (playerSpeedEffect != null)
        {
            playerSpeedEffect.Initialize(robot);
        }
    }

    public void TriggerDashAndArm()
    {
        if (config == null || robot == null)
        {
            return;
        }

        robot.TryDash();
        waitingForDash = true;

        if (!dashInProgress && IsRobotDashing())
        {
            waitingForDash = false;
            dashInProgress = true;
            dashStartPosition = playerRoot != null ? playerRoot.position : transform.position;
        }
    }

    private void Update()
    {
        if (config == null || robot == null || playerRoot == null)
        {
            return;
        }

        if (waitingForDash && !dashInProgress && robot.remainingDistance > 0.001f)
        {
            waitingForDash = false;
            dashInProgress = true;
            dashStartPosition = playerRoot.position;
        }

        if (dashInProgress && !IsRobotDashing())
        {
            dashInProgress = false;
            SpawnTrail(dashStartPosition, playerRoot.position);
            dashBuffExpiresAt = Mathf.Max(dashBuffExpiresAt, Time.time + config.LingerDuration);
        }

        bool hasPlayerBuff =
            playerTrailContacts > 0 ||
            Time.time < trailBuffExpiresAt ||
            Time.time < dashBuffExpiresAt;

        if (hasPlayerBuff)
        {
            playerSpeedEffect?.Apply(config.PlayerMoveSpeedBonusPercent);
        }
        else
        {
            playerSpeedEffect?.Clear();
        }
    }

    public void NotifyPlayerEnteredTrail()
    {
        playerTrailContacts++;
    }

    public void NotifyPlayerExitedTrail()
    {
        playerTrailContacts = Mathf.Max(0, playerTrailContacts - 1);
        if (playerTrailContacts == 0)
        {
            trailBuffExpiresAt = Mathf.Max(trailBuffExpiresAt, Time.time + config.LingerDuration);
        }
    }

    public void NotifyEnemyEnteredTrail(Enemy enemy)
    {
        if (enemy == null)
        {
            return;
        }

        if (!enemyContactCounts.TryGetValue(enemy, out int currentCount))
        {
            currentCount = 0;
        }

        enemyContactCounts[enemy] = currentCount + 1;
        GetEnemyEffect(enemy)?.Apply(config.EnemySlowMultiplier, config.LingerDuration);
    }

    public void NotifyEnemyStayedOnTrail(Enemy enemy)
    {
        if (enemy == null)
        {
            return;
        }

        GetEnemyEffect(enemy)?.Apply(config.EnemySlowMultiplier, config.LingerDuration);
    }

    public void NotifyEnemyExitedTrail(Enemy enemy)
    {
        if (enemy == null)
        {
            return;
        }

        if (!enemyContactCounts.TryGetValue(enemy, out int currentCount))
        {
            return;
        }

        currentCount--;
        if (currentCount <= 0)
        {
            enemyContactCounts.Remove(enemy);
            GetEnemyEffect(enemy)?.BeginLinger(config.LingerDuration);
            return;
        }

        enemyContactCounts[enemy] = currentCount;
    }

    private FairyLegsEnemySlowEffect GetEnemyEffect(Enemy enemy)
    {
        if (enemy == null)
        {
            return null;
        }

        if (enemyEffects.TryGetValue(enemy, out FairyLegsEnemySlowEffect existingEffect) && existingEffect != null)
        {
            return existingEffect;
        }

        FairyLegsEnemySlowEffect effect = enemy.GetComponent<FairyLegsEnemySlowEffect>();
        if (effect == null)
        {
            effect = enemy.gameObject.AddComponent<FairyLegsEnemySlowEffect>();
        }

        enemyEffects[enemy] = effect;
        return effect;
    }

    private bool IsRobotDashing()
    {
        return robot != null && robot.dashCooldown > 0f && robot.remainingDistance > 0.001f;
    }

    private void SpawnTrail(Vector3 startPosition, Vector3 endPosition)
    {
        Vector3 flatDelta = endPosition - startPosition;
        flatDelta.y = 0f;

        float length = flatDelta.magnitude;
        if (length <= 0.05f)
        {
            return;
        }

        GameObject trailObject = new GameObject("Fairy Trail");
        Vector3 midpoint = (startPosition + endPosition) * 0.5f;
        midpoint.y = Mathf.Min(startPosition.y, endPosition.y) + config.GroundOffset;

        trailObject.transform.position = midpoint;
        trailObject.transform.rotation = Quaternion.LookRotation(flatDelta.normalized, Vector3.up);

        BoxCollider trigger = trailObject.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(config.TrailWidth, config.TrailHeight, length);

        FairyTrailZone zone = trailObject.AddComponent<FairyTrailZone>();
        zone.Initialize(this, playerRoot);

        CreateVisual(trailObject.transform, length);
        Destroy(trailObject, config.TrailLifetime);
    }

    private void CreateVisual(Transform trailTransform, float length)
    {
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Visual";
        Destroy(visual.GetComponent<Collider>());
        visual.transform.SetParent(trailTransform, false);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(config.TrailWidth, 0.05f, length);

        Renderer renderer = visual.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        if (config.TrailMaterial != null)
        {
            renderer.material = new Material(config.TrailMaterial);
        }

        if (renderer.material != null && renderer.material.HasProperty("_Color"))
        {
            renderer.material.color = config.TrailColor;
        }
    }

    private void OnDisable()
    {
        playerSpeedEffect?.Clear();

        foreach (FairyLegsEnemySlowEffect effect in enemyEffects.Values)
        {
            effect?.ClearImmediate();
        }

        enemyEffects.Clear();
        enemyContactCounts.Clear();
    }
}

public class FairyTrailZone : MonoBehaviour
{
    private FairyLegsRuntime owner;
    private Transform playerRoot;
    private bool playerInside;
    private readonly HashSet<Enemy> trackedEnemies = new HashSet<Enemy>();

    public void Initialize(FairyLegsRuntime runtimeOwner, Transform playerRootTransform)
    {
        owner = runtimeOwner;
        playerRoot = playerRootTransform;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (owner == null)
        {
            return;
        }

        if (IsPlayer(other))
        {
            if (!playerInside)
            {
                playerInside = true;
                owner.NotifyPlayerEnteredTrail();
            }

            return;
        }

        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy != null && trackedEnemies.Add(enemy))
        {
            owner.NotifyEnemyEnteredTrail(enemy);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (owner == null)
        {
            return;
        }

        if (IsPlayer(other))
        {
            return;
        }

        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy == null)
        {
            return;
        }

        if (trackedEnemies.Add(enemy))
        {
            owner.NotifyEnemyEnteredTrail(enemy);
        }

        owner.NotifyEnemyStayedOnTrail(enemy);
    }

    private void OnTriggerExit(Collider other)
    {
        if (owner == null)
        {
            return;
        }

        if (IsPlayer(other))
        {
            if (playerInside)
            {
                playerInside = false;
                owner.NotifyPlayerExitedTrail();
            }

            return;
        }

        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy != null && trackedEnemies.Remove(enemy))
        {
            owner.NotifyEnemyExitedTrail(enemy);
        }
    }

    private bool IsPlayer(Collider other)
    {
        return playerRoot != null && other.transform.root == playerRoot;
    }

    private void OnDisable()
    {
        if (owner == null)
        {
            return;
        }

        if (playerInside)
        {
            playerInside = false;
            owner.NotifyPlayerExitedTrail();
        }

        foreach (Enemy enemy in trackedEnemies)
        {
            owner.NotifyEnemyExitedTrail(enemy);
        }

        trackedEnemies.Clear();
    }
}

public class FairyLegsPlayerSpeedEffect : MonoBehaviour
{
    private CombatRobot robot;
    private FieldInfo moveSpeedField;
    private bool isApplied;
    private float baseMoveSpeed;
    private float appliedMoveSpeed;

    public void Initialize(CombatRobot combatRobot)
    {
        robot = combatRobot;
        moveSpeedField = typeof(CombatRobot).GetField("moveSpeed", BindingFlags.Instance | BindingFlags.NonPublic);
    }

    public void Apply(float percentBonus)
    {
        if (robot == null || moveSpeedField == null)
        {
            return;
        }

        float currentMoveSpeed = (float)moveSpeedField.GetValue(robot);

        if (!isApplied)
        {
            baseMoveSpeed = currentMoveSpeed;
        }
        else if (!Mathf.Approximately(currentMoveSpeed, appliedMoveSpeed))
        {
            baseMoveSpeed = currentMoveSpeed;
        }

        appliedMoveSpeed = baseMoveSpeed * (1f + (percentBonus / 100f));
        moveSpeedField.SetValue(robot, appliedMoveSpeed);
        isApplied = true;
    }

    public void Clear()
    {
        if (!isApplied || robot == null || moveSpeedField == null)
        {
            return;
        }

        moveSpeedField.SetValue(robot, baseMoveSpeed);
        isApplied = false;
        appliedMoveSpeed = 0f;
    }

    private void OnDisable()
    {
        Clear();
    }
}

public class FairyLegsEnemySlowEffect : MonoBehaviour
{
    private NavMeshAgent agent;
    private bool isApplied;
    private float baseSpeed;
    private float appliedSpeed;
    private Coroutine lingerRoutine;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void Apply(float multiplier, float lingerDuration)
    {
        if (lingerRoutine != null)
        {
            StopCoroutine(lingerRoutine);
            lingerRoutine = null;
        }

        if (agent == null)
        {
            return;
        }

        float currentSpeed = agent.speed;
        if (!isApplied)
        {
            baseSpeed = currentSpeed;
        }
        else if (!Mathf.Approximately(currentSpeed, appliedSpeed))
        {
            baseSpeed = currentSpeed;
        }

        appliedSpeed = baseSpeed * Mathf.Clamp01(multiplier);
        if (agent.enabled)
        {
            agent.speed = appliedSpeed;
        }

        isApplied = true;
    }

    public void BeginLinger(float lingerDuration)
    {
        if (lingerRoutine != null)
        {
            StopCoroutine(lingerRoutine);
        }

        lingerRoutine = StartCoroutine(ClearAfterDelay(lingerDuration));
    }

    public void ClearImmediate()
    {
        if (lingerRoutine != null)
        {
            StopCoroutine(lingerRoutine);
            lingerRoutine = null;
        }

        Clear();
    }

    private IEnumerator ClearAfterDelay(float lingerDuration)
    {
        yield return new WaitForSeconds(lingerDuration);
        Clear();
        lingerRoutine = null;
    }

    private void Clear()
    {
        if (!isApplied || agent == null)
        {
            return;
        }

        if (agent.enabled)
        {
            agent.speed = baseSpeed;
        }

        isApplied = false;
        appliedSpeed = 0f;
    }

    private void OnDisable()
    {
        ClearImmediate();
    }
}
