using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Combat.Prototype;
using UnityEditor.UI;
using UnityEngine;

public class PlayerInitializer : MonoBehaviour
{
    [SerializeField] GameObject existingLeftArm;
    [SerializeField] GameObject existingRightArm;

    void Start()
    {
        Robot robot = RunData.currentRun.Robot;

        if (robot.leftArm?.combatPrefab is GameObject leftArmPrefab)
        {
            existingLeftArm?.SetActive(false);
            Destroy(existingLeftArm);
            existingLeftArm = null;

            Debug.Log(robot.leftArm?.partCommonData.name);

            GameObject instance = Instantiate(leftArmPrefab);
            instance.transform.parent = this.transform;
            instance.transform.SetParent(this.transform, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localScale = Vector3.one;
            instance.transform.localRotation = Quaternion.identity;

            HackForInputs(instance, false);
        }

        if (robot.rightArm?.combatPrefab is GameObject rightArmPrefab)
        {
            existingRightArm?.SetActive(false);
            Destroy(existingRightArm);
            existingRightArm = null;

            Debug.Log(robot.rightArm?.partCommonData.name);

            GameObject instance = Instantiate(rightArmPrefab);
            instance.transform.SetParent(this.transform, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localScale = new Vector3(-1, 1, 1);
            instance.transform.localRotation = Quaternion.identity;

            HackForInputs(instance, true);
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
}
