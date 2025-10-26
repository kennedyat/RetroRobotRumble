using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TrainChassis : MonoBehaviour
{
    // ---------------------- Passive Train Settings ----------------------
    [Header("Passive Train Settings")]
    public GameObject passiveTrainPrefab;  // prefab with TrainChassisPassiveProjectile
    public Transform firePoint;
    public float trainSummonInterval = 45f;  // Summon train every 45 seconds
    public float trainSpeed = 15f;
    public float trainLifetime = 10f;  // How long the train exists before disappearing

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
        if (!passiveTrainPrefab)
        {
            Debug.LogError("[TrainChassis] Missing passiveTrainPrefab.");
            return;
        }

        // Spawn the train at the prefab's configured position
        GameObject train = Instantiate(passiveTrainPrefab, transform.position, transform.rotation);
        train.transform.SetParent(null);
        if (!train.activeSelf) train.SetActive(true);

        // Set up rigidbody velocity directly like EagleChassis ultimate
        if (!train.TryGetComponent<Rigidbody>(out var rb))
        {
            rb = train.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.velocity = transform.forward * trainSpeed;

        // Configure the train projectile for damage and lifetime
        var trainProjectile = train.GetComponent<TrainChassisPassiveProjectile>();
        if (trainProjectile != null)
        {
            trainProjectile.SetupTrain(trainSpeed, trainLifetime, 1f);
        }

        // Cleanup after lifetime
        Destroy(train, trainLifetime);

        Debug.Log($"[TrainChassis] Summoned train at {transform.position} with velocity {rb.velocity}");
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
            _playerRb.constraints = RigidbodyConstraints.FreezeRotation;
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
                var enemyHealth = enemy.GetComponent<EnemyHealth>();
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
            var enemyHealth = other.GetComponent<EnemyHealth>();
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
        
        // Draw slam radius when in train form
        if (_isInTrainForm)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, slamRadius);
        }
    }
}