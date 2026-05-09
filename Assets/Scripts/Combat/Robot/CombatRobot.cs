using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Assets.Scripts.Combat.Robot
{
    /// <summary>
    /// Attached to CombatRobot prefab only!!!
    ///
    /// Receives inputs (from the player or some ai).
    /// Updates its children GameObjects.
    /// </summary>
    /// You can still directly access the Animator and Rigidbody, but you probably shouldn't.
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody))]
    public class CombatRobot : MonoBehaviour
    {
        // An external script is allowed to overwrite these.

        // In worldspace. Y should be zero, nd the magnitude should be at most 1.
        public Vector3 worldspaceMoveInput;
        // In degrees per second. Can be clamped.
        public float yawRotationalVelocity;
        // In degrees. Instant rotation, though can be limited by stuff.
        public float yawDelta;

        // Logic
        public float dashCooldown = 0;
        public Vector3 dashDirection = Vector3.zero;
        public float remainingDistance = 0;
        private Rigidbody rb;
        private CapsuleCollider cap;
        public bool ScriptedMovementLock { get; set; }

        // Params.

        [Header("| MOVEMENT PARAMETERS")]
        [SerializeField, Tooltip("Base movement speed of player")] private float _baseMoveSpeed = 10f;
        private float moveSpeed;

        [Header("| DASH PARAMETERS")]
        [SerializeField, Tooltip("Distance traveled with a dash")] private float _dashDistance = 5f;
        [SerializeField, Tooltip("Time taken for a dash + before another dash can be performed")] private float _dashDuration = 0.5f;

        [Header("| VISUAL PARAMETERS")]
        [SerializeField, Tooltip("Transform to tilt during movement")] private Transform _tiltPivot;
        [SerializeField, Tooltip("Amount of tilt, in degrees")] private float _tiltMagnitude = 15f;
        [SerializeField, Tooltip("Time to move half the distance to target tilt")] private float _tiltHalflife = 0.1f;
        
        void Start()
        {
            moveSpeed = _baseMoveSpeed;

        }

       

          void Awake()
        {
            rb = GetComponent<Rigidbody>();
            cap = GetComponent<CapsuleCollider>();
            rb.isKinematic = true; // MovePosition style
            rb.interpolation = RigidbodyInterpolation.Interpolate;       
        }

        void FixedUpdate()
        {

             if (StickerBehavior.Instance != null)
            {
                UpdateMoveSpeed(StickerBehavior.Instance.GetMoveSpeedBonus());
            }
            
            float dt = Time.fixedDeltaTime;

            if (ScriptedMovementLock)
            {
                dashCooldown = 0f;
                remainingDistance = 0f;
                yawDelta = 0f;
                yawRotationalVelocity = 0f;
                worldspaceMoveInput = Vector3.zero;
                UpdateModelTilt(dt);
                return;
            }

            dashCooldown = Mathf.Max(0f, dashCooldown - dt);

            ApplyRotation(dt);
            ApplyMovement(dt);

            UpdateModelTilt(dt); // or move to LateUpdate
        }
        public void TryDash()
        {
            if (worldspaceMoveInput.sqrMagnitude <= 0.2 ||
                    dashCooldown > 0)
            {
                return;
            }

            // more reasons to not dash
            dashCooldown = _dashDuration;
            dashDirection = worldspaceMoveInput.normalized;
            remainingDistance = _dashDistance;
            BarkManager.Instance?.PlayBark("Player Dash", "Player Movement");
        }
        private void ApplyRotation(float dt)
        {
            float yawStep = yawDelta + yawRotationalVelocity * dt;
            yawDelta = 0f;

            if (Mathf.Abs(yawStep) < 0.00001f) return;

            rb.MoveRotation(rb.rotation * Quaternion.AngleAxis(yawStep, Vector3.up));
        }

    private void ApplyMovement(float dt)
    {
        Vector3 desiredDelta;
        bool isDashing = dashCooldown > 0f && remainingDistance > 0.001f;

        if (isDashing)
        {
            float dashSpeed = _dashDistance / _dashDuration;
            float stepDist = Mathf.Min(dashSpeed * dt, remainingDistance);
            desiredDelta = dashDirection * stepDist;
        }
        else
        {
            Vector3 input = worldspaceMoveInput;
            input.y = 0f;
            desiredDelta = input * (moveSpeed * dt);
        }

        float dist = desiredDelta.magnitude;
        if (dist <= 0.00001f) return;

        Vector3 dir = desiredDelta / dist;
        Vector3 origin = cap.bounds.center;
        float radius = cap.radius * 0.8f;
        LayerMask mask = LayerMask.GetMask("Level", "Enemy");

        Vector3 totalMove = Vector3.zero;

        if (Physics.SphereCast(origin, radius, dir, out RaycastHit hit, dist, mask))
        {

            float safeMove = Mathf.Max(0f, hit.distance - 0.02f);
            totalMove = dir * safeMove;

            float remaining = dist - safeMove;
            Vector3 slideDir = Vector3.ProjectOnPlane(dir, hit.normal).normalized;
            slideDir.y = 0f; 

            if (remaining > 0.001f && slideDir.sqrMagnitude > 0.001f)
            {
                Vector3 slideOrigin = origin + totalMove;
                if (!Physics.SphereCast(slideOrigin, radius, slideDir, out _, remaining, mask))
                {
                    totalMove += slideDir * remaining;
                }
            }

            if (isDashing) remainingDistance -= safeMove;
        }
        else
        {
            totalMove = desiredDelta;
            if (isDashing) remainingDistance -= dist;
        }

        rb.MovePosition(rb.position + totalMove);
    }

        private Vector3 GetTargetVelocity()
        {
            if (dashCooldown > 0f)
                return dashDirection * (_dashDistance / _dashDuration);
            return worldspaceMoveInput * moveSpeed;
        }

        private void UpdateModelTilt(float dt)
        {
            Vector3 v = GetTargetVelocity(); v.y = 0f;

            float angle = (v.magnitude / Mathf.Max(0.0001f, moveSpeed)) * _tiltMagnitude;

            Vector3 axis = Vector3.Cross(Vector3.up, Quaternion.Inverse(rb.rotation) * v);
            if (axis.sqrMagnitude < 1e-6f) axis = Vector3.right;
            axis.Normalize();

            Quaternion target = Quaternion.AngleAxis(angle, axis);

            float decay = Mathf.Pow(0.5f, dt / Mathf.Max(0.0001f, _tiltHalflife));
            _tiltPivot.localRotation = Quaternion.Slerp(target, _tiltPivot.localRotation, decay);
        }

        private void UpdateMoveSpeed(float modifier)
        {
            moveSpeed = _baseMoveSpeed * 1f + (modifier/100f);
        }

    }
}

