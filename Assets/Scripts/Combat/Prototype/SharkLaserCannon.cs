using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Combat.Prototype
{
    // A nongeneric implementation of the Shark Laser Cannon.
    // No composition, no interfaces, no subclassing.
    //
    // This makes a direct assumption that the player is the only user.
    // The controller can be abstracted away later.
    //
    // This is a MonoBehavior for direct access, and familiarity.
    // This is not helpful for storing default values. A ScriptableObject *will* need to exist somewhere.
    public sealed partial class SharkLaserCannon : MonoBehaviour
    {
        // HACK: Ideally, input handling is handled elsewhere, and given to this object.
        // In edtior, we can just attach *all* arms simultaneously, then enable only two and disable the rest,
        // then set them to control differently.
        public enum LeftOrRightControls
        {
            LEFT_ARM, // Left click and Q
            RIGHT_ARM, // Right click and E
        }

        public LeftOrRightControls leftOrRightControls;
        public NormalAttack normalAttack;
        public SpecialAttack specialAttack;

        // We will use this for polling only!
        PlayerInput.PlayerActions input_map;
        InputAction normalInput;
        InputAction specialInput;

        float fullChargeTimeSeconds = 1;

        public GameObject tracerPrefab;
        public GameObject orbPrefab;

        void Start()
        {
            var inputs = new PlayerInput();

            input_map = inputs.Player;
            if (leftOrRightControls == LeftOrRightControls.LEFT_ARM)
            {
                normalInput = input_map.LeftArmNormal;
                specialInput = input_map.LeftArmSpecial;
            }
            else
            {
                normalInput = input_map.RightArmNormal;
                specialInput = input_map.RightArmSpecial;
            }

            input_map.Enable();
        }

        void FixedUpdate()
        {
            normalAttack.PollAndUpdate(this, normalInput.ReadValue<float>() > 0);
            specialAttack.PollAndUpdate(this, specialInput.ReadValue<float>() > 0);
        }

        public Ray GetShotPath(Vector3 player)
        {
            Ray cameraRay = new Ray(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward));

            RaycastHit cameraHitInfo;
            bool cameraHit = Physics.Raycast(cameraRay, out cameraHitInfo, 10);
            Vector3 cameraTarget = cameraHit ? cameraHitInfo.point : cameraRay.origin + cameraRay.direction.normalized * 10;

            Ray playerRay = new Ray();
            playerRay.origin = player;
            playerRay.direction = cameraTarget - player;
            return playerRay;
        }
    }

    public sealed partial class SharkLaserCannon
    {
        [Serializable]
        public sealed class NormalAttack
        {
            public void PollAndUpdate(SharkLaserCannon arm, bool pressed)
            {
                if (pressed)
                {
                    Ray shotPath = arm.GetShotPath(arm.transform.position + Vector3.up * 1.7f);

                    RaycastHit hitInfo;
                    bool actualHit = Physics.Raycast(shotPath, out hitInfo, 10);

                    var tracer = Instantiate(arm.tracerPrefab);
                    tracer.transform.position = shotPath.origin;
                    tracer.transform.LookAt(shotPath.origin + shotPath.direction);
                    tracer.transform.localScale = new Vector3(1, 1, 10);
                }
            }
        }

        [Serializable]
        public sealed class SpecialAttack
        {
            bool wasPressed = false;
            float chargeSeconds = 0;

            public void PollAndUpdate(SharkLaserCannon arm, bool pressed)
            {
                // Do the logic. Avoid mixing with modifying this object.
                if (!pressed && wasPressed)
                {
                    Ray shotPath = arm.GetShotPath(arm.transform.position + Vector3.up * 1.7f);

                    var instance = Instantiate(arm.orbPrefab);
                    var projectile = instance.GetComponent<Projectile>();
                    projectile.FollowRay(shotPath);

                    projectile.transform.localScale *= 1 + 4 * Mathf.Min(1, chargeSeconds / arm.fullChargeTimeSeconds);
                }

                // Modify this object. Avoid mixing with logic.
                if (!pressed && wasPressed)
                {
                    this.chargeSeconds = 0;
                }
                if (pressed)
                {
                    chargeSeconds += Time.fixedDeltaTime;
                }
                wasPressed = pressed;
            }
        }
    }
}

