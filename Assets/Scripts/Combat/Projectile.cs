using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float lifetime = 0;
    public Ray ray;

    public int pierce = 0;
    List<Collision> pierced = new List<Collision>();

    public void FollowRay(Ray actualRay)
    {
        ray = actualRay;

        transform.position = actualRay.origin;
        transform.LookAt(ray.origin + ray.direction);
    }

    protected void FixedUpdate()
    {
        transform.position += 20f * Time.fixedDeltaTime * ray.direction.normalized;
        lifetime += Time.fixedDeltaTime;

        if (lifetime > 10)
        {
            // a projectile probably doesn't live for more than 10s, right?
            Destroy(this.gameObject);
        }
    }

    // TODO: Currently never called?
    protected void OnCollisionEnter(Collision collision)
    {
        if (pierced.Count >= pierce)
        {
            Destroy(this.gameObject);
        }
        else
        {
            pierced.Add(collision);
        }
    }
}
