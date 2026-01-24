using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressionManager : MonoBehaviour
{

    private GameObject part;
    public bool unlock;
    public GameObject[] unlockUI;

    protected void Update()
    {
        if(unlock)
        {
            
            DisplayPart();
        }
    }

     public void UnlockSticker()
    {
        RunData.availableStickers.Add( RunData.lockedStickers[0]);
        RunData.lockedStickers.RemoveAt(0);
    }
    public void UnlockPart()
    {
        int amountParts =  RunData.lockedParts.Count;
        int randomNum = Random.Range(0, amountParts);

     if(RunData.lockedParts.Count>0)
        {
            PartType type = RunData.lockedParts[0];
            //GameObject unlockedPart = Instantiate(RunData.lockedParts[randomNum].combatPrefab, GetCenterCamera(), Quaternion.identity);

            foreach (GameObject ui in unlockUI)
            {
                ui.SetActive(true);
            }

           if (type.partSprite != null)
            {
                unlockUI[1].GetComponent<Image>().sprite = type.partSprite;
            }

            switch (type)
            {
                case ArmType arm:
                    RunData.availableArms.Add(arm);
                    //Destroy(unlockedPart.GetComponent<ArmBehavior>());
                    break;
                case ChassisType chassis:
                    RunData.availableChassis.Add(chassis);
                    //Destroy(unlockedPart.GetComponent<ChassisBehavior>());
                    break;
                case LegType leg:
                    RunData.availableLegs.Add(leg);
                    //Destroy(unlockedPart.GetComponent<LegBehavior>());
                    break;
            }

            //part = unlockedPart;
            //part.transform.position  = GetCenterCamera();
            RunData.lockedParts.RemoveAt(0);
        }
       
       

       
      
    }


    private void DisplayPart()
    {
         Vector3 targetPosition = GetCenterCamera();
      
    }      
        

    private Vector3 GetCenterCamera()
    {

        Camera mainCamera = Camera.main; 


        Vector3 viewportCenter = new Vector3(0.5f, 0.5f, 0f); 

        float distanceFromCamera = 7f; // Example distance
        Vector3 worldCenter = mainCamera.ViewportToWorldPoint(new Vector3(viewportCenter.x, viewportCenter.y, distanceFromCamera)); 
        return worldCenter;
    }
}
