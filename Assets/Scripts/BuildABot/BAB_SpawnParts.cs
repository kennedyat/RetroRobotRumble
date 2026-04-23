using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BAB_SpawnParts : MonoBehaviour
{
    [SerializeField, Tooltip("All currently available parts to spawn into the box")] List<GameObject> _parts;
    private Collider spawnArea;
    [SerializeField, Tooltip("The parent object for all spawned parts")] Transform _partsParent;
    [SerializeField, Tooltip("lmao")] bool _enableFunnyTestFeature = false;
    [SerializeField, Tooltip("lmao xd")] bool _enableEvenFunnierTestFeature = false;

    public GameObject tutorialManager;

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

          if (RunData.currentRound == 0)
        {
            tutorialManager.SetActive(true);   
        }
        else
        {
            tutorialManager.SetActive(false);  
        }

        AddPartsFromRunData(RunData.currentRun);

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

    private void AddPartsFromRunData(RunData currentRun)
    {
        List<ArmType> availableArms = RunData.availableArms ?? new List<ArmType>() { null };
        for (int i = 0; i < availableArms.Count; i++)
        {
            _parts.Add(availableArms[i].BABPrefab);
            availableArms[i].BABPrefab.GetComponent<BAB_PartPrefab>().runDataIndex = i;
        }

        List<ChassisType> availableChassis = RunData.availableChassis ?? new List<ChassisType>() { null };
        for (int i = 0; i < availableChassis.Count; i++)
        {
            _parts.Add(availableChassis[i].BABPrefab);
            availableChassis[i].BABPrefab.GetComponent<BAB_PartPrefab>().runDataIndex = i;
        }

        List<LegType> availableLegs = RunData.availableLegs ?? new List<LegType>() { null };
        for (int i = 0; i < availableLegs.Count; i++)
        {
            _parts.Add(availableLegs[i].BABPrefab);
            availableLegs[i].BABPrefab.GetComponent<BAB_PartPrefab>().runDataIndex = i;
        }
    }

    private void SpawnParts()
    {

        for (int i = 0; i < _parts.Count; i++)
        {
            BAB_PartPrefab partPrefab = _parts[i].GetComponent<BAB_PartPrefab>();

            for (int j = 0; j < partPrefab._partMaterials.Length; j++)
            {
                GameObject spawnedPart = Instantiate(_parts[i], GenerateSpawnPosition(spawnArea.bounds), GenerateSpawnRotation(), _partsParent);
                spawnedPart.GetComponent<BAB_PartPrefab>().ChangeMaterial(j);
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
