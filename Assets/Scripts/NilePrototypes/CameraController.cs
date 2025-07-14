using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    private Camera camera;

    [SerializeField] private Transform target;
    [SerializeField] private float cameraFollowSpeed = 0.5f;
    private Vector3 cameraFollowVelocity;

    private Vector2 lookInput;

    [SerializeField] private lookSensitivity mouseSensitivity;
    [SerializeField] private lookSensitivity stickSensitivity;
    [SerializeField] private CameraBounds cameraBounds;

    private CameraRotation cameraRotation;

    private bool isGamepad;
    private bool isSprinting;
    private Vector2 moveInput;

    [SerializeField] private int defaultFOV = 40;
    [SerializeField] private int sprintFOV = 80;

    private void Start()
    {
        camera = Camera.main;
    }


    void Update()
    {
        ApplyMovement();
        ApplyRotation();

        if (isSprinting && moveInput.sqrMagnitude != 0f)
        {
            camera.DOFieldOfView(sprintFOV, 0.5f).SetEase(Ease.OutExpo);
        } else
        {
            camera.DOFieldOfView(defaultFOV, 0.5f).SetEase(Ease.OutExpo);
        }
    }

    void ApplyMovement()
    {
        Vector3 targetPosition = Vector3.SmoothDamp(transform.position, target.position, ref cameraFollowVelocity, cameraFollowSpeed);
        transform.position = targetPosition;
    }

    void ApplyRotation()
    {
        lookSensitivity sensitivity;
        if (isGamepad)
        {
            sensitivity = stickSensitivity;
        } else
        {
            sensitivity = mouseSensitivity;
        }

        cameraRotation.yaw += lookInput.x * sensitivity.horizontal * Time.deltaTime;
        cameraRotation.pitch -= lookInput.y * sensitivity.vertical * Time.deltaTime;
        cameraRotation.pitch = Mathf.Clamp(cameraRotation.pitch, cameraBounds.min, cameraBounds.max);

        transform.eulerAngles = new Vector3(cameraRotation.pitch, cameraRotation.yaw, 0f);
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void Look(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
        isGamepad = context.control.device is Gamepad;
    }

    public void Sprint(InputAction.CallbackContext context)
    {
        isSprinting = context.started || context.performed;
    }
}

[Serializable]
public struct lookSensitivity
{
    public float horizontal;
    public float vertical;
}

public struct CameraRotation
{
    public float pitch;
    public float yaw;
}

[Serializable]
public struct CameraBounds
{
    public float min;
    public float max;
}
