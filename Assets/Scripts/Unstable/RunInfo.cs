using UnityEngine;

namespace Unstable
{
    public partial class RunInfo : MonoBehaviour
    {
        [SerializeField]
        private Robot _equipment;

        public void Start()
        {
            // Do destroy me when a run ends, though.
            // (Then make a new one.)
            DontDestroyOnLoad(this);
        }
    }

    public partial class RunInfo : IGetSetPlayerEquips
    {
        public ChassisType GetChassis() => _equipment.chassis;
        public ArmType GetLeftArm() => _equipment.leftArm;
        public ArmType GetRightArm() => _equipment.rightArm;
        public LegType GetLegs() => _equipment.legs;

        public void SetChassis(ChassisType type) => _equipment.chassis = type;
        public void SetLeftArm(ArmType type) => _equipment.leftArm = type;
        public void SetRightArm(ArmType type) => _equipment.rightArm = type;
        public void SetLegs(LegType type) => _equipment.legs = type;
    }
}
