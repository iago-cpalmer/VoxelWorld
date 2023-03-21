using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private bool isGrounded;
    public bool isSprinting;
    public bool sprintInQueue;
    public bool isFlying;
    private Transform cam;
    private World world;

    public static float walkSpeed = 3f;
    public float sprintSpeed = 6f;
    public float jumpForce = 5f;
    public float gravity = -9.8f;
    public float sensibility = 3f;
    public float playerWidthDown = 0.15f;
    public static float playerWidthSide = 0.15f;
    public float playerHeight = 1.8f;
    public float fieldOfView;

    private float horizontal;
    private float vertical;
    private float mouseHorizontal;
    private float mouseVertical;
    private Vector3 velocity;
    private float verticalMomemtum = 0;
    private bool jumpRequest;
    private float rotX;
    private float rotY;
    private Vector3 forward;

    private void Start()
    {
        cam = GameObject.Find("Main Camera").transform;
        cam.gameObject.GetComponent<Camera>().fieldOfView = fieldOfView;
        world = GameObject.Find("World").GetComponent<World>();
       
    }

    private void FixedUpdate()
    {
        CalculateVelocity();
        if (jumpRequest)
        {
            Jump();
        }

        
        transform.Translate(velocity, Space.World);
    }

    private void Update()
    {
        GetPlayerInput();
        rotation();
        /*
        if(isInsideBlock())
        {
            transform.position += world.GetNearestNonSolidBlock(transform.position);
        }*/
    }
    private void rotation()
    {
        rotX += mouseHorizontal * sensibility;
        rotY += mouseVertical * sensibility;

        rotY = Mathf.Clamp(rotY, -90f, 90f);

        //Camera rotation only allowed if game us not paused
        cam.rotation = Quaternion.Euler(-rotY, rotX, 0f);
        transform.rotation = Quaternion.Euler(0f, rotX, 0f);

    }
    private void CalculateVelocity()
    {

        

        // if we're sprinting, use the sprint multiplier.
        if(horizontal!=0 && vertical!=0)
        {
            if (isSprinting)
                velocity = ((transform.forward * vertical) + (transform.right * horizontal)).normalized * Time.fixedDeltaTime * sprintSpeed;
            else
                velocity = ((transform.forward * vertical) + (transform.right * horizontal)).normalized * Time.fixedDeltaTime * walkSpeed;
        } else
        {
            if (isSprinting)
                velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * sprintSpeed;
            else
                velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;
        }
        

        

        if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
        {
            velocity.z = 0;
            isSprinting = false;
        }
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
        {
            velocity.x = 0;
            isSprinting = false;
        }

        if (!isFlying)
        {
            // Apply vertical momentum (falling/jumping).
            velocity += Vector3.up * verticalMomemtum * Time.fixedDeltaTime;
            // Affect vertical momentum with gravity.
            if (verticalMomemtum > gravity)
                verticalMomemtum += Time.fixedDeltaTime * gravity;

            if (velocity.y < 0)
                velocity.y = checkDownSpeed(velocity.y);
            else if (velocity.y > 0)
                velocity.y = checkUpSpeed(velocity.y);

        }
        else
        {
            if (Input.GetKey(KeyCode.Space))
            {
                velocity.y += 0.3f;
            }
            else if (Input.GetKey(KeyCode.LeftShift))
            {
                velocity.y -= 0.3f;
            }
            
            if (velocity.y < 0)
                velocity.y = checkDownSpeed(velocity.y);
            else if (velocity.y > 0)
                velocity.y = checkUpSpeed(velocity.y);
        
        }
        



    }
    private void Jump()
    {
        verticalMomemtum = jumpForce;
        isGrounded = false;
        jumpRequest = false;

    }
    private void GetPlayerInput()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");

        if(Input.GetButton("Sprint") && isGrounded)
        {
            isSprinting = true;
        } 
        if(Input.GetButtonUp("Sprint"))
        {
            isSprinting = false;
        }

        if(Input.GetKeyDown(KeyCode.Tab))
        {
            isFlying = isFlying ? false : true;
        }

        
        if(isGrounded && Input.GetButton("Jump"))
        {
            jumpRequest = true;
        }
    }

    private float checkDownSpeed (float downSpeed)
    {
        if (
            (world.CheckForVoxel(new Vector3(transform.position.x - playerWidthDown, transform.position.y + downSpeed, transform.position.z - playerWidthDown)) && (!left && !back)) ||
            (world.CheckForVoxel(new Vector3(transform.position.x + playerWidthDown, transform.position.y + downSpeed, transform.position.z - playerWidthDown)) && (!right && !back)) ||
            (world.CheckForVoxel(new Vector3(transform.position.x + playerWidthDown, transform.position.y + downSpeed, transform.position.z + playerWidthDown)) && (!right && !front)) ||
            (world.CheckForVoxel(new Vector3(transform.position.x - playerWidthDown - 0.1f, transform.position.y + downSpeed, transform.position.z + playerWidthDown)) && (!left && !front))
           )                                                                                                                              
        {
            isGrounded = true;
            verticalMomemtum = 0;
            return 0;
        } else
        {
            isGrounded = false;
            return downSpeed;
        }
    }private float checkUpSpeed (float upSpeed)
    {
        if (
            (world.CheckForVoxel(new Vector3(transform.position.x - playerWidthDown, transform.position.y + playerHeight + upSpeed, transform.position.z - playerWidthDown)) && (!left && !back)) ||
            (world.CheckForVoxel(new Vector3(transform.position.x + playerWidthDown, transform.position.y + playerHeight + upSpeed, transform.position.z - playerWidthDown)) && (!right && !back)) ||
            (world.CheckForVoxel(new Vector3(transform.position.x + playerWidthDown, transform.position.y + playerHeight + upSpeed, transform.position.z + playerWidthDown)) && (!right && !front)) ||
            (world.CheckForVoxel(new Vector3(transform.position.x - playerWidthDown, transform.position.y + playerHeight + upSpeed, transform.position.z + playerWidthDown)) && (!left && !front))
           )                     
        {
            verticalMomemtum = 0;
            return 0;
        } else
        { 
            return upSpeed;
        }
    }
    public bool front
    {

        get
        {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z + playerWidthSide)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z + playerWidthSide))
                )
                return true;
            else
                return false;
        }

    }
    public bool back
    {

        get
        {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z - playerWidthSide)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z - playerWidthSide))
                )
                return true;
            else
                return false;
        }

    }
    public bool left
    {

        get
        {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidthSide, transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidthSide, transform.position.y + 1f, transform.position.z))
                )
                return true;
            else
                return false;
        }

    }
    public bool right
    {

        get
        {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x + playerWidthSide, transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x + playerWidthSide, transform.position.y + 1f, transform.position.z))
                )
                return true;
            else
                return false;
        }

    }

    private bool isInsideBlock()
    {
        
        int x = Mathf.FloorToInt(transform.position.x);
        int y = Mathf.FloorToInt(transform.position.y);
        int z = Mathf.FloorToInt(transform.position.z);

        if (
               world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z)) ||
               world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z))
               )
        {
            return true;
        }
        else
        {
            return false;
        }
        return false;
    }
}
