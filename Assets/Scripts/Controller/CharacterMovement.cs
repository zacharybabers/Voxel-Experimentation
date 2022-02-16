using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(CharacterController))]
public class CharacterMovement : MonoBehaviour
{
    [SerializeField] private float playerSpeed = 10f;
    [SerializeField] private float jumpHeight = 10f;
    [SerializeField] private float defaultGravity = -9.8f;
    [SerializeField] private float airDrag = -0.3f;
    [SerializeField] private float friction = -0.3f;
    [SerializeField] private float airborneMovement = 20f;
    [SerializeField] private float airborneOppositeMultiplier = 2f;
    [SerializeField] private float maxAirSpeed = 20f;
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float stepRotation = 31.4f;

    public float gravity = -9.8f;
    public Vector3 velocity;
    public Vector3 groundMomentum;
    public Vector3 lastNonZeroInput;
    
    public bool isGrounded;
    
    public CharacterController controller;
    [SerializeField] private Transform mainCamera;
    [SerializeField] private Transform lookTransform;
    


    private void Start()
    {
        gravity = defaultGravity;
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        Locomotion();
        RotateToInput();
    }

    private void Locomotion()
    {
        //Rotate to camera
        RotateAlongX();
        
        //process input
        var controlMovement = ProcessControlInput();
        
        //do jump
        CheckJump();

        if (isGrounded)
        {
            groundMomentum = controller.velocity;
        }
        
        if (isGrounded && velocity.y < 0f)
        {
            velocity.y = -9.8f;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }
        
        ApplyResistance();

        var finalMovement = velocity + controlMovement;

        controller.Move(finalMovement * Time.deltaTime);




    }

    private Vector3 GetControlMovement()
    {
        var controlThrow = GetControlThrow();

        var transform1 = transform;
        var camRight = mainCamera.right;
        camRight = new Vector3(camRight.x, 0, camRight.z).normalized;

        var camForward = mainCamera.forward;
        camForward = new Vector3(camForward.x, 0, camForward.z).normalized;
        
        var controlMovement = camRight * controlThrow.x + camForward * controlThrow.y;
        controlMovement = controlMovement.normalized;
        return controlMovement;
    }

    private Vector3 ProcessControlInput()
    {
        if (isGrounded)
        {
            var controlMovement = GetControlMovement() * playerSpeed;
            return controlMovement;
        }
        
        GetAirMovement();
        return Vector3.zero;
    }

    private void CheckJump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }
    }

    private void ApplyResistance()
    {
        if (isGrounded)
        {
            var direction = new Vector3(velocity.x, 0, velocity.z);
            AddForce(friction, direction);
        }
        else
        {
            var direction = new Vector3(velocity.x, 0, velocity.z);
            AddForce(airDrag, direction);
        }
    }
    
    private void GetAirMovement()
    {
        var controlMovement = GetControlMovement();
        
        var normalVelocity = new Vector3(velocity.x, 0f, velocity.z).normalized;
        if (Vector3.Dot(controlMovement, normalVelocity) < 0f)
        {
            AddForce(airborneMovement * airborneOppositeMultiplier, controlMovement);
        }
        else if (Vector3.Dot(controlMovement, normalVelocity) > .9f)
        {
            var vel = controller.velocity;
            var magnitude = new Vector2(vel.x, vel.z);
            var speed = magnitude.magnitude;
            if (speed < maxAirSpeed)
            {
                AddForce(airborneMovement, controlMovement);
            }
            else
            {
                AddForce(airborneMovement / 4f, controlMovement);
            }
        }
        else
        {
            AddForce(airborneMovement, controlMovement);
        }
    }

    private void RotateAlongX()
    {
        var mouseDelta = Input.GetAxis("Mouse X");
        var mouseX = mouseDelta * mouseSensitivity * Time.deltaTime;
        lookTransform.Rotate(Vector3.up, mouseX);
    }

    private void RotateToInput()
    {
        var controlMovement = GetControlMovement();
        if (!controlMovement.Equals(Vector3.zero))
        {
            lastNonZeroInput = controlMovement;
        }

        Quaternion targetRotation = Quaternion.identity;
        if (lastNonZeroInput != Vector3.zero) {
             targetRotation = Quaternion.LookRotation(lastNonZeroInput, Vector3.up);
        }
        
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, stepRotation * Time.deltaTime);
    }
    

    public Vector2 GetControlThrow()
    {
        return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }
    
    private void AddForce(float forceCoefficient, Vector3 direction)
         {
             velocity += Time.deltaTime * forceCoefficient * direction;
         }
}
