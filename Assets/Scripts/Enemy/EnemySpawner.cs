using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rand = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] List<GameObject> enemyPrefabs;
    [SerializeField] List<Transform> spawnPoints;
    [SerializeField] Transform enemyParent;

    [Header("Spawning")]
    [SerializeField, Tooltip("The time between enemy spawns")]
    float spawnDelay;
    [Tooltip("True if all enemies for the round have been spawned")]
    public bool allEnemiesSpawned = false;

    [Header("Points and Multipliers")]
    [SerializeField] int currentPoints;
    [SerializeField] int startingPoints;
    [SerializeField] float roundMultiplier = 5;
    [SerializeField] float expoBase = 2;

    [Header("Debug")]
    [SerializeField, Tooltip("Based on the provided list of prefabs above, the index of the enemy to always spawn.\nLeave as \"-1\" for NONE")]
    int forceIndexToSpawn = -1;
    [SerializeField, Tooltip("Controls the amount of forced enemies to spawn. Only works if any enemy is forced, and uses the above spawn delay")]
    int forcedSpawnCount = 5;

    protected void Start()
    {
        if (forceIndexToSpawn == -1)
            StartCoroutine(EnemySpawnSequence());
        else
            StartCoroutine(DebugSpawnSequence());

        Debug.Log("Current Round " + RunData.currentRunNum);
    }

    IEnumerator EnemySpawnSequence()
    {
        startingPoints = (int)Math.Ceiling(Mathf.Pow(expoBase, RunData.currentRunNum) * roundMultiplier);
        currentPoints = startingPoints;

        yield return new WaitForSeconds(spawnDelay);

        while (currentPoints > 0)
        {
            // 1/3: get the available enemies to spawn
            List<GameObject> canSpawn = new();
            foreach (GameObject e in enemyPrefabs)
            {
                if (e.GetComponent<Enemy>().GetSpawnCost() <= currentPoints)
                {
                    canSpawn.Add(e);
                }
            }

            // 2/3: pick a random one and spawn it at a random place
            int random = Rand.Range(0, canSpawn.Count);
            int sPoint = Rand.Range(0, spawnPoints.Count);

            // 3/3: spawn it
            GameObject reference = Instantiate(canSpawn[random], spawnPoints[sPoint].position, Quaternion.identity, enemyParent);
            currentPoints -= reference.GetComponent<Enemy>().GetSpawnCost();

            // wait some time
            yield return new WaitForSeconds(spawnDelay);
        }
        allEnemiesSpawned = true;
        yield return null;
    }

    IEnumerator DebugSpawnSequence()
    {
        for (int i = 0; i < forcedSpawnCount; i++)
        {
            // spawn exactly the specified enemy
            int sPoint = Rand.Range(0, spawnPoints.Count);
            GameObject reference = Instantiate(enemyPrefabs[forceIndexToSpawn], spawnPoints[sPoint].position, Quaternion.identity, enemyParent);

            // wait some time
            yield return new WaitForSeconds(spawnDelay);
        }
        allEnemiesSpawned = true;
    }
}
