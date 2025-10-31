using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;

public class EliteMelee : MonoBehaviour
{
    public enum EliteMeleeStates { Chasing = 0, Attacking, Death }
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
    [SerializeField, Tooltip("Temporary smite projectile to visualize the melee enemy's attack")]
    GameObject TEMP_Smite;
    [SerializeField, Tooltip("The distance the player needs to be within to take dmaage")]
    float TEMP_distanceThreshold = 1f;
    [SerializeField, Tooltip("The delay that a melee attack initially has")]
    float attackWindup = 0.2f;
    [SerializeField, Tooltip("The length of the attack animation")]
    float attackLength = 0.3f;
    [SerializeField, Tooltip("The refractory period after an attack (can't attack)")]
    float attackRecovery = 0.5f;
    bool playerAttackSet = false;

    void Update()
    {
        // death state
        if (gameObject.GetComponent<EnemyHealth>().GetHealth() <= 0)
        {
            state = EliteMeleeStates.Death;
            StopAllCoroutines();
            return;
        }
        // get the distance to the player
        Vector3 posZeroY = new(player.position.x, transform.position.y, player.position.z);
        float distance = Vector3.Distance(transform.position, posZeroY);

        // also look at the player, but only if we aren't attacking
        if (!playerAttackSet) transform.LookAt(posZeroY);

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
            // don't chase if the attack coroutine is in progress
            if (!playerAttackSet)
            {
                state = EliteMeleeStates.Chasing;
                ChasePlayer();
            }
        }
    }

    IEnumerator AttackPlayerSequence()
    {
        // attacks have a delay specified in the inspector
        // during this time, save the position of the player
        Vector3 playerPos = player.position;
        yield return new WaitForSeconds(attackWindup);

        // then TEMPORARILY summon a block and smite the player
        GameObject newBLock = Instantiate(TEMP_Smite, player.position + Vector3.up * 3, Quaternion.identity);

        // and TEMPORARILY deal damage based on distance to the stored position
        if (Vector3.Distance(playerPos, player.position) <= TEMP_distanceThreshold)
        {
            Debug.Log("You got hit nerd");
        }

        // wait for the animation to end
        yield return new WaitForSeconds(attackLength);

        // then destroy the TEMP block
        Destroy(newBLock);

        // and wait for the refractory period to end
        yield return new WaitForSeconds(attackRecovery);

        // reset the coroutine variable to true
        playerAttackSet = false;
    }
    
    void ChasePlayer()
    {
        // beeline straight towards the player
        Vector3 playerPos = player.position;
        Vector3 towardsPlayer = playerPos - transform.position;
        rb.MovePosition(transform.position + chaseSpeed * Time.deltaTime * towardsPlayer.normalized);
    }
}
