using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAbilityCooldown : MonoBehaviour
{
    [HideInInspector]
    public PartInstance leftArmNormal;
    public PartInstance leftArmSpecial;
    public PartInstance rightArmNormal;
    public PartInstance rightArmSpecial;

   public Image leftArmNormalOverlay;
   public Image leftArmSpecialOverlay;
   public Image rightArmNormalOverlay;
   public Image rightArmSpecialOverlay;

    void Update()
    {
        if(leftArmNormal != null && leftArmSpecial != null)
        {
             leftArmNormalOverlay.fillAmount =  leftArmNormal.RemainingCooldown 
        / leftArmNormal.MaxCooldown;

         leftArmSpecialOverlay.fillAmount =  leftArmSpecial.RemainingCooldown 
        / leftArmSpecial.MaxCooldown;

        }       
        if(rightArmNormal != null && rightArmSpecial != null)
        {
            rightArmNormalOverlay.fillAmount =  rightArmNormal.RemainingCooldown 
            / rightArmNormal.MaxCooldown;

            rightArmSpecialOverlay.fillAmount =  rightArmSpecial.RemainingCooldown 
            / rightArmSpecial.MaxCooldown;
        }
    }

}
