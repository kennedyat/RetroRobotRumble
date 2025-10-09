using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ProtoProjectile : MonoBehaviour
{
    private Rigidbody rb;
    public float projectileSpeed = 1f;
    public Vector3 aimVector;
    protected void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = (1 / (transform.localScale.x * 2)) * projectileSpeed * aimVector;
        StartCoroutine(nameof(DestroyProjectile));
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Level"))
        {
            Destroy(this.gameObject);
        }
    }

    IEnumerator DestroyProjectile()
    {
        yield return new WaitForSecondsRealtime(5f);
        Destroy(this.gameObject);
    }
}
