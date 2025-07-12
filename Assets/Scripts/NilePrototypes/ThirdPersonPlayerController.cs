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

    private float verticalVelocity;
    private float gravity = -9.81f;
    [SerializeField] private float gravityMultiplier = 3f;

    private int currentJumps;
    [SerializeField] private float jumpPower;
    [SerializeField] private int maxJumps = 2;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        ApplyMovement();
        ApplyGravity();      
    }

    private void ApplyMovement()
    {
        characterController.Move(moveSpeed * Time.deltaTime * moveDirection);
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
        
        moveDirection.y = verticalVelocity;
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();


        moveDirection = transform.forward * moveInput.y;
        moveDirection += (transform.right * moveInput.x);
        moveDirection.Normalize();
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
