using System.Linq;
using UnityEngine;

public partial class EquipScreen : MonoBehaviour
{
    [SerializeField, SerializeReference]
    private GameObject _playerEquipment;

    private IGetSetPlayerEquips GetPlayerEquipment()
    {
        return _playerEquipment.GetComponent(typeof(IGetSetPlayerEquips)) as IGetSetPlayerEquips;
    }
}

public partial class EquipScreen : IOpenEquipScreen
{
    public void InitFromDroppedParts(LimbType[] arms, LimbType[] legs)
    {
        // There's some approaches to this that makes sense to me.
        // * Adding a prefab child, editing it, adding grandchildren.
        // * Copying an invisible child (already part of a prefab), editing it, adding grandchildren.

        Debug.Log("Hello from Equip Screen!");

        Debug.Log("I have these parts.");
        Debug.Log(GetPlayerEquipment().GetChassis());
        Debug.Log(GetPlayerEquipment().GetLeftArm());
        Debug.Log(GetPlayerEquipment().GetRightArm());
        Debug.Log(GetPlayerEquipment().GetLegs());

        Debug.Log("These parts were dropped");
        Debug.Log(string.Join(", ", arms.Select(x => x.ToString())));
        Debug.Log(string.Join(", ", legs.Select(x => x.ToString())));

        Debug.Log("I'm gonna equip a thing.");
        GetPlayerEquipment().SetLeftArm(arms[0]);
    }

    public bool IsOpen()
    {
        // This shouldn't always be true.
        return true;
    }
}