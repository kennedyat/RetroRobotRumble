using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    public class Projectile : MonoBehaviour
    {
        public float maxDistance = 0;
        public float speed = 50f;
        public Ray ray;
        public bool overridden = false;  //If you dont want general projectile behavior

        public void FollowRay(Ray actualRay, float projectileRange)
        {
            ray = actualRay;
            maxDistance = projectileRange;
            transform.position = actualRay.origin;
            transform.LookAt(ray.origin + ray.direction);
        }

        protected void FixedUpdate()
        {
            if(!overridden)
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
            //Lol change when we change layer name. Also hard coded. Ideally will be in components
            if(collision.gameObject.tag == "Enemy")
        {
            Debug.Log("[Projectile] Hit!");
              collision.gameObject.GetComponent<Enemy>().DealDamage(2);
        }
            Debug.Log("[Projectile] Hit...something!");
            //Destroy(this.gameObject);
        }
    }

