using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class VFXPlaceHolder : MonoBehaviour
{

    private TrailRenderer _trail;
    public GameObject playerLimb;

    // Update is called once per frame
    protected void Update()
    {
        if (playerLimb != null)
            transform.position = playerLimb.transform.position;

    }
}
