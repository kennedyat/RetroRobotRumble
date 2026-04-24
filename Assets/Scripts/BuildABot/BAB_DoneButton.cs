using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BAB_DoneButton : MonoBehaviour
{
    [SerializeField] BAB_EquipPart chassisEquip;
    [SerializeField] BAB_EquipPart leftArmEquip;
    [SerializeField] BAB_EquipPart rightArmEquip;
    [SerializeField] BAB_EquipPart legsEquip;

    [SerializeField] GameObject doneButton;
    [SerializeField] GameObject testButton;
    private bool allEquipped = true;

    //Tutorial 
    

    private void Update()
    {
        doneButton.SetActive(chassisEquip.equippedPart != null &&
                             leftArmEquip.equippedPart != null &&
                             rightArmEquip.equippedPart != null &&
                             legsEquip.equippedPart != null);

        /*testButton.SetActive(chassisEquip.equippedPart != null &&
                             leftArmEquip.equippedPart != null &&
                             rightArmEquip.equippedPart != null &&
                             legsEquip.equippedPart != null);*/

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
        }
        else if (RunData.currentRound >= 4) // CHANGE BACK TO 4
        {
            RRRSceneManager.LoadFinalBoss();
        } else
        {
            RRRSceneManager.LoadCombatCutscene();
        }
    }

       public void PressTest()
    {
        SavePartsToRunData();
        RRRSceneManager.LoadTestScene();
    }
}
