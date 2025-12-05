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

    [Header("Spawning")]
    [SerializeField, Tooltip("The time between enemy spawns")] 
    float spawnDelay;
    [SerializeField, Tooltip("The time between spawning waves, after all points have been exhausted")]
    float waveDelay;

    [Header("Debug")]
    [SerializeField] int currentPoints;
    [SerializeField] int startingPoints;
    [SerializeField] int currentRound = 0;

    void Start()
    {
        StartCoroutine(WaveSequence());
    }

    IEnumerator EnemySpawnSequence()
    {
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
            GameObject reference = Instantiate(canSpawn[random], spawnPoints[sPoint].position, Quaternion.identity);

            currentPoints -= canSpawn[random].GetComponent<Enemy>().GetSpawnCost();

            // wait some time, hard coded for now
            yield return new WaitForSeconds(spawnDelay);
        }
        yield return null;
    }

    IEnumerator WaveSequence()
    {
        // DEBUG: wait a few seconds before starting it
        if (currentRound == 0) yield return new WaitForSeconds(5.0f);

        // 1/2: set the number of points to double
        // current round starts at zero, so increment it first
        currentRound++;
        startingPoints = (int)Mathf.Pow(2, currentRound);

        // 2/2: pause execution of this coroutine and run the spawn sequence
        currentPoints = startingPoints;
        yield return StartCoroutine(EnemySpawnSequence());

        // debugging stuff
        yield return new WaitForSeconds(waveDelay);

        // recurse but stop if we are at round 5
        if (currentRound < 5) StartCoroutine(WaveSequence());
    }
}
