using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum LimbData
{
    RightUpperArm,
    RightLowerArm,
    LeftUpperArm,
    LeftLowerArm,
    RightUpperLeg,
    RightLowerLeg,
    LeftUpperLeg,
    LeftLowerLeg,
    Body
};

 [System.Serializable]
    public struct Limb
    {
        public Collider collider;
        public LimbData limbData;
    }
public class LimbMetaData : MonoBehaviour
{

    int activeLayer;
    int defaultLayer;
    int IFLayer;

    public Limb[] limbList;

    public RuntimeDebugger debugger;
    Dictionary<LimbData, Collider> limbPair = new Dictionary<LimbData, Collider>();

    HashSet<LimbData> activeLimb = new();

    void Awake()
    {
        activeLayer = LayerMask.NameToLayer("Active");
        defaultLayer = LayerMask.NameToLayer("Default");
        IFLayer = LayerMask.NameToLayer("IF");

        foreach (Limb limb in limbList)
        {
            //limbPair.Add(limb.limbData, limb.collider);
            //debugger.OnDrawDefaultHitbox(limb.collider.gameObject);
        }
    }


    public void ActivateLimb(LimbData type)
    {

        //Get collider layer change it to hit
        //
        if (limbPair.TryGetValue(type, out var limb))
        {
            activeLimb.Add(type);
            limb.gameObject.layer = activeLayer;
            debugger.OnDrawActiveHitbox(limb.gameObject);
        }
    }

    public void DeactivateLimb(LimbData type)
    {

        if (activeLimb.Contains(type))
        {
            if (limbPair.TryGetValue(type, out var limb))
            {
                limb.gameObject.layer = defaultLayer;
            }
            activeLimb.Remove(type);
            debugger.OnDrawDefaultHitbox(limb.gameObject);
        }

    }

    public bool LimbDetection(Collider limb)
    {
        foreach (var pair in limbPair)
        {
            if (pair.Value == limb && activeLimb.Contains(pair.Key))
                return true;
        }
        return false;

    }
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"This {other.name} got hit");
    }
    /// Hit box
    /// Have a hit box script that: Allows you to edit hitboxes/ hitbox events/ assign to a arm
    /// Each arm has a hitbox
    /// Each hitbox has a way to disable, enable
    /// Runtime Debugger allows hitbox to appear in runtime
    /// Green if active, red if hitting
    /// When animation is active, trigger event
    /// 
    /// 
    /// 

}
