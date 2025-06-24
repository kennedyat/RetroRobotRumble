using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Vector3 direction;

    void FixedUpdate()
    {
        transform.position += direction.normalized * 5f * Time.fixedDeltaTime;
    }
}
