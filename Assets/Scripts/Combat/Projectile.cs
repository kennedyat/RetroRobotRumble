using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*ublic class Projectile : MonoBehaviour
{
    /*(public float lifetime = 0;
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
    }*/
    /* public float lifetime = 0f;
    public float maxLifetime = 10f;
    public float speed = 20f;
    public Ray ray;
    
    public float maxRange = Mathf.Infinity;
    private float distanceTraveled = 0f;

    public int pierce = 0;
    private List<Collision> pierced = new List<Collision>();

    // Version without range (for backward compatibility)
    public void FollowRay(Ray actualRay)
    {
        FollowRay(actualRay, Mathf.Infinity);
    }

    // Version with range
    public void FollowRay(Ray actualRay, float range)
    {
        ray = actualRay;
        maxRange = range;
        distanceTraveled = 0f;

        transform.position = actualRay.origin;
        transform.LookAt(ray.origin + ray.direction);
    }

    protected void FixedUpdate()
    {
        float moveDistance = speed * Time.fixedDeltaTime;
        transform.position += moveDistance * ray.direction.normalized;
        distanceTraveled += moveDistance;
        lifetime += Time.fixedDeltaTime;

        // Destroy if exceeded range
        if (distanceTraveled >= maxRange)
        {
            Destroy(this.gameObject);
            return;
        }

        // Destroy after max lifetime
        if (lifetime > maxLifetime)
        {
            Destroy(this.gameObject);
        }
    }

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
*/