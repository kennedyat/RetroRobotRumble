using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Obsolete]
public class TabFiltering : MonoBehaviour
{
    [SerializeField] private MechPartType mechPartType;

    void Start()
    {
        StartCoroutine(CullMechParts()); // TODO: Ensure that this called when all mech UI limbs have finished loading in, instead of using delay with coroutine
    }

    IEnumerator CullMechParts()
    {
        yield return new WaitForSeconds(0.01f);
        MechLimbUI[] components = GetComponentsInChildren<MechLimbUI>();
        foreach (MechLimbUI MechPart in components)
        {
            if (MechPart.CurrentPart.mechPartType != mechPartType)
            {
                Destroy(MechPart.gameObject);
            }
        }
    }
}
