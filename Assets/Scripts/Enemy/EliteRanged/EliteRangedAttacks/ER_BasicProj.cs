using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ER_BasicProj : MonoBehaviour
{
    private float speed;
    private int damage;
    private float lifetime;
    private int playerLayer, levelLayer;

    public void Init(float spd, int dmg, float life, float scale, int pl, int ll)
    {
        speed = spd;
        damage = dmg;
        lifetime = life;
        playerLayer = pl;
        levelLayer = ll;

        transform.localScale = Vector3.one * scale;

        Destroy(gameObject, lifetime);
    }

    protected void Update()
    {
        transform.position += speed * Time.deltaTime * transform.forward;
    }

    protected void OnTriggerEnter(Collider other)
    {
        int otherLayer = other.gameObject.layer;

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
