using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(MeshRenderer))]
public class HitBox : MonoBehaviour
{
    public AK.Wwise.Event hitEvent;
    BoxCollider box;
    MeshRenderer meshRenderer;
    public Action<Collider> OnHit;
    public bool isActive;

    public static List<HitBox> totalHitboxes = new();
    public RuntimeDebugger debugger;
    
    private Coroutine disableCoroutine; // Track the coroutine

    protected void Awake()
    {
        box = GetComponent<BoxCollider>();
        meshRenderer = GetComponent<MeshRenderer>();
        isActive = false;
        box.enabled = false;
        meshRenderer.enabled = false;

        totalHitboxes.Add(this);
    }

    public void EnableFrame(float duration)
    {
        // Stop any existing coroutine first
        if (disableCoroutine != null)
        {
            StopCoroutine(disableCoroutine);
            disableCoroutine = null;
        }

        if(meshRenderer != null && box != null)
        {
            meshRenderer.enabled = true;
            box.enabled = true;
        }
       
        if (debugger != null)
        {
            debugger.OnDrawDefaultHitbox(this.gameObject);
        }

        isActive = true;

        if (duration > 0)
            disableCoroutine = StartCoroutine(DisableFrameControlled(duration));
    }

    public void DisableFrame()
    {
        // Stop the coroutine if it's running
        if (disableCoroutine != null)
        {
            StopCoroutine(disableCoroutine);
            disableCoroutine = null;
        }

        if(meshRenderer != null && box != null)
        {
            box.enabled = false;
            meshRenderer.enabled = false;
        }
       
        isActive = false;
    }

    IEnumerator DisableFrameControlled(float duration)
    {
        yield return new WaitForSeconds(duration);
        disableCoroutine = null; // Clear reference
        DisableFrame();
    }

    protected void OnTriggerEnter(Collider collision)
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
            hitEvent.Post(gameObject);
            Debug.Log("Hit enemy successfully!");
        }
    }

    protected void OnTriggerExit(Collider collision)
    {
        if (debugger != null)
        {
            debugger.OnDrawDefaultHitbox(this.gameObject);
        }
    }

    public static void DisableAllHitBoxes()
    {
        // Use a for loop backwards to handle potential modifications during iteration
        for (int i = totalHitboxes.Count - 1; i >= 0; i--)
        {
            if (totalHitboxes[i] != null)
            {
                totalHitboxes[i].DisableFrame();
            }
        }
    }

    protected void OnDestroy()
    {
        // Stop any running coroutine
        if (disableCoroutine != null)
        {
            StopCoroutine(disableCoroutine);
            disableCoroutine = null;
        }
        
        // Remove this specific hitbox from the list instead of clearing all
        totalHitboxes.Remove(this);
    }
}