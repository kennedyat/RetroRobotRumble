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

        // In worldspace. Y should be zero, and the magnitude should be at most 1.
        public Vector3 worldspaceMoveInput;
        // In worldspace. We can just Quaternion.LookAt that direction, since up is always known.
        public Vector3 lookDirection;

        private bool isDashing = false;

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
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        // Model tilt should not affect gameplay.
        private void UpdateModelTilt()
        {

        }
    }
}

