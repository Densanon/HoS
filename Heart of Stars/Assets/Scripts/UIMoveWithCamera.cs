using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMoveWithCamera : MonoBehaviour
{
    Camera camera;
    CameraController controller;
    RectTransform myTransform;

    bool first = false;
    private void OnEnable()
    {
        if (!first)
        {
            camera = Camera.main;
            controller = camera.GetComponent<CameraController>();
            myTransform = GetComponent<RectTransform>();
            first = true;
        }
    }

    void Update()
    {
        MoveUIOnPlanet();
    }

    private void MoveUIOnPlanet()
    {
        if (controller.canMoveOnMap && !controller.planetaryCameraIsFrozen)
        {
            Vector2 pos = myTransform.anchoredPosition;
            //pos.y += Input.GetAxis("Mouse Y") * controller.cameraPlanetaryZoomRate * 100;
            pos.y += 215 * 1.75f * controller.camMoveY / camera.orthographicSize;
            //pos.x += Input.GetAxis("Mouse X") * controller.cameraPlanetaryZoomRate * 100;
            pos.x += 310 * 1.75f * controller.camMoveX / camera.orthographicSize;
            myTransform.anchoredPosition = pos;
        }
    }
}
