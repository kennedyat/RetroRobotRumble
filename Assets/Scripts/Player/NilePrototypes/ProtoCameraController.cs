using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtoCameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private bool followRotation = false;

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
