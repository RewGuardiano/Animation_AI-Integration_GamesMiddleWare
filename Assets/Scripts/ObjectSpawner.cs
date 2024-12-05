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
        if (!NetworkManager.Singleton.IsServer) // Only allow the server to spawn
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
        if (networkObject == null)
        {
            Debug.LogError("NetworkObject component is missing on the snowball prefab!");
            Destroy(snowBallInstance);
            isSpawning = false;
            return;
        }
        else
        {
            networkObject.Spawn(); // Spawn the object over the network
            StartCoroutine(ResetSpawnCooldown());
        }
    }

    private IEnumerator ResetSpawnCooldown()
    {
        yield return new WaitForSeconds(0.1f); // Small cooldown
        isSpawning = false;
    }
}
