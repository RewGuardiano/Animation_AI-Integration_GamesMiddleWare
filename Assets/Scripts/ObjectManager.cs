 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class ObjectManager : MonoBehaviour
{
    [SerializeField] private Button clientButton;
    [SerializeField] private Button hostButton;




    public void Awake()
    {
        hostButton.onClick.AddListener(StartHost);
        clientButton.onClick.AddListener(StartClient);

        
    }

    public void StartHost()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.StartHost();
            Debug.Log("Host started.");

        }
        else
        {
            Debug.LogError("NetworkManager not found!");
        }
    }

    public void StartClient()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.StartClient();
            Debug.Log("Client started.");
        }
        else
        {
            Debug.LogError("NetworkManager not found!");
        }
    }

   
}



