using System.Collections;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;

public class BAB_SpawnParts : MonoBehaviour
{
    [SerializeField, Tooltip("All currently available parts to spawn into the box")] GameObject[] _parts; // replace/populate with something that reads from a data struct of all the player's unlocked parts
    private Collider spawnArea;
    [SerializeField, Tooltip("The parent object for all spawned parts")] Transform _partsParent;
    [SerializeField, Tooltip("lmao")] bool _enableFunnyTestFeature = false;
    [SerializeField, Tooltip("lmao xd")] bool _enableEvenFunnierTestFeature = false;

    void Start()
    {
        spawnArea = GetComponent<Collider>();

        SpawnParts();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _enableFunnyTestFeature)
        {
            SpawnParts();
        }
        if (Input.GetKey(KeyCode.Space) && _enableEvenFunnierTestFeature)
        {
            SpawnParts();
        }
    }

    private void SpawnParts()
    {

        for (int i = 0; i < _parts.Length; i++)
        {
            BAB_AltColors altColors = _parts[i].GetComponent<BAB_AltColors>(); // arm specific for now, will need to be changed

            for (int j = 0; j < altColors._partMaterials.Length; j++)
            {
                GameObject spawnedPart = Instantiate(_parts[i], GenerateSpawnPosition(spawnArea.bounds), GenerateSpawnRotation(), _partsParent);
                spawnedPart.GetComponent<BAB_AltColors>().ChangeMaterial(j);
            }
        }
    }

    private Vector3 GenerateSpawnPosition(Bounds bounds)
    {
        return new Vector3(Random.Range(bounds.min.x, bounds.max.x),
                           Random.Range(bounds.min.y, bounds.max.y),
                           Random.Range(bounds.min.z, bounds.max.z));
    }

    private Quaternion GenerateSpawnRotation() {
        return Quaternion.Euler(Random.Range(0, 359), Random.Range(0, 359), Random.Range(0, 359));
    }
}
