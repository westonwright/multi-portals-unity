using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{

    public float mouseSensitivity = 100f;

    private Transform playerBody { get { return FindObjectOfType<CharacterController>().transform; } }

    public float xRotation = 0f;
    public float yRotation = 0f;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        //'yRotation' will always be zero unless set otherwise by playerMovement script so that it can counteract its corrective rotations when teleporting.
        playerBody.Rotate(Vector3.up * (mouseX + yRotation));
        yRotation = 0;
    }
}
