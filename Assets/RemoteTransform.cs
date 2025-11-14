using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Copies the global position and rotation of something else if set.
/// </summary>
///
/// Inspired by the node from Godot.
///
/// TODO: Copy children transforms from the transforms of a similar remote heirarchy.
/// Match shoulder to shoulder, elbow to elbow, hand to hand, etc.
public class RemoteTransform : MonoBehaviour
{
    public Transform remote; // nullable

    protected void LateUpdate()
    {
        if (remote == null)
        {
            return;
        }

        transform.SetPositionAndRotation(remote.position, remote.rotation);
    }
}
