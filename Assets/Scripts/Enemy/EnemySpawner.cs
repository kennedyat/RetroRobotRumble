using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // hard coded for now
    int[] pCosts = {1, 2, 4};
    [SerializeField] List<GameObject> enemyPrefabs;
    [SerializeField] int currentPoints;

    void Start()
    {
        StartCoroutine(EnemySpawnSequence());
    }

    IEnumerator EnemySpawnSequence()
    {
        // 1/3: get the available enemies to spawn
        List<GameObject> canSpawn = new();
        for (int i = 0; i < enemyPrefabs.Count; i++)
        {
            if (pCosts[i] <= currentPoints)
            {
                canSpawn.Add(enemyPrefabs[i]);
            }
        }

        // 2/3: pick a random one
        int random = Random.Range(0, canSpawn.Count);

        // 3/3: spawn it
        //Instantiate(something something something);

        // recursive for now
        yield return new WaitForSeconds(1.0f);
        if (currentPoints > 0) StartCoroutine(EnemySpawnSequence());
        yield return null;
    }
}
