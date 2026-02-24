using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    public class Projectile : MonoBehaviour
    {
    public float maxDistance = 0;
    public float speed = 50f;
    public Ray ray;
    public bool overridden = false;  // If you dont want general projectile behavior

    [Header("Combat")]
    [SerializeField] private int damage = 2;

    public void SetDamage(float newDamage)
    {
        damage = Mathf.Max(0, Mathf.RoundToInt(newDamage));
    }

    public void FollowRay(Ray actualRay, float projectileRange)
    {
        ray = actualRay;
        maxDistance = projectileRange;
        transform.position = actualRay.origin;
        transform.LookAt(ray.origin + ray.direction);
    }

    protected void FixedUpdate()
    {
        if (!overridden)
        {
            transform.position += speed * Time.fixedDeltaTime * ray.direction.normalized;

            if ((transform.position - ray.origin).sqrMagnitude > maxDistance * maxDistance)
            {
                Destroy(this.gameObject);
            }
        }
    }

    protected void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Debug.Log($"[Projectile] Hit Enemy for {damage}!");
            Enemy e = collision.GetComponent<Enemy>();
            if (e != null)
                e.DealDamage(damage);

            Destroy(this.gameObject);
            return;
        }

        Debug.Log("[Projectile] Hit...something!");
        // Destroy(this.gameObject);
    }
}

