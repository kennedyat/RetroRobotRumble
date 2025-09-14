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

        [Header("UI Related Serializeds")]
        [SerializeField] private Canvas RangeIndicatorCanvas;

        public LeftOrRightControls leftOrRightControls;

        public GameObject tracerPrefab;
        public GameObject orbPrefab;

        public float fullChargeTimeSeconds = 1;
        // We will use this for polling only!
        PlayerInput.PlayerActions input_map;
        InputAction normalInput;
        InputAction specialInput;

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

            Vector2 newSize = RangeIndicatorCanvas.GetComponent<RectTransform>().sizeDelta;
            newSize.y = NormalAttack.currentProjectileRange;
            RangeIndicatorCanvas.GetComponent<RectTransform>().sizeDelta = newSize;
            RangeIndicatorCanvas.transform.position = new Vector3(0f, 0.14f, NormalAttack.currentProjectileRange / 2);
        }

        void FixedUpdate()
        {
            normalAttack.PollAndUpdate(this, normalInput.ReadValue<float>() > 0);
            specialAttack.PollAndUpdate(this, specialInput.ReadValue<float>() > 0);

            if (specialInput != null)
            {
                // It's valid, you can use it safely
                specialInput.started += _ => Debug.Log("Special pressed!");
            }
            else
            {
                Debug.LogWarning("specialInput is null!");
            }

            if (normalInput != null)
            {
                // It's valid, you can use it safely
                normalInput.started += _ => Debug.Log("Normal pressed!");
            }
            else
            {
                Debug.LogWarning("NormalInput is null!");
            }

            Vector2 newSize = RangeIndicatorCanvas.GetComponent<RectTransform>().sizeDelta;
            newSize.y = NormalAttack.currentProjectileRange;
            RangeIndicatorCanvas.GetComponent<RectTransform>().sizeDelta = newSize;
            RangeIndicatorCanvas.transform.position = new Vector3(0f, 0.14f, NormalAttack.currentProjectileRange / 2);
        }

        // public Ray GetShotPathFirstPerson(Vector3 player)
        // {
        //     Ray cameraRay = new Ray(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward));

        //     RaycastHit cameraHitInfo;
        //     bool cameraHit = Physics.Raycast(cameraRay, out cameraHitInfo, 10);
        //     Vector3 cameraTarget = cameraHit ? cameraHitInfo.point : cameraRay.origin + cameraRay.direction.normalized * 10;

        //     Ray playerRay = new Ray();
        //     playerRay.origin = player;
        //     playerRay.direction = cameraTarget - player;
        //     return playerRay;
        // }

        public Ray GetShotPath()
        {
            Transform spawnPoint = transform.Find("SpawnPoint");

            Ray playerRay = new Ray();
            playerRay.origin = spawnPoint.position;
            playerRay.direction = spawnPoint.forward;
            return playerRay;
        }
    }

    public sealed partial class SharkLaserCannon
    {
        public NormalAttack normalAttack;
        public SpecialAttack specialAttack;

        [Serializable]
        public sealed class SpecialAttack
        {
            public void PollAndUpdate(SharkLaserCannon arm, bool pressed)
            {
                if (pressed)
                {
                    for (int dx = -3; dx <= 3; dx++)
                    {
                        for (int dy = 0; dy <= 6; dy++)
                        {
                            Vector3 offset = new Vector3(dx, dy, 0) / 4;

                            Ray playerRay = new Ray();
                            playerRay.origin = arm.transform.position + arm.transform.rotation * offset;
                            playerRay.direction = arm.transform.forward;

                            RaycastHit rayHitInfo;
                            bool hit = Physics.Raycast(playerRay, out rayHitInfo, 10);
                            Ray shotPath = new Ray(playerRay.origin, hit ? (rayHitInfo.point - playerRay.origin) : playerRay.direction);

                            var tracer = Instantiate(arm.tracerPrefab);
                            tracer.transform.position = shotPath.origin;
                            tracer.transform.LookAt(shotPath.origin + shotPath.direction);
                            tracer.transform.localScale = new Vector3(1, 1, (rayHitInfo.point - playerRay.origin).magnitude);
                        }
                    }
                }
            }
        }

        [Serializable]
        public sealed class NormalAttack
        {
            public bool wasPressed = false;
            public float chargeSeconds = 0;
            public float minProjectileRange = 2.5f; 
            public float maxProjectileRange = 5f;
            public static float currentProjectileRange = 2.5f;
            public void PollAndUpdate(SharkLaserCannon arm, bool pressed)
            {
                // Do the logic. Avoid mixing with modifying this object.
                if (!pressed && wasPressed)
                {
                    Ray shotPath = arm.GetShotPath();

                    var instance = Instantiate(arm.orbPrefab);
                    var projectile = instance.GetComponent<Projectile>();
                    projectile.FollowRay(shotPath, currentProjectileRange);

                    projectile.transform.localScale *= 1 + 4 * Mathf.Min(1, chargeSeconds / arm.fullChargeTimeSeconds);
                }

                // Modify this object. Avoid mixing with logic.
                if (!pressed && wasPressed)
                {
                    this.chargeSeconds = 0;
                    currentProjectileRange = minProjectileRange; 
                }
                if (pressed)
                {
                    chargeSeconds += Time.fixedDeltaTime;
                    currentProjectileRange = Mathf.Clamp(currentProjectileRange + Time.fixedDeltaTime, minProjectileRange, maxProjectileRange);
                }
                wasPressed = pressed;
            }
        }
    }
}

