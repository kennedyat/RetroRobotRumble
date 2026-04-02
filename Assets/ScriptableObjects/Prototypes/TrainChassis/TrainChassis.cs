using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TrainChassis : MonoBehaviour
{
    // ---------------------- Passive Train Settings ----------------------
    [Header("Passive Train Settings")]
    public HitBox passiveHitBox;  // HitBox component for passive ability
    public GameObject passiveIndicator;  // Indicator GameObject attached to chassis (enabled when needed)
    public float trainSummonInterval = 45f;  // Summon train every 45 seconds
    public float passiveHitBoxDuration = 0.5f;  // How long the hitbox stays active
    public float passiveIndicatorDelay = 1f;  // Show indicator 1 second before activation
    public float passiveHitBoxDistance = 3f;  // Distance in front of player to spawn hitbox
    public float passiveDamage = 30f;  // Damage dealt by passive hitbox
    public float passiveKnockback = 10f;  // Knockback force
    public float centerDamageMultiplier = 2f;  // Enemies in center receive more damage
    public float sideDamageMultiplier = 1f;  // Enemies on sides receive normal damage
    public float centerZoneWidth = 1.5f;  // Width of center damage zone

    // ---------------------- Ultimate (Train Form) ----------------------
    [Header("Ultimate - Train Form")]
    public float ultimateDuration = 10f;
    public float ultimateCooldown = 8f;
    public float minStopTime = 4f; // Can't stop before this time
    public float ultimateTrainSpeed = 20f;
    public float accelerationTime = 1f; // Time to reach full speed
    public float turnSpeedReduction = 0.5f; // 50% reduction
    public float collisionDamage = 50f;
    public float collisionKnockback = 15f;
    public float slamDamage = 75f;
    public float slamRadius = 8f;
    public float passiveFrequencyBoost = 2f; // 2x more frequent passive while in train form

    [Header("Train Form Visual & HitBox")]
    public GameObject trainModelPrefab; // Train visual model (with HitBox as child)
    private GameObject _activeTrainModel; // Currently active train model
    private HitBox _trainFormHitBox; // HitBox component from the spawned train model

    // ---------------------- Input (matches arm script style) ----------------------
    public PlayerInput.PlayerActions input_map;
    private PlayerInput _actions;
    private InputAction ultimateInput;
    private InputAction passiveTestInput; // Temporary for testing

    // ---------------------- Runtime State ----------------------
    private float _trainSummonTimer;
    private float _ultimateCooldownTimer;
    private bool _isInTrainForm = false;
    private float _trainFormTimer = 0f;
    private float _currentTrainSpeed = 0f;
    private float _originalPassiveInterval;
    private Rigidbody _playerRb;
    private PlayerController _playerController;
    private GameObject _activeHitBoxObject;  // GameObject that contains the active hitbox
    private HitBox _activePassiveHitBox;  // Currently active passive hitbox
    private bool _isPassiveActive = false;  // Is passive ability currently active

    private void Start()
    {
        // Input wrapper like in your Locomotive_Revised
        _actions = new PlayerInput();
        input_map = _actions.Player;
        ultimateInput = input_map.Ultimate; // <-- Action must exist in your InputActions asset

        // Temporary: Create U key input for testing passive ability
        passiveTestInput = new InputAction("PassiveTest", InputActionType.Button, "<Keyboard>/u");
        passiveTestInput.performed += OnPassiveTestPerformed;
        passiveTestInput.Enable();

        ultimateInput.performed += OnUltimatePerformed;
        input_map.Enable();

        // Get player references
        _playerRb = GetComponent<Rigidbody>();
        _playerController = GetComponent<PlayerController>();
        _originalPassiveInterval = trainSummonInterval;
    }

    private void OnDestroy()
    {
        if (ultimateInput != null)
            ultimateInput.performed -= OnUltimatePerformed;
        if (passiveTestInput != null)
            passiveTestInput.performed -= OnPassiveTestPerformed;
        _actions?.Dispose();
        passiveTestInput?.Dispose();

        // Clean up passive ability if active
        DeactivatePassiveHitBox();
        if (passiveIndicator != null)
        {
            passiveIndicator.SetActive(false);
        }
    }

    private void Update()
    {
        if (_isInTrainForm)
        {
            HandleTrainForm();
        }
        else
        {
            // Passive train summoning - automatic every 45 seconds
            _trainSummonTimer += Time.deltaTime;
            if (_trainSummonTimer >= trainSummonInterval)
            {
                _trainSummonTimer = 0f;
                SummonTrain();
            }
        }

        // Ultimate cooldown
        if (_ultimateCooldownTimer > 0f)
            _ultimateCooldownTimer = Mathf.Max(0f, _ultimateCooldownTimer - Time.deltaTime);
    }

    private void FixedUpdate()
    {
        // Manage passive hitbox OnHit subscription (same pattern as Locomotive)
        if (_activePassiveHitBox != null)
        {
            if (_activePassiveHitBox.isActive)
            {
                _activePassiveHitBox.OnHit += OnPassiveHitBoxHit;
            }
            else
            {
                _activePassiveHitBox.OnHit -= OnPassiveHitBoxHit;
            }
        }
    }

    // ---------------------- Input Callback ----------------------
    private void OnUltimatePerformed(InputAction.CallbackContext ctx)
    {
        TryCastUltimate();
    }

    // Temporary callback for testing passive ability with U key
    private void OnPassiveTestPerformed(InputAction.CallbackContext ctx)
    {
        SummonTrain();
        Debug.Log("[TrainChassis] Manual train summon triggered by U key!");
    }

    public bool TryCastUltimate()
    {
        if (_isInTrainForm)
        {
            // If already in train form, try to stop early
            if (_trainFormTimer >= minStopTime)
            {
                EndTrainForm();
                return true;
            }
            return false; // Can't stop yet
        }
        else
        {
            // Start train form
            if (_ultimateCooldownTimer > 0f)
                return false;

            StartTrainForm();
            return true;
        }
    }

    // ---------------------- Passive Train Summoning ----------------------
    private void SummonTrain()
    {
        if (_isPassiveActive)
        {
            Debug.Log("[TrainChassis] Passive already active, skipping summon.");
            return;
        }

        if (!passiveHitBox)
        {
            Debug.LogError("[TrainChassis] Missing passiveHitBox reference.");
            return;
        }

        // Start the passive ability sequence: indicator -> hitbox
        StartCoroutine(PassiveAbilitySequence());
    }

    private IEnumerator PassiveAbilitySequence()
    {
        _isPassiveActive = true;

        // Step 1: Show indicator for 1 second
        if (passiveIndicator != null)
        {
            // Enable the existing indicator GameObject
            passiveIndicator.SetActive(true);

            Debug.Log("[TrainChassis] Showing passive indicator for 1 second...");
            yield return new WaitForSeconds(passiveIndicatorDelay);

            // Disable the indicator
            passiveIndicator.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[TrainChassis] passiveIndicator is null, skipping indicator.");
            yield return new WaitForSeconds(passiveIndicatorDelay);
        }

        // Step 2: Activate hitbox using transform from reference
        ActivatePassiveHitBox();

        // Step 3: Wait for hitbox duration
        yield return new WaitForSeconds(passiveHitBoxDuration);

        // Step 4: Deactivate and clean up
        DeactivatePassiveHitBox();
        _isPassiveActive = false;

        Debug.Log("[TrainChassis] Passive ability sequence complete.");
    }

    private void ActivatePassiveHitBox()
    {
        if (passiveHitBox == null)
        {
            Debug.LogError("[TrainChassis] Cannot activate passive hitbox - missing reference!");
            return;
        }

        // Create a GameObject to hold the hitbox
        _activeHitBoxObject = new GameObject("TrainPassiveHitBox");

        // Copy transform from reference hitbox (using world values to account for parent transforms)
        _activeHitBoxObject.transform.position = passiveHitBox.transform.position;
        _activeHitBoxObject.transform.rotation = passiveHitBox.transform.rotation;
        _activeHitBoxObject.transform.localScale = passiveHitBox.transform.lossyScale; // Use lossyScale to get world scale

        // Copy collider and renderer from the reference hitbox prefab if it exists
        // MUST add these BEFORE HitBox component, as HitBox.Awake() needs them
        if (passiveHitBox != null)
        {
            BoxCollider referenceCollider = passiveHitBox.GetComponent<BoxCollider>();
            MeshRenderer referenceRenderer = passiveHitBox.GetComponent<MeshRenderer>();

            if (referenceCollider != null)
            {
                BoxCollider newCollider = _activeHitBoxObject.AddComponent<BoxCollider>();
                newCollider.size = referenceCollider.size;
                newCollider.center = referenceCollider.center;
                newCollider.isTrigger = true;
            }

            if (referenceRenderer != null)
            {
                MeshRenderer newRenderer = _activeHitBoxObject.AddComponent<MeshRenderer>();
                newRenderer.material = referenceRenderer.material;
                MeshFilter referenceFilter = passiveHitBox.GetComponent<MeshFilter>();
                if (referenceFilter != null)
                {
                    MeshFilter newFilter = _activeHitBoxObject.AddComponent<MeshFilter>();
                    newFilter.mesh = referenceFilter.mesh;
                }
            }
        }
        else
        {
            // If no reference hitbox, add default components
            BoxCollider defaultCollider = _activeHitBoxObject.AddComponent<BoxCollider>();
            defaultCollider.size = Vector3.one * 2f;
            defaultCollider.isTrigger = true;

            MeshRenderer defaultRenderer = _activeHitBoxObject.AddComponent<MeshRenderer>();
            MeshFilter defaultFilter = _activeHitBoxObject.AddComponent<MeshFilter>();
            defaultFilter.mesh = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<MeshFilter>().sharedMesh;
            Destroy(GameObject.CreatePrimitive(PrimitiveType.Cube)); // Clean up temp object
        }

        // NOW add the HitBox component (after collider and renderer exist)
        _activePassiveHitBox = _activeHitBoxObject.AddComponent<HitBox>();

        // Set up hitbox
        HitBoxManager.currentHitbox = _activePassiveHitBox;

        // Enable the hitbox for the specified duration
        // Note: OnHit subscription is handled in FixedUpdate (same pattern as Locomotive)
        _activePassiveHitBox.EnableFrame(passiveHitBoxDuration);

        Debug.Log($"[TrainChassis] Activated passive hitbox at {_activeHitBoxObject.transform.position}");
    }

    private void DeactivatePassiveHitBox()
    {
        if (_activePassiveHitBox != null)
        {
            // Note: OnHit unsubscription is handled in FixedUpdate
            _activePassiveHitBox.DisableFrame();
            _activePassiveHitBox = null;
        }

        if (_activeHitBoxObject != null)
        {
            Destroy(_activeHitBoxObject);
            _activeHitBoxObject = null;
        }
    }

    private void OnPassiveHitBoxHit(Collider other)
    {
        Debug.Log($"[TrainChassis] OnPassiveHitBoxHit called! Collider: {other.name}, Tag: {other.tag}");

        if (other.CompareTag("Enemy"))
        {
            Debug.Log($"[TrainChassis] Enemy detected! Processing damage and knockback...");

            // Calculate damage multiplier based on position
            float damageMultiplier = CalculatePassiveDamageMultiplier(other);
            int finalDamage = Mathf.RoundToInt(passiveDamage * damageMultiplier);

            // Deal damage
            var enemyHealth = other.GetComponent<Enemy>();
            if (enemyHealth != null)
            {
                enemyHealth.DealDamage(finalDamage);
                Debug.Log($"[TrainChassis] Dealt {finalDamage} damage to {other.name}");
            }
            else
            {
                Debug.LogWarning($"[TrainChassis] No EnemyHealth component found on {other.name}!");
            }

            // Apply knockback
            var enemyRb = other.GetComponent<Rigidbody>();
            if (enemyRb != null && _activeHitBoxObject != null)
            {
                Vector3 knockbackDirection = (other.transform.position - _activeHitBoxObject.transform.position).normalized;
                enemyRb.AddForce(knockbackDirection * passiveKnockback, ForceMode.VelocityChange);
                Debug.Log($"[TrainChassis] Applied knockback to {other.name}, force: {passiveKnockback}");
            }
            else
            {
                if (enemyRb == null)
                    Debug.LogWarning($"[TrainChassis] No Rigidbody found on {other.name}!");
                if (_activeHitBoxObject == null)
                    Debug.LogWarning("[TrainChassis] _activeHitBoxObject is null!");
            }

            Debug.Log($"[TrainChassis] Passive hitbox hit enemy: {other.name}, dealt {finalDamage} damage (multiplier: {damageMultiplier})");
        }
        else
        {
            Debug.Log($"[TrainChassis] Hit object is not an enemy: {other.name}, tag: {other.tag}");
        }
    }

    private float CalculatePassiveDamageMultiplier(Collider enemyCollider)
    {
        if (_activeHitBoxObject == null)
            return sideDamageMultiplier;

        // Calculate perpendicular distance from center line of hitbox
        Vector3 hitBoxCenter = _activeHitBoxObject.transform.position;
        Vector3 enemyCenter = enemyCollider.bounds.center;
        Vector3 hitBoxForward = _activeHitBoxObject.transform.forward;

        // Project enemy position onto hitbox forward direction
        Vector3 toEnemy = enemyCenter - hitBoxCenter;
        float distanceAlongHitBox = Vector3.Dot(toEnemy, hitBoxForward);

        // Calculate perpendicular distance from center line
        Vector3 perpendicularOffset = toEnemy - (hitBoxForward * distanceAlongHitBox);
        float perpendicularDistance = perpendicularOffset.magnitude;

        // Check if enemy is in center zone
        if (perpendicularDistance <= centerZoneWidth / 2f)
        {
            return centerDamageMultiplier;
        }
        else
        {
            return sideDamageMultiplier;
        }
    }

    // ---------------------- Train Form Ultimate ----------------------
    private void StartTrainForm()
    {
        _isInTrainForm = true;
        _trainFormTimer = 0f;
        _currentTrainSpeed = 0f;

        // Boost passive frequency
        trainSummonInterval = _originalPassiveInterval / passiveFrequencyBoost;

        // Disable PlayerController to prevent movement conflicts
        if (_playerController != null)
        {
            _playerController.enabled = false;
        }

        // Set rigidbody constraints for train movement
        if (_playerRb != null)
        {
            _playerRb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        }

        // Spawn train visual model
        if (trainModelPrefab != null)
        {
            _activeTrainModel = Instantiate(trainModelPrefab, transform);
            _activeTrainModel.transform.localPosition = Vector3.zero;
            _activeTrainModel.transform.localRotation = Quaternion.identity;

            // Find HitBox component in the spawned train model
            _trainFormHitBox = _activeTrainModel.GetComponentInChildren<HitBox>();

            if (_trainFormHitBox != null)
            {
                HitBoxManager.currentHitbox = _trainFormHitBox;
                _trainFormHitBox.OnHit += OnTrainFormHit;
                _trainFormHitBox.EnableFrame(ultimateDuration);
                Debug.Log("[TrainChassis] Found HitBox in train model");
            }
            else
            {
                Debug.LogWarning("[TrainChassis] No HitBox found in train model prefab!");
            }
        }

        // Make player invulnerable (you'll need to implement this based on your health system)
        // For now, we'll assume there's a way to make the player invulnerable

        Debug.Log("[TrainChassis] Started Train Form!");
    }

    private void HandleTrainForm()
    {
        _trainFormTimer += Time.deltaTime;

        // Accelerate to full speed over accelerationTime
        if (_trainFormTimer < accelerationTime)
        {
            _currentTrainSpeed = Mathf.Lerp(0f, ultimateTrainSpeed, _trainFormTimer / accelerationTime);
        }
        else
        {
            _currentTrainSpeed = ultimateTrainSpeed;
        }

        // Move forward automatically
        if (_playerRb != null)
        {
            // Use velocity for more reliable movement
            Vector3 forwardVelocity = transform.forward * _currentTrainSpeed;
            _playerRb.velocity = new Vector3(forwardVelocity.x, _playerRb.velocity.y, forwardVelocity.z);
        }

        // Handle passive summoning while in train form (boosted frequency)
        _trainSummonTimer += Time.deltaTime;
        if (_trainSummonTimer >= trainSummonInterval)
        {
            _trainSummonTimer = 0f;
            SummonTrain();
        }

        // Auto-end after duration
        if (_trainFormTimer >= ultimateDuration)
        {
            EndTrainForm();
        }
    }

    private void EndTrainForm()
    {
        _isInTrainForm = false;
        _trainFormTimer = 0f;
        _currentTrainSpeed = 0f;

        // Re-enable PlayerController
        if (_playerController != null)
        {
            _playerController.enabled = true;
        }

        // Restore rigidbody constraints
        if (_playerRb != null)
        {
            _playerRb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        }

        // Clean up HitBox
        if (_trainFormHitBox != null)
        {
            _trainFormHitBox.OnHit -= OnTrainFormHit;
            _trainFormHitBox.DisableFrame();
            _trainFormHitBox = null;
        }

        // Clean up train visual model
        if (_activeTrainModel != null)
        {
            Destroy(_activeTrainModel);
            _activeTrainModel = null;
        }

        // Restore original passive interval
        trainSummonInterval = _originalPassiveInterval;

        // Trigger slam attack
        TriggerSlamAttack();

        // Start cooldown
        _ultimateCooldownTimer = ultimateCooldown;

        Debug.Log("[TrainChassis] Ended Train Form!");
    }

    private void TriggerSlamAttack()
    {
        // Find all enemies in slam radius
        Collider[] enemies = Physics.OverlapSphere(transform.position, slamRadius);

        foreach (var enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                // Deal slam damage
                var enemyHealth = enemy.GetComponent<Enemy>();
                if (enemyHealth != null)
                {
                    enemyHealth.DealDamage(Mathf.RoundToInt(slamDamage));
                }

                // Apply knockback
                var enemyRb = enemy.GetComponent<Rigidbody>();
                if (enemyRb != null)
                {
                    Vector3 knockbackDirection = (enemy.transform.position - transform.position).normalized;
                    enemyRb.AddForce(knockbackDirection * collisionKnockback, ForceMode.VelocityChange);
                }
            }
        }

        Debug.Log($"[TrainChassis] Slam attack hit {enemies.Length} enemies!");
    }

    private void OnTrainFormHit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Deal collision damage
            var enemyHealth = other.GetComponent<Enemy>();
            if (enemyHealth != null)
            {
                enemyHealth.DealDamage(Mathf.RoundToInt(collisionDamage));
            }

            // Apply knockback
            var enemyRb = other.GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                Vector3 knockbackDirection = (other.transform.position - transform.position).normalized;
                enemyRb.AddForce(knockbackDirection * collisionKnockback, ForceMode.VelocityChange);
            }

            Debug.Log($"[TrainChassis] Train form hit enemy: {other.name}");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isInTrainForm)
        {
            if (other.CompareTag("Level"))
            {
                // Hit a wall - turn around
                transform.Rotate(0, 180, 0);
                Debug.Log("[TrainChassis] Hit wall, turning around!");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw spawn point
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1f);

        // Draw passive hitbox position (if assigned)
        if (passiveHitBox != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(passiveHitBox.transform.position, 0.5f);

            // Draw center zone
            Gizmos.color = Color.yellow;
            BoxCollider referenceCollider = passiveHitBox.GetComponent<BoxCollider>();
            if (referenceCollider != null)
            {
                Vector3 center = passiveHitBox.transform.position;
                Vector3 size = referenceCollider.size;
                size.x = centerZoneWidth;  // Override width with center zone width
                Gizmos.DrawWireCube(center, size);
            }
        }
        else
        {
            // Draw indicator position if hitbox not assigned
            Gizmos.color = Color.cyan;
            Vector3 indicatorPos = transform.position + transform.forward * passiveHitBoxDistance;
            Gizmos.DrawWireSphere(indicatorPos, 0.5f);
            Gizmos.DrawRay(transform.position, transform.forward * passiveHitBoxDistance);
        }

        // Draw slam radius when in train form
        if (_isInTrainForm)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, slamRadius);
        }
    }
}