using System;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "ArmBehavior/ShootsHitscan")]
public partial class ShootsHitscan : ArmBehaviorData
{
    public GameObject tracerPrefab;
    public float rpm = 900;
    public int magSize = 20;
    public float reloadPeriodSeconds = 1;
    public int reloadIncrement = 20;

    public override IArmBehavior MakeInstance()
    {
        return Instantiate(this);
    }
}

public partial class ShootsHitscan : IArmBehavior
{
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
            reloadCooldown = reloadPeriodSeconds;
            if (shotCooldown <= 0 && magCount > 0)
            {
                shotCooldown += 60 / rpm;
                magCount -= 1;
                Shoot(owner.transform.position + Vector3.up * 1.5f);
            }
        }
        else
        {
            if (reloadCooldown <= 0 && magCount < magSize)
            {
                reloadCooldown += reloadPeriodSeconds;
                magCount += reloadIncrement;
                magCount = Math.Min(magCount, magSize);
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

        RaycastHit cameraHitInfo;
        bool cameraHit = Physics.Raycast(cameraRay, out cameraHitInfo, 10);
        Vector3 target = cameraHit ? cameraHitInfo.point : cameraRay.origin + cameraRay.direction.normalized * 10;

        Ray actualRay = new Ray();
        actualRay.origin = origin;
        actualRay.direction = target - actualRay.origin;

        RaycastHit actualHitInfo;
        bool actualHit = Physics.Raycast(actualRay, out actualHitInfo, 10);

        var tracer = Instantiate(tracerPrefab);
        tracer.transform.position = actualRay.origin;
        tracer.transform.LookAt(actualRay.origin + actualRay.direction);

        if (actualHit)
        {
            tracer.transform.localScale = new Vector3(1, 1, actualHitInfo.distance);
        }
        else
        {
            tracer.transform.localScale = new Vector3(1, 1, 100);
        }

        // gameplay logic
        if (actualHit)
        {
            Debug.Log(cameraHitInfo.collider);
            Debug.Log(actualHitInfo.collider);
        }
    }
}
