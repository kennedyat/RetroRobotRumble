using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(MeshRenderer))]
public class HitBox : MonoBehaviour
{
    //For duration, maybe have it be in relation to animation times rather than set timers???
    BoxCollider box;
    MeshRenderer meshRenderer;
    public Action<Collider> OnHit; // Delegate to notify abilities
    public bool isActive;


    public RuntimeDebugger debugger;



    protected void Awake()
    {
        //this.enabled = false;
        box = GetComponent<BoxCollider>();
        meshRenderer = GetComponent<MeshRenderer>();
        isActive = false;
        box.enabled = false;
        meshRenderer.enabled = false;
    }
    public void EnableFrame(float duration)
    {
        Debug.Log("Enabled");
        meshRenderer.enabled = true;
        box.enabled = true;
        if (debugger != null)
        {
            debugger.OnDrawDefaultHitbox(this.gameObject);
        }

        isActive = true;

        if (duration > 0)
            StartCoroutine(DisableFrameControlled(duration));

    }

    public void DisableFrame()
    {
        Debug.Log("Disabled");
        box.enabled = false;
        meshRenderer.enabled = false;
        isActive = false;
    }

    IEnumerator DisableFrameControlled(float duration)
    {

        yield return new WaitForSeconds(duration);
        DisableFrame();
    }




    protected void OnTriggerStay(Collider collision)
    {

        if (!collision.CompareTag("Enemy"))
            return;

        if (OnHit != null)
        {
            OnHit?.Invoke(collision);
            if (debugger != null)
            {
                debugger.OnDrawActiveHitbox(this.gameObject);
            }
        }



    }

    protected void OnTriggerExit(Collider collision)
    {
        if (debugger != null)
        {
            debugger.OnDrawDefaultHitbox(this.gameObject);
        }
    }
}
