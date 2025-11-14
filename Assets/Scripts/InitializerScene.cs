using System.Collections.Generic;
using UnityEngine;

// This initializer scene is more an example of interacting with RunData and RRRSceneManager.
public class InitializerScene : MonoBehaviour
{
    public List<ArmType> arms;
    public List<ChassisType> chassis;
    public List<LegType> legs;

    protected void Start()
    {
        RunData.currentRun.availableArms = arms;
        RunData.currentRun.availableChassis = chassis;
        RunData.currentRun.availableLegs = legs;

        // For now, just jump directly to BAB.
        RRRSceneManager.LoadBuildABot();
    }
}
