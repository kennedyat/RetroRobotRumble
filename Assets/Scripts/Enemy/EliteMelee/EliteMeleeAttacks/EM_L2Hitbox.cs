using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class EM_L2Hitbox : MonoBehaviour
{
    int damage;
    int playerLayer;

    public void Init(int d, float radius, int pl, bool renderThis = false)
    {
        damage = d;
        playerLayer = pl;

        float pScaleX = transform.parent.localScale.x;
        float pScaleY = transform.parent.localScale.y;
        float pScaleZ = transform.parent.localScale.z;

        transform.localScale = new Vector3(radius / pScaleX, 1 / pScaleY, radius / pScaleZ);
        transform.localPosition = Vector3.up * 1.5f;

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
