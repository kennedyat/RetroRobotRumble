using UnityEngine;

/// Passive train projectile for Train Chassis chest part.
/// Handles train movement and collision detection with different damage zones.
[RequireComponent(typeof(Rigidbody))]
public class TrainChassisPassiveProjectile : MonoBehaviour
{
    [Header("Train Movement")]
    [Tooltip("Forward speed in units/sec")]
    public float speed = 15f;

    [Tooltip("How long the train exists before disappearing")]
    public float lifetime = 10f;

    [Tooltip("Width of the train for collision detection")]
    public float trainWidth = 2f;

    [Header("Damage Zones")]
    [Tooltip("Damage multiplier for center hits")]
    public float centerDamageMultiplier = 2f;

    [Tooltip("Damage multiplier for side hits")]
    public float sideDamageMultiplier = 1f;

    private Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb != null)
        {
            _rb.useGravity = false;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            Debug.Log($"[TrainChassisPassiveProjectile] Awake: Rigidbody setup complete");
        }
        else
        {
            Debug.LogError("[TrainChassisPassiveProjectile] Awake: No Rigidbody found!");
        }
    }

    void Start()
    {
        Debug.Log($"[TrainChassisPassiveProjectile] Start: Script is running on {gameObject.name}");
    }

    /// Called by TrainChassis when summoning
    public void SetupTrain(float trainSpeed, float trainLifetime, float width)
    {
        speed = trainSpeed;
        lifetime = trainLifetime;
        trainWidth = width;
        Debug.Log($"[TrainChassisPassiveProjectile] SetupTrain: speed={speed}, lifetime={lifetime}");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Calculate damage multiplier based on hit position
            float damageMultiplier = CalculateDamageMultiplier(other);
            
            // Apply damage like ProtoProjectile - simple and direct
            int baseDamage = 10;
            int finalDamage = Mathf.RoundToInt(baseDamage * damageMultiplier);
            other.GetComponent<Enemy>().DealDamage(finalDamage);
            
            Debug.Log($"[TrainChassis] Hit enemy {other.name}, dealt {finalDamage} damage (multiplier: {damageMultiplier})");
            
            // Don't destroy the train - let it continue moving (like EagleChassis ultimate)
        }
    }

    private float CalculateDamageMultiplier(Collider enemyCollider)
    {
        // Calculate the distance from the center of the train to the enemy
        Vector3 trainCenter = transform.position;
        Vector3 enemyCenter = enemyCollider.bounds.center;
        
        // Project enemy position onto the train's forward direction
        Vector3 trainForward = transform.forward;
        Vector3 toEnemy = enemyCenter - trainCenter;
        float distanceAlongTrain = Vector3.Dot(toEnemy, trainForward);
        
        // Calculate perpendicular distance from train center line
        Vector3 perpendicularOffset = toEnemy - (trainForward * distanceAlongTrain);
        float perpendicularDistance = perpendicularOffset.magnitude;
        
        // Use actual transform scale instead of script parameter
        float actualTrainWidth = transform.localScale.x; // Use X scale as width
        float centerThreshold = actualTrainWidth * 0.3f; // Center 30% of actual train width
        
        if (perpendicularDistance <= centerThreshold)
        {
            return centerDamageMultiplier;
        }
        else
        {
            return sideDamageMultiplier;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw train collision zones using actual transform scale
        Vector3 actualScale = transform.localScale;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, actualScale);
        
        // Draw center zone
        Gizmos.color = Color.yellow;
        float centerWidth = actualScale.x * 0.3f;
        Vector3 centerScale = new Vector3(centerWidth, actualScale.y, actualScale.z);
        Gizmos.DrawWireCube(transform.position, centerScale);
        
        // Draw movement direction
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 3f);
    }
}

