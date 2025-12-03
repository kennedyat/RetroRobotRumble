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

    public void Init(Transform targetTransform, float spd, float dmg, float life, GameObject shooter)
    {
        target = targetTransform;
        speed = spd;
        damage = dmg;
        lifetime = life;
        owner = shooter;

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 toTarget = (target.position - transform.position).normalized;
        transform.position += toTarget * speed * Time.deltaTime;

        if (toTarget.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(toTarget, Vector3.up);
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
