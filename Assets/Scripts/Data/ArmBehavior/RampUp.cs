using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI; 
[CreateAssetMenu(menuName = "ArmBehavior/RampUp")]
public partial class RampUp : ArmBehaviorData
{
    public GameObject tracerPrefab;
    public float baseAttackSpeed = 3f; //number of projectiles fired per second
    public float currentAttackSpeed = 0f; 
    public float maxAttackSpeed = 8f; //number of projectiles fired per second
    public float attackSpeedRampUp = 1.75f; //number of seconds it takes to ramp up from base to max attack speed
    public float attackSpeedFallOff = 1f; //number of seconds it takes for attack speed to fall off after releasing attack

    public Overheat overheatReference; 
    public override IArmBehavior MakeInstance()
    {
        return Instantiate(this);
    }
}

public partial class RampUp : IArmBehavior
{
    public bool active;
    public float shotCooldown = 0.0f; // counts down to 0

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
        if(shotCooldown > 0f)
        {
            shotCooldown -= Time.fixedDeltaTime; 
        }
        //Time between shots is attack speed/60 
        if (active)
        { 
            currentAttackSpeed += (maxAttackSpeed - baseAttackSpeed) / attackSpeedRampUp * Time.fixedDeltaTime;
            if(currentAttackSpeed >= maxAttackSpeed)
            {
                currentAttackSpeed = maxAttackSpeed; 
            }
            if (shotCooldown <= 0)
            {
                shotCooldown += 1 / currentAttackSpeed;
                Shoot(owner.transform.position + Vector3.up * 1.5f);
                overheatReference.IncrementOverheat(); 
            }
        }
        else
        {
            currentAttackSpeed -= (maxAttackSpeed - baseAttackSpeed) / attackSpeedFallOff * Time.fixedDeltaTime; 
            if(currentAttackSpeed <= baseAttackSpeed)
            {
                currentAttackSpeed = baseAttackSpeed; 
            }
        }
    }

    internal void OnProjectileEnter(Projectile projectile, Collision collision)
    {
        //Debug.Log(collision);
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
            //Debug.Log(cameraHitInfo.collider);
            //Debug.Log(actualHitInfo.collider);
        }
    }
}
