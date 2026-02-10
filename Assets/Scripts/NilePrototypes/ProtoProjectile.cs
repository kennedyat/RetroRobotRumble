using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ProtoProjectile : MonoBehaviour
{
    private Rigidbody rb;
    public float projectileSpeed = 1f;
    [SerializeField] private int damage = 1;
    [SerializeField] float lifetime = 5f;
    public Vector3 aimVector;

    // for layers, like the enemy class
    protected static int enemyLayer, levelLayer;

    protected void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = (1 / (transform.localScale.x * 2)) * projectileSpeed * aimVector;

        enemyLayer = LayerMask.NameToLayer("Enemy");
        levelLayer = LayerMask.NameToLayer("Level");

        Destroy(this.gameObject, lifetime);
    }

    protected void OnTriggerEnter(Collider other)
    {
        int otherLayer = other.gameObject.layer;

        if (otherLayer == levelLayer)
        {
            Destroy(this.gameObject);
        }

        if (otherLayer == enemyLayer)
        {
            other.GetComponent<Enemy>().DealDamage(damage);
            other.GetComponent<Enemy>().InflictStun(10, true);
        }
    }
}
