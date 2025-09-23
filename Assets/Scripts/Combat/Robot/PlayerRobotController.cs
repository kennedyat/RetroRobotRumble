using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Combat.Robot;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
namespace Assets.Scripts.Combat.Robot
{
    [RequireComponent(typeof(CombatRobot))]
    public class PlayerRobotController : MonoBehaviour
    {
        [SerializeField, Tooltip("Look sensitivity for gamepad, in degrees/second")]
        private float gamepadSensitivity = 180f;
        [SerializeField, Tooltip("Look sensitivity for mouse")]
        private float mouseSensitivity = 0.1f;

        public void Move(InputAction.CallbackContext context)
        {
            var robot = GetComponent<CombatRobot>();
            var moveInput = Vector2.ClampMagnitude(context.ReadValue<Vector2>(), 1);

            var cameraSpaceDirection = new Vector3(moveInput.x, 0, moveInput.y);

            // This is cool but wrong, since the camera points into the floor.
            // Camera.main.transform.InverseTransformDirection(cameraSpaceDirection);

            // So, whatever. Boo.
            var cameraYaw = Camera.main.transform.rotation.eulerAngles.y;
            var worldspaceMoveInput = Quaternion.AngleAxis(cameraYaw, Vector3.up) * cameraSpaceDirection;

            robot.worldspaceMoveInput = worldspaceMoveInput;
        }

        // TODO: Separate into mouse look vs controller look.
        public void Look(InputAction.CallbackContext context)
        {
            var robot = GetComponent<CombatRobot>();

            if (context.control.device is Mouse)
            {
                var input = context.ReadValue<Vector2>();
                // idk the units of the input, so the sensitivity is kind of a random number.
                robot.yawDelta += input.x * mouseSensitivity;
            }
            else if (context.control.device is Gamepad)
            {
                var input = context.ReadValue<Vector2>();
                robot.yawRotationalVelocity = input.x * gamepadSensitivity;
            }
        }

        public void Dash(InputAction.CallbackContext context)
        {
            // if (context.started && !isDashing && moveInput.sqrMagnitude != 0)
            // {
            //     Debug.Log("dash!");
            //     isDashing = true;
            //     StartCoroutine(DashMovement());
            // }
        }
    }
}
