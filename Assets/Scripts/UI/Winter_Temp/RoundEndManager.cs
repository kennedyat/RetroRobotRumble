using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class RoundEndManager : MonoBehaviour
{
    [SerializeField] EnemySpawner enemySpawner;
    [SerializeField] Transform enemyParent;
    [SerializeField] PlayerHealth playerHealth;
    [SerializeField] GameObject victoryInterface;
    [SerializeField] GameObject defeatInterface;
    [SerializeField] GameObject combatInterface;
    [SerializeField] ProgressionManager progressionManager;
    [SerializeField] VictoryScreenController victoryScreenController;
    [SerializeField] UnityEngine.InputSystem.PlayerInput playerInput;

    private bool unlock = false;
    private bool roundEnded = false;

    // Update is called once per frame
    void Update()
    {
        if (!roundEnded)
        {
            //Debug
            if(Input.GetKeyDown(KeyCode.Tab))
            {
                VictorySequence();
            }
            if(Input.GetKeyDown(KeyCode.BackQuote))
            {
                Debug.Log("Final  Boss");
                 RRRSceneManager.LoadFinalBoss();
            }
                   
            if (enemySpawner.allEnemiesSpawned && enemyParent.childCount <= 0)
            {
                if (enemySpawner.currentWave >= 2)
                {
                    VictorySequence();
                } else
                {
                    StartNextWave();
                    
                }
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
        //progressionManager.UnlockPart();
        //progressionManager.unlock = true;
        // disable player input
        
        victoryInterface.SetActive(true);
        //victoryScreenController.StartVictorySequence();
        playerInput.DeactivateInput();
        PlayerInitializer.sharedPlayerInput.Disable();
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

    public void VictoryButton()
    {
        //progressionManager.unlock = unlock;
        RunData.EndCurrentRound();
    }

    public void DefeatButton()
    {
        SceneManager.LoadScene("MainMenu");
    }

    void StartNextWave()
    {
        StartCoroutine(enemySpawner.EnemySpawnSequence());
    }
}
