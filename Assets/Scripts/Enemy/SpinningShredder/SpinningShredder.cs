using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinningShredder : Enemy
{
    [Header("References")]
    [SerializeField, Tooltip("Leave as empty (null) if this is not supposed to split")]
    GameObject splitPrefab;

    [Header("Attacking")]
    [SerializeField, Tooltip("Speed of the spinner as it charges forward")]
    float chargeSpeed = 4f;
    [SerializeField, Tooltip("Maximum random offset from the player")]
    float degOffset = 1f;
    [SerializeField, Tooltip("Knockback dealt to enemies and the player")]
    float knockbackStrength = 4f;
    [SerializeField, Tooltip("Knockback done to self when hitting something")]
    float selfKnockback = 2f;
    [SerializeField, Tooltip("Refractory period before this enemy can attack again")]
    float refractoryPeriod = 4f;

    [Header("Group Behavior")]
    [SerializeField, Tooltip("How far the spinners will try to stay apart from each other")]
    float separationDistance = 1f;

    // for group behavior
    static List<SpinningShredder> shredders = new();

    protected override void Start()
    {
        base.Start();
        navMeshAgent.radius = separationDistance;

        // add this to the shredder list
        shredders.Add(this);

        StartCoroutine(AttackLogic());
    }

    IEnumerator AttackLogic()
    {
        while (currentState != EnemyState.Death)
        {
            yield return new WaitWhile(() => currentState == EnemyState.Stunned);
            attackCoroutine = StartCoroutine(AttackSequence());
            attackStarted = true;
            yield return new WaitWhile(() => attackStarted);
        }
    }

    IEnumerator AttackSequence()
    {
        while (currentState != EnemyState.Stunned && currentState != EnemyState.Death)
        {
            // get in range of the player
            currentState = EnemyState.Chasing;
            while (!LineOfSight() || !WithinDistance())
            {
                navMeshAgent.SetDestination(player.position);
                yield return null;
            }

            // prepare to attack
            currentState = EnemyState.Attacking;
            navMeshAgent.ResetPath();
            FacePlayer();

            // somehow figure out group behavior as well
        }
    }

    protected override void DeathState()
    {
        base.DeathState();

        // remove this from the shredder list
        shredders.Remove(this);

        // if this is supposed to split, then do that
        if (splitPrefab != null)
        {
            // i will figure this out later bruh
        }
    }
}
