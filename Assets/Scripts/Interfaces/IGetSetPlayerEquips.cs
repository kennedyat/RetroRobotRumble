
public interface IGetSetPlayerEquips
{
    ChassisType GetChassis();
    LimbType GetLeftArm();
    LimbType GetRightArm();
    LimbType GetLegs();

    void SetChassis(ChassisType type);
    void SetLeftArm(LimbType type);
    void SetRightArm(LimbType type);
    void SetLegs(LimbType type);
}
