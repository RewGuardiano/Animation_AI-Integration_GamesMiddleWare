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
    public float ikWeight = 1f;
    public float pickupRange = 2f;
    [SerializeField] public Transform Hand;
    private bool isPickingUp = false;
     ObjectScript focusObject;
    private bool isHoldingObject = false;

    private List<ObjectScript> allPickupItems = new List<ObjectScript>();



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

        // Manually add objects for testing (you can add real objects here)
        allPickupItems.AddRange(FindObjectsOfType<ObjectScript>());

        // Log the objects added to the list
        foreach (ObjectScript obj in allPickupItems)
        {
            Debug.Log("Added object: " + obj.name);
        }
    }

    // Finds the closest pickupable object within range
    private ObjectScript ClosestObject()
    {
        ObjectScript closestObject = null;
        float closestDistance = float.MaxValue;

        foreach (ObjectScript obj in allPickupItems)
        {
            float distance = Vector3.Distance(transform.position, obj.transform.position);
            if (distance < closestDistance && distance <= pickupRange)
            {
                closestDistance = distance;
                closestObject = obj;
            }
        }
        return closestObject;
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the object has the "Pickup" tag and if it has an ObjectScript component
        if (other.CompareTag("Pickupable"))
        {
            ObjectScript objectScript = other.GetComponent<ObjectScript>();
            if (objectScript != null && !allPickupItems.Contains(objectScript)) // Only add if not already in the list
            {
                allPickupItems.Add(objectScript);
                Debug.Log("Added object to pickup list: " + objectScript.name);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Remove the object from the pickup list when the player exits the trigger zone
        if (other.CompareTag("Pickupable"))
        {
            ObjectScript objectScript = other.GetComponent<ObjectScript>();
            if (objectScript != null)
            {
                allPickupItems.Remove(objectScript);
                Debug.Log("Removed object from pickup list: " + objectScript.name);
            }
        }
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

        if (Input.GetKeyDown(KeyCode.E) && !isHoldingObject)
        {
            focusObject = ClosestObject();  // Assign closest object

            if (focusObject != null)
            {
                isPickingUp = true;
                animator.SetTrigger("PickUp");
                Debug.Log("Attempting to pick up: " + focusObject.name);
            }
            else
            {
                Debug.Log("No object to pick up.");
            }
        }

        // Throw logic
        if (Input.GetKeyDown(KeyCode.T) && isHoldingObject)
        {
            ThrowObject();
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


    // IK logic for pickup
    private void OnAnimatorIK(int layerIndex)
    {
        if (isPickingUp && focusObject != null)
        {
            Vector3 directionToObject = (focusObject.transform.position - transform.position).normalized;
            float distanceToObject = Vector3.Distance(transform.position, focusObject.transform.position);
            float clampedDistance = Mathf.Min(distanceToObject, pickupRange);

            Vector3 targetPosition = transform.position + directionToObject * clampedDistance;

            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, ikWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, ikWeight);
            animator.SetIKPosition(AvatarIKGoal.RightHand, targetPosition);
            animator.SetIKRotation(AvatarIKGoal.RightHand, Quaternion.LookRotation(directionToObject));

            if (Vector3.Distance(Hand.position, focusObject.transform.position) <= 0.4f)
            {
                AttachObjectToHand();
                isPickingUp = false;
                isHoldingObject = true;
            }
        }
        else
        {
            // Reset IK weights
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
        }
    }

    // Attaches the object to the hand
    private void AttachObjectToHand()
    {
        focusObject.transform.SetParent(Hand);
        focusObject.transform.localPosition = Vector3.zero;
        focusObject.transform.localRotation = Quaternion.identity;

        Rigidbody rb = focusObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        Collider col = focusObject.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
    }


    private void ThrowObject()
    {
        if (focusObject != null)
        {
            animator.SetTrigger("isThrowing");

            focusObject.transform.SetParent(null);

            Rigidbody rb = focusObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;

                Vector3 throwDirection = transform.forward + Vector3.up * 0.6f;
                float throwForce = 10f;
                rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
            }

            Collider col = focusObject.GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = true;
            }

            focusObject = null;
            isHoldingObject = false;
        }
    }





    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0,1,0,0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);

        
    }
}
