using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform followTarget;
  

    [SerializeField] float distance = 5;

    [SerializeField] float rotationspeed = 0.5f;


    float rotationY;
    float rotationX;

    [SerializeField] float minVerticalAngle = -45;
    [SerializeField] float maxverticalAngle = 45;

    [SerializeField] Vector2 framingOffset;

    [SerializeField] bool invertX;
    [SerializeField] bool invertY;

    float invertXVal;
    float invertYVal;

 
    private void LateUpdate()
    {
        if (followTarget == null) return;

        invertXVal = (invertX) ? -1 : 1;
        invertYVal= (invertY) ? -1 : 1;

        rotationY += Input.GetAxis("Mouse X") * invertXVal * rotationspeed;

     
        rotationX += Input.GetAxis("Mouse Y") * invertYVal * rotationspeed;
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxverticalAngle);

         var targetRotation = Quaternion.Euler(rotationX,rotationY,0);



        var focusPosition = followTarget.position + new Vector3(framingOffset.x,framingOffset.y);

        transform.position = focusPosition - targetRotation * new Vector3(0, 0, distance);
       
        transform.rotation = targetRotation;

        

    }
    public Quaternion PlanarRotation => Quaternion.Euler(0, rotationY, 0);//Properties function 

    public void SetFollowTarget(Transform target)
    {
        followTarget = target;
    }


}
