using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Lunge")]
public class Lunge : Abilities
{
    // Start is called before the first frame update
    public float distance = 5f;
    public float speed = 10f;

    private bool hitObject;

    public override void Effect(GameObject effected)
    {
        
        if (effected.TryGetComponent<Rigidbody>(out var rb))
        {
            if (!hitObject)
            {
                 Vector3 pos = Vector3.Lerp(rb.position, rb.position + (effected.transform.forward * speed), Time.fixedDeltaTime);
                rb.MovePosition(pos);
            }
           

        }

    }

    void OnTriggerStay(Collider other)
    {
        hitObject = true;
        
    }
}
