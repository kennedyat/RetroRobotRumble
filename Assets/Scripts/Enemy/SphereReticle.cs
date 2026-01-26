using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereReticle : MonoBehaviour
{
    float time;
    float speed;
    float radius;
    public void Init(float d, float t, float radius)
    {

        Destroy(gameObject, t);
    }
}
