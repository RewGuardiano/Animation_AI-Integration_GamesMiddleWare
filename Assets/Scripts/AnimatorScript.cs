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
       animator.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {


        if (Input.GetKey(KeyCode.W))
        {
            animator.SetBool("IsWalking", true);
        }
      
        else
        {
            animator.SetBool("IsWalking", false);
        }
    }
}
