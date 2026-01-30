using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteMelee : Enemy
{
    #region Variables
    public enum EliteMeleeState { Chasing = 0, Chasing_TangentialDash, Death}
    public enum AttackType { Light1 = 0, Light2, Heavy1, Heavy2, NONE }
    AttackType currentAttack;
    Queue<AttackType> attackQueue;

    [Header("Attack Datas")]
    [SerializeField, Tooltip("DO NOT CHANGE THE ORDER OR ANY REFERENCES HERE, YOU CAN MODIFY THE SCRIPTABLE OBJECTS BUT NOT THEIR ORDER HERE")]
    EliteMeleeAttackData[] data = new EliteMeleeAttackData[4];

    [Header("Debug")]
    [SerializeField] EliteMeleeState currentState;
    [SerializeField, Tooltip("Use this to force what the attack will be")]
    AttackType forceAttack = AttackType.NONE;
    #endregion

    protected override void Start()
    {
        base.Start();

        // choose 2 random attacks
        List<AttackType> list = new()
        {
            AttackType.Light1,
            AttackType.Light2,
            AttackType.Heavy1,
            AttackType.Heavy2
        };

        attackQueue.Enqueue(list[Random.Range(0, list.Count)]);
        list.Remove(attackQueue.Peek());
        attackQueue.Enqueue(list[Random.Range(0, list.Count)]);
        
        StartCoroutine(AttackSequence());
    }

    #region Attacking Logic
    IEnumerator AttackSequence()
    {
        while (true)
        {
            // get the attack range
            AttackType nextAttack;
            if (forceAttack == AttackType.NONE)
            {
                nextAttack = attackQueue.Dequeue();
                attackQueue.Enqueue(nextAttack);
            }
            else
            {
                nextAttack = forceAttack;
            }
            attackRange = EnumToAttackRange(nextAttack);
            
            // pathfind until in attack range and LOS

            // perform the attack

            // dash depending on the distance to the player
        }
    }

    float EnumToAttackRange(AttackType t)
    {
        return data[(int)t].attackRange;
    }
    #endregion

    #region Enemy Functions
    #endregion
}