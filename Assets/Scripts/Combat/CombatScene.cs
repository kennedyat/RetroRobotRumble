using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatScene : MonoBehaviour
{
    public void TemporaryEndCombatPressed()
    {
        RRRSceneManager.LoadBuildABot();
    }
}
