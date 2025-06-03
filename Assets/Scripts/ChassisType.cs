using UnityEngine;

[CreateAssetMenu(fileName = "MyChassis", menuName = "ScriptableObjects/ChassisType", order = 1)]
public class ChassisType : ScriptableObject
{
    // Fields that can be configured without compiling.
    public int maxHealth = 100;
    public Sprite chassisSprite;
    public string chassisName;
    public string chassisDescription;

    // // Fields can also be Unity stuff.
    // public GameObject modelPrefab;
    // public Sprite sprite;

    // // If we need additional behavior/hooks, we can split that off into another object like this.
    // public Unstable.ChassisBehavior specificBehavior;

    // // Or maybe an enum would be better? Bleh, I'll figure it out later.
}