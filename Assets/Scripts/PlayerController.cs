using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
     [SerializeField] float walkSpeed = 3f;  // Walking speed
    [SerializeField] float runSpeed = 7f;   // Running speed when Shift is held

    [SerializeField] float rotationspeed = 500f;

    Quaternion targetRotation;

    Animator animator;

    


    CameraController cameraController;
    // Start is called before the first frame update
    void Start()
    {
        cameraController = Camera.main.GetComponent<CameraController>();

        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

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


        if (moveInput.magnitude > 0)
        {
            transform.position += moveDirection * currentSpeed * Time.deltaTime;
            targetRotation = Quaternion.LookRotation(moveDirection);
        }
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation,rotationspeed * Time.deltaTime); //smoother character rotation

        // Update animator parameters with damping for smooth transitions
        animator.SetFloat("horizontal", h, 0.2f, Time.deltaTime);
        animator.SetFloat("vertical", adjustedVertical, 0.2f, Time.deltaTime); // Use adjusted vertical for running


    }
}
