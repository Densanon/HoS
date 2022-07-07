using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Camera c_Camera;
    Transform t_Camera;
    bool b_InMap = false;
    bool b_scrolling = false;

    float scrollRate = 0.25f;

    bool b_canMoveOnMap = false;

    private void Awake()
    {
        c_Camera = Camera.main;
        t_Camera = c_Camera.transform;
    }

    private void OnEnable()
    {
        Main.OnWorldMap += AdjustCameraSettings;
    }

    private void OnDisable()
    {
        Main.OnWorldMap -= AdjustCameraSettings;
    }

    private void AdjustCameraSettings(bool overWorld)
    {
        if (overWorld)
        {
            t_Camera.rotation = Quaternion.identity;
            t_Camera.position = new Vector3(0f, 0f, -.5f);
            b_InMap = false;
            return;
        }

        t_Camera.Rotate(45f, 0f, 0f);
        t_Camera.position = new Vector3(0f, 10f, -10f);
        b_InMap = true;
    }


    void LateUpdate()
    {
        if (b_InMap)
        {
            if (Input.GetMouseButtonDown(2))
            {
                b_canMoveOnMap = true;
            }

            if (Input.GetMouseButtonUp(2))
            {
                b_canMoveOnMap = false;
            }


            if (b_canMoveOnMap)
            {
                Vector3 pos = t_Camera.position;
                pos.z += Input.GetAxis("Mouse Y") * scrollRate * -1;
                pos.x += Input.GetAxis("Mouse X") * scrollRate * -1;
                t_Camera.position = pos; 
            }
        }
    }
}
