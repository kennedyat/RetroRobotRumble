using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabFiltering : MonoBehaviour
{
    [SerializeField] private MechPartType mechPartType;

    void Start()
    {
        MechLimbUI[] components = GetComponentsInChildren<MechLimbUI>();
        foreach (MechLimbUI MechPart in components)
        {
            if (MechPart.CurrentPart.mechPartType != mechPartType)
            {
                Destroy(MechPart.gameObject);
            }
        }
    }

    // IEnumerator CullMechParts()
    // {
    // }
}
