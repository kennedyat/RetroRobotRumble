using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BAB_AltColors : MonoBehaviour
{
    [Tooltip("All of the recolored materials available for this part")]
    public Material[] _partMaterials;
    
    public void ChangeMaterial(int materialIndex)
    {
        MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>();

        for (int i = 0; i < meshes.Length; i++)
        {
            meshes[i].material = _partMaterials[materialIndex];
        }
    }
}
