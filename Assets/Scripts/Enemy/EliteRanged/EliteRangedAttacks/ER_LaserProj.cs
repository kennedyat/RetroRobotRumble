using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ER_LaserProj : MonoBehaviour
{
    // copy pasted from FB_BurnArea
    float timer;
    float burnCooldown;
    int playerLayer;
    int damage;

    public void Init(int d, float cooldown, int pl)
    {
        damage = d;
        burnCooldown = cooldown;
        playerLayer = pl;

        // position and scale is controlled by elite ranged
    }

    protected void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == playerLayer && timer <= 0)
        {
            other.GetComponent<PlayerHealth>().TakeDamage(damage);
            timer = burnCooldown;
        }
    }

    protected void Update()
    {
        timer -= Time.deltaTime;
    }
}
