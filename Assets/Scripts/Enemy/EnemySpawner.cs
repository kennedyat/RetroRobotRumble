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
    //[SerializeField, Tooltip("The time between spawning waves, after all points have been exhausted")]
    //float waveDelay;
    [Header("Debug")]
    [SerializeField] int currentPoints;
    [SerializeField] int startingPoints;
    public int currentRound = 0;
    public float roundMultiplier = 5;
    public float expoBase = 2;

    void Start()
    {
        StartCoroutine(EnemySpawnSequence());
        currentRound = RunData.currentRunNum;
        Debug.Log("Current Round " + currentRound);
    }

    IEnumerator EnemySpawnSequence()
    {
        startingPoints = (int)Math.Ceiling(Mathf.Pow(expoBase, currentRound) * roundMultiplier);
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

            currentPoints -= canSpawn[random].GetComponent<Enemy>().GetSpawnCost();

            // wait some time, hard coded for now
            yield return new WaitForSeconds(spawnDelay);
        }
        allEnemiesSpawned = true;
        yield return null;
    }

    // Deprecated debug function
    // IEnumerator WaveSequence()
    // {
    //     // DEBUG: wait a few seconds before starting it
    //     if (currentRound == 0) yield return new WaitForSeconds(5.0f);

    //     // 1/2: set the number of points to double
    //     // current round starts at zero, so increment it first
    //     currentRound++;
    //     startingPoints = (int)Mathf.Pow(2, currentRound);

    //     // 2/2: pause execution of this coroutine and run the spawn sequence
    //     currentPoints = startingPoints;
    //     yield return StartCoroutine(EnemySpawnSequence());

    //     // debugging stuff
    //     yield return new WaitForSeconds(waveDelay);

    //     // recurse but stop if we are at round 5
    //     if (currentRound < 5) StartCoroutine(WaveSequence());
    // }
}
