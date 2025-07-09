using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtoProjectile : MonoBehaviour
{
    private Rigidbody rb;
    public float projectileSpeed = 1f;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.up * projectileSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Level"))
        {
            Destroy(this.gameObject);
        }
    }
}
