using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class RoundEndManager : MonoBehaviour
{
    [SerializeField] EnemySpawner enemySpawner;
    [SerializeField] Transform enemyParent;
    [SerializeField] PlayerHealth playerHealth;
    [SerializeField] GameObject victoryInterface;
    [SerializeField] GameObject defeatInterface;
    [SerializeField] GameObject combatInterface;
    [SerializeField] PlayerInput playerInput;

    private bool roundEnded = false;

    // Update is called once per frame
    void Update()
    {
        if (!roundEnded)
        {
            if (enemySpawner.allEnemiesSpawned && enemyParent.childCount <= 0)
            {
                StartCoroutine(VictorySequence());
            }
            if (playerHealth.currentHealth <= 0)
            {
                StartCoroutine(DefeatSequence());
            }
        }
    }

    IEnumerator VictorySequence()
    {
        Debug.Log("YOU WIN YOU WIN YOU WIN YOU WIN");
        roundEnded = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        combatInterface.SetActive(false);
        // disable player input

        victoryInterface.SetActive(true);
        yield return null;
    }

    IEnumerator DefeatSequence()
    {
        Debug.Log("YOU LOSE YOU LOSE YOU LOSE YOU LOSE");
        roundEnded = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        combatInterface.SetActive(false);
        // disable player input

        defeatInterface.SetActive(true);
        yield return null;
    }
}
