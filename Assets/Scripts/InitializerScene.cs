using System.Collections.Generic;
using UnityEngine;

public class InitializerScene : MonoBehaviour
{
    public List<ArmType> arms;

    void Start()
    {
        RunData.currentRun.availableArms = arms;

        // For now, just jump directly to BAB.
        RRRSceneManager.LoadBuildABot();
    }
}
