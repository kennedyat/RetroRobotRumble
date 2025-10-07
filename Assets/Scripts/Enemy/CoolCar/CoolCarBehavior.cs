using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CoolCarBehavior : MonoBehaviour
{
    [SerializeField, Tooltip("The player's transform.")]
    Transform player;
    [SerializeField, Tooltip("The speed of the car as it chases the player.")]
    float moveSpeed;
    [SerializeField]
    float rotationSpeed;
    [SerializeField, Tooltip("Distance the player needs to be within before the car starts its attack.")]
    float attackRange;
    [SerializeField]
    float circlingRadius;
    private Rigidbody rb;

    [SerializeField, Tooltip("Speed of the car as it dashes towards the player.")]
    float attackDashSpeed;
    [SerializeField, Tooltip("Time in seconds the car spends winding up before attacking.")]
    float windUpTime;
    [SerializeField, Tooltip("Distance the car winds backward over windUpTime seconds.")]
    float windUpDistance;
    [SerializeField, Tooltip("Time the car is stunned when hits something.")]
    float stunPeriod;
    [SerializeField, Tooltip("The damage this car does to the player upon impact.")]
    int damage;
    [SerializeField, Tooltip("The distance the player will be knocked back when it hits the car.")]
    float knockbackDistance;

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
            // other.GetComponent<whatever the player script is called>().DealDamage(damage);

            // inflict a knockback on the player
            Vector3 forceVector = player.transform.position - transform.position;

            // make the knockback stronger depending on whether the car was attacking or the player just ran into it for fun
            //int attackMultiplier = attackStarted ? 5 : 1;
            other.GetComponent<Rigidbody>().AddForce(/*attackMultiplier * */knockbackDistance * forceVector, ForceMode.VelocityChange);

        }
        if (attackStarted)
        {
            if (other.CompareTag("Player") || other.CompareTag("Level") || other.CompareTag("Enemy"))
            {
                stunned = true;
            }
        }
    }

    protected void OnTriggerStay(Collider other)
    {
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
        Vector3 backwardsPos = Vector3.Normalize(ZeroY(transform.position) - playerPosition) * windUpDistance;

        // lerp to that position
        float time = 0;
        while (time < windUpTime)
        {
            time += Time.deltaTime;

            // if we hit something on the way there, stop
            if (!stunned)
                rb.MovePosition(Vector3.Lerp(startPosition, startPosition + backwardsPos, time / windUpTime));

            yield return new WaitForEndOfFrame();
        }
        stunned = false;

        // 2/2: dash towards the player direction and go forward without stopping
        while (!stunned) // stunned is controlled by collision (see below)
        {
            rb.MovePosition(transform.position + Time.deltaTime * attackDashSpeed * transform.forward);
            yield return new WaitForEndOfFrame();
        }

        // the car hit something, make it wait before doing anything else
        yield return new WaitForSeconds(stunPeriod);

        // reset variables first so the knockback multiplier works
        stunned = false;
        attackStarted = false;
    }

    Vector3 ZeroY(Vector3 input)
    {
        input.y = 0;
        return input;
    }
}
