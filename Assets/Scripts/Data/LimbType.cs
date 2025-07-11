using System;
using UnityEngine;

[Obsolete("Use ArmType or LegType", false)]
public class LimbType : ScriptableObject
{

    public int LimbIndex;

    [Serializable]
    public struct LeftArmData
    {
        public int damage;
        public Sprite leftArmSprite;
        public string leftArmName;
        public string leftArmDescription;

        // public int enemyUserDamage;

        // model (as prefab)?
        // sprite for ui?
        // animation?
        // frame data?
        // ...
    }

    [Serializable]
    public struct RightArmData
    {
        public int damage;
        public Sprite rightArmSprite;
        public string rightArmName;
        public string rightArmDescription;

    }

    [Serializable]
    public struct LegsData
    {
        public float speed;
        public Sprite legsSprite;
        public string legsName;
        public string legsDescription;

    }

    public LeftArmData leftArmData;
    public RightArmData rightArmData;
    public LegsData legsData;

    // We want to have asymmetrical enemies. Maybe they get their own data?
    // Or maybe the enemy data goes inline with the player data?
    // public LeftArmData enemyLeftArmData;
    // public RightArmData enemyRightArmData;
    // public LegsData enemyLegsData;

    // We can also break this up by department.
    // public LeftArmArt leftArmArt;
}
