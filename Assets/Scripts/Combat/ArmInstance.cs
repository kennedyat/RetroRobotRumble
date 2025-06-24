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
    public void Activate()
    {
        foreach (var behavior in behaviors)
        {
            behavior.Activate();
        }
    }

    public void Deactivate()
    {
        foreach (var behavior in behaviors)
        {
            behavior.Deactivate();
        }
    }

    public void FixedUpdate()
    {
        foreach (var behavior in behaviors)
        {
            behavior.FixedUpdate();
        }
    }
}