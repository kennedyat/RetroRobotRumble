using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatScene : MonoBehaviour

{
    public AK.Wwise.Event GameRoundStartEvent;
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // AUDIO: Game Round Start VO
        GameRoundStartEvent.Post(gameObject);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TemporaryEndCombatPressed();
        }
    }

    public void TemporaryEndCombatPressed()
    {
        RRRSceneManager.LoadBuildABot();
    }
}
