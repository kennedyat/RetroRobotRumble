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

    //Tutorial 
    

    private void Update()
    {
        doneButton.SetActive(chassisEquip.equippedPart != null &&
                             leftArmEquip.equippedPart != null &&
                             rightArmEquip.equippedPart != null &&
                             legsEquip.equippedPart != null);
    }

    public void PressDone()
    {
        GameObject chassisPrefab = chassisEquip.equippedPart;
        GameObject leftArmPrefab = leftArmEquip.equippedPart;
        GameObject rightArmPrefab = rightArmEquip.equippedPart;
        GameObject legsPrefab = legsEquip.equippedPart;

        if (chassisPrefab != null)
        {
            RunData.currentRun.equippedChassis = chassisPrefab.GetComponent<BAB_PartPrefab>().runDataIndex;
        }
        if (leftArmPrefab != null)
        {
            RunData.currentRun.equippedLeftArm = leftArmPrefab.GetComponent<BAB_PartPrefab>().runDataIndex;
        }
        if (rightArmPrefab != null)
        {
            RunData.currentRun.equippedRightArm = rightArmPrefab.GetComponent<BAB_PartPrefab>().runDataIndex;
        }
        if (legsPrefab != null)
        {
            RunData.currentRun.equippedLegs = legsPrefab.GetComponent<BAB_PartPrefab>().runDataIndex;
        }
        if(RunData.currentRound == 0)
            RRRSceneManager.LoadCombatTutorial();
        else
            RRRSceneManager.LoadCombat();
    }
}
