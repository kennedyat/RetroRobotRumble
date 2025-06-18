
public interface IOpenEquipScreen
{
    // Given a list of parts, brings up a UI to let the player select parts.
    // The UI can then change parts with `IGetSetPlayerEquips`.
    //
    // The list of parts is either the player's unlocked parts between runs, or parts dropped by enemies in a run.
    // Enemies do not drop their chassis, and you are not allowed to change your chassis mid run.
    public void InitFromParts(ChassisType[] chassis, ArmType[] arms, LegType[] legs, IGetSetPlayerEquips playerEquips);

    // Something to poll.
    // Other game logic can check if the equip screen is open and if so, pause themselves.
    // (I don't think having the screen closing as an event is good, the event could be forgotten to be subscribed to.)
    public bool IsOpen();
}
