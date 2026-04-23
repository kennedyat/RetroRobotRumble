using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TestSceneManager : MonoBehaviour
{
    [Header("Back to BAB")]
    [SerializeField] Button backToBabButton;

    [Header("Current Loadout Display")]
    [SerializeField] TextMeshProUGUI loadoutText;

    [Header("Enemy Spawner Panel")]
    [SerializeField] GameObject spawnerPanel;
    



    private void Start()
    {
        backToBabButton.onClick.AddListener(ReturnToBAB);
        

        

        LockCursor();
        UpdateLoadoutDisplay();
    }

    private void Update()
    {
        if (spawnerPanel.activeSelf)
            FreeCursor();

    }

    private void ReturnToBAB()
    {
   
        RRRSceneManager.LoadBuildABot();
    }

    private void FreeCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void UpdateLoadoutDisplay()
    {
        if (loadoutText == null) return;

        var run = RunData.currentRun;
        string leftArm  = run.equippedLeftArm.HasValue  ? RunData.availableArms[run.equippedLeftArm.Value].partCommonData.name  : "None";
        string rightArm = run.equippedRightArm.HasValue ? RunData.availableArms[run.equippedRightArm.Value].partCommonData.name : "None";
        string chassis  = RunData.availableChassis[run.equippedChassis].partCommonData.name;
        string legs     = RunData.availableLegs[run.equippedLegs].partCommonData.name;

        loadoutText.text = $"L: {leftArm}  |  R: {rightArm}  |  {chassis}  |  {legs}";
    }



}