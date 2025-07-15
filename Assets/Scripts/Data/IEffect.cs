using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct EffectContext
{
    public GameObject source;
    public GameObject target;
    public Vector3 direction;

}
public interface IEffect
{
    // Start is called before the first frame update

    public void ApplyEffect(EffectContext ctx);

    public void RemoveEffect();

    public void PerTick();
}
