using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CoolCarBehavior : MonoBehaviour
{
    public enum CarStates { Patrolling = 0, WindingUp, Attacking, Stunned, Death }
    public CarStates State { get; private set; }

    [Header("Patrolling")]
    [SerializeField, Tooltip("The player's transform.")]
    Transform player;
    [SerializeField, Tooltip("The speed of the car as it chases the player.")]
    float moveSpeed;
    [SerializeField]
    float rotationSpeed;
    [SerializeField]
    float circlingRadius;
    [SerializeField, Tooltip("Distance the player needs to be within before the car starts its attack.")]
    float attackRange;
    private Rigidbody rb;

    [Header("Attacking")]
    [SerializeField, Tooltip("Time in seconds the car spends winding up before attacking.")]
    float windUpTime;
    [SerializeField, Tooltip("Distance the car winds backward over windUpTime seconds.")]
    float windUpDistance;
    [SerializeField, Tooltip("Speed of the car as it dashes towards the player.")]
    float attackDashSpeed;
    [SerializeField, Tooltip("Time the car is stunned when hits something.")]
    float stunPeriod;
    [SerializeField, Tooltip("The damage this car does to the player upon impact.")]
    int damage;
    [SerializeField, Tooltip("The distance the player will be knocked back when it hits the car.")]
    float knockbackDistance;
    [SerializeField, Tooltip("Knockback distance is multiplied if the car crashes into the player instead of the player running into the car.")]
    float knockbackMultiplier;

    // AUDIO: I'm trying something here
    public AK.Wwise.Event CarDeathEvent;
    public AK.Wwise.Event CarOnLaunchEvent;
    public AK.Wwise.Event CarOnWindUpEvent;
    public AK.Wwise.Event CarOnHittingPlayerEvent;
    public AK.Wwise.Event CarOnCollisionWithNonPlayerEvent;
    public AK.Wwise.Event PlasticImpactEvent;
    public AK.Wwise.Event FootStepEvent;

    bool attackStarted = false;
    bool stunned = false;

    protected void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    protected void FixedUpdate()
    {
        if (player == null)
            return;

        // death state if dies
        if (gameObject.GetComponent<EnemyHit>().GetHealth() <= 0)
        {
            // AUDIO: the car is dead, play a death sound

            CarDeathEvent.Post(gameObject);

            State = CarStates.Death;
            rb.constraints = RigidbodyConstraints.FreezeAll;
            StopAllCoroutines();
            return;
        }

        // initiate attack if player is within range
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
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
                FootStepEvent.Post(gameObject);

                State = CarStates.Patrolling;
                CircleAndApproachPlayer();
            }
        }
    }

    void CircleAndApproachPlayer()
    {
        Vector3 toPlayer = (player.position - transform.position).normalized;
        Vector3 perpendicular = Vector3.Cross(toPlayer, Vector3.up).normalized;
        Vector3 circlingDirection = (toPlayer + perpendicular * Mathf.Sin(Time.time * rotationSpeed)).normalized;
        rb.MovePosition(rb.position + moveSpeed * Time.fixedDeltaTime * circlingDirection);
        Quaternion targetRotation = Quaternion.LookRotation(toPlayer, Vector3.up);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed));
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>().TakeDamage(damage);

            // inflict a knockback on the player
            Vector3 forceVector = Vector3.Normalize(player.transform.position - transform.position);

            // make the knockback stronger depending on whether the car was attacking or the player just ran into it for fun
            // for now the player can only run into the car "for fun" when the car is stunned and not attacking
            float attackMultiplier = State == CarStates.Attacking ? knockbackMultiplier : 1.0f;
            other.GetComponent<Rigidbody>().AddForce(attackMultiplier * knockbackDistance * forceVector, ForceMode.VelocityChange);
        }
        // keeping it separate to make it clear
        else if (other.CompareTag("Enemy") && State == CarStates.Attacking) // only allow this when the cars are attacking
        {
            // per Daniel the designer, damage the other enemy
            // temporary error handling for now
            try
            {
                other.GetComponent<EnemyHit>().DealDamage(damage);
            }
            catch
            {
                Debug.LogError("No script named EnemyHit attached!");
            }

            // inflict a knockback in the same way
            // this time there is no multiplier, just a constant
            Vector3 forceVector = Vector3.Normalize(other.transform.position - transform.position);
            other.GetComponent<Rigidbody>().AddForce(0.25f * knockbackDistance * forceVector, ForceMode.VelocityChange);
        }
        if (attackStarted) // enemy hit something while attacking, stun it and play audio
        {
            // these are separated because of audio
            if (other.CompareTag("Player"))
            {
                stunned = true;

                // AUDIO: we hit the player
                CarOnHittingPlayerEvent.Post(gameObject);

            }
            else if (other.CompareTag("Level"))
            {
                stunned = true;

                // AUDIO: we crashed into something
                PlasticImpactEvent.Post(gameObject);

            }
            else if (other.CompareTag("Enemy"))
            {
                stunned = true;

                // AUDIO: the car hit another enemy
                CarOnCollisionWithNonPlayerEvent.Post(gameObject);
            }
        }
    }

    protected void OnTriggerStay(Collider other)
    {
        // to prevent the bug where winding up could cause it to go out of bounds
        if (attackStarted)
        {
            if (other.CompareTag("Level"))
            {
                stunned = true;
            }
        }
    }

    IEnumerator AttackSequence()
    {
        // update state
        State = CarStates.WindingUp;

        // get the two needed positions
        Vector3 playerPosition = ZeroY(player.transform.position);
        Vector3 startPosition = transform.position;

        // look at the player
        // height scaled to match the height of the car or else we would get random x rotations looking down
        Vector3 lookPos = playerPosition;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);

        // 1/2: dash backwards
        // first store the position of the backwards dash
        Vector3 backwardsPos = Vector3.Normalize(-1 * transform.forward) * windUpDistance;

        // AUDIO: the car is winding up, play a wind-up sound
        CarOnWindUpEvent.Post(gameObject);

        // note: it should match the duration of windUpTime

        // lerp to that position
        float time = 0;
        while (time < windUpTime)
        {
            time += Time.deltaTime;

            // if we hit something on the way there, stop
            // this keeps the car in place for the entire windup duration, but looks weird for the player
            if (!stunned)
                rb.MovePosition(Vector3.Lerp(startPosition, startPosition + backwardsPos, time / windUpTime));

            yield return new WaitForEndOfFrame();
        }
        stunned = false;

        // update state
        State = CarStates.Attacking;

        // 2/2: dash towards the player direction and go forward without stopping

        // AUDIO: the car is dashing forward after winding up, idk what sound matches lol
        CarOnLaunchEvent.Post(gameObject);

        while (!stunned) // stunned is controlled by collision
        {
            rb.MovePosition(transform.position + Time.deltaTime * attackDashSpeed * transform.forward);
            yield return new WaitForEndOfFrame();
        }

        // update the state again
        State = CarStates.Stunned;

        // AUDIO: DO NOT put anything here, collisions are controlled in OnTriggerEnter (detecting collisions)

        // the car hit something, make it wait before doing anything else
        yield return new WaitForSeconds(stunPeriod);

        // reset variables
        stunned = false;
        attackStarted = false;
    }

    // helper function
    Vector3 ZeroY(Vector3 input)
    {
        input.y = 0;
        return input;
    }
}
