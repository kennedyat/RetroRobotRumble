using System.Collections.Generic;
using UnityEngine;

// This initializer scene is more an example of interacting with RunData and RRRSceneManager.
public class InitializerScene : MonoBehaviour
{
    public List<ArmType> arms;

    protected void Start()
    {
        RunData.currentRun.availableArms = arms;

        // For now, just jump directly to BAB.
        RRRSceneManager.LoadBuildABot();
    }
}
