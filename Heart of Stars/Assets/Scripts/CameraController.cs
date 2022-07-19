using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static Action OnCameraZoomContinue = delegate { };
    public static Action OnNeedZoomInfo = delegate { };
    public static Action<string> SaveCameraState = delegate { };

    Camera myCamera;
    Transform myTransform;
    bool inMap = false;
    public bool atUniverse = false;

    Transform targetTransform;
    Vector3 cameraNormalPosition;
    Vector3 cameraStartingZoomPosition;
    Vector3 cameraEndingZoomPosition;
    float f_journeyLength;
    float f_startTime;
    float f_speed = 1f;
    float f_ZoomedOut = 15f;
    float f_ZoomedIn = 1.5f;
    float f_NormalizeZoom = 5f;
    float f_InterpolatedTime = 0f;
    float UpperBounds = 10f;
    bool b_zoom = true;
    bool b_Normalize = false;
    bool b_ZoomIn = false;
    bool b_zooming = false;

    float scrollRate = 0.25f;

    bool b_canMoveOnMap = false;

    private void Awake()
    {
        myCamera = Camera.main;
        myTransform = myCamera.transform;
        cameraNormalPosition = myTransform.position;
    }

    private void OnEnable()
    {
        Main.OnWorldMap += AdjustCameraSettings;
        Main.OnGoingToHigherLevel += ZoomFromGivenObject;
        Main.SendCameraState += LoadCamState;
        HexTileInfo.OnStartingTile += FindStartingPosition;
        Depthinteraction.SpaceInteractionHover += ZoomIntoSpaceobject;
    }

    private void OnDisable()
    {
        Main.OnWorldMap -= AdjustCameraSettings;
        Main.OnGoingToHigherLevel -= ZoomFromGivenObject;
        Main.SendCameraState -= LoadCamState;
        HexTileInfo.OnStartingTile -= FindStartingPosition;
        Depthinteraction.SpaceInteractionHover -= ZoomIntoSpaceobject;
    }

    private void Update()
    {
        if (!inMap)
        {
            CheckZoomIn();
            CheckZoomOut();
            CheckNormalize();
            if(!atUniverse && Input.mouseScrollDelta.y < 0f && b_zoom && !b_zooming)
            {
                b_zoom = false;
                b_ZoomIn = false;
                f_InterpolatedTime = 0f;
            }
        }
    }

    void LateUpdate()
    {
        if (inMap)
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
                Vector3 pos = myTransform.position;
                pos.z += Input.GetAxis("Mouse Y") * scrollRate * -1;
                pos.x += Input.GetAxis("Mouse X") * scrollRate * -1;
                myTransform.position = pos; 
            }

            float f = Input.mouseScrollDelta.y;
            if(f != 0)
            {
                float val = f * -1 * 0.25f;
                float size = myCamera.orthographicSize;
                if (size + val < UpperBounds && size + val > f_ZoomedIn)
                {
                    myCamera.orthographicSize += val;
                }
                if(size < f_ZoomedIn)
                {
                    myCamera.orthographicSize = f_ZoomedIn;
                }
            }
        }
    }

    void CheckZoomIn()
    {
        if (targetTransform != null && b_zoom && b_zooming)
        {
            // Set our position as a fraction of the distance between the markers.
            myTransform.position = Vector3.Lerp(cameraStartingZoomPosition, cameraEndingZoomPosition, f_InterpolatedTime);

            myCamera.orthographicSize = Mathf.Lerp(f_NormalizeZoom, f_ZoomedIn, f_InterpolatedTime);

            f_InterpolatedTime += 0.5f * Time.deltaTime;
            if (f_InterpolatedTime > f_speed)
            {
                f_InterpolatedTime = 0f;
                myTransform.position = cameraEndingZoomPosition;
                targetTransform = null;
                b_Normalize = true;
                b_ZoomIn = true;
                atUniverse = false;
                OnCameraZoomContinue?.Invoke();
            }
        }
    }

    void CheckZoomOut()
    {
        if(!b_zoom && !b_ZoomIn)
        {
            myCamera.orthographicSize = Mathf.Lerp(f_NormalizeZoom, f_ZoomedOut, f_InterpolatedTime);

            f_InterpolatedTime += 0.5f * Time.deltaTime;
            if (f_InterpolatedTime > 1f)
            {
                f_InterpolatedTime = 0f;
                b_zoom = true;
                OnNeedZoomInfo?.Invoke();
            }
        }
    }

    void CheckNormalize()
    {
        if (b_Normalize)
        {
            if (b_ZoomIn)
            {
                myCamera.orthographicSize = Mathf.Lerp(f_ZoomedOut, f_NormalizeZoom, f_InterpolatedTime);
                f_InterpolatedTime += 0.5f * Time.deltaTime;
                myTransform.position = cameraNormalPosition;
            }
            else
            {
                myCamera.orthographicSize = Mathf.Lerp(f_ZoomedIn, f_NormalizeZoom, f_InterpolatedTime);
                f_InterpolatedTime += 0.5f * Time.deltaTime;

                // Distance moved equals elapsed time times speed..
                //float distCovered = (Time.time - f_startTime) * f_speed;

                // Fraction of journey completed equals current distance divided by total distance.
                //float fractionOfJourney = distCovered / f_journeyLength;

                // Set our position as a fraction of the distance between the markers.
                myTransform.position = Vector3.Lerp(cameraStartingZoomPosition, cameraEndingZoomPosition, f_InterpolatedTime);
            }

            if(f_InterpolatedTime > f_speed)
            {
                b_Normalize = false;
                myTransform.position = cameraNormalPosition;
                myCamera.orthographicSize = 5f;
                b_zoom = true;
                targetTransform = null;
                b_zooming = false;
            }
        }
    }

    private void ZoomIntoSpaceobject(GameObject obj)
    {
        targetTransform = obj.transform;
        cameraEndingZoomPosition = targetTransform.position;
        cameraStartingZoomPosition = myTransform.position;
        cameraEndingZoomPosition.z = cameraStartingZoomPosition.z;
        f_journeyLength = Vector3.Distance(cameraStartingZoomPosition, cameraEndingZoomPosition);
        f_startTime = Time.time;
        f_InterpolatedTime = 0f;
        b_zooming = true;
    }

    private void ZoomFromGivenObject(GameObject obj)
    {
        targetTransform = obj.transform;
        cameraStartingZoomPosition = targetTransform.position;
        cameraEndingZoomPosition = cameraNormalPosition;
        cameraStartingZoomPosition.z = cameraEndingZoomPosition.z;
        f_journeyLength = Vector3.Distance(cameraStartingZoomPosition, cameraEndingZoomPosition);
        f_startTime = Time.time;
        f_InterpolatedTime = 0f;
        b_Normalize = true;
    }

    private void AdjustCameraSettings(bool inOverWorld)
    {
        if (inOverWorld)
        {
            myTransform.rotation = Quaternion.identity;
            myTransform.position = new Vector3(0f, 0f, -.5f);
            inMap = false;
            return;
        }

        myTransform.Rotate(45f, 0f, 0f);
        myTransform.position = new Vector3(0f, 10f, -.5f);
        myCamera.orthographicSize = 1.5f;
        inMap = true;
    }

    private void FindStartingPosition(Transform tran)
    {
        Vector3 pos = tran.position;
        pos.y += 10;
        pos.z -= 10;
        myTransform.position = pos;
    }

    string SaveCameraSettings()
    {
        return $"{transform.position};{transform.rotation};{myCamera.orthographicSize}";
    }

    private void LoadCamState(string state)
    {
        //(7.82, 10.00, -6.30);(0.38268, 0.00000, 0.00000, 0.92388);5.75
        //(7.82, 10.00, -6.30); (0.70710, 0.00000, 0.00000, 0.70711); 1.5

        if (state != null)
        {
            string[] ar = state.Split(";");

            string s = ar[0].Remove(0, 1);
            s = s.Remove(s.Length - 1);
            string[] str = s.Split(",");
            transform.position = new Vector3(float.Parse(str[0]), float.Parse(str[1].Remove(0, 1)), float.Parse(str[2].Remove(0, 1)));

            s = ar[1].Remove(0, 1);
            s = s.Remove(s.Length - 1);
            str = s.Split(",");
            transform.rotation = new Quaternion(float.Parse(str[0]), float.Parse(str[1].Remove(0, 1)), float.Parse(str[2].Remove(0, 1)), float.Parse(str[3].Remove(0, 1)));

            myCamera.orthographicSize = float.Parse(ar[2]);
        }
    }

    private void OnApplicationQuit()
    {
        SaveCameraState?.Invoke(SaveCameraSettings());
    }
}
