using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REEProjectiles : MonoBehaviour
{
    private Transform target;
    private float speed;
    private int damage;
    private float lifetime;
    private GameObject owner;
    private int playerLayer, levelLayer;

    public void Init(Transform targetTransform, float spd, int dmg, float life, int pl, int ll)
    {
        target = targetTransform;
        speed = spd;
        damage = dmg;
        lifetime = life;
        playerLayer = pl;
        levelLayer = ll;

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (isTracking && target != null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 toTarget = (target.position - transform.position).normalized;
        transform.position += speed * Time.deltaTime * toTarget;

        if (toTarget.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(currentDirection, Vector3.up);
        }
    }

    protected void OnTriggerEnter(Collider other)
    {
        int otherLayer = other.gameObject.layer;

        if (otherLayer == playerLayer)
        {
            other.GetComponent<PlayerHealth>().TakeDamage(damage);
            Destroy(gameObject);
        }

        if (otherLayer == levelLayer)
        {
            Destroy(gameObject);
        }
    }
}
