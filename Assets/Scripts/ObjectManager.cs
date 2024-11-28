using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class ObjectManager : MonoBehaviour
{
    
    public Button SpawnButton;
    public GameObject snowBallCloneTemplate; 
 


    public void Start()
    {
        SpawnButton.onClick.AddListener(ButtonClicked);
    }


    

    public void ButtonClicked()
    {
        if (snowBallCloneTemplate == null)
        {
            Debug.LogError("snowBallCloneTemplate is not assigned!");
            return;
        }

        GameObject snowBallInstance = Instantiate(snowBallCloneTemplate);

        NetworkObject networkObject = snowBallInstance.GetComponent<NetworkObject>();
        if (networkObject == null)
        {
            Debug.LogError("NetworkObject component is missing on the snowball prefab!");
            Destroy(snowBallInstance);
            return;
        }

        networkObject.Spawn();
    }
}

