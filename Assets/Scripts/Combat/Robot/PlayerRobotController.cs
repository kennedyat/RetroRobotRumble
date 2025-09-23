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
                robot.yawDelta += input.x;
            }
            else if (context.control.device is Gamepad)
            {
                var input = context.ReadValue<Vector2>();
                robot.yawRotationalVelocity = input.x * 360;
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
