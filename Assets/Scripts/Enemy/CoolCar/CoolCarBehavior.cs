using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
public class CoolCarBehavior : Enemy
{
    public enum CarStates { Chasing = 0, WindingUp, Attacking, Stunned, Death }
    public CarStates State { get; private set; }

    [Header("Chasing")]
    [SerializeField]
    float rotationSpeed;
    [SerializeField]
    float circlingRadius;

    // these variables are initialized on start, randomly
    float curveDirection;
    float timeOffset;

    [Header("Attacking")]
    [SerializeField, Tooltip("The collider attached to this car, used to prevent it from winding up in a wall.")]
    GameObject windUpCollider;
    [SerializeField, Tooltip("Time in seconds the car spends winding up before attacking.")]
    float windUpTime;
    [SerializeField, Tooltip("Distance the car winds backward over windUpTime seconds.")]
    float windUpDistance;
    [SerializeField, Tooltip("Speed of the car as it dashes towards the player.")]
    float attackDashSpeed;
    [SerializeField, Tooltip("Time the car is stunned when hits something.")]
    float stunPeriod;
    [SerializeField, Tooltip("The distance the player will be knocked back when it hits the car.")]
    float knockbackDistance;
    [SerializeField, Tooltip("Knockback distance is multiplied if the car crashes into the player instead of the player running into the car.")]
    float knockbackMultiplier;

    bool attackStarted = false;
    bool stunned = false;

    protected override void Start()
    {
        base.Start();

        // so that all the enemies dont curve the same way
        curveDirection = Random.value < 0.5f ? 1f : -1f;

        // and all the enemies don't curve at the same time
        timeOffset = Random.Range(0, 10f);
    }

    protected void FixedUpdate()
    {
        if (Terminate()) return;

        if (attackStarted) return;

        if (WithinDistance() && LineOfSight())
        {
            if (!attackStarted && !stunned)
            {
                attackStarted = true;
                StartCoroutine(AttackSequence());
            }
        }
        else
        {
            if (!attackStarted)
            {
                // AUDIO: the car is moving, play "footsteps" sounds here

                State = CarStates.Chasing;
                navMeshAgent.SetDestination(player.transform.position);
            }
        }
    }

    protected override void DeathState()
    {
        base.DeathState();

        // AUDIO: the car is dead, play a death sound
        State = CarStates.Death;
        StopCoroutine(AttackSequence());
    }

    /*
    void CircleAndApproachPlayer()
    {
        Vector3 toPlayer = (player.position - transform.position).normalized;
        Vector3 perpendicular = Vector3.Cross(toPlayer, Vector3.up).normalized;
        Vector3 circlingDirection = (toPlayer + curveDirection * Mathf.Sin(Time.time * rotationSpeed + timeOffset) * perpendicular).normalized;
        rb.MovePosition(rb.position + moveSpeed * Time.fixedDeltaTime * circlingDirection);
        Quaternion targetRotation = Quaternion.LookRotation(toPlayer, Vector3.up);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed));
    }
    */

    protected void OnTriggerEnter(Collider other)
    {
        if (State == CarStates.Death) return;

        int otherLayer = other.gameObject.layer;

        if (otherLayer == playerLayer)
        {
            other.GetComponent<PlayerHealth>().TakeDamage(attackDamage);

            // inflict a knockback on the player
            Vector3 forceVector = Vector3.Normalize(other.transform.position - transform.position);

            // make the knockback stronger depending on whether the car was attacking or the player just ran into it for fun
            // for now the player can only run into the car "for fun" when the car is stunned and not attacking
            float attackMultiplier = State == CarStates.Attacking ? knockbackMultiplier : 1.0f;
            other.GetComponent<Rigidbody>().AddForce(attackMultiplier * knockbackDistance * forceVector, ForceMode.VelocityChange);
        }
        // keeping it separate to make it clear
        else if (otherLayer == enemyLayer && State == CarStates.Attacking) // only allow this when the cars are attacking
        {
            // per Daniel the designer, damage the other enemy
            other.GetComponent<Enemy>().DealDamage(attackDamage);

            // inflict a knockback in the same way
            // this time there is no multiplier, just a constant
            Vector3 forceVector = Vector3.Normalize(other.transform.position - transform.position);
            other.GetComponent<Rigidbody>().AddForce(0.25f * knockbackDistance * forceVector, ForceMode.VelocityChange);
        }
        
        if (attackStarted && State == CarStates.Attacking) // enemy hit something while attacking, stun it and play audio
        {
            // these are separated because of audio
            if (otherLayer == playerLayer)
            {
                stunned = true;

                // AUDIO: we hit the player
            }
            else if (otherLayer == levelLayer)
            {
                stunned = true;

                // AUDIO: we crashed into something
            }
            else if (otherLayer == enemyLayer)
            {
                stunned = true;

                // AUDIO: the car hit another enemy
            }
        }

        if (attackStarted && State == CarStates.WindingUp && !windUpCollider.GetComponent<CollisionChecker>().Clear)
        {
            stunned = true;
            rb.velocity = Vector3.zero;
        }
    }

    IEnumerator AttackSequence()
    {
        // update state
        State = CarStates.WindingUp;

        // remove navigation
        navMeshAgent.enabled = false;

        // remove all drag
        rb.drag = 0;

        // look at the player and store their position at the same time
        // height scaled to match the height of the car or else we would get random x rotations looking down
        Vector3 lookPos = SetY(player.transform.position, transform.position.y);
        transform.LookAt(lookPos);

        // 1/2: dash backwards
        // first store the position of the backwards dash
        Vector3 backwardsPos = -1 * transform.forward;

        // AUDIO: the car is winding up, play a wind-up sound
        // note: it should match the duration of windUpTime
        rb.velocity = backwardsPos * (windUpDistance / windUpTime);
        yield return new WaitForSeconds(windUpTime);
        stunned = false;

        // update state
        State = CarStates.Attacking;

        // 2/2: dash towards the player direction and go forward without stopping
        // AUDIO: the car is dashing forward after winding up, idk what sound matches lol
        rb.velocity = transform.forward * attackDashSpeed;
        yield return new WaitUntil(() => stunned); // stunned is controlled by collision

        // reset velocity
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.drag = 10;
   
        // update the state again
        State = CarStates.Stunned;

        // AUDIO: DO NOT put anything here, collisions are controlled in OnTriggerEnter (detecting collisions)

        // the car hit something, make it wait before doing anything else
        yield return new WaitForSeconds(stunPeriod);

        // reset variables
        stunned = false;
        attackStarted = false;
        navMeshAgent.enabled = true;

        // push the car out of any walls if it is in any
    }
}