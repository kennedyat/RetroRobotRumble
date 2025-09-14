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
    private Vector2 lookInput;
    private Vector3 lookDirection;
    private bool isDashing = false;
    private float sensitivity;
    

    [Header("| MOVEMENT PARAMETERS")]
    [SerializeField, Tooltip("Base movement speed of player")] private float _moveSpeed = 1f;
    [SerializeField, Tooltip("Transform to tilt during movement")] private Transform _tiltPivot;
    [SerializeField, Tooltip("Amount of tilt, in degrees")] private float _tiltMagnitude = 15f;
    [SerializeField, Tooltip("Time taken to tilt, in seconds")] private float _tiltSpeed = 0.5f;

    [Header("| DASH PARAMETERS")]
    [SerializeField, Tooltip("Distance traveled with a dash")] private float _dashDistance = 5f;
    [SerializeField, Tooltip("Time taken for a dash + before another dash can be performed")] private float _dashDuration = 0.5f;

    [Header("| JUMP PARAMETERS")]
    [SerializeField, Tooltip("Force applied for a jump")] private float _jumpPower;

    [Header("| CAMERA PARAMETERS")]
    [SerializeField, Tooltip("Look sensitivity for gamepad")] private float _gamepadSensitivity = 1f;
    [SerializeField, Tooltip("Look sensitivity for gamepad")] private float _mouseSensitivity = 1f;

    #region Animation
    private Animator anim;
    int _MoveID;
    #endregion
    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        _MoveID =  Animator.StringToHash("MotionSpeed");
    }

    private void FixedUpdate()
    {
        ApplyRotation();
        ApplyMovement();
    }

    private void ApplyRotation()
    {
        float cameraRotation = lookInput.x * sensitivity * Time.deltaTime;
        lookDirection = new Vector3(rigidbody.rotation.eulerAngles.x, rigidbody.rotation.eulerAngles.y + cameraRotation, rigidbody.rotation.eulerAngles.z);

        transform.rotation = Quaternion.Euler(lookDirection);
        
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

        Debug.Log(moveDirection);

        Vector3 targetTilt = new Vector3(moveDirection.z * _tiltMagnitude, lookDirection.y, -moveDirection.x * _tiltMagnitude);
        _tiltPivot.DORotate(targetTilt, _tiltSpeed);

    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        moveDirection = transform.forward * moveInput.y;
        moveDirection += (transform.right * moveInput.x);
        moveDirection.Normalize();

        anim.SetFloat(_MoveID, moveInput.magnitude);

    }

    public void Look(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();

        if (context.control.device is Mouse)
        {
            sensitivity = _mouseSensitivity;
        }
        else if (context.control.device is Gamepad)
        {
            sensitivity = _gamepadSensitivity;
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

}
