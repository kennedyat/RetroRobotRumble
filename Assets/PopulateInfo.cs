using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PopulateInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statsText;
    
    void Update()
    {
        if (StickerBehavior.Instance != null)
        {
            statsText.text = $"Tab to Victory Screen\n" +
                            $"Attack Damage: {StickerBehavior.Instance.GetAttackDamage()}\n" +
                           $"Crit Chance: {StickerBehavior.Instance.GetCritChance()}%\n" +
                           $"Max Health: +{StickerBehavior.Instance.GetMaxHealthBonus()}\n" +
                           $"Move Speed: {StickerBehavior.Instance.currentStickerMods.moveSpeed}\n" +
                           $"Attack Speed: {StickerBehavior.Instance.currentStickerMods.attackSpeed}\n" +
                           $"Special CD: {StickerBehavior.Instance.currentStickerMods.specialCooldown}\n" +
                           $"Ult Charge: {StickerBehavior.Instance.currentStickerMods.ultimateCharge}\n" +
                           $"Lifesteal: {StickerBehavior.Instance.currentStickerMods.lifesteal}\n" +
                           $"Damage Res: {StickerBehavior.Instance.currentStickerMods.damageRes}";
        }
        else
        {
            statsText.text = "No StickerBehavior!!!";
        }
    }
}
