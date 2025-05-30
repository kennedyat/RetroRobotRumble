// Similarly, your UI can hold a reference to a GameObject, where one of its components implements this interface.
// When setting up / initializing stuff, you can call the getters to setup the robot as-is.
// When the player drags and drops stuff, you should call the setters.
public interface IGetSetPlayerEquips
{
    ChassisType GetChassis();
    LimbType GetLeftArm();
    LimbType GetRightArm();
    LimbType GetLegs();

    void SetLeftArm(LimbType type);
    void SetRightArm(LimbType type);
    void SetLegs(LimbType type);
}