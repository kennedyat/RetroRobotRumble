using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// LITERALLY copy paste from MM proj
public class FinalBossProj : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private float lifetime;
    private int damage;
    private int playerLayer;
    private int levelLayer;

    public void Init(Vector3 dir, float spd, float life, int dmg, int pl, int ll)
    {
        direction = dir;
        speed = spd;
        lifetime = life;
        damage = dmg;
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

        else if (otherLayer == levelLayer)
        {
            Destroy(gameObject);
        }
    }
}
