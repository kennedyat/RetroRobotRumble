using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class FB_PlayerCollider : MonoBehaviour
{
    public bool playerTookDamage = false;
    int enemyMask, enemyProjMask;

    protected void Start()
    {
        enemyMask = LayerMask.NameToLayer("Enemy");
        enemyProjMask = LayerMask.NameToLayer("EnemyProj");
    }

    protected void Update()
    {
        transform.position = transform.parent.position;
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == enemyMask || other.gameObject.layer == enemyProjMask)
        {
            // check if it should damage the player
            try
            {
                if (other.GetComponent<FB_CountAsAttack>().countAsAttack)
                {
                    playerTookDamage = true;
                }
            }
            catch
            {
                return;
            }
        }
    }
}
