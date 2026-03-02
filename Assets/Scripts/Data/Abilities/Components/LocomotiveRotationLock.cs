using UnityEngine;
using Assets.Scripts.Combat.Robot;

/// <summary>
/// Helper MonoBehaviour component that locks rotation during Locomotive Special charge.
/// Runs in LateUpdate to ensure it clears rotation values after all input processing.
/// </summary>
public class LocomotiveRotationLock : MonoBehaviour
{
    private CombatRobot combatRobot;
    private Quaternion lockedRotation;
    private bool isLocked = false;
    
    public void Initialize(CombatRobot robot)
    {
        combatRobot = robot;
    }
    
    public void LockRotation(Quaternion rotation)
    {
        isLocked = true;
        lockedRotation = rotation;
    }
    
    public void UnlockRotation()
    {
        isLocked = false;
    }
    
    private void LateUpdate()
    {
        if (isLocked && combatRobot != null)
        {
            // Clear rotation values to prevent any rotation
            // This must run after PlayerRobotController.Look() sets these values
            combatRobot.yawRotationalVelocity = 0f;
            combatRobot.yawDelta = 0f;
            
            // Force rotation to stay locked
            transform.rotation = lockedRotation;
        }
        else if (!isLocked && combatRobot != null)
        {
            // When unlocked, make sure we're not interfering
            // Don't clear values - let PlayerRobotController handle rotation normally
        }
    }
}
