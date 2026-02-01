using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PartDebug : MonoBehaviour
{
    [Header("Check Debug")]
    [SerializeField] public bool isDebug;
    [Header("Test Parts - Populate in Inspector")]
    [SerializeField] private List<ArmType> testArms = new List<ArmType>();
    [SerializeField] private List<ChassisType> testChassis = new List<ChassisType>();
    [SerializeField] private List<LegType> testLegs = new List<LegType>();

    [Header("UI Parents - Assign Grid Layout Groups")]
    [SerializeField] private GameObject canvas;
    [SerializeField] private Transform armButtonParent;
    [SerializeField] private Transform chassisButtonParent;
    [SerializeField] private Transform legButtonParent;
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private Animator animator;
    [SerializeField] private HitBoxManager hitBoxManager;
    private PlayerInitializer playerInitializer;

    protected void Awake()
    {
        // Initialize RunData lists
        RunData.availableArms = testArms;
        RunData.availableChassis = testChassis;
        RunData.availableLegs = testLegs;
        RunData.lockedParts = new List<PartType>();

        // Set default equipment using INDICES
        // equippedLeftArm = 0 means "use testArms[0]"
        // equippedRightArm = 1 means "use testArms[1]"
        RunData.currentRun.equippedLeftArm = testArms.Count > 0 ? 0 : (int?)null;
        RunData.currentRun.equippedRightArm = testArms.Count > 1 ? 1 : (int?)null;
        RunData.currentRun.equippedChassis = testChassis.Count > 0 ? 0 : 0;
        RunData.currentRun.equippedLegs = testLegs.Count > 0 ? 0 : 0;

        Debug.Log($"RunData initialized - Left Arm Index: {RunData.currentRun.equippedLeftArm}, Right Arm Index: {RunData.currentRun.equippedRightArm}");
    }

    protected void Start()
    {
        playerInitializer = FindObjectOfType<PlayerInitializer>();
        
        // Generate buttons - cast to List<PartType>
        GenerateButtons(armButtonParent, testArms.ConvertAll(x => (PartType)x), EquipArm);
        GenerateButtons(chassisButtonParent, testChassis.ConvertAll(x => (PartType)x), EquipChassis);
        GenerateButtons(legButtonParent, testLegs.ConvertAll(x => (PartType)x), EquipLegs);
        
        Debug.Log($"[PartDebug] Total Arms: {testArms.Count}, Chassis: {testChassis.Count}, Legs: {testLegs.Count}");
    }

    private void Update()
    {
        // Force cursor visible every frame (in case something is hiding it)
        if(Input.GetKeyDown(KeyCode.Escape))
    {
        UnityEngine.InputSystem.PlayerInput playerInput = FindObjectOfType<UnityEngine.InputSystem.PlayerInput>();        
        // Toggle canvas state
        bool isCurrentlyActive = canvas.activeSelf;
        canvas.SetActive(!isCurrentlyActive);
        
        if (!isCurrentlyActive) // Canvas is now being shown
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            
            if (playerInput != null)
            {
                playerInput.DeactivateInput();
            }
            animator.enabled = false;
            
        }
        else // Canvas is now being hidden
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            
            if (playerInput != null)
            {
                playerInput.ActivateInput();
            }
            animator.enabled = true;
        }
    }
        
      
    }

    private void GenerateButtons(Transform parent, List<PartType> parts, System.Action<int> onClickAction)
    {
        if (parent == null || buttonPrefab == null)
        {
            Debug.LogWarning("Parent or button prefab is null!");
            return;
        }

        Debug.Log($"[GenerateButtons] Starting for {parent.name} with {parts.Count} parts");

        // Clear existing buttons
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }

        // Create button for each part
        for (int i = 0; i < parts.Count; i++)
        {
            if (parts[i] == null)
            {
                Debug.LogWarning($"[GenerateButtons] Part at index {i} is null, skipping");
                continue;
            }

            int index = i; // Capture index for closure
            Button btn = Instantiate(buttonPrefab, parent);
            btn.gameObject.name = parts[i].partCommonData.name; // Set GameObject name
            
            // Set button text to part name
            Text btnText = btn.GetComponentInChildren<Text>();
             TMPro.TextMeshProUGUI tmpText = btn.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = parts[i].name;
                Debug.Log($"[GenerateButtons] Created button: {parts[i].partCommonData.name}");
            }
            else
            {
                Debug.LogWarning($"[GenerateButtons] Button has no Text component!");
            }
            
            // Add click listener
            btn.onClick.AddListener(() => onClickAction(index));
        }

        Debug.Log($"[GenerateButtons] Generated {parts.Count} buttons in {parent.name}");
    }

    private void EquipArm(int index)
    {
        // Toggle: Left -> Right -> Unequip
        if (RunData.currentRun.equippedLeftArm == index)
        {
            // Currently on left, move to right
           // RunData.currentRun.equippedLeftArm = null;
            RunData.currentRun.equippedRightArm = index;
            Debug.Log($"Moved to RIGHT arm: {testArms[index].partCommonData.name}");
        }
        else if (RunData.currentRun.equippedRightArm == index)
        {
            // Currently on right, unequip
            //RunData.currentRun.equippedRightArm = null;
            Debug.Log($"Unequipped arm: {testArms[index].partCommonData.name}");
        }
        else
        {
            // Not equipped, equip to left
            RunData.currentRun.equippedLeftArm = index;
            Debug.Log($"Equipped LEFT arm: {testArms[index].partCommonData.name}");
        }
        
        ReloadPlayer();
    }

    private void EquipChassis(int index)
    {
        RunData.currentRun.equippedChassis = index;
        Debug.Log($"Equipped chassis: {testChassis[index].partCommonData.name}");
        ReloadPlayer();
    }

    private void EquipLegs(int index)
    {
        RunData.currentRun.equippedLegs = index;
        Debug.Log($"Equipped legs: {testLegs[index].partCommonData.name}");
        ReloadPlayer();
    }

    private void ReloadPlayer()
    {
        //animator.enabled = false;
        hitBoxManager.ClearHitBox();
        //hitBoxManager.Disable();
        if (playerInitializer != null)
        {
            playerInitializer.SendMessage("Start", SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            Debug.LogWarning("PlayerInitializer not found!");
        }
    }
}