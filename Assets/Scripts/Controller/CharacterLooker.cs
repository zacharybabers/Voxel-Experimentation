using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterLooker : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 100f;

    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject aimCamera;

    private float yRotation = 0f;

    private void Start()
    {
       mainCamera.SetActive(true);
       aimCamera.SetActive(false);
    }

    private void Update()
    {
        UpdateYRotation();
        CheckCamera();
        
    }

    private void UpdateYRotation()
    {
        var mouseDelta = Input.GetAxis("Mouse Y");

        yRotation -= mouseDelta;
        yRotation = Mathf.Clamp(yRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(yRotation, 0f, 0f);
    }

    private void CheckCamera()
    {
        if (Input.GetButtonUp("Aim") && !aimCamera.activeInHierarchy)
        {
            mainCamera.SetActive(false);
            aimCamera.SetActive(true);
        }
        else if (Input.GetButtonUp("Aim") && !mainCamera.activeInHierarchy)
        {
            mainCamera.SetActive(true);
            aimCamera.SetActive(false);
        }
    }
}
