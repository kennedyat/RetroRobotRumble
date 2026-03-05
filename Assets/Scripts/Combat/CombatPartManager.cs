using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Central manager that tracks all active abilities
/// Provides query methods so abilities can check each other
/// </summary>
public class CombatPartManager : MonoBehaviour
{
    private Dictionary<string, ICombatPart> registeredAbilities = new();
    
   public event Action<ICombatPart, PartState, PartState> OnAbilityStateChanged;
    public event Action<ICombatPart, float> OnCooldownUpdated;
    public float TimeBetweenAbilities = .2f;
    
    [Header("Ultimate Points")]
    public float maxUltimatePoints = 100f;
    [SerializeField] Slider ultChargeBar;
    [SerializeField] Image[] UISprites;

    public float CurrentUltimatePoints { get; private set; }
    public bool IsUltimateReady => CurrentUltimatePoints >= maxUltimatePoints;

    void Update()
    {
        if (ultChargeBar.maxValue != maxUltimatePoints)
        {
            ultChargeBar.maxValue = maxUltimatePoints;
        }
        ultChargeBar.value = CurrentUltimatePoints;
    }
    public void RegisterAbility(string name, ICombatPart combatPart)
    {
        registeredAbilities[name] = combatPart;
    }
    
    public void UnregisterAbility(string name)
    {
        registeredAbilities.Remove(name);
    }

    /*public void IsAbilityActiveCurrently()
    {
        bool IsActive = false;
        foreach(ICombatPart part in registeredAbilities)
        {
            IsActive =  (part.CurrentState == PartState.Active); 
        }
        return IsActive;
    }*/
    
    public bool IsAbilityActive(string abilityName)
    {
        return registeredAbilities.TryGetValue(abilityName, out var part) && part.CurrentState == PartState.Active;
    }
    
    public bool CanUseAbility(string abilityName)
    {
        return registeredAbilities.TryGetValue(abilityName, out var part) && part.CanUse;
    }
    
    public PartState GetAbilityState(string abilityName)
    {
        return registeredAbilities.TryGetValue(abilityName, out var part) ? part.CurrentState : PartState.Ready;
    }
    
    public float GetAbilityCooldown(string abilityName)
    {
        return registeredAbilities.TryGetValue(abilityName, out var ability) ? ability.RemainingCooldown : 0f;
    }
    
    public float GetCooldownPercent(string abilityName)
    {
        if (!registeredAbilities.TryGetValue(abilityName, out var part) || part.MaxCooldown <= 0f)
            return 0f;
        return part.RemainingCooldown / part.MaxCooldown;
    }
    
    public bool IsAnyAbilityBlocking()
    {
        foreach (var ability in registeredAbilities.Values)
        {
            Debug.Log($"[PartInstance] [CombatManager] {ability.PartName}: {ability.CurrentState }");
            if ( ability.CurrentState == PartState.Active)
                return true;
        }
        return false;
    }
    
    public void NotifyStateChanged(ICombatPart ability, PartState oldState, PartState newState)
    {
        OnAbilityStateChanged?.Invoke(ability, oldState, newState);
    }
    
    public void NotifyCooldownUpdated(ICombatPart ability, float remaining)
    {
        OnCooldownUpdated?.Invoke(ability, remaining);
    }

    public void AddUltimatePoints(float points)
    {
        Debug.Log("ultimate: current ultimate points = " + CurrentUltimatePoints);
        Debug.Log("ultimate: points added = " + points);
        if (IsUltimateReady) return;
        CurrentUltimatePoints = Mathf.Min(CurrentUltimatePoints + points, maxUltimatePoints);
        Debug.Log("ultimate: new ultimate points = " + CurrentUltimatePoints);

        if (IsUltimateReady)
        {
            foreach (Image sprite in UISprites)
            {
                sprite.DOFade(1, 0.5f);
            }
        }
    }

    public void ConsumeUltimatePoints()
    {
        CurrentUltimatePoints = 0f;
        foreach (Image sprite in UISprites)
            {
                sprite.DOFade(0.1f, 0.5f);
            }
       
    }
}