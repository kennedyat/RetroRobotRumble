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
    public void Awake()
    {
        Instance = this;
        currentStickerMods = new Stats(); // Correlate to current run perhaps?  
        if(RunData.availableStickers!=null)   
            AddStickerModifications(RunData.availableStickers);   
    }
    protected void AddStickerModifications(List<Sticker>? stickers = null, Sticker ? sticker = null)
    {
        if(stickers != null)
        {
            for(int index = 0; index<stickers.Count; index++)
            {
                UpdateModifications(stickers [index]);
                
            }
        }
        if(sticker != null)
        {
            UpdateModifications(sticker);
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

    protected void UpdateModifications(Sticker sticker)
    {
        switch(sticker)
        {
            case CommonSticker common:
                currentStickerMods.attackDamage += common.attackDamage;
                currentStickerMods.criticalChance += common.criticalChance;
                currentStickerMods.attackSpeed += common.attackSpeed;
                currentStickerMods.specialCooldown += common.specialCooldown; 
                break;

            case RareSticker rare:

                currentStickerMods.attackDamage += rare.attackDamage;
                currentStickerMods.criticalChance += rare.criticalChance;
                currentStickerMods.attackSpeed += rare.attackSpeed;
                currentStickerMods.specialCooldown += rare.specialCooldown; 
                currentStickerMods.maxHealth += rare.maxHealth;
                currentStickerMods.moveSpeed += rare.moveSpeed;
                currentStickerMods.ultimateCharge += rare.ultimateCharge; 
                break;
            
            case LegendarySticker legendary:
                currentStickerMods.lifesteal += legendary.lifesteal;
                currentStickerMods.damageRes += legendary.damageRes;
                currentStickerMods.stickerBoost += legendary.stickerBoost;
                currentStickerMods.holoDrop += legendary.holoDrop;
                break; 

        }
    }



    // Central place to update every modification for player.
    //Can move to another script

   public void ActivateTemporary(Sticker sticker, float duration)
{
    UpdateModifications(sticker);
       Debug.Log($"[StickerBehavior] BEFORE activate: moveSpeed={currentStickerMods.moveSpeed}");
    UpdateModifications(sticker);
    Debug.Log($"[StickerBehavior] AFTER activate: moveSpeed={currentStickerMods.moveSpeed}");
    
    StartCoroutine(DeactivateAfterDelay(sticker, duration));
}

private IEnumerator DeactivateAfterDelay(Sticker sticker, float duration)
{
    yield return new WaitForSeconds(duration);
    RemoveModifications(sticker);
}

private void RemoveModifications(Sticker sticker)
{
    switch (sticker)
    {
        case CommonSticker common:
            currentStickerMods.attackDamage -= common.attackDamage;
            currentStickerMods.criticalChance -= common.criticalChance;
            currentStickerMods.attackSpeed -= common.attackSpeed;
            currentStickerMods.specialCooldown -= common.specialCooldown;
            break;
        case RareSticker rare:
            currentStickerMods.attackDamage -= rare.attackDamage;
            currentStickerMods.criticalChance -= rare.criticalChance;
            currentStickerMods.maxHealth -= rare.maxHealth;
            currentStickerMods.moveSpeed -= rare.moveSpeed;
            currentStickerMods.attackSpeed -= rare.attackSpeed;
            currentStickerMods.specialCooldown -= rare.specialCooldown;
            currentStickerMods.ultimateCharge -= rare.ultimateCharge;
            break;
        case LegendarySticker legendary:
            currentStickerMods.lifesteal -= legendary.lifesteal;
            currentStickerMods.damageRes -= legendary.damageRes;
            break;
    }
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
