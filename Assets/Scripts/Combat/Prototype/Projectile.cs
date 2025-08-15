using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;

namespace Assets.Scripts.Combat.Prototype
{
    public class Projectile : MonoBehaviour
    {
        public float maxDistance = 10;

        public float lifetime = 0;
        public float speed = 50f;
        public Ray ray;

        public void FollowRay(Ray actualRay)
        {
            ray = actualRay;

            transform.position = actualRay.origin;
            transform.LookAt(ray.origin + ray.direction);
        }

        void FixedUpdate()
        {
            transform.position += ray.direction.normalized * speed * Time.fixedDeltaTime;
            lifetime += Time.fixedDeltaTime;

            if ((transform.position - ray.origin).sqrMagnitude > maxDistance * maxDistance || lifetime > 3)
            {
                // a projectile probably doesn't live for more than 3s, right?
                Destroy(this.gameObject);
            }
        }

        // TODO: Currently never called?
        void OnCollisionEnter(Collision collision)
        {

        }
    }

}