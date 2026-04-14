using UnityEngine;

/// Passive projectile for Eagle Chassis chest part.
/// Handles homing movement only. Enemies handle damage themselves.
[RequireComponent(typeof(Rigidbody))]
public class EagleChassisPassiveProjectile : MonoBehaviour
{
    [Header("Motion")]
    [Tooltip("Forward speed in units/sec")]
    public float speed = 18f;

    [Tooltip("How fast the projectile can rotate toward the target (deg/sec)")]
    public float turnRate = 720f;

    private Transform _target;
    private Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    /// Called by AutoChest when firing
    public void SetTarget(Transform t) => _target = t;

    void Update()
    {
        if (_target != null && _target.gameObject.activeInHierarchy)
        {
            // Aim toward target center if possible
            Vector3 aimPoint = _target.position;
            if (_target.TryGetComponent<Collider>(out var col))
                aimPoint = col.bounds.center;

            Vector3 toTarget = (aimPoint - transform.position).normalized;
            Quaternion desired = Quaternion.LookRotation(toTarget, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desired, turnRate * Time.deltaTime);

            _rb.velocity = transform.forward * speed;
        }
        else
        {
            // No target: just fly forward
            _rb.velocity = transform.forward * speed;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Let enemies handle their own damage logic
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}
