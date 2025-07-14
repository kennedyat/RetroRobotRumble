using Unity.VisualScripting.AssemblyQualifiedNameParser;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class BuildABotScreen : MonoBehaviour
{
    private IGetSetPlayerEquips _playerEquips;

    [SerializeField] private Transform _partEntryList;
    [SerializeField] private GameObject _partEntryPrefab;

    [SerializeField] private BuildABotDropTarget _chassisTarget;
    [SerializeField] private BuildABotDropTarget _leftArmTarget;
    [SerializeField] private BuildABotDropTarget _rightArmTarget;
    [SerializeField] private BuildABotDropTarget _legsTarget;

    [SerializeField] private Image[] _tabButtons;
    [SerializeField] private Color _inactiveColor, _activeColor;

    private void Start()
    {
        FilterPartsList(0);
    }

    private BuildABotEntry AddPartEntry(ScriptableObject part, bool equipped)
    {
        GameObject instance = Instantiate(_partEntryPrefab);
        BuildABotEntry entry = instance.GetComponent<BuildABotEntry>();

        entry.Initialize(part, equipped);

        instance.transform.SetParent(_partEntryList.transform);
        entry.GetComponent<RectTransform>().localScale = Vector3.one;

        return entry;
    }

    public void FilterPartsList(int tab)
    {
        foreach (Image im in _tabButtons)
        {
            im.color = _inactiveColor;
            // We could also make the active button larger in size as well
        }
        _tabButtons[tab].color = _activeColor;

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

        var equippedChassis = AddPartEntry(playerEquips.GetChassis(), true);
        foreach (ChassisType chassisSingular in chassis)
        {
            AddPartEntry(chassisSingular, false);
        }

        var equippedLeftArm = AddPartEntry(playerEquips.GetLeftArm(), true);
        var equippedRightArm = AddPartEntry(playerEquips.GetRightArm(), true);
        foreach (ArmType arm in arms)
        {
            AddPartEntry(arm, false);
        }

        var equippedLegs = AddPartEntry(playerEquips.GetLegs(), true);
        foreach (LegType leg in legs)
        {
            AddPartEntry(leg, false);
        }

        _chassisTarget.Initialize(equippedChassis, playerEquips);
        _leftArmTarget.Initialize(equippedLeftArm, playerEquips);
        _rightArmTarget.Initialize(equippedRightArm, playerEquips);
        _legsTarget.Initialize(equippedLegs, playerEquips);
    }

    public bool IsOpen()
    {
        // This shouldn't always be true.
        return true;
    }
}
