using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CameraScalerMatchHelper : MonoBehaviour
{
    // Set this to the in-world distance between the left & right edges of your scene.
    public float sceneWidth = 10;

    public Camera pCamera;
    void Start()
    {
        if (pCamera == null) pCamera = GetComponent<Camera>();
        float ratio = Screen.height * 1f / Screen.width;

        // Adjust the camera's height so the desired scene width fits in view
        if (ratio > 19.5f / 9f + 0.00001f)
        {
            float unitsPerPixel = sceneWidth / Screen.width;
            float desiredHalfHeight = 0.5f * unitsPerPixel * Screen.height;
            pCamera.orthographicSize = desiredHalfHeight;
        }
    }

}