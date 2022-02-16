using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    private CharacterMovement characterMovement;
    private Vector3 posDifference;
    void Start()
    {
        characterMovement = FindObjectOfType<CharacterMovement>();
        posDifference = transform.position - characterMovement.transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = characterMovement.transform.position + posDifference;
    }
}
