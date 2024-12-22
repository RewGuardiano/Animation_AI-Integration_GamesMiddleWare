using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TMPro.Examples;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;



public class PlayerController : NetworkBehaviour
{
    private const float pickUpTolerance = 0.4f;
    [SerializeField] float walkSpeed = 1f;  // Walking speed
    [SerializeField] float runSpeed = 2f;   // Running speed when Shift is held
    [SerializeField] float rotationspeed = 100f;

    [Header("IK Settings")]
    public float pickupRange = 10f;
    [SerializeField] public Transform Hand;
    Transform RightHand;
    private bool isPickingUp = false;
     ObjectScript focusObject;
    private bool isHoldingObject = false;

    List<ObjectScript> allPickupItems = new List<ObjectScript>();
    

    [Header("Ground Check Settings")]
    [SerializeField] float groundCheckRadius = 0.2f;
    [SerializeField] Vector3 groundCheckOffset;
    [SerializeField] LayerMask groundLayer;
    float ySpeed;

    Quaternion targetRotation;
    Animator animator;
    ObjectSpawner objectSpawner;
    HandCloneObject handCloneObj;
    [SerializeField]  CharacterController characterController;

    private bool isGrounded;
    CameraController cameraController;

    private void Awake()
    {
     
        if (Hand == null)
        {
            Hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        }
        handCloneObj = FindObjectOfType<HandCloneObject>(); // Reference to HandCloneObject
       
        RightHand = Instantiate(handCloneObj.HandObjectClone).transform;

        characterController = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponent<Animator>();

        // Ensure HandObjectClone is set
        if (handCloneObj != null && handCloneObj.HandObjectClone == null)
        {
            Debug.LogError("HandCloneObject's HandObjectClone is not assigned. Please assign it in the Inspector.");
        }

    }


    void Start()
    {
        

        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        cameraController = Camera.main.GetComponent<CameraController>();
        if (cameraController != null)
        {
            cameraController.SetFollowTarget(transform);
        }
        else
        {
            Debug.LogError("CameraController not found on the Main Camera.");
        }
        DisableCursor();

    }

    public void AddPickupItem(ObjectScript item)
    {
        if (!allPickupItems.Contains(item))
        {
            allPickupItems.Add(item);
            Debug.Log($"Added new pickupable item: {item.name}");
        }
    }




    // Finds the closest pickupable object within range
    private ObjectScript ClosestObject()
    {
        ObjectScript closestObject = null;
        float closestDistance = float.MaxValue;

        Debug.Log($"Total pickup items in list: {allPickupItems.Count}");

        foreach (ObjectScript obj in allPickupItems)
        {
            if (obj == null || !obj.CompareTag("Pickupable")) continue;

            float distance = Vector3.Distance(transform.position, obj.transform.position);
            if (distance < closestDistance && distance <= pickupRange)
            {
                closestDistance = distance;
                closestObject = obj;
            }
        
            else
            {
                Debug.Log($"Skipping {obj.name} as it doesn't have the 'Pickupable' tag.");
            }
        }

        if (closestObject != null)
        {
            Debug.Log("Closest object found: " + closestObject.name);
        }
        else
        {
            Debug.Log("No object within pickup range.");
        }

        return closestObject;
    }

    

    // Update is called once per frame
    void Update()
    {
      
        if (!IsOwner) return;

      

        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            EnableCursor();
        }
        else if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            DisableCursor();
        }


        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (Input.GetKeyDown(KeyCode.P))
        {
            animator.SetTrigger("isPunching");
        }

        if (Input.GetKeyDown(KeyCode.E) && !isHoldingObject)
        {
            focusObject = ClosestObject(); // Assign closest object

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

        characterController.Move(velocity * Time.deltaTime);

        if (moveInput.magnitude > 0)
        {
           
            targetRotation = Quaternion.LookRotation(moveDirection);
        }
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation,rotationspeed * Time.deltaTime); //smoother character rotation

      
        // Update animator parameters with damping for smooth transitions
        animator.SetFloat("horizontal", h, 0.2f, Time.deltaTime);
        animator.SetFloat("vertical", adjustedVertical, 0.2f, Time.deltaTime); // Use adjusted vertical for running


    }

    void LateUpdate()
    {

        if (isPickingUp || isHoldingObject)
        {
            RightHand.position = Hand.position;
            RightHand.rotation = Hand.rotation;
        }

        // Handle physics for held objects
        if (isHoldingObject && focusObject != null)
        {
            Rigidbody rb = focusObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true; // Disable physics while holding the object
            }
        }

    }

    [ServerRpc]
    private void RequestSnowBallServerRpc()
    {
        objectSpawner.SpawnBall();
    }

    private void EnableCursor()
    {
        Cursor.lockState = CursorLockMode.None; // Unlock the cursor
        Cursor.visible = true; // Make the cursor visible
    }
    private void DisableCursor()
    {
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen
        Cursor.visible = false; // Hide the cursor
    }

    void GroundCheck()
    {


        isGrounded =  Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius, groundLayer);// to check if the player is standing on the ground 

    }
    private Vector3 ikTargetPosition;
    private Quaternion ikTargetRotation;

    private float maxReachDistance = 2.5f;
    private float reachSpeed = 3f;

    // IK logic for pickup

    
    private void OnAnimatorIK(int layerIndex)
    {
        if (isPickingUp && (focusObject != null))
        {
            Transform HandTransform = RightHand;

            Vector3 directionToObject = (focusObject.transform.position - transform.position).normalized;
            float distanceToObject = Vector3.Distance(transform.position, focusObject.transform.position);

            float clampedDistance = Mathf.Min(distanceToObject, maxReachDistance);

            Vector3 targetPosition = transform.position + directionToObject * clampedDistance;

            focusObject.GetComponent<Rigidbody>().isKinematic = true;

            // Lerp hand to target
            ikTargetPosition = Vector3.Lerp(ikTargetPosition, targetPosition, Time.deltaTime * reachSpeed);
            ikTargetRotation = Quaternion.LookRotation(focusObject.transform.position - HandTransform.position);

            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKPosition(AvatarIKGoal.RightHand, ikTargetPosition);
            animator.SetIKRotation(AvatarIKGoal.RightHand, ikTargetRotation);

            float distanceToHand = Vector3.Distance(HandTransform.position, focusObject.transform.position) - focusObject.transform.localScale.x;
            if (distanceToHand <= pickUpTolerance)
            {
                AttachObjectToHand(focusObject);
                isPickingUp = false;
            }
        }
        else
        {
            // Reset IK weights
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
        }
    }

    private void AttachObjectToHand(ObjectScript obj)
    {
        if (focusObject == null)
        {
            Debug.LogError("No focusObject found to attach.");
            return;
        }

        // Ensure the object is spawned and is a NetworkObject
        NetworkObject networkObject = focusObject.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            if (IsServer)
            {
                // Reparent the object to the hand on the server
                print("Host tried to attach object");

                
                    networkObject.transform.SetParent(RightHand, false); // Keep original world position
                

                // Sync position and rotation on the server
                UpdatePositionAndRotation(obj);
            }
            else
            {
                // Request the server to handle reparenting
                SubmitReparentRequestServerRpc(networkObject.NetworkObjectId);
                UpdatePositionAndRotation(obj);
            }
        }

       

        var objCollider = obj.GetComponent<Collider>();
        if (objCollider != null)
        {
            objCollider.enabled = false; // Disable the collider to prevent unwanted interactions
        }

        isHoldingObject = true;
    }


    // Sync position and rotation
    private void UpdatePositionAndRotation(ObjectScript obj)
    {
        obj.transform.localPosition = Vector3.zero; 
        obj.transform.localRotation = Quaternion.identity; 
    }



    [ServerRpc]
    private void SubmitReparentRequestServerRpc(ulong objectId)
    {
        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject networkObject);
        if (networkObject != null)
        {
          
           networkObject.transform.SetParent(RightHand, false); // Avoid nested NetworkObjects
           
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
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

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

    [ServerRpc]
    private void SubmitThrowRequestServerRpc(ulong objectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject networkObject))
        {
            var obj = networkObject.GetComponent<ObjectScript>();
            if (obj != null)
            {
                networkObject.TrySetParent((GameObject)null);
                ApplyThrowForce(obj);
            }
        }
    }

    private void ApplyThrowForce(ObjectScript obj)
    {
        var objRigidbody = obj.GetComponent<Rigidbody>();
        if (objRigidbody != null)
        {
            objRigidbody.isKinematic = false;
            objRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // Apply throw force
            Vector3 throwDirection = transform.forward + Vector3.up * 0.6f;
            float throwForce = 10f;
            objRigidbody.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        }

        var objCollider = obj.GetComponent<Collider>();
        if (objCollider != null)
        {
            objCollider.enabled = true;
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0,1,0,0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);

        
    }

   
}
