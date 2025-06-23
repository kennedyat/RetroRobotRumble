using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Lunge")]
public class Lunge : Abilities
{
    // Start is called before the first frame update

    [Header("Lunge Parameters")]
    public float distance = 5f;
    public float speed = 10f;



    public override void Effect(GameObject effected, GameObject player)
    {

        if (effected.TryGetComponent<Rigidbody>(out var rb))
        {
            if (!onHit)
            {
                Vector3 pos = Vector3.Lerp(rb.position, rb.position + (effected.transform.forward * speed), Time.fixedDeltaTime);
                rb.MovePosition(pos);

                Debug.Log("Lunging");
            }
            else
            {
                Debug.Log("Lunge stopped");
            }


        }

    }

    public override void OnCollision(GameObject effected, GameObject player)
    {
        
    }

}
