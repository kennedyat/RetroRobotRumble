using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ST_Hazard : MonoBehaviour
{
    int playerLayer;
    int damage;

    public void Init(int d, int pl, float xScale, float zScale, float lifetime)
    {
        damage = d;
        playerLayer = pl;

        transform.localScale = new Vector3(xScale, transform.localScale.y, zScale);
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);

        Destroy(gameObject, lifetime);
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == playerLayer)
        {
            BarkManager.Instance?.PlayBark("Player Take Damage (Hazard)", "Stego Slicer");
            other.GetComponent<PlayerHealth>().TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
