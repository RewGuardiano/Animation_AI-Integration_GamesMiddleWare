using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;

public class ClientNetworkAnimator : NetworkTransform
{
    [SerializeField] public Animator animator;

    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
