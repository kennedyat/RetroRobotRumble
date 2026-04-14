using UnityEngine;

public class EagleChassisUltimateProjectile : MonoBehaviour
{
    public float lifetime = 5f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            // Enemy handles damage logic
            // Don't destroy here (it pierces)
        }
    }
}
