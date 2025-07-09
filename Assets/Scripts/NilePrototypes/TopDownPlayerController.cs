using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class TopDownPlayerController : MonoBehaviour
{
    private Vector2 moveInput;
    private CharacterController characterController;
    private Vector3 direction;

    [SerializeField] private bool manualAim = true;
    [SerializeField] private float smoothTime = 0.05f;
    private float currentVelocity;

    [SerializeField] private float speed = 1f;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (!manualAim && moveInput.sqrMagnitude != 0)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float smoothedAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref currentVelocity, smoothTime);
            transform.rotation = Quaternion.Euler(0, smoothedAngle, 0);
        }
        characterController.Move(speed * Time.deltaTime * direction);
    }


    public void TopDownMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        direction = new Vector3(moveInput.x, 0f, moveInput.y);            
    }

    public void TopDownLook(InputAction.CallbackContext context)
    {
        if (manualAim)
        {
            Vector2 input = context.ReadValue<Vector2>();

            if (context.control.device is Mouse)
            {
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
}
