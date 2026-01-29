using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Components/Summon")]
public class SummonsComponent : PartComponent
{
    [Header("Train Settings")]
    public GameObject trainPrefab; //Change for  more modularity
    public float summonInterval = 45f;
    
    [Header("Spawn Settings")]
    public float spawnDistance = 3f; 
    public float indicatorDelay = 1f;
    public GameObject indicatorPrefab;
    
    [Header("Damage")]
    public float centerDamageMultiplier = 2f;
    public float sideDamageMultiplier = 1f;
    public float trainWidth = 2f;
    
    public override void Initialize(PartContext context)
    {
        context.CustomData["TrainSummonTimer"] = 0f;
        context.CustomData["TrainIsShowingIndicator"] = false;
        context.CustomData["TrainActiveIndicator"] = null;
        context.CustomData["PooledTrainInstance"] = null;
    }
    
    public override void OnExecute(PartContext context)
    {
        // Passive ability - runs automatically in OnUpdate
    }
    
    public override void OnUpdate(PartContext context, float deltaTime)
    {
        if (context.Owner == null) return;
        
        float summonTimer = (float)context.CustomData["TrainSummonTimer"];
        bool isShowingIndicator = (bool)context.CustomData["TrainIsShowingIndicator"];
        
        // Check for passive frequency boostttt
        float currentInterval = summonInterval;
        if (context.CustomData.ContainsKey("TrainFormActive") && (bool)context.CustomData["TrainFormActive"])
        {
            float boost = context.CustomData.ContainsKey("PassiveFrequencyBoost") 
                ? (float)context.CustomData["PassiveFrequencyBoost"] 
                : 2f;
            currentInterval = summonInterval / boost;
        }
        
        summonTimer += deltaTime;
        
        // Show indicator before summoning (ideally lol)
        if (summonTimer >= currentInterval - indicatorDelay && !isShowingIndicator)
        {
            ShowIndicator(context);
            isShowingIndicator = true;
            context.CustomData["TrainIsShowingIndicator"] = true;
        }
        
        // Summon train
        if (summonTimer >= currentInterval)
        {
            SummonTrain(context);
            summonTimer = 0f;
            isShowingIndicator = false;
            HideIndicator(context);
            context.CustomData["TrainIsShowingIndicator"] = false;
        }
        
        context.CustomData["TrainSummonTimer"] = summonTimer;
    }
    
    private void ShowIndicator(PartContext context)
    {
        if (indicatorPrefab == null) return;
        
        Vector3 spawnPos = context.Owner.position + context.Owner.forward * spawnDistance;
        GameObject indicator = GameObject.Instantiate(indicatorPrefab, spawnPos, context.Owner.rotation);
        context.CustomData["TrainActiveIndicator"] = indicator;
    }
    
    private void HideIndicator(PartContext context)
    {
        GameObject indicator = context.CustomData["TrainActiveIndicator"] as GameObject;
        if (indicator != null)
        {
            GameObject.Destroy(indicator);
            context.CustomData["TrainActiveIndicator"] = null;
        }
    }
    
    private void SummonTrain(PartContext context)
    {
        if (trainPrefab == null)
        {
            Debug.LogError("[SummonsComponent] No train prefab assigned!");
            return;
        }
        
        // Get or create pooled train
        GameObject train = context.CustomData["PooledTrainInstance"] as GameObject;
        
        if (train == null)
        {
            train = GameObject.Instantiate(trainPrefab);
            train.name = "PooledTrain";
            train.SetActive(false);
            context.CustomData["PooledTrainInstance"] = train;
        }
        
        // Get the movement component
        TrainMovement trainMovement = train.GetComponent<TrainMovement>();
        if (trainMovement == null)
        {
            Debug.LogError("[SummonsComponent] Train prefab missing TrainMovement component!");
            return;
        }
        
        // Start the train movement
        Vector3 spawnPos = context.Owner.position + context.Owner.forward * spawnDistance;
        trainMovement.StartMovement(spawnPos, context.Owner.forward, context.Owner.rotation);

        var box = train.GetComponentInChildren<HitBox>();
        box.EnableFrame(0);
        box.OnHit = (Collider target) => OnHitboxHit(target, 4, 4, context);
    }
}