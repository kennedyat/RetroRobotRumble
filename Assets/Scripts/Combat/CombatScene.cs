using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatScene : MonoBehaviour

{
    #if !UNITY_STANDALONE_LINUX
    public AK.Wwise.Event GameRoundStartEvent;
    #endif
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // AUDIO: Game Round Start VO
        #if !UNITY_STANDALONE_LINUX
        GameRoundStartEvent.Post(gameObject);
        #endif
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
