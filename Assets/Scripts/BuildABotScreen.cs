using System;
using System.Collections.Generic;
using System.Linq;
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

    [SerializeField] private GameObject _doneButton;

    [SerializeField] private Image[] _tabButtons;
    [SerializeField] private Color _inactiveColor, _activeColor;

    private void Start()
    {
        AddPartsFromRunData(RunData.currentRun);

        FilterPartsList(0);
    }

    private void AddPartsFromRunData(RunData currentRun)
    {
        // TODO: Chassis and legs.

        var availableArms = currentRun.availableArms ?? new List<ArmType>() { null };
        var arms = availableArms.Select((part, index) => AddPartEntry(part, index)).ToList();

        _chassisTarget.Initialize(null);
        if (currentRun.equippedLeftArm is int yay)
        { _leftArmTarget.Initialize(arms[yay]); }
        if (currentRun.equippedRightArm is int yay2)
        { _rightArmTarget.Initialize(arms[yay2]); }
        _legsTarget.Initialize(null);

    }

    private BuildABotEntry AddPartEntry(ScriptableObject part, int index)
    {
        GameObject instance = Instantiate(_partEntryPrefab);
        BuildABotEntry entry = instance.GetComponent<BuildABotEntry>();

        entry.Initialize(part, index);

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

    public void Update()
    {
        bool validRobot = RunData.currentRun.equippedLeftArm is not null;
        validRobot &= RunData.currentRun.equippedRightArm is not null;

        _doneButton.SetActive(validRobot);
    }

    public void DonePressed()
    {
        RRRSceneManager.LoadCombat();
    }
}
