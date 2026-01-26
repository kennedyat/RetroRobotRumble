using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineReticle : MonoBehaviour
{
    float length;
    float time;
    float speed;
    float width;

    public void Init(float l, float t, float w)
    {
        time = t;
        length = l;
        width = w;

        // PHYSICS IS USEFUL LETS GOOOOO
        speed = length / time;

        // set the y scale of this object to half the length
        // width also needs scale
        transform.localScale = new(width, length / 2, 1f);

        // deactivate this reticle after some time
        Invoke(nameof(Deactivate), time);
    }

    void Update()
    {
        // use this for the reticles that fill themselves
        // but too bad im lazy so not yet
    }

    void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
