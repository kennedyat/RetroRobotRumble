using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ProtoArm : MonoBehaviour
{
    [SerializeField] GameObject projectileObject;
    [SerializeField] Transform spawnPoint;

    public void BasicAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            GameObject projectile = Instantiate(projectileObject, spawnPoint.position, Quaternion.Euler(90, spawnPoint.rotation.eulerAngles.y, 0));
            projectile.transform.localScale = Vector3.one * 0.25f;
        }
    }

    public void SpecialAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            GameObject projectile = Instantiate(projectileObject, spawnPoint.position, Quaternion.Euler(90, spawnPoint.rotation.eulerAngles.y, 0));
            projectile.transform.localScale = Vector3.one * 0.5f;
        }
    }
}
