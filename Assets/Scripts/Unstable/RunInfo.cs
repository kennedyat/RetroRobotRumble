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
        public LimbType GetLeftArm() => _equipment.leftArm;
        public LimbType GetLegs() => _equipment.legs;
        public LimbType GetRightArm() => _equipment.rightArm;

        public void SetChassis(ChassisType type) => _equipment.chassis = type;
        public void SetLeftArm(LimbType type) => _equipment.leftArm = type;
        public void SetLegs(LimbType type) => _equipment.legs = type;
        public void SetRightArm(LimbType type) => _equipment.rightArm = type;
    }
}
