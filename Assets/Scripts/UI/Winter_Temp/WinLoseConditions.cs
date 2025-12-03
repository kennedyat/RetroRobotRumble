using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinLoseConditions : MonoBehaviour
{
    [SerializeField] EnemySpawner enemySpawner;
    [SerializeField] Transform enemyParent;
    [SerializeField] PlayerHealth playerHealth;

    // Update is called once per frame
    void Update()
    {
        if (enemySpawner.allEnemiesSpawned && enemyParent.childCount <= 0)
        {
            Debug.Log("YOU WIN YOU WIN YOU WIN YOU WIN");
        }
        if (playerHealth.currentHealth <= 0)
        {
            Debug.Log("YOU LOSE YOU LOSE YOU LOSE YOU LOSE");
        }
     }
}
