using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Transform cam;

    protected void LateUpdate()
    {
        transform.LookAt(transform.position + cam.forward);
    }
}
