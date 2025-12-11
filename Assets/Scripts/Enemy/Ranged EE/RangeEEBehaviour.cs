using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeEEBehaviour : Enemy
{
    public enum EliteRangedState { Chasing = 0, Shooting, Retreating, Death }

    [Header("References")]
    [SerializeField, Tooltip("The projectile to be shot")] 
    GameObject projectilePrefab;
    [SerializeField, Tooltip("Where projectiles appear/are instantiated")] 
    Transform firePoint;

    [Header("Movement Settings")]
    [SerializeField, Tooltip("How close the player needs to be for this enemy to start retreating")]
    float retreatRange = 2f;
    [SerializeField, Tooltip("I (Kevin) do not know what this does, ask Alex!")]
    float rotationSpeed = 8f;

    [Header("Attack Settings")]
    [SerializeField, Tooltip("The cooldown in seconds between attacks")] 
    float fireInterval = 1.5f;
    [SerializeField, Tooltip("The speed of the projectile fired")]
    float projectileSpeed = 20f;
    [SerializeField, Tooltip("How long the projectile lasts")]
    float projectileLifetime = 5f;

    [Header("Dash Settings")]
    [SerializeField, Tooltip("How far this enemy dashes")] 
    float dashDistance = 5f;
    [SerializeField, Tooltip("How long it takes to complete a dash")] 
    float dashDuration = 0.2f;
    [SerializeField, Tooltip("Time between dashes. Elite enemies dash 'off cooldown'")] 
    float dashCooldown = 5f;

    [Header("Debug")]
    public EliteRangedState currentState = EliteRangedState.Chasing;
    public bool isInvulnerable = false;

    private float fireTimer = 0f;
    private float dashTimer = 0f;
    private bool isDashing = false;

    void Update()
    {
        if (Terminate()) return;

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
            currentState = EliteRangedState.Chasing;
        }
        else if (distance < retreatRange)
        {
            currentState = EliteRangedState.Retreating;
        }
        else
        {
            currentState = EliteRangedState.Shooting;
        }

        if (currentState == EliteRangedState.Shooting && fireTimer >= fireInterval)
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
        if (Terminate()) return;

        if (isDashing)
            return;

        Vector3 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;
        Vector3 dir = toPlayer.normalized;

        Vector3 moveDir = Vector3.zero;

        switch (currentState)
        {
            case EliteRangedState.Chasing:
                moveDir = dir;
                break;

            case EliteRangedState.Shooting:
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

            case EliteRangedState.Retreating:
                moveDir = -dir;
                break;
        }

        rb.MovePosition(rb.position + moveSpeed * Time.fixedDeltaTime * moveDir);

        Vector3 flatDir = new Vector3(dir.x, 0f, dir.z);
        if (flatDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(flatDir, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * rotationSpeed));
        }
    }

    protected override void DeathState()
    {
        currentState = EliteRangedState.Death;
        StopCoroutine(nameof(DashRoutine));
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    public override int DealDamage(int damageToDeal)
    {
        // if this enemy is dashing, it is invulnerable and we cannot deal damage
        if (isInvulnerable) damageToDeal = 0;

        // set to zero to still show effects
        return base.DealDamage(damageToDeal);
    }

    void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null || player == null)
            return;

        GameObject projObj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        REEProjectiles proj = projObj.GetComponent<REEProjectiles>();
            
        proj.Init(player, projectileSpeed, attackDamage, projectileLifetime, playerLayer, levelLayer);
    }

    void TryDash(float distanceToPlayer)
    {
        if (player == null)
            return;

        Vector3 toPlayer = (player.position - transform.position).normalized;
        Vector3 dashDir = Vector3.zero;

        switch (currentState)
        {
            case EliteRangedState.Chasing:
                dashDir = toPlayer;
                break;

            case EliteRangedState.Shooting:
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

            case EliteRangedState.Retreating:
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
