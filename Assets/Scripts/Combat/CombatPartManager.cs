using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central manager that tracks all active abilities
/// Provides query methods so abilities can check each other
/// </summary>
public class CombatPartManager : MonoBehaviour
{
    private Dictionary<string, ICombatPart> registeredAbilities = new();
    
   public event Action<ICombatPart, PartState, PartState> OnAbilityStateChanged;
    public event Action<ICombatPart, float> OnCooldownUpdated;
    
   public void RegisterAbility(string name, ICombatPart combatPart)
    {
        registeredAbilities[name] = combatPart;
    }
    
    public void UnregisterAbility(string name)
    {
        registeredAbilities.Remove(name);
    }
    
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
            if (ability is PartInstance instance && instance.BlocksOthers)
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
}