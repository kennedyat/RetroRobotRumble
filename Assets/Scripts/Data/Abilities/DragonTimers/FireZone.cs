using System.Collections.Generic;
using UnityEngine;

public class FireZoneDOT : MonoBehaviour
{
    public float tickInterval = 0.5f;
    public int damagePerTick = 5;

    private float t = 0f;
    private readonly HashSet<Enemy> inside = new HashSet<Enemy>();

    [SerializeField] private bool damageActive = false;

    private BoxCollider box;

    private void Awake()
    {
        box = GetComponent<BoxCollider>();
        if (box != null)
            box.isTrigger = true;
    }

    public void SetActiveDamage(bool active)
    {
        damageActive = active;
        t = 0f;

        if (!damageActive)
        {
            inside.Clear();
            return;
        }

        RefreshInside();
    }

    public void RefreshInside()
    {
        if (box == null)
            return;

        Vector3 center = box.bounds.center;
        Vector3 halfExtents = box.bounds.extents;

        Collider[] hits = Physics.OverlapBox(center, halfExtents, transform.rotation);

        inside.Clear();
        foreach (var c in hits)
        {
            if (c == null)
                continue;

            Enemy e = c.GetComponentInParent<Enemy>();
            if (e != null)
                inside.Add(e);
        }
    }

    private void Update()
    {
        if (!damageActive)
        {
            return;
        }
            

        t += Time.deltaTime;
        if (t < tickInterval)
        {
            return;
        }
            

        t = 0f;

        inside.RemoveWhere(e => e == null);

        foreach (var e in inside)
            e.DealDamage(damagePerTick);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!damageActive)
            return;

        Enemy e = other.GetComponentInParent<Enemy>();
        if (e != null)
            inside.Add(e);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!damageActive)
            return;

        Enemy e = other.GetComponentInParent<Enemy>();
        if (e != null)
            inside.Remove(e);
    }

    public void ClearInside()
    {
        inside.Clear();
        t = 0f;
    }
}
