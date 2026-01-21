using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class FB_Hitbox : MonoBehaviour
{
    int damage;
    int playerLayer;

    public void Init(int d, float xScale, float zScale, int pl, bool renderThis)
    {
        damage = d;
        playerLayer = pl;

        float fbScaleX = transform.parent.localScale.x;
        float fbScaleY = transform.parent.localScale.y;
        float fbScaleZ = transform.parent.localScale.z;

        transform.localScale = new Vector3(xScale / fbScaleX, 1 / fbScaleY, zScale / fbScaleZ);
        transform.localPosition = new Vector3(0, 0, zScale / fbScaleX / 2);

        if (!renderThis)
        {
            GetComponent<Renderer>().enabled = false;
        }
    }

    protected void OnTriggerEnter(Collider other)
    {
        // same as other projectiles, except we don't destroy this
        int otherLayer = other.gameObject.layer;

        if (otherLayer == playerLayer)
        {
            other.GetComponent<PlayerHealth>().TakeDamage(damage);
        }
    }
}
