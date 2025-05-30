using UnityEngine;

namespace Unstable
{
    public partial class RunInfo : MonoBehaviour
    {
        public Robot equipment;

        public void Start()
        {
            // Do destroy me when starting a new run, though. 
            DontDestroyOnLoad(this);
        }
    }

    public partial class RunInfo : IGetSetPlayerEquips
    {
        public ChassisType GetChassis() => equipment.chassis;
        public LimbType GetLeftArm() => equipment.leftArm;
        public LimbType GetLegs() => equipment.legs;
        public LimbType GetRightArm() => equipment.rightArm;

        public void SetLeftArm(LimbType type) => equipment.leftArm = type;
        public void SetLegs(LimbType type) => equipment.legs = type;
        public void SetRightArm(LimbType type) => equipment.rightArm = type;
    }
}
