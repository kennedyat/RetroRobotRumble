using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class BuildABotScreen : MonoBehaviour
{
    private IGetSetPlayerEquips _playerEquips;
}

public partial class BuildABotScreen : IOpenEquipScreen
{
    public void InitFromParts(ChassisType[] chassis, ArmType[] arms, LegType[] legs, IGetSetPlayerEquips playerEquips)
    {
        _playerEquips = playerEquips;
    }

    public bool IsOpen()
    {
        // This shouldn't always be true.
        return true;
    }
}
