using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class FB_Spotlight : MonoBehaviour
{
    Light sLight;

    public void Init(float r, float time)
    {
        sLight = GetComponent<Light>();

        // boost the radius slightly because of how light works
        r *= 1.05f;
        float height = transform.position.y;
        float angle = Mathf.Atan2(r, height) * Mathf.Rad2Deg * 2;
        sLight.spotAngle = angle;
        sLight.innerSpotAngle = angle;

        Destroy(gameObject, time);
    }
}