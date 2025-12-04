using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REEProjectiles : MonoBehaviour
{
    private Transform target;
    private float speed;
    private float damage;
    private float lifetime;
    private GameObject owner;

    [SerializeField, Tooltip("Distance to player at which tracking stops and the projectile continues straight.")]
    private float stopTrackingDistance = 3f;

    private bool isTracking = true;
    private Vector3 currentDirection;

    public void Init(Transform targetTransform, float spd, float dmg, float life, GameObject shooter)
    {
        target = targetTransform;
        speed = spd;
        damage = dmg;
        lifetime = life;
        owner = shooter;

        if (target != null)
        {
            currentDirection = (target.position - transform.position).normalized;
        }

        isTracking = true;

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (isTracking && target != null)
        {
            Vector3 toTarget = target.position - transform.position;
            float sqrDist = toTarget.sqrMagnitude;
            Vector3 toTargetDir = toTarget.normalized;
            if (sqrDist <= stopTrackingDistance * stopTrackingDistance)
            {
                isTracking = false;
                currentDirection = toTargetDir;
            }
            else
            {
                currentDirection = toTargetDir;
            }
        }
        else if (isTracking && target == null)
        {
            isTracking = false;
        }
        transform.position += currentDirection * speed * Time.deltaTime;
        if (currentDirection.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(currentDirection, Vector3.up);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damage);
            }

            Destroy(gameObject);
        }

        if (other.CompareTag("Level"))
        {
            Destroy(gameObject);
        }
    }
}
