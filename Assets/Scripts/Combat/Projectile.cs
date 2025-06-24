using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float lifetime = 0;
    public Vector3 direction;

    void FixedUpdate()
    {
        transform.position += direction.normalized * 20f * Time.fixedDeltaTime;
        lifetime += Time.fixedDeltaTime;


        if (lifetime > 10)
        {
            // a projectile probably doesn't live for more than 10s, right?
            Destroy(this.gameObject);
        }
    }
}
