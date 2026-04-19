using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TestEnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [Tooltip("Drag all enemy prefabs you want to test here")]
    [SerializeField] List<GameObject> enemyPrefabs = new();

    [Header("Spawn Settings")]
    [SerializeField] List<Transform> spawnPoints;
    [SerializeField] Transform enemyParent;
    [SerializeField] float spawnRadius = 2f;

    [Header("UI")]
    [SerializeField] TMP_Dropdown enemyDropdown;
    [SerializeField] Button spawnButton;
    [SerializeField] Button killAllButton;


    private List<GameObject> activeEnemies = new();
    private int spawnIndex = 0;

    private void Start()
    {
        PopulateDropdown();

        spawnButton.onClick.AddListener(SpawnSelected);
        if (killAllButton != null)
            killAllButton.onClick.AddListener(KillAll);

     
    }

    private void Update()
    {
        activeEnemies.RemoveAll(e => e == null);
 
    }

    private void PopulateDropdown()
    {
        enemyDropdown.ClearOptions();

        var options = new List<string>();
        foreach (var prefab in enemyPrefabs)
            options.Add(prefab != null ? prefab.name : "NULL");

        enemyDropdown.AddOptions(options);
    }

    private void SpawnSelected()
    {
        int i = enemyDropdown.value;
        if (i < 0 || i >= enemyPrefabs.Count || enemyPrefabs[i] == null)
        {
            Debug.LogWarning("[TestEnemySpawner] Invalid selection.");
            return;
        }

        Vector3 pos = GetSpawnPosition();
        pos += new Vector3(Random.Range(-spawnRadius, spawnRadius), 0f, Random.Range(-spawnRadius, spawnRadius));

        var instance = Instantiate(enemyPrefabs[i], pos, Quaternion.identity, enemyParent);
        activeEnemies.Add(instance);

        Debug.Log($"[TestEnemySpawner] Spawned {enemyPrefabs[i].name}");
    }

    public void KillAll()
    {
        foreach (var e in activeEnemies)
            if (e != null) Destroy(e);

        activeEnemies.Clear();
    }

    private Vector3 GetSpawnPosition()
    {
        if (spawnPoints != null && spawnPoints.Count > 0)
            return spawnPoints[spawnIndex++ % spawnPoints.Count].position;

        return Vector3.zero;
    }


}