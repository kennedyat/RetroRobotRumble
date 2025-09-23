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

        // Logic.
        private bool isDashing = false;

        // Params.

        [Header("| MOVEMENT PARAMETERS")]
        [SerializeField, Tooltip("Base movement speed of player")] private float _moveSpeed = 1f;

        [Header("| DASH PARAMETERS")]
        [SerializeField, Tooltip("Distance traveled with a dash")] private float _dashDistance = 5f;
        [SerializeField, Tooltip("Time taken for a dash + before another dash can be performed")] private float _dashDuration = 0.5f;

        [Header("| VISUAL PARAMETERS")]
        [SerializeField, Tooltip("Transform to tilt during movement")] private Transform _tiltPivot;
        [SerializeField, Tooltip("Amount of tilt, in degrees")] private float _tiltMagnitude = 15f;
        [SerializeField, Tooltip("Time taken to tilt, in seconds")] private float _tiltSpeed = 0.5f;

        protected void FixedUpdate()
        {
            FixedUpdateRootTransform();
        }

        private void FixedUpdateRootTransform()
        {
            var rb = GetComponent<Rigidbody>();

            Vector3 currentVelocity = rb.velocity;
            Vector3 targetVelocity = worldspaceMoveInput * _moveSpeed;

            Vector3 velocityChange = targetVelocity - currentVelocity;
            velocityChange.y = 0;
            Vector3.ClampMagnitude(velocityChange, _moveSpeed);

            rb.AddForce(velocityChange, ForceMode.VelocityChange);
        }

        protected void Update()
        {
            UpdateRootRotation();
            UpdateModelTilt();
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

        }
    }
}

