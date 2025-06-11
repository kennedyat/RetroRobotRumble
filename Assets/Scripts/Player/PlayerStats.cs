using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    // Start is called before the first frame update
    public float health;

    public float normalDamage;

    public float specialDamage;

    public float speed;

    public void SetMaxHealth(float addedHealth)
    {
        health += addedHealth;
    }
    
    public void SetSpeed(float addedSpeed)
    {
        Debug.Log("Changed speed from: " + speed + " --> " + (speed + addedSpeed));
        speed += addedSpeed;
    }


    
}
