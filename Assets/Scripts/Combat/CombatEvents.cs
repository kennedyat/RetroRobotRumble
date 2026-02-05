using System;
using UnityEngine;

public static class CombatEvents
{
    public static event Action<Transform> OnOwnerHitEnemy;

    public static void RaiseOwnerHitEnemy(Transform owner)
    {
        OnOwnerHitEnemy?.Invoke(owner);
    }
}
