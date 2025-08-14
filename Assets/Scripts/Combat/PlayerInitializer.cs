using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInitializer : MonoBehaviour
{
    [SerializeField] GameObject existingLeftArm;
    [SerializeField] GameObject existingRightArm;

    void Start()
    {
        Robot robot = RunData.currentRun.Robot;

        if (robot.leftArm?.partCommonData is PartCommonData leftArm)
        {
            existingLeftArm?.SetActive(false);
            Destroy(existingLeftArm);
            existingLeftArm = null;

            Debug.Log(robot.leftArm?.partCommonData.name);
        }

        if (robot.rightArm?.partCommonData is PartCommonData rightArm)
        {
            existingRightArm?.SetActive(false);
            Destroy(existingRightArm);
            existingRightArm = null;

            Debug.Log(robot.rightArm?.partCommonData.name);
        }
    }
}
