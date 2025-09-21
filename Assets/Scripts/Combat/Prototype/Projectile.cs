using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Combat.Prototype
{
    public class Projectile : MonoBehaviour
    {
        public float maxDistance = 0;
        public float speed = 50f;
        public Ray ray;

        public void FollowRay(Ray actualRay, float projectileRange)
        {
            ray = actualRay;
            maxDistance = projectileRange;
            transform.position = actualRay.origin;
            transform.LookAt(ray.origin + ray.direction);
        }

        void FixedUpdate()
        {
            transform.position += ray.direction.normalized * speed * Time.fixedDeltaTime;

            if ((transform.position - ray.origin).sqrMagnitude > maxDistance * maxDistance)
            {
                Destroy(this.gameObject);
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            Destroy(this.gameObject);
        }
    }

}
