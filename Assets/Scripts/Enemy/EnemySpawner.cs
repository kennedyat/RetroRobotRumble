using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Rand = UnityEngine.Random;
using DG.Tweening;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] List<GameObject> commonEnemyPrefabs;
    [SerializeField] List<GameObject> eliteEnemyPrefabs;
    [SerializeField] bool simpleEliteSpawn = false;
    [SerializeField] List<Transform> spawnPoints;
    [SerializeField] Transform enemyParent;
    [SerializeField] RectTransform roundInfoText;

    [Header("Spawning")]
    [SerializeField, Tooltip("The time between enemy spawns")]
    float spawnDelay;
    [Tooltip("True if all enemies for the round have been spawned")]
    public bool allEnemiesSpawned = false;

    [Header("Rounds, Waves, and Points")]
    [SerializeField] int currentRound;
    public int currentWave;

    [SerializeField] int currentPoints;
    [SerializeField] int startingPoints;
    // [SerializeField] float roundMultiplier = 5;
    // [SerializeField] float expoBase = 2;

    [Header("Debug")]
    [SerializeField, Tooltip("Based on the provided list of prefabs above, the index of the enemy to always spawn.\nLeave as \"-1\" for NONE")]
    int forceIndexToSpawn = -1;
    [SerializeField, Tooltip("Controls the amount of forced enemies to spawn. Only works if any enemy is forced, and uses the above spawn delay")]
    int forcedSpawnCount = 5;

    protected void Start()
    {
        roundInfoText.GetComponent<TextMeshProUGUI>().text = "// Round " + RunData.currentRound + " >> Wave 1";
        currentWave = -1;

        if (forceIndexToSpawn == -1)
            StartCoroutine(EnemySpawnSequence());
        else
            StartCoroutine(DebugSpawnSequence());
    }

    public IEnumerator EnemySpawnSequence()
    {
        //startingPoints = (int)Math.Ceiling(Mathf.Pow(expoBase, RunData.currentRunNum) * roundMultiplier);
        allEnemiesSpawned = false;
        currentWave++;

        StartCoroutine(UpdateRoundInfoText());

        startingPoints = (RunData.currentRound * 10) + (currentWave * 5);
        currentPoints = startingPoints;

        // Debug.Log("Enemy Spawner: Current Round is " + RunData.currentRound);
        // Debug.Log("Enemy Spawner: Current Wave is " + currentWave);
        // Debug.Log("Enemy Spawner: Starting Points is " + startingPoints);

        yield return new WaitForSeconds(spawnDelay);

        while (currentPoints > 0)
        {
            // 1/3: get the available enemies to spawn
            List<GameObject> canSpawn = new();
            foreach (GameObject e in commonEnemyPrefabs)
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

        // spawn elites
        if (simpleEliteSpawn)
        {
            if (currentWave == 2)
            {
                // method 1: 
                // spawns one melee and one ranged elite per round number in the final wave (i.e. round 1 = 1 of each, round 2 = 2 of each, round 3 = 3 of each)
                for (int i = 0; i < RunData.currentRound; i++)
                {
                    foreach (GameObject e in eliteEnemyPrefabs)
                    {
                        int sPoint = Rand.Range(0, spawnPoints.Count);
                        GameObject reference = Instantiate(e, spawnPoints[sPoint].position, Quaternion.identity, enemyParent);
                        yield return new WaitForSeconds(spawnDelay);
                    }
                }
            }
        }
        else
        {
            // method 2: 
            // potentially have it be round number * wave number (i.e. round 1, wave 2 = 2 of each elite type)
            // this would mean each wave has 1, 2, 3, 2, 4, 6, 3, 6, 9 of each elite (might be overkill lmao)

            for (int i = 0; i < RunData.currentRound * (currentWave + 1); i++)
            {
                foreach (GameObject e in eliteEnemyPrefabs)
                {
                    int sPoint = Rand.Range(0, spawnPoints.Count);
                    GameObject reference = Instantiate(e, spawnPoints[sPoint].position, Quaternion.identity, enemyParent);
                    yield return new WaitForSeconds(spawnDelay);
                }
            }
        }

        allEnemiesSpawned = true;
        yield return null;
    }

    IEnumerator UpdateRoundInfoText()
    {
        float delayDuration = 0.25f;

        roundInfoText.transform.DOScale(1.25f, delayDuration).SetEase(Ease.OutQuint);
        yield return new WaitForSeconds(delayDuration * 2);

        roundInfoText.GetComponent<TextMeshProUGUI>().text = "// Round " + RunData.currentRound + " >> Wave " + (currentWave + 1);
        yield return new WaitForSeconds(delayDuration * 2);

        roundInfoText.transform.DOScale(0.8f, delayDuration).SetEase(Ease.OutQuint);
        yield return null;
    }

    IEnumerator DebugSpawnSequence()
    {
        for (int i = 0; i < forcedSpawnCount; i++)
        {
            // spawn exactly the specified enemy
            int sPoint = Rand.Range(0, spawnPoints.Count);
            GameObject reference = Instantiate(commonEnemyPrefabs[forceIndexToSpawn], spawnPoints[sPoint].position, Quaternion.identity, enemyParent);

            // wait some time
            yield return new WaitForSeconds(spawnDelay);
        }
        allEnemiesSpawned = true;
    }
}
