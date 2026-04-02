using UnityEngine;

/// <summary>
/// Interface for components that can intercept and mitigate player damage.
/// Used by abilities like Locomotive Special that need to block or reduce damage.
/// </summary>
public interface IDamageInterceptor
{
    /// <summary>
    /// Attempts to intercept damage. Returns true if damage was fully mitigated, false otherwise.
    /// </summary>
    /// <param name="damageAmount">The amount of damage being dealt</param>
    /// <param name="damageSourcePosition">The world position where the damage is coming from</param>
    /// <returns>True if damage was mitigated (player takes no damage), false if damage should proceed normally</returns>
    bool TryMitigateDamage(float damageAmount, Vector3 damageSourcePosition);
}
