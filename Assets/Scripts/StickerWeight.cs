using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StickerRarity
{
    Common,
    Rare,
    Legendary
}
public class StickerWeight : MonoBehaviour
{
    //Give weight to stickers
    [SerializeField] private int commonWeight = 75;
    [SerializeField] private int rareWeight = 20;
    [SerializeField] private int legendaryWeight = 5;

    
    public  StickerRarity GetStickerRarity()
    {
        //Get total weight (cwhich can change, prolly needs balancing)
        int totalWeight = commonWeight + rareWeight + legendaryWeight;

        //gacha game lol
        float roll = Random.Range(0f, totalWeight);

        int currentWeight = 0;

        currentWeight += commonWeight;
        if(roll<=currentWeight)
        {
            return StickerRarity.Common;
        }
        currentWeight+= rareWeight;
        if(roll<=currentWeight)
        {
             return StickerRarity.Rare;
        }
        currentWeight+= legendaryWeight;
        if(roll<=currentWeight)
        {
             return StickerRarity.Legendary;
        }
        return StickerRarity.Common;
        
    }

}
