using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class FinalRoundEndManager : MonoBehaviour
{
    [SerializeField] FinalBoss finalBoss;
    [SerializeField] PlayerHealth playerHealth;
    [SerializeField] GameObject victoryInterface;
    [SerializeField] GameObject defeatInterface;
    [SerializeField] GameObject combatInterface;
    UnityEngine.InputSystem.PlayerInput playerInput;

    private bool unlock = false;
    private bool roundEnded = false;

    // Update is called once per frame
    void Update()
    {
        if (!roundEnded)
        {
            //Debug
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                VictorySequence();
            }
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                Debug.Log("Final Boss");
                RRRSceneManager.LoadFinalBoss();
            }

            if (finalBoss.isPhase2 && finalBoss.GetHealth() <= 0)
            {
                VictorySequence();
            }

            if (playerHealth.currentHealth <= 0)
            {
                DefeatSequence();
            }
        }
    }

    void VictorySequence()
    {
        Debug.Log("YOU WIN YOU WIN YOU WIN YOU WIN");
        roundEnded = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        combatInterface.SetActive(false);
        // disable player input

        victoryInterface.SetActive(true);
        //playerInput.DeactivateInput();
        //PlayerInitializer.sharedPlayerInput.Disable();
    }

    void DefeatSequence()
    {
        Debug.Log("YOU LOSE YOU LOSE YOU LOSE YOU LOSE");
        roundEnded = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        combatInterface.SetActive(false);
        // disable player input

        defeatInterface.SetActive(true);
    }

    public void DefeatButton()
    {
        SceneManager.LoadScene("Credits");
    }
}
