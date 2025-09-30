using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField, Tooltip("The health of this enemy.")]
    int health;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Projectile"))
            this.health--;
        if (this.health <= 0)
            Destroy(this.gameObject);
    }
}
