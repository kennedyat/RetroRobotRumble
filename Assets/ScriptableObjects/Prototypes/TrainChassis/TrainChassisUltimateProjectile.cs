using UnityEngine;

public class TrainChassisUltimateProjectile : MonoBehaviour
{
    public float lifetime = 5f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Enemy handles damage logic
            // Don't destroy here (it pierces)
        }
    }
}
