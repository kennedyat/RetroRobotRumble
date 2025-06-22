using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Abilities/Knockback")]
public class Knockback : Abilities
{
    // Start is called before the first frame update
    public float knockbackDistance = 5f;
    public float knockbackSpeed = 5f;
    public List<string> canHit;



    public override void Effect(GameObject effected)
    {
        foreach (var hitObject in canHit)
            if (hitObject == effected.tag)
            {
                Debug.Log("Getting there");
                if (effected.TryGetComponent<Rigidbody>(out var rb))
                {
                    Debug.Log("Hit");
                    rb.AddForce(effected.transform.forward * knockbackDistance * knockbackSpeed, ForceMode.Impulse);
                }
            
           }
              
    }
}