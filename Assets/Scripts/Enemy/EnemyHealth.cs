using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField, Tooltip("The health of this enemy.")]
    int health;

    public int DealDamage(int damageToDeal)
    {
        int realDamage = damageToDeal;

        // insert any damage more calculations here
        // realDamage = damageToDeal * damageResist * damageMultiplier;

        // nile told me (kevin) dont subtract for overkill damage
        // if player deals 10 to a 5 hp enemy count it as 10 not 5

        health -= realDamage;
        if (health <= 0)
        {
            Destroy(this.gameObject);
        }
        // use the return value if we need access to how much damage it did
        // like lifesteal calculations or damage trackers
        return realDamage;
    }
}
