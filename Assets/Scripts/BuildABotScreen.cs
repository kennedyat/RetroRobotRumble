using Unity.VisualScripting.AssemblyQualifiedNameParser;
using UnityEngine;

public partial class BuildABotScreen : MonoBehaviour
{
    private IGetSetPlayerEquips _playerEquips;

    [SerializeField] private GameObject _partEntryList;
    [SerializeField] private GameObject _partEntryPrefab;

    private void AddPartEntry(ScriptableObject part, bool equipped)
    {
        GameObject instance = Instantiate(_partEntryPrefab);
        BuildABotEntry entry = instance.GetComponent<BuildABotEntry>();

        entry.Initialize(part, equipped);

        instance.transform.SetParent(_partEntryList.transform);
        entry.GetComponent<RectTransform>().localScale = Vector3.one;
    }

    public void FilterPartsList(int tab)
    {
        BuildABotEntry[] entries = _partEntryList.GetComponentsInChildren<BuildABotEntry>(includeInactive: true);
        foreach (BuildABotEntry entry in entries)
        {
            if (tab == 0)
            {
                entry.gameObject.SetActive(true);
            }
            else if (tab == 1)
            {
                entry.gameObject.SetActive(entry.PartIsChassis());
            }
            else if (tab == 2)
            {
                entry.gameObject.SetActive(entry.PartIsArm());
            }
            else if (tab == 3)
            {
                entry.gameObject.SetActive(entry.PartIsLegs());
            }
            else
            {
                // why
            }
        }
    }
}

public partial class BuildABotScreen : IOpenEquipScreen
{
    public void InitFromParts(ChassisType[] chassis, ArmType[] arms, LegType[] legs, IGetSetPlayerEquips playerEquips)
    {
        _playerEquips = playerEquips;
        AddPartEntry(playerEquips.GetChassis(), true);

        foreach (ChassisType chassisSingular in chassis)
        {
            AddPartEntry(chassisSingular, false);
        }

        AddPartEntry(playerEquips.GetLeftArm(), true);
        AddPartEntry(playerEquips.GetRightArm(), true);
        foreach (ArmType arm in arms)
        {
            AddPartEntry(arm, false);
        }

        AddPartEntry(playerEquips.GetLegs(), true);
        foreach (LegType leg in legs)
        {
            AddPartEntry(leg, false);
        }
    }

    public bool IsOpen()
    {
        // This shouldn't always be true.
        return true;
    }
}
