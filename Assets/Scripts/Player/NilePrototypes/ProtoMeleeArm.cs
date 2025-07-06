using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtoMeleeArm : MonoBehaviour
{
    [SerializeField] GameObject hitboxObject;
    [SerializeField] Transform hitboxSpawn;

    [SerializeField] float hitboxLifetime = 1f;

    public void BasicAttack()
    {
        // destroy all children of spawn, instantiate hitbox, adjust scale, destroy after lifetime
    }

    public void SpecialAttack()
    {
        // destroy all children of spawn, instantiate hitbox, adjust scale (BEEG), destroy after lifetime
    }
}
