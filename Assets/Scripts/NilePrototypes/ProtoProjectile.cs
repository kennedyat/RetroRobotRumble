using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtoProjectile : MonoBehaviour
{
    private Rigidbody rb;
    public float projectileSpeed = 1f;
    public Vector3 aimVector;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = aimVector * projectileSpeed * (1/(transform.localScale.x * 2));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Level"))
        {
            Destroy(this.gameObject);
        }
    }
}
