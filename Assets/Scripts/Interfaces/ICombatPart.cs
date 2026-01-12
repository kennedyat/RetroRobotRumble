using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum PartState
{
    Ready,
    Active,
    Cooldown,
    Locked
}
public interface ICombatPart
{
    string PartName { get; }
    PartState CurrentState { get; }
    float RemainingCooldown { get; }
     float MaxCooldown { get; }
    bool CanUse { get; }

}
