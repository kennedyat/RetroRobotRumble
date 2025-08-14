using System.Collections;
using System.Collections.Generic;
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
        }
    }
}
