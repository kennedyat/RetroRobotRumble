using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tracer : MonoBehaviour
{
    float lifetime = 0;

    // Tracers shouldn't affect gameplay, so not FixedUpdate is fine
    protected void Update()
    {
        lifetime += Time.deltaTime;
        if (lifetime > 4 / 60f)
        {
            Destroy(gameObject);
        }
    }
}
