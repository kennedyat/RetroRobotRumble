using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class EM_L1Hitbox : MonoBehaviour
{
    int damage;
    int playerLayer;

    public void Init(int d, float xScale, float zScale, int pl, bool renderThis = false)
    {
        damage = d;
        playerLayer = pl;

        float pScaleX = transform.parent.localScale.x;
        float pScaleY = transform.parent.localScale.y;
        float pScaleZ = transform.parent.localScale.z;

        transform.localScale = new Vector3(xScale / pScaleX, 1 / pScaleY, zScale / pScaleZ);
        transform.localPosition = new Vector3(0, 0, zScale / pScaleX / 2);

        GetComponent<Renderer>().enabled = renderThis;
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
