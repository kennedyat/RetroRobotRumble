using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EliteMelee : MonoBehaviour
{
    public enum EliteMeleeStates { Chasing = 0, Attacking }
    public EliteMeleeStates state { get; private set; }
    [Header("References")]
    [SerializeField, Tooltip("The transform of the player")]
    Transform player;
    [SerializeField, Tooltip("The rigidbody of this enemy")]
    Rigidbody rb;
    [Header("Chasing")]
    [SerializeField, Tooltip("The speed that the enemy chases the player down at")]
    float chaseSpeed = 5f;
    [SerializeField, Tooltip("The range of the enemy, or the range it needs to be within before initiating attacks")]
    float attackDistance = 5f;
    [Header("Attacking")]
    float attackWindup = 0.2f;
    float attackLength = 0.3f;
    float attackRecovery = 0.5f;
    bool playerAttackSet = false;

    void Update()
    {
        // get the distance to the player, compensated for a small offset
        Vector3 posZeroY = new(player.position.x + 0.45f, transform.position.y, player.position.z);
        float distance = Vector3.Distance(transform.position, posZeroY);

        // also look at the player
        transform.LookAt(posZeroY);

        // if the player is close enough, start attacking
        if (distance <= attackDistance)
        {
            state = EliteMeleeStates.Attacking;
            if (!playerAttackSet)
            {
                playerAttackSet = true;
                StartCoroutine(AttackPlayerSequence());
            }
        }
        // otherwise continue chasing
        else
        {
            state = EliteMeleeStates.Chasing;
            ChasePlayer();
        }
    }

    IEnumerator AttackPlayerSequence()
    {
        yield return null;
        playerAttackSet = false;
    }
    
    void ChasePlayer()
    {
        // beeline straight towards the player
        Vector3 playerPos = player.position + Vector3.right * 0.45f;
        Vector3 towardsPlayer = playerPos - transform.position;
        rb.MovePosition(transform.position + chaseSpeed * Time.deltaTime * towardsPlayer.normalized);
    }
}
