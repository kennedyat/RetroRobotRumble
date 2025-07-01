using System;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "ArmBehavior/SharkLaser")]
public partial class SharkLaser : ArmBehaviorData
{
    public GameObject projectilePrefab;
    public float chargeTime = 0.0f;
    public float maxCharge = 100.0f;
    public float maxChargeTime = 1.9f;

    public float minAttackSpeed = 1.5f;
    public override IArmBehavior MakeInstance()
    {
        return Instantiate(this);
    }

}
public partial class SharkLaser : IArmBehavior
{

        public bool active;
        public float shotCooldown = 0.0f; 
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
            if (shotCooldown > 0f)
            {
                shotCooldown -= Time.fixedDeltaTime;
            }
            if (shotCooldown <= 0)
            {
                shotCooldown += 1 / minAttackSpeed; 
                Shoot(owner.transform.position + Vector3.up * 1.5f);
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

            var instance = Instantiate(projectilePrefab);
            var projectile = instance.GetComponent<Projectile>();
            projectile.FollowRay(actualRay);
        //projectile.originator = this; 
        }
}

