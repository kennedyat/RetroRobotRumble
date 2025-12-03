using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatScene : MonoBehaviour
{
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //TemporaryEndCombatPressed();
        }
    }

    public void TemporaryEndCombatPressed()
    {
        RRRSceneManager.LoadBuildABot();
    }
}
