using UnityEngine;

public class ConstantRotator : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 60f; // degrees per second

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }
}
