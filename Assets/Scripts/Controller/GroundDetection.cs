using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundDetection : MonoBehaviour
{
    [SerializeField] private CharacterMovement characterMovement;

    private void Awake()
    {
        var player = characterMovement.gameObject;
        var ignored = player.GetComponentsInChildren<Collider>();
        var myCollider = GetComponent<Collider>();
        foreach (var collider1 in ignored)
        {
            Physics.IgnoreCollision(myCollider, collider1);
        }
    }
    
    

    private void OnTriggerEnter(Collider other)
    {
        characterMovement.isGrounded = true;
        //characterMovement.currentJumps = pureMovement.maxJumps;
    }

    private void OnTriggerStay(Collider other)
    {
        characterMovement.isGrounded = true;
    }


    private void OnTriggerExit(Collider other)
    {
        characterMovement.isGrounded = false;
        characterMovement.velocity.x = characterMovement.groundMomentum.x;
        characterMovement.velocity.z = characterMovement.groundMomentum.z;
    }
}
