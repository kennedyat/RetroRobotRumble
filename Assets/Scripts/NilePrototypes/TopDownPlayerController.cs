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
    [SerializeField] private GameObject manualVirtualCam;
    [SerializeField] private GameObject autoVirtualCam;
    private float currentVelocity;

    [SerializeField] private float moveSpeed = 1f;

    private float gravity = -9.81f;
    [SerializeField] private float gravityMultiplier = 3f;
    private float verticalVelocity;

    [SerializeField] private float jumpPower;
    private int currentJumps;
    [SerializeField] private int maxJumps = 2;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (!manualAim && moveInput.sqrMagnitude != 0)
        {
            ApplyRotation();
        }
        ApplyMovement();
        ApplyGravity();

        manualVirtualCam.SetActive(manualAim);
        autoVirtualCam.SetActive(!manualAim);        
    }

    private void ApplyRotation()
    {
        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        float smoothedAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref currentVelocity, smoothTime);
        transform.rotation = Quaternion.Euler(0f, smoothedAngle, 0f);
    }

    private void ApplyMovement()
    {
        characterController.Move(moveSpeed * Time.deltaTime * direction);
    }

    private void ApplyGravity()
    {
        if (IsGrounded() && verticalVelocity < 0f)
        {
            verticalVelocity = -1f;
        } else
        {
            verticalVelocity += gravity * gravityMultiplier * Time.deltaTime;
        }
        
        direction.y = verticalVelocity;
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

    public void TopDownJump(InputAction.CallbackContext context)
    {
        if (!context.started || (!IsGrounded() && currentJumps >= maxJumps))
        {
            return;
        }

        if (currentJumps == 0)
        {
            StartCoroutine(WaitForLanding());
        }

        currentJumps++;
        verticalVelocity = jumpPower;
    }

    private IEnumerator WaitForLanding()
    {
        yield return new WaitUntil(() => !IsGrounded());
        yield return new WaitUntil(IsGrounded);
        currentJumps = 0;
    }

    private bool IsGrounded() => characterController.isGrounded;
}
