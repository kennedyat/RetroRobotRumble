using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AK.Wwise;

public class BAB_DoneButton : MonoBehaviour
{
    [SerializeField] BAB_EquipPart chassisEquip;
    [SerializeField] BAB_EquipPart leftArmEquip;
    [SerializeField] BAB_EquipPart rightArmEquip;
    [SerializeField] BAB_EquipPart legsEquip;

    [SerializeField] GameObject doneButton;
    [SerializeField] GameObject testButton;
    [SerializeField] AK.Wwise.Event DoneButtonSFX;
    [SerializeField] AK.Wwise.Event GoBackSFX;
    private bool allEquipped = true;

    //Tutorial 
    

    private void Update()
    {
        doneButton.SetActive(chassisEquip.equippedPart != null &&
                             leftArmEquip.equippedPart != null &&
                             rightArmEquip.equippedPart != null &&
                             legsEquip.equippedPart != null);

        testButton.SetActive(chassisEquip.equippedPart != null &&
                             leftArmEquip.equippedPart != null &&
                             rightArmEquip.equippedPart != null &&
                             legsEquip.equippedPart != null);

        if (allEquipped && doneButton.activeSelf && RunData.currentRound == 0)
        {
            TutorialManager.Instance.AdvanceStep(); 
            allEquipped = false;
        }
      
    }

     private void SavePartsToRunData()
    {
        if (chassisEquip.equippedPart != null)
            RunData.currentRun.equippedChassis = chassisEquip.equippedPart.GetComponent<BAB_PartPrefab>().runDataIndex;
        if (leftArmEquip.equippedPart != null)
            RunData.currentRun.equippedLeftArm = leftArmEquip.equippedPart.GetComponent<BAB_PartPrefab>().runDataIndex;
        if (rightArmEquip.equippedPart != null)
            RunData.currentRun.equippedRightArm = rightArmEquip.equippedPart.GetComponent<BAB_PartPrefab>().runDataIndex;
        if (legsEquip.equippedPart != null)
            RunData.currentRun.equippedLegs = legsEquip.equippedPart.GetComponent<BAB_PartPrefab>().runDataIndex;
    }

    public void PressDone()
    {
  
         SavePartsToRunData();

        
        if (RunData.currentRound == 0)
        {
            RRRSceneManager.LoadCombatTutorial();
            DoneButtonSFX.Post(gameObject);   // DONE / CONFIRM SFX   
        }
        else if (RunData.currentRound >= 4) // CHANGE BACK TO 4
        {
            RRRSceneManager.LoadFinalBoss();
            GoBackSFX.Post(gameObject);   // GO BACK SFX  
        } else
        {
            RRRSceneManager.LoadCombat();
            GoBackSFX.Post(gameObject);   // GO BACK SFX   
        }
    }

       public void PressTest()
    {
        SavePartsToRunData();
        RRRSceneManager.LoadTestScene();
    }
}
