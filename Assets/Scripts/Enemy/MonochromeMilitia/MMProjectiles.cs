using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MMProjectiles : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private float lifetime;
    private float damage = 5f;
    private int playerLayer;
    private int levelLayer;

    public void Init(Vector3 dir, float spd, float life, int pl, int ll)
    {
        direction = dir;
        speed = spd;
        lifetime = life;
        playerLayer = pl;
        levelLayer = ll;

        Destroy(gameObject, lifetime);
    }

    protected void Update()
    {
        transform.position += speed * Time.deltaTime * direction;
    }

    protected void OnTriggerEnter(Collider other)
    {
        int otherLayer = other.gameObject.layer;

        // playerLayer is the layer that the player is in, passed by value
        // to this projectile with the init function
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
