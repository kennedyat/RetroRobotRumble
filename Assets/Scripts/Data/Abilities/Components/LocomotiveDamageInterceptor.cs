using UnityEngine;

/// <summary>
/// Component that intercepts damage for Locomotive Special ability.
/// Implements IDamageInterceptor to work with PlayerHealth.
/// </summary>
public class LocomotiveDamageInterceptor : MonoBehaviour, IDamageInterceptor
{
    private LocomotiveSpecialComponent component;
    private PartContext context;
    private bool isIntercepting = false;
    
    public void Initialize(LocomotiveSpecialComponent comp, PartContext ctx)
    {
        component = comp;
        context = ctx;
    }
    
    public void SetIntercepting(bool intercepting)
    {
        isIntercepting = intercepting;
    }
    
    public bool TryMitigateDamage(float damageAmount, Vector3 damageSourcePosition)
    {
        if (!isIntercepting || component == null || context == null)
            return false;
        
        return component.TryMitigateDamage(context, damageAmount, damageSourcePosition);
    }
}
