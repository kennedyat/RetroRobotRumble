using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cooldown : MonoBehaviour
{
    // Start is called before the first frame update
    

    public float GetCooldown(float cooldown, out float currentCooldown)
    {
        return currentCooldown = Mathf.Max(cooldown, 0f);
    }

    
}
