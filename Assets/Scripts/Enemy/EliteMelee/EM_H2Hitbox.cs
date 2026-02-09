using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class EM_H2Hitbox : MonoBehaviour
{
    int damage;
    int playerLayer;
    float damageCooldown;
    float timer;
    public bool HasDamagedPlayer { get; private set; } = false;

    public void Init(float radius, int damagePerTick, float tickRate, int pl, bool renderThis = false)
    {
        // because of how radius works
        radius *= 2;

        damage = damagePerTick;
        playerLayer = pl;
        damageCooldown = tickRate;

        float pScaleX = transform.parent.localScale.x;
        float pScaleY = transform.parent.localScale.y;
        float pScaleZ = transform.parent.localScale.z;

        transform.localScale = new Vector3(radius / pScaleX, 1 / pScaleY, radius / pScaleZ);
        transform.localPosition = Vector3.up * 1.5f;

        GetComponent<Renderer>().enabled = renderThis;

        HasDamagedPlayer = false;
    }

    protected void OnTriggerStay(Collider other)
    {
        // same as other projectiles, except we don't destroy this
        int otherLayer = other.gameObject.layer;

        if (otherLayer == playerLayer && timer <= 0)
        {
            other.GetComponent<PlayerHealth>().TakeDamage(damage);
            timer = damageCooldown;
            HasDamagedPlayer = true;
        }
    }

    protected void Update()
    {
        timer -= Time.deltaTime;
    }
}
