using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rand = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [Serializable]
    struct EnemySpawnData
    {
        public GameObject prefab;
        public int cost;
    }

    [SerializeField] List<EnemySpawnData> enemyPrefabs;
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
            List<EnemySpawnData> canSpawn = new();
            foreach (EnemySpawnData e in enemyPrefabs)
            {
                if (e.cost <= currentPoints)
                {
                    canSpawn.Add(e);
                }
            }

            // 2/3: pick a random one
            int random = Rand.Range(0, canSpawn.Count);

            // 3/3: spawn it
            GameObject reference = Instantiate(canSpawn[random].prefab, transform.position, Quaternion.identity);

            // NOTE: the enemies will need some sort of initialization function because they dont have a reference to the player yet
            // pathfinding and attacking will NOT work
            //reference.initialize();

            currentPoints -= canSpawn[random].cost;
            Debug.Log("spawned " + canSpawn[random].prefab + " with " + currentPoints + " points left");

            // wait some time, hard coded for now
            yield return new WaitForSeconds(2.0f);
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
        yield return new WaitForSeconds(5.0f);

        // recurse but stop if we are at round 5
        if (currentRound < 5) StartCoroutine(WaveSequence());
    }
}
