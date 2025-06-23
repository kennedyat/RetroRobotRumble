using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
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

    void Awake()
    {
         Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Start()
    {
        

       

        yaw = cameraFollow.transform.eulerAngles.y;
        pitch =  cameraFollow.transform.eulerAngles.x;
       // pitch = cameraFollow.transform.eulerAngles.x;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        float mouseX = Mouse.current.delta.ReadValue().x * mouseSensitivity * Time.deltaTime;
        float mouseY = Mouse.current.delta.ReadValue().y * mouseSensitivity * Time.deltaTime;
        yaw += mouseX;
        pitch -= mouseY;

         pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);


        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        cameraFollow.transform.rotation = Quaternion.Slerp(
            cameraFollow.transform.rotation,
            rotation,
            Time.deltaTime * rotationSmoothing
        );
        

    }


   
}
