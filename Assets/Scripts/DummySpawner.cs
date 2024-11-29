using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class DummySpawner : NetworkBehaviour
{
    [SerializeField] private GameObject dummyPrefab;
    private CameraController cameraController;

    private bool dummySpawned = false;  // Flag to track if the dummy is already spawned

    private void Start()
    {
        // Find the camera in the scene
        cameraController = FindObjectOfType<CameraController>();
    }

    // This ServerRpc will only spawn the dummy on the server and synchronize with clients
    [ServerRpc(RequireOwnership = false)]  // Allow clients to call this RPC
    public void SpawnDummyServerRpc()
    {
        // Ensure the dummy is only spawned once
        if (dummySpawned) return;

        if (!IsServer) return; // Only the server should spawn the dummy

        // Spawn the dummy
        GameObject dummyClone = Instantiate(dummyPrefab, Vector3.zero, Quaternion.identity);
        dummyClone.GetComponent<NetworkObject>().Spawn(); // This ensures it gets spawned across the network

        // Mark the dummy as spawned
        dummySpawned = true;

        // Call SetFollowTarget for the client who owns this dummy
        if (IsOwner) // Only the client who owns the dummy will follow it
        {
            if (cameraController != null)
            {
                cameraController.SetFollowTarget(dummyClone.transform);
            }
        }
    }

    // Call this function from a client or host to spawn the dummy
    public void RequestSpawnDummy()
    {
        // Only the host or server should call the server RPC to spawn the dummy
        if (IsServer || IsHost)
        {
            SpawnDummyServerRpc();  // Server will handle the spawning and synchronization
        }
    }
}