using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Combat.Prototype;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInitializer : MonoBehaviour
{
    [SerializeField] GameObject existingLeftArm;
    [SerializeField] GameObject existingRightArm;
    [SerializeField] GameObject parentObject;
    [SerializeField] GameObject originalRig;

    [SerializeField] Image leftIcon;
    [SerializeField] Image rightIcon;
    protected void Start()
    {


        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Robot robot = RunData.currentRun.Robot;
        var swapJoint = originalRig.GetComponent<ArmSwap>();
        if ((robot.leftArm != null ? robot.leftArm.combatPrefab : null) is GameObject leftArmPrefab)
        {
            if (existingLeftArm != null)
            {
                existingLeftArm.SetActive(false);
                Destroy(existingLeftArm);
                existingLeftArm = null;
            }

            if (robot.leftArm != null)
            {
                Debug.Log(robot.leftArm.partCommonData.name);
            }

            GameObject instance = Instantiate(leftArmPrefab);
            instance.transform.SetParent(parentObject.transform, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localScale = Vector3.one;
            instance.transform.localRotation = Quaternion.identity;

            // HACK: Arms should set their own remote transforms. Maybe.
            instance.transform.Find("Remote Transform").GetComponent<RemoteTransform>().remote =
                    this.transform.Find("Smooth Rotation").Find("Tilt Pivot");

            HackForInputs(instance, false);
            swapJoint.SwapJoint("RightArm", instance);
            Debug.Log(instance.name);

            //Change when get the chance
            var iconSpriteRenderer = instance.transform.Find("Special Icon").GetComponent<SpriteRenderer>();
            if (iconSpriteRenderer != null)
            {
                leftIcon.sprite = iconSpriteRenderer.sprite;
            }

        }

        if ((robot.rightArm != null ? robot.rightArm.combatPrefab : null) is GameObject rightArmPrefab)
        {
            if (existingRightArm != null)
            {
                existingRightArm.SetActive(false);
                Destroy(existingRightArm);
                existingRightArm = null;
            }

            if (robot.rightArm != null)
            {
                Debug.Log(robot.rightArm.partCommonData.name);
            }

            GameObject instance = Instantiate(rightArmPrefab);
            instance.transform.SetParent(parentObject.transform, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localScale = new Vector3(-1, 1, 1);
            instance.transform.localRotation = Quaternion.identity;

            // HACK: Arms should set their own remote transforms. Maybe.
            instance.transform.Find("Remote Transform").GetComponent<RemoteTransform>().remote =
                    this.transform.Find("Smooth Rotation").Find("Tilt Pivot");

            HackForInputs(instance, true);
            swapJoint.SwapJoint("LeftArm", instance);
            Debug.Log(instance.name);

            //Change when get the chance
            var iconSpriteRenderer = instance.transform.Find("Special Icon").GetComponent<SpriteRenderer>();
            if (iconSpriteRenderer != null)
            {
                rightIcon.sprite = iconSpriteRenderer.sprite;
            }

        }

         
       
        
    }

    private void HackForInputs(GameObject arm, bool right)
    {
        if (arm.GetComponent<SharkLaserCannon>() is SharkLaserCannon yay)
        {
            yay.leftOrRightControls = right ? SharkLaserCannon.LeftOrRightControls.RIGHT_ARM : SharkLaserCannon.LeftOrRightControls.LEFT_ARM;
        }
        if (arm.GetComponent<OverheatMinigun>() is OverheatMinigun yay2)
        {
            yay2.leftOrRightControls = right ? OverheatMinigun.LeftOrRightControls.RIGHT_ARM : OverheatMinigun.LeftOrRightControls.LEFT_ARM;
        }
        if (arm.GetComponent<Shinkansen_Revised>() is Shinkansen_Revised yay3)
        {
            yay3.leftOrRightControls = right ? LeftOrRightControls.RIGHT_ARM : LeftOrRightControls.LEFT_ARM;
        }
        if (arm.GetComponent<Locomotive_Revised>() is Locomotive_Revised yay4)
        {
            yay4.leftOrRightControls = right ? LeftOrRightControls.RIGHT_ARM : LeftOrRightControls.LEFT_ARM;
        }
    }

   protected void Update()
     {
          if (Input.GetKeyDown(KeyCode.Escape)) // Example: pressing Escape
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
