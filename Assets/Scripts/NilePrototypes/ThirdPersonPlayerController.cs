using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonPlayerController : MonoBehaviour
{
    private CharacterController characterController;

    private Vector2 moveInput;
    private Vector3 moveDirection;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    private bool isSprinting = false;

    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashDuration = 0.5f;
    private bool isDashing = false;

    private float verticalVelocity;
    private float gravity = -9.81f;
    [SerializeField] private float gravityMultiplier = 3f;

    private int currentJumps;
    [SerializeField] private float jumpPower;
    [SerializeField] private int maxJumps = 2;

    [SerializeField] private Transform cameraPivot;
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private Transform armPivot;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        //Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Locked;

        ApplyRotation();
        ApplyMovement();
    }
    private void ApplyRotation()
    {
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, cameraPivot.eulerAngles.y, transform.eulerAngles.z);
        armPivot.rotation = Quaternion.Euler(cameraPivot.eulerAngles.x + 20, armPivot.eulerAngles.y, armPivot.eulerAngles.z);
    }

    private void ApplyMovement()
    {
        moveDirection = transform.forward * moveInput.y;
        moveDirection += (transform.right * moveInput.x);
        moveDirection.Normalize();

        ApplyGravity();

        float targetSpeed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;

        moveDirection.x *= targetSpeed;
        moveDirection.y *= moveSpeed;
        moveDirection.z *= targetSpeed;

        characterController.Move(Time.deltaTime * moveDirection);
    }

    private void ApplyGravity()
    {
        if (IsGrounded() && verticalVelocity <= 0f)
        {
            verticalVelocity = -1f;
        } else
        {
            verticalVelocity += gravity * gravityMultiplier * Time.deltaTime;
        }
        moveDirection.y = verticalVelocity;
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void Sprint(InputAction.CallbackContext context)
    {
        isSprinting = context.started || context.performed;
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (context.started && !isDashing && moveInput.sqrMagnitude != 0)
        {
            Debug.Log("dash 1!");
            isDashing = true;
            StartCoroutine(DashMovement());
        }
    }

    private IEnumerator DashMovement()
    {
        Debug.Log("dash 2!");
        transform.DOMoveX(transform.position.x + (moveDirection.x * dashDistance), dashDuration).SetEase(Ease.OutSine);
        transform.DOMoveZ(transform.position.z + (moveDirection.z * dashDistance), dashDuration).SetEase(Ease.OutSine);
        yield return new WaitForSeconds(dashDuration);
        Debug.Log("dash 3!");
        isDashing = false;
        yield return null;
    }

    public void Jump(InputAction.CallbackContext context)
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
