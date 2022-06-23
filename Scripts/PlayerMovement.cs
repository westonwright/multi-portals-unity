using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller {get { return GetComponent<CharacterController>(); } }

    private MouseLook mouseLook { get { return FindObjectOfType<MouseLook>(); } }

    public float speed = 12f;
    public float gravity = -9.81f;
    public float jumpHeight = .5f;
    public float stepOffset = .3f;

    public Transform groundCheck;
    public float groundDistance = 0.1f;
    public LayerMask groundMask;
    public Transform spawnPoint;

    Vector3 velocity;
    bool isGrounded;
    void Start()
    {
        //if(PlayerPrefsX.GetVector3("SpawnPoint") == new Vector3(0,0,0))
        
            //transform.position = spawnPoint.position;
            //transform.rotation = spawnPoint.rotation;
        
        /*else
        {
            //transform.position = PlayerPrefsX.GetVector3("SpawnPoint");
        }
        if (PlayerPrefsX.GetVector3("PlayerScale") != new Vector3(0, 0, 0))
        {
            transform.localScale = PlayerPrefsX.GetVector3("PlayerScale");
        }*/


    }

    void Update()
    {
        float newSpeed = speed * transform.localScale.x;
        float newGravity = gravity * (transform.localScale.x/4);
        float newJumpHeight = jumpHeight * transform.localScale.x;
        float newStepOffset = stepOffset * transform.localScale.x;
        float newGroundDistance = groundDistance * transform.localScale.x;


        isGrounded = Physics.CheckSphere(groundCheck.position, newGroundDistance, groundMask);

        controller.stepOffset = newStepOffset;

        if(isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = (transform.right * x) + (transform.forward * z);

        controller.Move(move * newSpeed * Time.deltaTime);

        if(Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(newJumpHeight * (-2f * transform.localScale.x) * gravity);
        }

        velocity.y += newGravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        float xRot = transform.rotation.eulerAngles.x;
        float zRot = transform.rotation.eulerAngles.z;
        if(xRot != 0)
        {
            if (Mathf.Abs(xRot) > 90)
            {
                if (Mathf.Abs(xRot) <= 270)
                {
                    //this section doesn't seem to happen ever. Might need to change the xRot to the inverse if its current value here if problems come up?
                    xRot = (Mathf.Sign(xRot) * 90) - ((xRot - (Mathf.Sign(xRot) * 90)));
                }
                else
                {
                    xRot = -((Mathf.Sign(xRot) * 90) - (((xRot - (Mathf.Sign(xRot) * 180))) - (Mathf.Sign(xRot) * 90)));
                }
                //Debug.Log("new xRot " + xRot);
            }
            xRot = Mathf.Clamp(xRot, -90, 90);
            float xRotBefore = xRot;
            xRot = Mathf.SmoothStep(xRot, 0, .2f);
            mouseLook.xRotation += xRotBefore - xRot;
        }
        if(zRot != 0)
        {
            if(Mathf.Abs(zRot) > 180)
            {
                //Debug.Log("zRot over value " + zRot);
                zRot = Mathf.Sign(zRot) * -(360 - Mathf.Abs(zRot));
                //Debug.Log("new zRot " + zRot);
            }
            //float zRotBefore = zRot;
            zRot = Mathf.SmoothStep(zRot, 0, .2f);
            //mouseLook.yRotation += (zRotBefore - zRot);
        }
        Vector3 fixRot = new Vector3(xRot, transform.rotation.eulerAngles.y, zRot);
        transform.rotation = Quaternion.Euler(fixRot);  
    }

}
