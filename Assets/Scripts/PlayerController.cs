using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class PlayerController : MonoBehaviour
{
     [SerializeField] float walkSpeed = 1f;  // Walking speed
    [SerializeField] float runSpeed = 5f;   // Running speed when Shift is held
    [SerializeField] float rotationspeed = 500f;

    [Header("IK Settings")]
    [SerializeField] public Transform pickupTarget;
    public float ikWeight = 1f;
    public float pickupRange = 2f;
    [SerializeField] public Transform Hand;
    private bool isPickingUp = false;
    private GameObject currentPickupObject;



    [Header("Ground Check Settings")]
    [SerializeField] float groundCheckRadius = 0.2f;
    [SerializeField] Vector3 groundCheckOffset;
    [SerializeField] LayerMask groundLayer;
    float ySpeed;

    Quaternion targetRotation;

    Animator animator;

    [SerializeField]  CharacterController characterController;

    bool isGrounded;


    CameraController cameraController;
    // Start is called before the first frame update
    void Awake()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
        characterController = characterController.GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (Input.GetKeyDown(KeyCode.P))
        {
            animator.SetTrigger("isPunching");
        }

        if (Input.GetKeyDown(KeyCode.E))
        {

            animator.SetTrigger("PickUp");

        }

        if(Input.GetKeyDown(KeyCode.T))
        {

            animator.SetTrigger("isThrowing");

        }

        bool isRunning = Input.GetKey(KeyCode.LeftShift) && v > 0; // Running only when moving forward with Shift held

        // Adjust speed and animation input based on walking or running
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        float adjustedVertical ; // Scale vertical input when running

        if (isRunning)
        {
            adjustedVertical = v * 2f;
        }
        else
        {
            adjustedVertical = v;
        }

        Vector3 moveInput = new Vector3(h, 0, v).normalized;
        Vector3 moveDirection = cameraController.PlanarRotation * moveInput;


        GroundCheck();
        if (isGrounded) 
        {
            ySpeed = -0.5f;
        }
        else
        {
            ySpeed += Physics.gravity.y * Time.deltaTime;// increases the y speed if the player is falling 
        }

        characterController.Move(moveDirection * currentSpeed * Time.deltaTime);

        var velocity = moveDirection * currentSpeed;
        velocity.y = ySpeed;


        if (moveInput.magnitude > 0)
        {
            characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
            targetRotation = Quaternion.LookRotation(moveDirection);
        }
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation,rotationspeed * Time.deltaTime); //smoother character rotation

        // Update animator parameters with damping for smooth transitions
        animator.SetFloat("horizontal", h, 0.2f, Time.deltaTime);
        animator.SetFloat("vertical", adjustedVertical, 0.2f, Time.deltaTime); // Use adjusted vertical for running


      


    }
    void GroundCheck()
    {
       isGrounded =  Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius, groundLayer);// to check if the player is standing on the ground 

    }

   
    
    
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0,1,0,0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
    }
}
