using Unity.Netcode;
using UnityEngine;

public class ObjectScript : NetworkBehaviour
{
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Server RPC method, adjusted to accept serializable parameters
    [ServerRpc]
    public void PickupObjectServerRpc(Vector3 handPosition, Quaternion handRotation)
    {
        if (!IsServer) return;

        // Attach the object to the hand on the server
        AttachToHand(handPosition, handRotation);
    }

    // Attach logic
    private void AttachToHand(Vector3 handPosition, Quaternion handRotation)
    {
        transform.SetParent(null); // Optional, in case it's already attached
        transform.position = handPosition;
        transform.rotation = handRotation;

        if (rb != null)
        {
            rb.isKinematic = true; // Disable physics
        }

        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false; // Disable collisions
        }
    }
}
