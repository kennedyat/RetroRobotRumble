using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalBoss : MonoBehaviour
{
    enum AttackType { Range = 0, Melee }
    enum Arm { Gungnir = 0, Trishula }
    enum Order { First = 0, Second }
    
    struct AttackData
    {
        public AttackType Type { get; private set; }
        public Arm Arm { get; private set; }
        public Order Order { get; private set; }
        void Init(AttackType at, Arm a, Order o) { Type = at; Arm = a; Order = o; }
    }

    [Header("References")]
    [SerializeField] GameObject projectilePrefab;

    #region Attacks
    void DecideAttack(AttackData data)
    {
        // is there a better way?
        if (data.Type == AttackType.Range)
        {
            if (data.Arm == Arm.Gungnir)
            {
                if (data.Order == Order.First)
                {
                    GungnirR1();
                }
                else
                {
                    GungnirR2();
                }
            }
            else
            {
                if (data.Order == Order.First)
                {
                    TrishulaR1();
                }
                else
                {
                    TrishulaR2();
                }
            }
        }
        else
        {
            if (data.Arm == Arm.Gungnir)
            {
                if (data.Order == Order.First)
                {
                    GungnirM1();
                }
                else
                {
                    GungnirM2();
                }
            }
            else
            {
                if (data.Order == Order.First)
                {
                    TrishulaM1();
                }
                else
                {
                    TrishulaM2();
                }   
            }
        }
    }
    
    void GungnirR1()
    {
        // 360 laser shot for 8 seconds
    }

    void GungnirR2()
    {
        // instantly shoot the enemy
    }

    void GungnirM1()
    {
        // charge forward a few times
    }

    void GungnirM2()
    {
        // basically samus final smash
    }

    void TrishulaR1()
    {
        // shoot 8 shots, rotate each shot
    }

    void TrishulaR2()
    {
        // shoot shots that split into smaller shots
    }

    void TrishulaM1()
    {
        // pantheon tap q
    }

    void TrishulaM2()
    {
        // darius q
    }
    #endregion
}
