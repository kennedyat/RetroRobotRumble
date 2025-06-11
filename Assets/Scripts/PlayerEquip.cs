using System.Collections;
using System.Collections.Generic;
using Unity.XR.Oculus.Input;
using UnityEngine;

public class PlayerEquip : MonoBehaviour
{
    private PlayerStats _stats;
    //public PlayerAttack _attack;
    // Start is called before the first frame update
    void Awake()
    {
        _stats = GetComponent<PlayerStats>();
        //_attack = GetComponent<PlayerAttack>();
    }

    // Update is called once per frame
    public void Equip(MechPart gear)
    {
        float speed;
        speed = gear.speed;
        _stats.SetSpeed(1f);
        switch (gear.mechPartType)
        {
            case MechPartType.Arm:
                                
                break;
            case MechPartType.Chassis:
                //SetStatusEffect(gear.damage, gear.partName);
                break;
            case MechPartType.Legs:

                //SetSpecialAbility(gear.damage, gear.partName);
                break;

        }
    }

    private void Health()
    {

    }

    private void NormalDamageGiven()
    {

    }
    private void SpecialDamageGiven()
    {

    }
    private void Speed()
    {

    }
    
    private void Effect()
    {
        
    }

}
