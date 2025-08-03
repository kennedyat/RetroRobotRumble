using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;

public class HitBox : MonoBehaviour
{
    //For duration, maybe have it be in relation to animation times rather than set timers???
    BoxCollider box;
    MeshRenderer meshRenderer;
    public Action<Collider> OnHit; // Delegate to notify abilities
    public bool isActive;

    public RuntimeDebugger debugger;



    public void Awake()
    {
        this.enabled = false;
        box = GetComponent<BoxCollider>();
        meshRenderer = GetComponent<MeshRenderer>();

         box.enabled = false;
        meshRenderer.enabled = false;
    }
    public void EnableIFrame()
    {
        Debug.Log("Enabled");
        meshRenderer.enabled = true;
        box.enabled = true;
        debugger.OnDrawDefaultHitbox(this.gameObject);
    }

    public void DisableIFrame()
    {
        Debug.Log("Disabled");
        box.enabled = false;
        meshRenderer.enabled = false;
    }

    public void OnEnable()
    {

    }
    public void OnDisable()
    {

    }

    public void ModifyBox()
    {

    }

    public void OnTriggerStay(Collider collision)
    {

        if (!collision.CompareTag("Enemy")) return;

        if (OnHit != null)
        {
            OnHit?.Invoke(collision);
            debugger.OnDrawActiveHitbox(this.gameObject);
        }
      

        
    }
    
     public void OnTriggerExit(Collider collision)
    {
         debugger.OnDrawDefaultHitbox(this.gameObject);
    }
}
