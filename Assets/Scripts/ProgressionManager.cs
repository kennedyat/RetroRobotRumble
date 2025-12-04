using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionManager : MonoBehaviour
{
    public void IncreaseDifficulty()
    {
        
    }

    public void UnlockPart()
    {
        int amountParts =  RunData.lockedParts.Count;
        int randomNum = Random.Range(0, amountParts);


       
       

        PartType part = RunData.lockedParts[randomNum];
        GameObject unlockedPart = Instantiate(RunData.lockedParts[randomNum].combatPrefab, GetCenterCamera(), Quaternion.identity);
       
        switch (part)
        {
            case ArmType arm:
                RunData.availableArms.Add(arm);
                break;
            case ChassisType chassis:
                RunData.availableChassis.Add(chassis);
                break;
            case LegType leg:
                RunData.availableLegs.Add(leg);
                break;
        }

        StartCoroutine(DisplayPart(unlockedPart));

      
    }

    IEnumerator DisplayPart(GameObject part)
    {
      

       float time = 0f;

        while (time < 10f)
    {
        part.transform.position = Vector3.Lerp(transform.position, GetCenterCamera(), time/10f);
       
        part.transform.Rotate(Vector3.up * 120f * Time.deltaTime);
        // Add time since last frame
        time += Time.deltaTime;

        yield return null; // wait next frame
    }

             
        

       

        RunData.EndCurrentRun();
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
