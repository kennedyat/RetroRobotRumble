using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MechPartType
{
    Chassis,
    Arm,
    Legs
}

[Obsolete("Use PartCommonData (or ArmType/ChassisType/LegType for more specific stuff)", false)]
public class MechPart : ScriptableObject
{
    // Fields that can be configured without compiling.
    public MechPartType mechPartType;
    public int maxHealth;
    public int damage;
    public int speed;
    public Sprite partSprite;
    public string partName;
    public string partDescription;

    // // Fields can also be Unity stuff.
    // public GameObject modelPrefab;
    // public Sprite sprite;

    // // If we need additional behavior/hooks, we can split that off into another object like this.
    // public Unstable.ChassisBehavior specificBehavior;

    // // Or maybe an enum would be better? Bleh, I'll figure it out later.
}