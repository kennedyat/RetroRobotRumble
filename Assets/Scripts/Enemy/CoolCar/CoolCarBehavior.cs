using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoolCarBehavior : MonoBehaviour
{
    public Transform player; 
    public float moveSpeed = 2f;
    public float rotationSpeed = 2f;   
    public float attackRange = 5f; 
    public float circlingRadius = 5f;
    private Rigidbody rb;
    //private bool isAttacking = false;

    [SerializeField, Tooltip("Speed of the car as it dashes towards the player.")]
    float travelSpeed;
    [SerializeField, Tooltip("Time in seconds the car spends winding up before attacking.")]
    float windUpTime;
    [SerializeField, Tooltip("Distance the car winds backward over windUpTime seconds.")]
    float windUpDistance;
    //[SerializeField, Tooltip("Distance the player needs to be within before the car starts its attack.")]
    //float playerDetectionRange;
    [SerializeField, Tooltip("Time the car is stunned when hits something.")]
    float stunPeriod;

    bool triggerSet = false;
    bool stunned = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("No Rigidbody attached to enemy!");
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            if (!stunned && !triggerSet)
            {
                //isAttacking = true;
                triggerSet = true;
                Debug.Log("Enemy entered attack mode!");
                StartCoroutine(AttackSequence());
            }
        }
        else
        {
            if (!triggerSet)
            {
                //isAttacking = false;
                CircleAndApproachPlayer();
            }
        }
    }

    void CircleAndApproachPlayer()
    {
        Vector3 toPlayer = (player.position - transform.position).normalized;
        Vector3 perpendicular = Vector3.Cross(toPlayer, Vector3.up).normalized;
        Vector3 circlingDirection = (toPlayer + perpendicular * Mathf.Sin(Time.time * rotationSpeed)).normalized;
        rb.MovePosition(rb.position + circlingDirection * moveSpeed * Time.fixedDeltaTime);
        Quaternion targetRotation = Quaternion.LookRotation(toPlayer, Vector3.up);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Enemy detected player via trigger!");
        }
        if (triggerSet)
        {
            stunned = true;
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

            transform.position = Vector3.Lerp(startPosition, startPosition + backwardsPos, time / windUpTime);
            yield return new WaitForEndOfFrame();
        }

        // 2/2: dash towards the player direction and go forward without stopping
        while (!stunned) // stunned is controlled by collision (see below)
        {
            transform.position += Time.deltaTime * travelSpeed * transform.forward;
            yield return new WaitForEndOfFrame();
        }

        // the car hit something, make it wait before doing anything else
        yield return new WaitForSeconds(stunPeriod);

        // then reset variables
        stunned = false;
        triggerSet = false;
    }

    Vector3 ZeroY(Vector3 input)
    {
        input.y = 0;
        return input;
    }
}
