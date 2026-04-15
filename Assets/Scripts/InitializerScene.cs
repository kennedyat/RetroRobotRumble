using System.Collections.Generic;
using UnityEngine;

// This initializer scene is more an example of interacting with RunData and RRRSceneManager.
public class InitializerScene : MonoBehaviour
{
     public bool startTutorial;
    public bool startTest;
    public List<ArmType> arms;
    public List<ChassisType> chassis;
    public List<LegType> legs;

    public List<PartType> parts;
    public List<Sticker> stickers;
    public List<Sticker> commonStickers;
    public List<Sticker> rareStickers;
    public List<Sticker> legendaryStickers;

   

    protected void Start()
    {
        RunData.availableArms = arms;
        RunData.availableChassis = chassis;
        RunData.availableLegs = legs;
        RunData.lockedParts = parts;

        RunData.commonStickers = commonStickers;
        RunData.rareStickers = rareStickers;
        RunData.legendaryStickers = legendaryStickers;
        RunData.availableStickers = stickers;

        
         if(startTest)
        {
            RunData.test = true; 
         
        }else if(startTutorial)
        {
            RunData.currentRound = 0;   
        }
        else
        {
            RunData.currentRound = 1;
        }

        // For now, just jump directly to BAB.
        RRRSceneManager.LoadBuildABot();
    }
}
