using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class FB_DotHitbox : MonoBehaviour
{
    int damage;
    float tickRate;
    int playerLayer;
    float timer;

    public void Init(int d, float tickRate, float xScale, float zScale, int pl, bool renderThis = false)
    {
        damage = d;
        this.tickRate = tickRate;
        playerLayer = pl;

        float fbScaleX = transform.parent.localScale.x;
        float fbScaleY = transform.parent.localScale.y;
        float fbScaleZ = transform.parent.localScale.z;

        transform.localScale = new Vector3(xScale / fbScaleX, 1 / fbScaleY, zScale / fbScaleZ);

        transform.localPosition = new Vector3(0, 0, zScale / fbScaleX / 2);

        GetComponent<Renderer>().enabled = renderThis;

    }

    protected void Update()
    {
        timer -= Time.deltaTime;
    }

    protected void OnTriggerStay(Collider other)
    {
        // same as other projectiles, except we don't destroy this
        int otherLayer = other.gameObject.layer;

        if (otherLayer == playerLayer && timer <= 0f)
        {
            other.GetComponent<PlayerHealth>().TakeDamage(damage);
            timer = tickRate;
        }
    }
}
