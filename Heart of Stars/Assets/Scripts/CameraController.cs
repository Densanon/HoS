using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Camera c_Camera;
    Transform t_Camera;
    bool b_InMap = false;

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
        HexTileInfo.OnStartingTile += FindStartingPosition;
    }

    private void OnDisable()
    {
        Main.OnWorldMap -= AdjustCameraSettings;
        HexTileInfo.OnStartingTile -= FindStartingPosition;
    }

    private void AdjustCameraSettings(bool overWorld)
    {
        if (overWorld)
        {
            t_Camera.rotation = Quaternion.identity;
            t_Camera.position = new Vector3(0f, 0f, -.5f);
            c_Camera.orthographicSize = 5;
            b_InMap = false;
            return;
        }

        t_Camera.Rotate(45f, 0f, 0f);
        c_Camera.orthographicSize = 1.5f;
        b_InMap = true;
    }

    private void FindStartingPosition(Transform tran)
    {
        Vector3 pos = tran.position;
        pos.y += 10;
        pos.z -= 10;
        t_Camera.position = pos;
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
