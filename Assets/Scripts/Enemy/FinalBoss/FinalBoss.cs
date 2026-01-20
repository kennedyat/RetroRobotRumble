using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalBoss : Enemy
{
    #region Attack Variables
    // the melees are all even, the ranges are all odd, gungnir is first 4, trishula is last 4
    public enum AttackTypes { Gungnir_M1 = 0, Gungnir_R1, Gungnir_M2, Gungnir_R2, Trishula_M1, Trishula_R1, Trishula_M2, Trishula_R2 }

    [Serializable] struct AttackData
    {
        public AttackTypes type;
        public float attackRange;
        public int damage;
        public float duration;
    }

    [SerializeField, Tooltip("For designers to change values")]
    AttackData[] attackDatas = new AttackData[8];
    Queue<AttackTypes> attackQueue = new();
    #endregion

    #region Other Variables
    bool isAttacking = false;
    [SerializeField, Tooltip("The waiting time between attacks")]
    float waitTime = 2.0f;

    [SerializeField, Tooltip("How much to multiple waitTime by in phase 2")]
    float waitTimeMultiplier = 2;

    [Header("References")]
    [SerializeField] GameObject projectilePrefab;
    #endregion

    #region Unity Functions
    protected override void Start()
    {
        base.Start();
    }

    protected void Update()
    {
        // if he's not attacking, face the player until in range
        if (!isAttacking)
        {
            transform.LookAt(SetY(player.position, transform.position.y));
            rb.MovePosition(moveSpeed * Time.deltaTime * (player.position - transform.position));
        }
    }

    #endregion

    #region Bentley Logic
    IEnumerator BentleySequence()
    {
        // 1/x: pick the attack
        
        // 2/x: get into range for the attack

        // 3/x: execute that attack

        // 4/x: check for feedback, did we hit, cuz if we did the attack sequence needs to change

        // 5/x: wait the wait period
        yield return new WaitForSeconds(waitTime * waitTimeMultiplier);
    }

    void FillQueue()
    {
        /* rules: no same attack range (melee, range) back to back
        * no same arm (gungnir, trishula) back to back
        * 1 or 2, pick random for first, then pick the other one
        */

        // assume the queue is EMPTY, so clear it to be safe
        attackQueue.Clear();
    }

    void ShuffleQueue()
    {
        
    }
    #endregion

    #region Attacks
    IEnumerator GungnirR1()
    {
        // 360 laser shot for 8 seconds
        yield return null;
    }

    IEnumerator GungnirR2()
    {
        // instantly shoot the enemy
        yield return null;
    }

    IEnumerator GungnirM1()
    {
        // charge forward a few times
        yield return null;
    }

    IEnumerator GungnirM2()
    {
        // basically samus final smash
        yield return null;
    }

    IEnumerator TrishulaR1()
    {
        // shoot 8 shots, rotate each shot
        yield return null;
    }

    IEnumerator TrishulaR2()
    {
        // shoot shots that split into smaller shots
        yield return null;
    }

    IEnumerator TrishulaM1()
    {
        // pantheon tap q
        yield return null;
    }

    IEnumerator TrishulaM2()
    {
        // darius q
        yield return null;
    }
    #endregion
}
