using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public partial class ArmInstance
{
    [SerializeField]
    private ArmType _leftArm;

    [SerializeField]
    List<IArmBehavior> normalBehaviors;
    List<IArmBehavior> specialBehaviors;
    List<IEffect> effects;

    public bool IsNormal;

    public ArmInstance(ArmType leftArm)
    {
        this._leftArm = leftArm;

        normalBehaviors = leftArm.normalBehaviorData.Select(x => x.MakeInstance()).ToList();

        specialBehaviors = leftArm.specialBehaviorData.Select(x => x.MakeInstance()).ToList();
        effects = leftArm.statusEffectData.Select(x => x.MakeInstance()).ToList();
    }

}

// Arms behave like their individual components!
public partial class ArmInstance : IArmBehavior, IEffect
{
    public void Activate(GameObject owner, ArmInstance arm)
    {
        if (!IsNormal) //a bit hard coded, will be discarded
        {
            foreach (var behavior in specialBehaviors)
            {
                behavior.Activate(owner, arm);
            }
        }

        if (IsNormal)
        {
             foreach (var behavior in normalBehaviors)
            {
                behavior.Activate(owner, arm);
            }
        }
        
    }

    public void Deactivate(GameObject owner, ArmInstance arm)
    {
         if (!IsNormal)
        {
            foreach (var behavior in specialBehaviors)
            {
                behavior.Deactivate(owner, arm);
            }
        }

        if (IsNormal)
        {
             foreach (var behavior in normalBehaviors)
            {
                behavior.Deactivate(owner, arm);
            }
        }
    }

    public void FixedUpdateFromArm(GameObject owner, ArmInstance arm)
    {
          if (!IsNormal)
        {
            foreach (var behavior in specialBehaviors)
            {
                behavior.FixedUpdateFromArm(owner, arm);
            }
        }

        if (IsNormal)
        {
             foreach (var behavior in normalBehaviors)
            {
                behavior.FixedUpdateFromArm(owner, arm);
            }
        }
    }


    public void ApplyEffect(EffectContext ctx)
    {
        foreach (var effect in effects)
        {
           effect.ApplyEffect(ctx);
        }
    }

    public void RemoveEffect()
    {
        foreach (var effect in effects)
        {
           effect.RemoveEffect();
        }
    }

    public void PerTick()
    {
        foreach (var effect in effects)
        {
           effect.PerTick();
        }
    }
}