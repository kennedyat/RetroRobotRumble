using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MMProjectiles : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private float lifetime;
    private GameObject owner;
    private float damage = 5f;

    public void Init(Vector3 dir, float spd, float life, GameObject shooter)
    {
        direction = dir;
        speed = spd;
        lifetime = life;
        owner = shooter;

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(owner.name + " shot " + other.gameObject.name);

        if (other.isTrigger || other.gameObject.CompareTag("Enemy"))
        {
            Debug.LogWarning("Ignoring collision with: " + other.gameObject.name);
            return;
        }

        PlayerHealth ph = other.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
