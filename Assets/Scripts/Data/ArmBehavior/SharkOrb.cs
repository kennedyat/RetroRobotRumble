using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "ArmBehavior/SharkOrb")]
public partial class SharkOrb : ArmBehaviorData
{
    public GameObject projectilePrefab;

    // seconds
    public float fullChargeTime = 5;

    // seconds, plus or minus from full charge.
    // you can be in the window without being "fully" charged
    // but eh, just give it the full charge.
    public float justTimeWindow = 0.1f;

    public override IArmBehavior MakeInstance()
    {
        return Instantiate(this);
    }

}

public partial class SharkOrb : IArmBehavior
{
    public bool active;
    public float startChargeTime;

    public void Activate(GameObject owner, ArmInstance arm)
    {
        active = true;
        Debug.Log("he;l;o");
        startChargeTime = Time.fixedTime;
    }

    public void Deactivate(GameObject owner, ArmInstance arm)
    {
        if (active)
        {
            Debug.Log("he;l;o");
            Shoot(owner, arm);
        }
        active = false;
        startChargeTime = 0;
    }

    public void FixedUpdateFromArm(GameObject owner, ArmInstance arm)
    {
        // do nothing.
        // we can do the deltaTime approach if thats easier.
    }

    internal void OnProjectileEnter(Projectile projectile, Collision collision)
    {

    }

    void Shoot(GameObject owner, ArmInstance arm)
    {
        var origin = owner.transform.position + Vector3.up * 1.5f;
        var chargeTime = Time.fixedTime - startChargeTime;
        bool just = Mathf.Abs(chargeTime - fullChargeTime) < justTimeWindow;

        var instance = Instantiate(projectilePrefab);
        var projectile = instance.GetComponent<Projectile>();
        projectile.FollowRay(GetShotPath(origin));

        projectile.transform.localScale *= 1 + 4 * Mathf.Min(1, chargeTime / fullChargeTime);

        Debug.Log(projectile);

        // oops, i didnt plan that out.
        // projectile.originator = null;
    }

    Ray GetShotPath(Vector3 origin)
    {
        Ray cameraRay = new Ray(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward));

        RaycastHit hitInfo;
        bool hit = Physics.Raycast(cameraRay, out hitInfo, 10);
        Vector3 target = hit ? hitInfo.point : cameraRay.origin + cameraRay.direction.normalized * 10;

        Ray actualRay = new Ray();
        actualRay.origin = origin;
        actualRay.direction = target - actualRay.origin;

        return actualRay;
    }
}
