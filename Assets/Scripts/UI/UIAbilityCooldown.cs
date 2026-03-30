using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAbilityCooldown : MonoBehaviour
{
   public ArmBehavior leftArm = null;
   public ArmBehavior rightArm = null;

   public Image leftArmNormalOverlay;
   public Image leftArmSpecialOverlay;
   public Image rightArmNormalOverlay;
   public Image rightArmSpecialOverlay;

    void Update()
    {
        if(leftArm.normalAbility != null && rightArm.normalAbility != null)
        {
             leftArmNormalOverlay.fillAmount =  leftArm.normalAbility.RemainingCooldown 
        / leftArm.normalAbility.MaxCooldown;

         leftArmSpecialOverlay.fillAmount =  leftArm.specialAbility.RemainingCooldown 
        / leftArm.specialAbility.MaxCooldown;

         rightArmNormalOverlay.fillAmount =  rightArm.normalAbility.RemainingCooldown 
        / rightArm.normalAbility.MaxCooldown;

         rightArmSpecialOverlay.fillAmount =  rightArm.specialAbility.RemainingCooldown 
        / rightArm.specialAbility.MaxCooldown;
        }       
    }

}
