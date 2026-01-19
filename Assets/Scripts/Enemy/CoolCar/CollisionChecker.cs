using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CollisionChecker : MonoBehaviour
{
    public bool Clear { get; private set; }
    Collider col;
    [SerializeField] LayerMask mask;
    protected void Start()
    {
        col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == mask) Clear = false;
    }

    protected void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == mask) Clear = true;
    }
}
