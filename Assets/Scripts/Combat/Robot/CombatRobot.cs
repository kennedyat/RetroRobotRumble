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

        // Params.

        [Header("| MOVEMENT PARAMETERS")]
        [SerializeField, Tooltip("Base movement speed of player")] private float _moveSpeed = 1f;

        [Header("| DASH PARAMETERS")]
        [SerializeField, Tooltip("Distance traveled with a dash")] private float _dashDistance = 5f;
        [SerializeField, Tooltip("Time taken for a dash + before another dash can be performed")] private float _dashDuration = 0.5f;

        [Header("| VISUAL PARAMETERS")]
        [SerializeField, Tooltip("Transform to tilt during movement")] private Transform _tiltPivot;
        [SerializeField, Tooltip("Amount of tilt, in degrees")] private float _tiltMagnitude = 15f;
        [SerializeField, Tooltip("Time to move half the distance to target tilt")] private float _tiltHalflife = 0.1f;

        protected void FixedUpdate()
        {
            FixedUpdateRootTransform();
             UpdateRootRotation();
            UpdateModelTilt();
        }

        private void FixedUpdateRootTransform()
        {
            var rb = GetComponent<Rigidbody>();

            Vector3 currentVelocity = rb.velocity;

            dashCooldown -= Mathf.Min(Time.fixedDeltaTime, dashCooldown);

            Vector3 velocityChange = GetTargetVelocity() - currentVelocity;
            velocityChange.y = 0;
            if (dashCooldown <= 0)
            {
                rb.AddForce(velocityChange, ForceMode.VelocityChange);
                remainingDistance = _dashDistance;
            }
            else
            {
                Dash(rb);
            }
          
        }
           

        protected void Update()
        {
            //UpdateRootRotation();
            //UpdateModelTilt();
        }

        // Rotation should not affect gameplay.
        private void UpdateRootRotation()
        {
            // Apply rotation instantly.
            transform.rotation *= Quaternion.AngleAxis(yawDelta, Vector2.up);
            yawDelta = 0;

            transform.rotation *= Quaternion.AngleAxis(yawRotationalVelocity * Time.deltaTime, Vector2.up);

        }

        // Model tilt should not affect gameplay.
        private void UpdateModelTilt()
        {
            float angle = GetTargetVelocity().magnitude / _moveSpeed * _tiltMagnitude;
            Vector3 axis = Vector3.Cross(Vector3.up, Quaternion.Inverse(transform.rotation) * GetTargetVelocity());
            Quaternion target = Quaternion.AngleAxis(angle, axis);

            float decay = Mathf.Pow(0.5f, Time.deltaTime / _tiltHalflife);

            _tiltPivot.transform.localRotation = Quaternion.Slerp(target, _tiltPivot.transform.localRotation, decay);
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
        }

        public void Dash(Rigidbody rb)
        {
           
            if(remainingDistance> 0.01f)
            {
                float dashSpeed = _dashDistance / _dashDuration;
                float stepDist = Mathf.Min(dashSpeed * Time.fixedDeltaTime, remainingDistance);

                if (Physics.SphereCast(transform.position, GetComponent<CapsuleCollider>().radius,
                dashDirection, out RaycastHit hit, stepDist))
                {
                    rb.MovePosition(transform.position + dashDirection * (hit.distance - 0.01f));
                   // remainingDistance = 0;
                    return;
                }
                else
                {
                    rb.MovePosition(transform.position + stepDist *dashDirection);
                    remainingDistance -= stepDist;
                }
              
            }
        }

        private Vector3 GetTargetVelocity()
        {
            if (dashCooldown > 0)
            {
                // Speed decreases linearly with time.
                float targetSpeed = dashCooldown * 1 / _dashDuration * _dashDistance * 2 / _dashDuration / _dashDuration;
                return transform.position + _dashDistance * Time.fixedDeltaTime * dashDirection;
            }
            else
            {
                return worldspaceMoveInput * _moveSpeed;
            }
        }
    }
}

