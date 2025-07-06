using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    public bool followRotation = false;

    // Update is called once per frame
    void Update()
    {
        transform.position = target.position;
        if (followRotation)
        {
            transform.rotation = target.rotation;
        }
    }
}
