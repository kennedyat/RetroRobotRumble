using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField, Tooltip("The health of this enemy.")]
    int health;

    public int DealDamage(int damageToDeal)
    {
        // realDamage is the amount of damage actually dealt ignoring overkill damage
        // ex. dealing 10 to a 5 hp enemy counts as 5 dmg)
        // in some games it doesnt work like this (like TFT, i know someone reading this is here)
        // but no idea how we want it
        int realDamage = 0;
        if (health < damageToDeal)
        {
            realDamage = health;
        }
        else
        {
            realDamage = damageToDeal;
        }

        health -= realDamage;
        if (health <= 0)
            Destroy(this.gameObject);

        // use the return value if we need access to how much damage it did
        // like lifesteal calculations or damage trackers
        return realDamage;
    }
}
