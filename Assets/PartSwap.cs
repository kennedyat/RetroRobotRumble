using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PartSwap : MonoBehaviour
{
    [Header("Test Parts - Populate in Inspector")]
    [SerializeField] private List<ArmType> testArms = new List<ArmType>();
    [SerializeField] private List<ChassisType> testChassis = new List<ChassisType>();
    [SerializeField] private List<LegType> testLegs = new List<LegType>();

    [Header("UI")]
    [SerializeField] private TMP_Dropdown leftArmDropdown;
    [SerializeField] private TMP_Dropdown rightArmDropdown;
    [SerializeField] private TMP_Dropdown chassisDropdown;
    [SerializeField] private TMP_Dropdown legDropdown;
    [SerializeField] private Button applyButton;

    [Header("References")]
    [SerializeField] private HitBoxManager hitBoxManager;

    private PlayerInitializer playerInitializer;

    private void Awake()
    {
        RunData.availableArms = testArms;
        RunData.availableChassis = testChassis;
        RunData.availableLegs = testLegs;
        RunData.lockedParts = new List<PartType>();
    }

    private void Start()
    {
        playerInitializer = FindObjectOfType<PlayerInitializer>();

        PopulateDropdown(leftArmDropdown,  testArms.ConvertAll(x => x.partCommonData.name));
        PopulateDropdown(rightArmDropdown, testArms.ConvertAll(x => x.partCommonData.name));
        PopulateDropdown(chassisDropdown,  testChassis.ConvertAll(x => x.partCommonData.name));
        PopulateDropdown(legDropdown,      testLegs.ConvertAll(x => x.partCommonData.name));

        if (RunData.currentRun.equippedLeftArm.HasValue)
            leftArmDropdown.value = RunData.currentRun.equippedLeftArm.Value;
        if (RunData.currentRun.equippedRightArm.HasValue)
            rightArmDropdown.value = RunData.currentRun.equippedRightArm.Value;
        chassisDropdown.value = RunData.currentRun.equippedChassis;
        legDropdown.value     = RunData.currentRun.equippedLegs;

        applyButton.onClick.AddListener(ApplyParts);
    }

    private void PopulateDropdown(TMP_Dropdown dropdown, List<string> names)
    {
        if (dropdown == null) return;
        dropdown.ClearOptions();
        dropdown.AddOptions(names);
    }

    private void ApplyParts()
    {
        RunData.currentRun.equippedLeftArm  = leftArmDropdown.value;
        RunData.currentRun.equippedRightArm = rightArmDropdown.value;
        RunData.currentRun.equippedChassis  = chassisDropdown.value;
        RunData.currentRun.equippedLegs     = legDropdown.value;

        hitBoxManager.ClearHitBox();
        if (playerInitializer != null)
            playerInitializer.SendMessage("Start", SendMessageOptions.DontRequireReceiver);
        else
            Debug.LogWarning("[PartSwap] PlayerInitializer not found!");
    }

    private void OnDestroy()
{
    RunData.availableArms = null;
    RunData.availableChassis = null;
    RunData.availableLegs = null;
}
}