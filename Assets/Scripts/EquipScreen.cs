using System.Linq;
using UnityEngine;

public partial class EquipScreen : MonoBehaviour
{
    private IGetSetPlayerEquips _playerEquips;
    private ChassisType[] _chassis;
    private LimbType[] _arms;
    private LimbType[] _legs;

    public void DoThings()
    {
        Debug.Log("Hello from Equip Screen!");

        Debug.Log("I have these parts.");
        Debug.Log(_playerEquips.GetChassis());
        Debug.Log(_playerEquips.GetLeftArm());
        Debug.Log(_playerEquips.GetRightArm());
        Debug.Log(_playerEquips.GetLegs());

        Debug.Log("These parts were dropped");
        Debug.Log(string.Join(", ", _chassis.Select(x => x.ToString())));
        Debug.Log(string.Join(", ", _arms.Select(x => x.ToString())));
        Debug.Log(string.Join(", ", _legs.Select(x => x.ToString())));

        Debug.Log("I'm gonna equip a thing.");
        _playerEquips.SetLeftArm(_arms[0]);
    }
}

public partial class EquipScreen : IOpenEquipScreen
{
    public void InitFromParts(ChassisType[] chassis, LimbType[] arms, LimbType[] legs, IGetSetPlayerEquips playerEquips)
    {
        _playerEquips = playerEquips;
        _chassis = chassis;
        _arms = arms;
        _legs = legs;

        // There's some approaches to this that makes sense to me.
        // * Adding a prefab child, editing it, adding grandchildren.
        // * Copying an invisible child (already part of a prefab), editing it, adding grandchildren.

        DoThings();
    }

    public bool IsOpen()
    {
        // This shouldn't always be true.
        return true;
    }
}
