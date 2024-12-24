using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(NetworkObject), typeof(NetworkTransform))]
public class ObjectScript : NetworkBehaviour
{
    public override void OnNetworkObjectParentChanged(NetworkObject parentNetworkObject)
    {
        print("Snowball parentChangedCalled");

        base.OnNetworkObjectParentChanged(parentNetworkObject);
    }
}
