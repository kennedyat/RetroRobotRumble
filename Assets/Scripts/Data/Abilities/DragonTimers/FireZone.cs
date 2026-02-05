using System.Collections.Generic;
using UnityEngine;

public class FireZoneDOT : MonoBehaviour
{
    public float tickInterval = 0.5f;
    public int damagePerTick = 5;

    private float t = 0f;
    private readonly HashSet<Enemy> inside = new HashSet<Enemy>();

    private void Update()
    {
        t += Time.deltaTime;
        if (t < tickInterval)
            return;
        t = 0f;

        foreach (var e in inside)
        {
            if (e != null)
                e.DealDamage(damagePerTick);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Enemy e = other.GetComponent<Enemy>();
        if (e != null)
            inside.Add(e);
    }

    private void OnTriggerExit(Collider other)
    {
        Enemy e = other.GetComponent<Enemy>();
        if (e != null)
            inside.Remove(e);
    }
}
