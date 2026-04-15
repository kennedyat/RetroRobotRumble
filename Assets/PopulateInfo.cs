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
                        $"+ {StickerBehavior.Instance.GetAttackDamageBonus()}%\n" +
                        $"+ {StickerBehavior.Instance.GetCritChanceBonus()}%\n" +
                        $"+ {StickerBehavior.Instance.GetMoveSpeedBonus()}%\n" +
                        $"+ {StickerBehavior.Instance.GetAttackSpeedBonus()}%\n" +
                        $"- {StickerBehavior.Instance.GetSpecialCooldownBonus()}%\n" +
                        $"- {StickerBehavior.Instance.GetUltimateChargeBonus()}%\n" +
                        $"+ {StickerBehavior.Instance.GetMaxHealthBonus()}HP\n" +
                        $"+ {StickerBehavior.Instance.GetLifestealBonus()}%\n" +
                        $"+ {StickerBehavior.Instance.GetDamageResBonus()}%\n" +
                        $"+ {StickerBehavior.Instance.GetStickerBoostBonus()}%\n" +
                        $"+ {StickerBehavior.Instance.GetHoloDropBonus()}%\n";
        }
        else
        {
            statsText.text = "No StickerBehavior!!!";
        }
    }
}
