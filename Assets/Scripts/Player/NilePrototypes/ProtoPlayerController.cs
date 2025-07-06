using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class ProtoPlayerController : MonoBehaviour
{
    //private Vector2 moveInput;
    private CharacterController characterController;
    private Vector3 direction;

    [SerializeField] private float speed = 1f;
    [SerializeField] private float gamepadRotateSmoothing = 1000f;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        characterController.Move(direction * speed * Time.deltaTime);
    }


    public void Move(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        direction = new Vector3(input.x, 0f, input.y);
    }

    public void Look(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        if (context.control.device is Mouse)
        {
            //Debug.Log("mouse position: " + input);

            Ray ray = Camera.main.ScreenPointToRay(input);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            float rayDistance;

            if (groundPlane.Raycast(ray, out rayDistance))
            {
                Vector3 raycastPoint = ray.GetPoint(rayDistance);
                Vector3 lookPoint = new Vector3(raycastPoint.x, transform.position.y, raycastPoint.z);
                transform.LookAt(lookPoint);
            }
        } 
        else if (context.control.device is Gamepad)
        {
            Debug.Log("right stick position: " + input);

            Vector3 inputDirection = Vector3.right * input.x + Vector3.forward * input.y;
            if (inputDirection.sqrMagnitude > 0f)
            {
                float targetRotationY = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.Euler(0f, targetRotationY, 0f);
                transform.rotation = targetRotation;
            }
        }
    }
}
