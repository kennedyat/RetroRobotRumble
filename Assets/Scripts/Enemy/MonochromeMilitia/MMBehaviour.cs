using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Combat.Prototype;
using UnityEngine;

public class MMBehaviour : MonoBehaviour
{
    public enum EnemyState
    {
        ChasePlayer,
        StopAndFire
    }

    [Header("References")]
    public Transform player;
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float minDistanceBetweenUnits = 2f;
    public float stopRange = 10f;

    [Header("Attack Settings")]
    public float attackCooldown = 1.2f;
    public float projectileSpread = 1.5f;

    [Header("Projectile Settings")]
    public float projectileSpeed = 25f;
    public float projectileLifetime = 3f;

    private Rigidbody rb;
    private bool isAttacking = false;
    private bool canShoot = true;
    private MMBehaviour[] allMilitia;

    [Header("Debug")]
    public EnemyState currentState = EnemyState.ChasePlayer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("No Rigidbody attached to Monochrome Militia!");
        }
        allMilitia = FindObjectsOfType<MMBehaviour>();

        player = GameObject.FindWithTag("Player").transform;
    }

    void FixedUpdate()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > stopRange)
        {
            currentState = EnemyState.ChasePlayer;
            isAttacking = false;
            MoveTowardPlayer();
        }
        else
        {
            currentState = EnemyState.StopAndFire;
            rb.velocity = Vector3.zero;
            isAttacking = true;

            if (canShoot)
            {
                StartCoroutine(ShootRoutine());
            }
        }
    }

    void MoveTowardPlayer()
    {
        Vector3 toPlayer = (player.position - transform.position).normalized;
        Vector3 separation = Vector3.zero;

        foreach (var ally in allMilitia)
        {
            if (ally != this)
            {
                Vector3 toAlly = transform.position - ally.transform.position;
                float dist = toAlly.magnitude;
                if (dist < minDistanceBetweenUnits && dist > 0.001f)
                {
                    separation += toAlly.normalized * (minDistanceBetweenUnits - dist);
                }
            }
        }

        float separationStrength = 2.5f;
        Vector3 moveDir = (toPlayer + separation * separationStrength).normalized;
        rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);

        Vector3 flatDir = new Vector3(toPlayer.x, 0f, toPlayer.z);
        if (flatDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(flatDir, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * 5f));
        }
    }

    IEnumerator ShootRoutine()
    {
        canShoot = false;

        Vector3 targetPos = player.position;
        targetPos += new Vector3(
            Random.Range(-projectileSpread, projectileSpread),
            Random.Range(-projectileSpread, projectileSpread),
            Random.Range(-projectileSpread, projectileSpread)
        );

        Vector3 direction = (targetPos - firePoint.position).normalized;
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(direction));

        MMProjectiles projScript = proj.GetComponent<MMProjectiles>();
        if (projScript != null)
        {
            projScript.Init(direction, projectileSpeed, projectileLifetime, gameObject);
        }

        yield return new WaitForSeconds(attackCooldown);
        canShoot = true;
    }
}
