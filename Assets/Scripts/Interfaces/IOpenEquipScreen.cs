
public interface IOpenEquipScreen
{
    // Given a list of dropped parts from the encounter(?),
    // brings up something to let the player replace their parts.
    // Arms can be used as both left and right arms.
    public void InitFromDroppedParts(LimbType[] arms, LimbType[] legs);

    // Something to poll.
    // Other game logic should not move forward while the equip screen is open.
    // (I don't think having the screen closing as an event is good, the event could be forgotten to subscribe.)
    public bool IsOpen();
}