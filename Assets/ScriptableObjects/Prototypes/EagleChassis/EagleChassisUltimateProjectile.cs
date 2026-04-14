using UnityEngine;

public class EagleChassisUltimateProjectile : MonoBehaviour
{
    public float lifetime = 5f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
