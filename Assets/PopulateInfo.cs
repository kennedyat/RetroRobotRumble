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
            statsText.text = //$"Tab to Victory Screen\n" +
                        $"+ {StickerBehavior.Instance.GetAttackDamage()}%\n" +
                        $"+ {StickerBehavior.Instance.GetCritChance()}%\n" +
                        $"+ {StickerBehavior.Instance.currentStickerMods.moveSpeed}%\n" +
                        $"+ {StickerBehavior.Instance.currentStickerMods.attackSpeed}%\n" +
                        $"- {StickerBehavior.Instance.currentStickerMods.specialCooldown}%\n" +
                        $"- {StickerBehavior.Instance.currentStickerMods.ultimateCharge}%\n" +
                        $"+ {StickerBehavior.Instance.GetMaxHealthBonus()}HP\n" +
                        $"+ {StickerBehavior.Instance.currentStickerMods.lifesteal}%\n" +
                        $"+ {StickerBehavior.Instance.currentStickerMods.damageRes}%\n" +
                        $"+ 0%\n" +
                        $"+ 0%\n";
        }
        else
        {
            statsText.text = "No StickerBehavior!!!";
        }
    }
}
