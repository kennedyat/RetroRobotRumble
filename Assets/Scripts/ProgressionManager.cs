using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressionManager : MonoBehaviour
{

    private GameObject part;
    public bool unlock;
    public GameObject[] unlockUI;
    public StickerWeight stickerWeight;
    
    private PartType currentUnlockedPart;

  
    protected void Update()
    {
        if(unlock)
        {
            
            DisplayPart();
        }
    }

    public void StickerGenerator()
    {
        
    }

     public Sticker GetUnlockSticker()
    {   
        StickerRarity rarity = stickerWeight.GetStickerRarity(); // Added parentheses!
        List<Sticker> stickerList = null;

        switch(rarity)
        {
            case StickerRarity.Common:
                stickerList = RunData.commonStickers;
                break;  // IMPORTANT: Add break statements!
            case StickerRarity.Rare:
                stickerList = RunData.rareStickers;
                break;
            case StickerRarity.Legendary:
                stickerList = RunData.legendaryStickers;  // Fixed typo: legendaryStickersStickers
                break;
        }
        switch(rarity)
        {
            case StickerRarity.Common:
                stickerList = RunData.commonStickers;
                break;
            case StickerRarity.Rare:
                stickerList = RunData.rareStickers;
                break;
            case StickerRarity.Legendary:
                stickerList = RunData.legendaryStickers;
                break;


        }

        if (stickerList != null && stickerList.Count > 0)
        {
            int randomIndex = Random.Range(0, stickerList.Count);
            return stickerList[randomIndex];
        }

        return null;
        //RunData.availableStickers.Add( RunData.lockedStickers[0]);
        //RunData.lockedStickers.RemoveAt(0);

    }
    public void UnlockSticker(Sticker sticker)
    {
        // Add to available stickers
        RunData.availableStickers.Add(sticker);
        
    }

    public void UnlockPart()
    {
      if (RunData.lockedParts.Count > 0)
        {
            PartType type = RunData.lockedParts[0];
            
            switch (type)
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

            currentUnlockedPart = type;
            RunData.lockedParts.RemoveAt(0);
        }

        
    
    }

    public PartType GetUnlockedPart()
    {
        return currentUnlockedPart;
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
