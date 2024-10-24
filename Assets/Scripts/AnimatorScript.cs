using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;

public class AnimatorScript : MonoBehaviour
{

    public Animator animator;
    public float speed = 1.0f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {


        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            // Set the walking animation
            animator.SetBool("IsWalking", true);

            // Calculate the movement direction
            Vector3 movement = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) movement += Vector3.forward;
            if (Input.GetKey(KeyCode.S)) movement += Vector3.back;
            if (Input.GetKey(KeyCode.A)) movement += Vector3.left;
            if (Input.GetKey(KeyCode.D)) movement += Vector3.right;

            // Move the character
            transform.Translate(movement.normalized * speed * Time.deltaTime);
        }
      
        else
        {
            animator.SetBool("IsWalking", false);
        }
    }
}
