using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ArmBehaviorData : ScriptableObject
{
    // Since SOs are shared, we'd like to return an unshared `Instance`.
    // This can be *the same* type if lazy, where you return Instantiate(this).
    // You can also create an inner class and return one of those.
    public abstract IArmBehavior MakeInstance();
}
