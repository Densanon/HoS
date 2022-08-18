using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMoveWithCamera : MonoBehaviour
{
    Camera myCamera;
    CameraController controller;
    UIItemManager manager;

    bool first = false;

    #region UnityEngine
    private void OnEnable()
    {
        if (!first)
        {
            myCamera = Camera.main;
            controller = myCamera.GetComponent<CameraController>();
            manager = GetComponent<UIItemManager>();
            first = true;
        }
    }
    void Update()
    {
        MoveUIOnPlanet();
    }
    #endregion

    private void MoveUIOnPlanet()
    {
        if (controller.canMoveOnMap && !controller.planetaryCameraIsFrozen) manager.ResetUILocationOnScreen();
    }
}
