using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeEEBehaviour : MonoBehaviour
{
    public enum EliteState
    {
        ChasePlayer,
        Attack,
        Retreat
    }

    [Header("References")]
    public Transform player;
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float attackRange = 8f;
    public float retreatRange = 2f;
    public float rotationSpeed = 8f;

    [Header("Attack Settings")]
    public float fireInterval = 1.5f;
    public float projectileSpeed = 20f;
    public float projectileLifetime = 5f;
    public float projectileDamage = 10f;

    [Header("Dash Settings")]
    public float dashDistance = 5f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 5f;

    [Header("Debug")]
    public EliteState currentState = EliteState.ChasePlayer;
    public bool isInvulnerable = false;

    private Rigidbody rb;
    private float fireTimer = 0f;
    private float dashTimer = 0f;
    private bool isDashing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("No Rigidbody on EliteRangedBehaviour!");
        }

        player = GameObject.FindWithTag("Player").transform;
    }

    void Update()
    {
        if (player == null)
        {
            return;
        }
            

        if (!isDashing)
        {
            fireTimer += Time.deltaTime;
        }

        if (dashTimer > 0f)
        {
            dashTimer -= Time.deltaTime;
        }

        if (isDashing)
        {
            return;
        }
            

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > attackRange)
        {
            currentState = EliteState.ChasePlayer;
        }
        else if (distance < retreatRange)
        {
            currentState = EliteState.Retreat;
        }
        else
        {
            currentState = EliteState.Attack;
        }

        if (currentState == EliteState.Attack && fireTimer >= fireInterval)
        {
            FireProjectile();
            fireTimer = 0f;
        }

        if (dashTimer <= 0f)
        {
            TryDash(distance);
        }
    }

    void FixedUpdate()
    {
        if (player == null || isDashing)
            return;

        Vector3 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;
        Vector3 dir = toPlayer.normalized;

        Vector3 moveDir = Vector3.zero;

        switch (currentState)
        {
            case EliteState.ChasePlayer:
                moveDir = dir;
                break;

            case EliteState.Attack:
                if (distance > attackRange * 0.95f)
                {
                    moveDir = dir;
                }
                else if (distance < attackRange * 0.8f)
                {
                    moveDir = -dir;
                }
                else
                {
                    moveDir = Vector3.zero;
                }
                break;

            case EliteState.Retreat:
                moveDir = -dir;
                break;
        }

        rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);

        Vector3 flatDir = new Vector3(dir.x, 0f, dir.z);
        if (flatDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(flatDir, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * rotationSpeed));
        }
    }

    void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null || player == null)
            return;

        GameObject projObj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        REEProjectiles proj = projObj.GetComponent<REEProjectiles>();
        if (proj != null)
        {
            proj.Init(player, projectileSpeed, projectileDamage, projectileLifetime, gameObject);
        }
    }

    void TryDash(float distanceToPlayer)
    {
        if (player == null)
            return;

        Vector3 toPlayer = (player.position - transform.position).normalized;
        Vector3 dashDir = Vector3.zero;

        switch (currentState)
        {
            case EliteState.ChasePlayer:
                dashDir = toPlayer;
                break;

            case EliteState.Attack:
                Vector3 side = Vector3.Cross(Vector3.up, toPlayer).normalized;
                if (Random.value > 0.5f)
                {
                    dashDir = side;
                }
                else
                {
                    dashDir = -side;
                }
                break;

            case EliteState.Retreat:
                dashDir = -toPlayer;
                break;
        }

        if (dashDir.sqrMagnitude > 0.001f)
        {
            StartCoroutine(DashRoutine(dashDir));
        }
    }

    IEnumerator DashRoutine(Vector3 direction)
    {
        isDashing = true;
        isInvulnerable = true;
        dashTimer = dashCooldown;

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + direction.normalized * dashDistance;

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dashDuration);
            Vector3 newPos = Vector3.Lerp(startPos, endPos, t);
            rb.MovePosition(newPos);
            yield return null;
        }

        isDashing = false;
        isInvulnerable = false;
    }
}
