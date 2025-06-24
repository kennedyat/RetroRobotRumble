using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ArmBehavior/ShootsProjectiles")]
public class ShootsProjectiles : ArmBehaviorData
{
    public GameObject projectilePrefab;

    public override IArmBehavior MakeInstance()
    {
        return new Instance { data = this };
    }

    [Serializable]
    public class Instance : IArmBehavior
    {
        public ShootsProjectiles data;

        public void Activate(GameObject owner, ArmInstance arm)
        {
            Ray cameraRay = new Ray(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward));

            RaycastHit hitInfo;
            bool hit = Physics.Raycast(cameraRay, out hitInfo, 10);

            var instance = Instantiate(data.projectilePrefab);
            var projectile = instance.GetComponent<Projectile>();

            Vector3 instance_position = owner.transform.position + Vector3.up * 1.5f;
            instance.transform.position = instance_position;

            Vector3 target;
            if (hit)
            {
                target = hitInfo.point;
            }
            else
            {
                target = cameraRay.origin + cameraRay.direction.normalized * 10;
            }

            Debug.Log(hit);
            Debug.Log(target);

            Vector3 direction = target - instance_position;

            instance.transform.LookAt(target);
            projectile.direction = direction;
        }

        public void Deactivate(GameObject owner, ArmInstance arm)
        {

        }

        public void FixedUpdate(GameObject owner, ArmInstance arm)
        {

        }
    }
}
