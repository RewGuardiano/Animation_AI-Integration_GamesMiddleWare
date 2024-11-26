using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class ObjectManager : MonoBehaviour
{
    public GameObject snowBallCloneTemplate;

    public void ButtonClicked()
    {
        GameObject snowBallInstance = Instantiate(snowBallCloneTemplate);

        NetworkObject networkObject = snowBallInstance.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn();
        }
        else
        {
            Debug.LogError("snowball prefab missing NetworkObject component");
        }

    }
}

