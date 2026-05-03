using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FB_BurnArea : MonoBehaviour
{
    float tickTimer = 99999f;
    float burnCooldown;
    int playerLayer;
    int damage;

    public void Init(int d, float cooldown, int pl, float xScale, float zScale, float lifetime)
    {
        damage = d;
        burnCooldown = cooldown;
        playerLayer = pl;

        transform.localScale = new Vector3(xScale, transform.localScale.y, zScale);
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);

        Destroy(gameObject, lifetime);
        tickTimer = burnCooldown;
    }

    protected void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == playerLayer && tickTimer <= 0)
        {
            other.GetComponent<PlayerHealth>().TakeDamage(damage);
            tickTimer = burnCooldown;
        }
    }

    protected void Update()
    {
        tickTimer -= Time.deltaTime;
    }
}
