using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public partial class ArmInstance
{
    private ArmType _leftArm;

    List<IArmBehavior> behaviors;

    public ArmInstance(ArmType leftArm)
    {
        this._leftArm = leftArm;

        behaviors = leftArm.armBehaviorData.Select(x => x.MakeInstance()).ToList();
    }
}

// Arms behave like their individual components!
public partial class ArmInstance : IArmBehavior
{
    public void Activate(GameObject owner, ArmInstance arm)
    {
        foreach (var behavior in behaviors)
        {
            behavior.Activate(owner, arm);
        }
    }

    public void Deactivate(GameObject owner, ArmInstance arm)
    {
        foreach (var behavior in behaviors)
        {
            behavior.Deactivate(owner, arm);
        }
    }

    public void FixedUpdate(GameObject owner, ArmInstance arm)
    {
        foreach (var behavior in behaviors)
        {
            behavior.FixedUpdate(owner, arm);
        }
    }
}