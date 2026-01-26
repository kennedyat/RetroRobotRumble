using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereReticle : MonoBehaviour
{
    float time;
    float speed;
    float radius;
    public void Init(float t, float r)
    {
        radius = r;
        time = t;
        speed = radius / time;

        Destroy(gameObject, t);
    }
}
