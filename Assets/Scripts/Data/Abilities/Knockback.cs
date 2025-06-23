using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Abilities/Knockback")]
public class Knockback : Abilities
{
    // Start is called before the first frame update

     [Header("Knockback Parameters")]
    public float knockbackDistance = 5f;
    public float knockbackSpeed = 5f;

 
    public override void Effect(GameObject effected, GameObject player)
    {
        foreach (var hitObject in canHit)
            if (hitObject == effected.tag)
            {
                Debug.Log("Getting there");
                if (effected.TryGetComponent<Rigidbody>(out var rb))
                {
                    Debug.Log("Effected: " + effected.name + " Transform: " + effected.transform.forward);
                    rb.AddForce(player.transform.forward * knockbackDistance * knockbackSpeed, ForceMode.Impulse);
                }

            }

    }
    
    public override void OnCollision(GameObject effected, GameObject player)
    {
        
    }
}