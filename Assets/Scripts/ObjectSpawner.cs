using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class ObjectSpawner : MonoBehaviour
{
    public Button SpawnButton;
    public GameObject snowBallCloneTemplate;
    private bool isSpawning = false; // Prevent rapid clicks

    public void Awake()
    {
        SpawnButton.onClick.AddListener(SpawnBall);
    }

    public void SpawnBall()
    {
        if (!NetworkManager.Singleton.IsServer) // This is correct
        {
            Debug.LogWarning("Only the server can spawn snowballs!");
            return;
        }

        if (isSpawning) return; // Prevent multiple calls
        isSpawning = true;

        if (snowBallCloneTemplate == null)
        {
            Debug.LogError("snowBallCloneTemplate is not assigned!");
            isSpawning = false;
            return;
        }

        GameObject snowBallInstance = Instantiate(snowBallCloneTemplate);

        NetworkObject networkObject = snowBallInstance.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn(true); // Spawn the object over the network
            // Notify all PlayerController instances about this new pickupable object
            ObjectScript objectScript = snowBallInstance.GetComponent<ObjectScript>();
            if (objectScript != null)
            {
                NotifyClientsAboutPickupableObject(objectScript); // Call the new method here
            }

            StartCoroutine(ResetSpawnCooldown());
        }
        else
        {
            Debug.LogError("NetworkObject component is missing on the snowball prefab!");
            Destroy(snowBallInstance);
            isSpawning = false;
            return;
        }
    }
    private void NotifyClientsAboutPickupableObject(ObjectScript objectScript)
    {
        AddPickupItemClientRpc(objectScript.NetworkObjectId);
    }

    [ClientRpc]
    private void AddPickupItemClientRpc(ulong networkObjectId)
    {
        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject networkObject);
        if (networkObject != null && networkObject.TryGetComponent(out ObjectScript objectScript))
        {
            foreach (PlayerController player in FindObjectsOfType<PlayerController>())
            {
                player.AddPickupItem(objectScript);
            }
        }
    }



    private IEnumerator ResetSpawnCooldown()
    {
        yield return new WaitForSeconds(0.1f); // Small cooldown
        isSpawning = false;
    }
}
