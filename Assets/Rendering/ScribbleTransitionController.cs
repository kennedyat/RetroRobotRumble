using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ScribbleTransitionController : MonoBehaviour
{
    public ScribbleTransitionRenderFeature renderFeature;
    [Range(0f, 1f)]
    public float progress = 0;

    void Update()
    {
        if (renderFeature != null)
        {
            renderFeature.settings.progress = progress;
        }
    }
}