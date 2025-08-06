using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerControllerRevised : MonoBehaviour
{
    private Rigidbody rigidbody;
    private Vector2 moveInput;
    private Vector3 moveDirection;
    private Vector2 aimInput;
    private bool manualAim = true;
    private float currentVelocity;
    private bool isDashing = false;

    [Header("| MOVEMENT PARAMETERS")]
    [SerializeField, Tooltip("Base movement speed of player")] private float _moveSpeed = 1f;

    [Header("| DASH PARAMETERS")]
    [SerializeField, Tooltip("Distance traveled with a dash")] private float _dashDistance = 5f;
    [SerializeField, Tooltip("Time taken for a dash + before another dash can be performed")] private float _dashDuration = 0.5f;

    [Header("| JUMP PARAMETERS")]
    [SerializeField, Tooltip("Force applied for a jump")] private float _jumpPower;

    [Header("| CAMERA PARAMETERS")]
    [SerializeField, Tooltip("Target transform for camera to follow")] private Transform _cameraTarget;
    [SerializeField, Tooltip("Time taken for camera to follow target")] private float _smoothTime = 0.05f;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!manualAim && moveInput.sqrMagnitude != 0)
        {
            ApplyRotation();
        }

        ApplyMovement();
        CameraMovement();
    }

    private void ApplyRotation()
    {
        float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
        float smoothedAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref currentVelocity, _smoothTime);
        rigidbody.MoveRotation(Quaternion.Euler(0f, smoothedAngle, 0f));
    }

    private void ApplyMovement()
    {
        Vector3 currentVelocity = rigidbody.velocity;
        Vector3 targetVelocity = moveDirection;
        targetVelocity *= _moveSpeed;

        Vector3 velocityChange = (targetVelocity - currentVelocity);
        velocityChange = new Vector3(velocityChange.x, 0f, velocityChange.z);
        Vector3.ClampMagnitude(velocityChange, _moveSpeed);

        rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    private void CameraMovement()
    {
        if (moveInput.sqrMagnitude == 0f && aimInput.sqrMagnitude == 0f)
        {
            _cameraTarget.DOLocalMoveZ(0, 1f);
        }
        else
        {
            _cameraTarget.DOLocalMoveZ(1, 1f);
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
    }

    public void Aim(InputAction.CallbackContext context)
    {
        aimInput = context.ReadValue<Vector2>();

        if (context.control.device is Mouse)
        {
            MouseAim();
        }
        else if (context.control.device is Gamepad)
        {
            GamepadAim();
        }
    }

    private void MouseAim()
    {
        manualAim = true;
        Ray ray = Camera.main.ScreenPointToRay(aimInput);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 raycastPoint = ray.GetPoint(rayDistance);
            Vector3 lookPoint = new Vector3(raycastPoint.x, transform.position.y, raycastPoint.z);
            Vector3 lookDirection = lookPoint - rigidbody.position;
            rigidbody.MoveRotation(Quaternion.LookRotation(lookDirection));
        }
    }

    private void GamepadAim()
    {
        if (aimInput.sqrMagnitude == 0f)
        {
            manualAim = false;
            return;
        } 
        else
        {
            manualAim = true;

            Vector3 inputDirection = Vector3.right * aimInput.x + Vector3.forward * aimInput.y;
            if (inputDirection.sqrMagnitude > 0f)
            {
                float targetRotationY = Mathf.Atan2(aimInput.x, aimInput.y) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.Euler(0f, targetRotationY, 0f);
                rigidbody.MoveRotation(targetRotation);
            }
        }
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (context.started && !isDashing && moveInput.sqrMagnitude != 0)
        {
            Debug.Log("dash!");
            isDashing = true;
            StartCoroutine(DashMovement());
        }
    }

    private IEnumerator DashMovement()
    {
        rigidbody.DOMoveX(transform.position.x + (moveDirection.x * _dashDistance), _dashDuration).SetEase(Ease.OutSine);
        rigidbody.DOMoveZ(transform.position.z + (moveDirection.z * _dashDistance), _dashDuration).SetEase(Ease.OutSine);
        yield return new WaitForSeconds(_dashDuration);

        isDashing = false;
        yield return null;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        Vector3 jumpForce = Vector3.zero;

        if (IsGrounded())
        {
            jumpForce = Vector3.up * _jumpPower;
        }

        rigidbody.AddForce(jumpForce, ForceMode.VelocityChange);
    }

    bool IsGrounded()
    {
        return Physics.Raycast(rigidbody.position, -Vector3.up, 0.1f);
    }

}
