using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    public GameObject cameraFollow;
    public float pitchMin = 20f;
    public float pitchMax = 60f;

    public float mouseSensitivity = 50f;
    public float rotationSmoothing = 10f;
    private float yaw;
    private float pitch;

    protected void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    protected void Start()
    {

        yaw = cameraFollow.transform.eulerAngles.y;
        pitch = cameraFollow.transform.eulerAngles.x;
        // pitch = cameraFollow.transform.eulerAngles.x;
    }

    // Update is called once per frame
    protected void FixedUpdate()
    {

        float mouseX = Mouse.current.delta.ReadValue().x * mouseSensitivity * Time.fixedDeltaTime;
        float mouseY = Mouse.current.delta.ReadValue().y * mouseSensitivity * Time.fixedDeltaTime;
        yaw += mouseX;
        pitch -= mouseY;

        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        cameraFollow.transform.rotation = Quaternion.Slerp(
            cameraFollow.transform.rotation,
            rotation,
            Time.fixedDeltaTime * rotationSmoothing
        );

    }
}
