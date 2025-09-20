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
    private bool isAttacking = false;

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
        if (player == null)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            if (!isAttacking)
            {
                isAttacking = true;
                Debug.Log("Enemy entered attack mode!");
            }
        }
        else
        {
            isAttacking = false;
            CircleAndApproachPlayer();
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
    }
}
