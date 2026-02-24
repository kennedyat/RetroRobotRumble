using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Stats
{
    public int attackDamage; // implemented!
    public int criticalChance; // implemented!
    public int maxHealth; // implemented!
    public int moveSpeed; // implemented!
    public int attackSpeed;
    public int specialCooldown;
    public int ultimateCharge;
    public int lifesteal; // implemented!
    public int damageRes; // implemented!
    public int stickerBoost; // implemented!
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

        // applying sticker boost
        // we do indeed love hardcoding

        float StickerBoostFactor = 1 + GetStickerBoostBonus()/100f;
        
        // currently only applies to common/rare buffs, might add legendaries for funsies later if we wanna get REALLY broken

        currentStickerMods.attackDamage = Mathf.CeilToInt(GetAttackDamageBonus() * StickerBoostFactor);
        currentStickerMods.criticalChance = Mathf.CeilToInt(GetCritChanceBonus() * StickerBoostFactor);
        currentStickerMods.maxHealth = Mathf.CeilToInt(GetMaxHealthBonus() * StickerBoostFactor);
        currentStickerMods.moveSpeed = Mathf.CeilToInt(GetMoveSpeedBonus() * StickerBoostFactor);
        currentStickerMods.attackSpeed = Mathf.CeilToInt(GetAttackSpeedBonus() * StickerBoostFactor);
        currentStickerMods.specialCooldown = Mathf.CeilToInt(GetSpecialCooldownBonus() * StickerBoostFactor);
        currentStickerMods.ultimateCharge = Mathf.CeilToInt(GetUltimateChargeBonus() * StickerBoostFactor);

    }  
  

    public int GetAttackDamageBonus() => currentStickerMods.attackDamage;
    public int GetCritChanceBonus() => currentStickerMods.criticalChance;
    public int GetMaxHealthBonus() => currentStickerMods.maxHealth;
    public int GetMoveSpeedBonus() => currentStickerMods.moveSpeed;
    public int GetAttackSpeedBonus() => currentStickerMods.attackSpeed;
    public int GetSpecialCooldownBonus() => currentStickerMods.specialCooldown;
    public int GetUltimateChargeBonus() => currentStickerMods.ultimateCharge;
    public int GetLifestealBonus() => currentStickerMods.lifesteal;
    public int GetDamageResBonus() => currentStickerMods.damageRes;
    public int GetStickerBoostBonus() => currentStickerMods.stickerBoost;
    public int GetHoloDropBonus() => currentStickerMods.holoDrop;

    //Attack speed in.......?
    //Special cooldown in Part Instance
    //UltCharge in Chassis behavior or Part Instance

    
}
