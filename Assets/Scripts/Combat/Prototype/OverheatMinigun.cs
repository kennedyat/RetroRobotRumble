using System;
using Unity.VisualScripting;
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
    public sealed partial class OverheatMinigun : MonoBehaviour
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

        // We will use this for polling only!
        PlayerInput.PlayerActions input_map;
        InputAction normalInput;
        InputAction specialInput;

        [Header("Normal Parameters")]
        public GameObject projectilePrefab;
        public float initialShotsPerSecond = 3;
        public float initialSpreadDegrees = 25;
        public float initialToFullSeconds = 1.75f;

        public float fullShotsPerSecond = 8;
        public float fullSpreadDegrees = 45;
        public float fullToInitialSeconds = 1;

        public float heatPerShot = 3;
        public float shotHeatCooldownBuffer = 0.5f;

        [Header("Special Parameters")]
        public float empowerCooldown = 25;
        public float empowerDuration = 8;

        public float empoweredSpeedFactor = 1.5f;
        public float empoweredSizeFactor = 1.5f; // in each dimension.

        const float MAX_HEAT = 100;
        [Header("Arm-level Parameters")]
        public float cooldownHeatPerSecond = MAX_HEAT / 2.5f;
        public float overheatLockoutSeconds = 5;

        [Header("Arm-level Runtime Variables")]
        public bool overheated = false;
        public float currentHeat = 0;
        public float timeUntilNotOverheated = 0;
        public float timeUntilCooldown = 0;

        public float timeUntilUnempowered = 0;

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
            // on the arm level, if you are overheated, forcefully override the behavior.
            if (overheated)
            {
                timeUntilNotOverheated -= Time.fixedDeltaTime;
                if (timeUntilNotOverheated <= 0)
                {
                    overheated = false;
                    currentHeat = 0;
                    // completely reset the normal attack.
                    // (this is redundant)
                    normalAttack = new NormalAttack();
                }
            }
            else
            {
                normalAttack.PollAndUpdate(this, normalInput.ReadValue<float>() > 0);
                if (currentHeat >= MAX_HEAT)
                {
                    normalAttack = new NormalAttack(); // completely reset the normal attack.
                    // and do stuff.
                    overheated = true;
                    timeUntilNotOverheated = overheatLockoutSeconds;
                }
            }

            // when overheated, *this arm's* special attack still works.
            // in the general case, whether or not it works should be configurable?
            specialAttack.PollAndUpdate(this, specialInput.ReadValue<float>() > 0);
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

        private static Quaternion RandomRotation(float spreadDegrees)
        {
            return Quaternion.AngleAxis(UnityEngine.Random.Range(-spreadDegrees / 2, spreadDegrees / 2), Vector3.up);
        }

        private Ray GetShotPath(Transform player, float spreadDegrees)
        {
            Ray playerRay = new Ray();
            playerRay.origin = player.position;
            playerRay.direction = RandomRotation(spreadDegrees) * player.forward;
            return playerRay;
        }

        private void Shoot(Transform player, float spreadDegrees)
        {
            // heat management
            if (timeUntilUnempowered <= 0)
            {
                currentHeat += heatPerShot;
            }
            timeUntilCooldown = shotHeatCooldownBuffer;

            // the actual shot
            var instance = Instantiate(projectilePrefab);
            var projectile = instance.GetComponent<Projectile>();
            projectile.FollowRay(GetShotPath(player, spreadDegrees));

            if (timeUntilUnempowered > 0)
            {
                instance.transform.localScale *= empoweredSpeedFactor;
                projectile.speed *= empoweredSizeFactor;
            }
        }

        void Update()
        {
            Transform diamond = transform.Find("WorldspaceUI").Find("Normal").Find("Diamond");

            float spreadDegrees = initialSpreadDegrees + normalAttack.currentRampup * (fullSpreadDegrees - initialShotsPerSecond);
            float x = Mathf.Tan(spreadDegrees / 2 * Mathf.Deg2Rad);

            diamond.localScale = new Vector3(x, 1, 1);
        }
    }

    public sealed partial class OverheatMinigun
    {
        public NormalAttack normalAttack;
        public SpecialAttack specialAttack;

        [Serializable]
        public sealed class NormalAttack
        {
            public float timeUntilNextShot = 0;
            public float currentRampup = 0;

            public void PollAndUpdate(OverheatMinigun arm, bool pressed)
            {
                if (pressed && timeUntilNextShot <= 0)
                {
                    float spreadDegrees = arm.initialSpreadDegrees + currentRampup * (arm.fullSpreadDegrees - arm.initialShotsPerSecond);

                    arm.Shoot(arm.transform, spreadDegrees);

                    float shotsPerSecond = arm.initialShotsPerSecond + currentRampup * (arm.fullShotsPerSecond - arm.initialShotsPerSecond);
                    timeUntilNextShot = 1 / shotsPerSecond;
                }

                if (pressed)
                {
                    currentRampup += Time.fixedDeltaTime / arm.initialToFullSeconds;
                }
                else
                {
                    currentRampup -= Time.fixedDeltaTime / arm.fullToInitialSeconds;
                }
                currentRampup = Mathf.Clamp(currentRampup, 0, 1);

                timeUntilNextShot -= Time.fixedDeltaTime;
                if (timeUntilNextShot <= 0)
                {
                    timeUntilNextShot = 0;
                }

                if (arm.timeUntilCooldown <= 0)
                {
                    arm.currentHeat -= Time.fixedDeltaTime * arm.cooldownHeatPerSecond;
                    if (arm.currentHeat <= 0)
                    {
                        arm.currentHeat = 0;
                    }
                }
                else
                {
                    arm.timeUntilCooldown -= Time.fixedDeltaTime;
                    if (arm.timeUntilCooldown <= 0)
                    {
                        arm.timeUntilCooldown = 0;
                    }
                }
            }

            public void Shoot()
            {
                Debug.Log("Bang!");
            }
        }

        [Serializable]
        public sealed class SpecialAttack
        {
            public float timeUntilNextEmpower = 0;

            public void PollAndUpdate(OverheatMinigun arm, bool pressed)
            {
                if (pressed && timeUntilNextEmpower <= 0)
                {
                    timeUntilNextEmpower = arm.empowerCooldown + arm.empowerDuration;
                    arm.timeUntilUnempowered = arm.empowerDuration;
                    arm.currentHeat = 0;
                    arm.timeUntilNotOverheated = 0;
                }

                timeUntilNextEmpower -= Time.fixedDeltaTime;
                if (timeUntilNextEmpower <= 0)
                {
                    timeUntilNextEmpower = 0;
                }

                arm.timeUntilUnempowered -= Time.fixedDeltaTime;
                if (arm.timeUntilUnempowered <= 0)
                {
                    arm.timeUntilUnempowered = 0;
                }
            }
        }
    }
}

