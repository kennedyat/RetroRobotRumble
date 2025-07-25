using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;

public class HitBox : MonoBehaviour
{
    // Start is called before the first frame update
    BoxCollider box;
    public Action<Collider> OnHit; // Delegate to notify abilities
    public bool isActive;

    public RuntimeDebugger debugger;



    public void EnableIFrame()
    {

    }

    public void DisableIFrame()
    {

    }

    public void OnEnable()
    {
        box.enabled = true;
    }
    public void OnDisable()
    {
        box.enabled = false;
    }

    public void ModifyBox()
    {

    }

    void OnTriggerStay(Collider other)
    {
        if (!isActive || other.CompareTag("Enemy")) return;
        OnHit?.Invoke(other);
        debugger.OnDrawActiveHitbox(this.gameObject);
    }
}
