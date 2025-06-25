using System;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "ArmBehavior/ShootsProjectiles")]
public class ShootsProjectiles : ArmBehaviorData
{
    public GameObject projectilePrefab;
    public float rpm = 900;
    public int magSize = 20;
    public float reloadPeriodSeconds = 1;
    public int reloadIncrement = 20;

    public override IArmBehavior MakeInstance()
    {
        return new Instance { data = this };
    }

    [Serializable]
    public class Instance : IArmBehavior
    {
        public ShootsProjectiles data;
        public bool active;
        public float shotCooldown; // counts down to 0
        public int magCount;
        public float reloadCooldown; // counts down to 0

        public void Activate(GameObject owner, ArmInstance arm)
        {
            active = true;
        }

        public void Deactivate(GameObject owner, ArmInstance arm)
        {
            active = false;
        }

        public void FixedUpdateFromArm(GameObject owner, ArmInstance arm)
        {
            if (shotCooldown > 0)
            {
                shotCooldown -= Time.fixedDeltaTime;
            }
            if (reloadCooldown > 0)
            {
                reloadCooldown -= Time.fixedDeltaTime;
            }

            if (active)
            {
                reloadCooldown = data.reloadPeriodSeconds;
                if (shotCooldown <= 0 && magCount > 0)
                {
                    shotCooldown += 60 / data.rpm;
                    magCount -= 1;
                    Shoot(owner.transform.position + Vector3.up * 1.5f);
                }
            }
            else
            {
                if (reloadCooldown <= 0 && magCount < data.magSize)
                {
                    reloadCooldown += data.reloadPeriodSeconds;
                    magCount += data.reloadIncrement;
                    magCount = Math.Min(magCount, data.magSize);
                }
            }
        }

        internal void OnProjectileEnter(Projectile projectile, Collision collision)
        {
            Debug.Log(collision);
        }

        void Shoot(Vector3 origin)
        {
            Ray cameraRay = new Ray(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward));

            RaycastHit hitInfo;
            bool hit = Physics.Raycast(cameraRay, out hitInfo, 10);
            Vector3 target = hit ? hitInfo.point : cameraRay.origin + cameraRay.direction.normalized * 10;

            Ray actualRay = new Ray();
            actualRay.origin = origin;
            actualRay.direction = target - actualRay.origin;

            var instance = Instantiate(data.projectilePrefab);
            var projectile = instance.GetComponent<Projectile>();
            projectile.FollowRay(actualRay);
            projectile.originator = this;
        }
    }
}
