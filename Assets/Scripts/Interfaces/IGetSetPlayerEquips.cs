
public interface IGetSetPlayerEquips
{
    ChassisType GetChassis();
    ArmType GetLeftArm();
    ArmType GetRightArm();
    LegType GetLegs();

    void SetChassis(ChassisType type);
    void SetLeftArm(ArmType type);
    void SetRightArm(ArmType type);
    void SetLegs(LegType type);
}
