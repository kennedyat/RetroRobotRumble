using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Stats
{
    public int attackDamage;
    public int criticalChance;
    public int maxHealth;
    public int moveSpeed;
    public int attackSpeed;
    public int specialCooldown;
    public int ultimateCharge;
    public int lifesteal;
    public int damageRes;
    public int stickerBoost;
    public int holoDrop;
}
public class StickerBehavior : MonoBehaviour
{
    public static StickerBehavior Instance { get; private set; }
    public Stats currentStickerMods;
    private PlayerHealth playerHealth;
    public void Awake()
    {
        Instance = this;
        currentStickerMods = new Stats(); // Correlate to current run perhaps?  
        if(RunData.availableStickers!=null)   
            AddStickerModifications(RunData.availableStickers);   
    }
    protected void AddStickerModifications(List<Sticker> stickers)
    {
        /* Add common then rare  then legendary - additive
        Update the stickers modifications in their respective places
        */

        //fun bad code -> Just add the correlating stcker to the given index i suppose
        //We love hardcoding lol
        
        for(int index = 0; index<stickers.Count; index++)
        {
            //TODO: Bring switch case in when complete
            switch(stickers[index])
        {
            case CommonSticker common:
                currentStickerMods.attackDamage+= common.attackDamage;
                currentStickerMods.criticalChance+= common.criticalChance;
                currentStickerMods.attackSpeed+= common.attackSpeed;
                currentStickerMods.specialCooldown+= common.specialCooldown; 
                break;

            case RareSticker rare:

                currentStickerMods.attackDamage+= rare.attackDamage;
                currentStickerMods.criticalChance+= rare.criticalChance;
                currentStickerMods.attackSpeed+= rare.attackSpeed;
                currentStickerMods.specialCooldown+= rare.specialCooldown; 
                currentStickerMods.maxHealth+= rare.maxHealth;
                currentStickerMods.moveSpeed+= rare.moveSpeed;
                currentStickerMods.ultimateCharge+= rare.ultimateCharge; 
                break;
            
            case LegendarySticker legendary:
                currentStickerMods.lifesteal+= legendary.lifesteal;
                currentStickerMods.damageRes+= legendary.damageRes;
                currentStickerMods.stickerBoost+= legendary.stickerBoost;
                currentStickerMods.holoDrop+= legendary.holoDrop;
                break; 

        }
        }


    }  



    // Central place to update every modification for player.
    //Can move to another script

   

    public int GetAttackDamage() => currentStickerMods.attackDamage;
    public int GetCritChance() => currentStickerMods.criticalChance;
    public int GetMaxHealthBonus() => currentStickerMods.maxHealth;

    //-----Player Damage

    //-----PartInstance
    //Attack damage call EnemyDamage
    // crit chance....? Probably same function for attack damage
    //Player Move spead in Player movement
    //Player maxhealth in Player Health
    //Attack speed in.......?
    //Special cooldown in Part Instance
    //UltCharge in Chassis behavior or Part Instance
    //Lifesteal seperate tie between Enemy damage and playerhealth
    //Damage res probably within player health.

    
}
