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
    float journeyLength;
    float cameraZoomTimeReference;
    float cameraZoomTime = 1f;
    float cameraZoomedOutSize = 15f;
    float cameraZoomedInSize = 1.5f;
    float cameraNormalZoomSize = 5f;
    float cameraZoomCurrentTimerTime = 0f;
    float cameraPlanetaryZoomOutBounds = 10f;
    bool canOverWorldZoom = true;
    bool needsNormalZoom = false;
    bool canOverWorldZoomIn = false;
    bool isZooming = false;
    bool planetaryCameraIsFrozen = false;

    float cameraPlanetaryZoomRate = 0.25f;

    bool canMoveOnMap = false;

    #region Unity Methods
    private void Awake()
    {
        myCamera = Camera.main;
        myTransform = myCamera.transform;
        cameraNormalPosition = myTransform.position;
    }

    private void OnEnable()
    {
        Main.OnWorldMap += TransitionCameraSettings;
        Main.OnGoingToHigherLevel += ZoomFromGivenObject;
        Main.SendCameraState += LoadCamState;
        HexTileInfo.OnStartingTile += FindStartingPosition;
        Depthinteraction.SpaceInteractionHover += ZoomIntoSpaceobject;
    }

    private void OnDisable()
    {
        Main.OnWorldMap -= TransitionCameraSettings;
        Main.OnGoingToHigherLevel -= ZoomFromGivenObject;
        Main.SendCameraState -= LoadCamState;
        HexTileInfo.OnStartingTile -= FindStartingPosition;
        Depthinteraction.SpaceInteractionHover -= ZoomIntoSpaceobject;
    }
    #endregion

    #region Space Camera Controls
    private void Update()
    {
        if (!inMap)
        {
            CheckZoomIn();
            CheckZoomOut();
            CheckNormalize();
            if(!atUniverse && Input.mouseScrollDelta.y < 0f && canOverWorldZoom && !isZooming)
            {
                canOverWorldZoom = false;
                canOverWorldZoomIn = false;
                cameraZoomCurrentTimerTime = 0f;
            }
        }
    }

    void CheckZoomIn()
    {
        if (targetTransform != null && canOverWorldZoom && isZooming)
        {
            // Set our position as a fraction of the distance between the markers.
            myTransform.position = Vector3.Lerp(cameraStartingZoomPosition, cameraEndingZoomPosition, cameraZoomCurrentTimerTime);

            myCamera.orthographicSize = Mathf.Lerp(cameraNormalZoomSize, cameraZoomedInSize, cameraZoomCurrentTimerTime);

            cameraZoomCurrentTimerTime += 0.5f * Time.deltaTime;
            if (cameraZoomCurrentTimerTime > cameraZoomTime)
            {
                cameraZoomCurrentTimerTime = 0f;
                myTransform.position = cameraEndingZoomPosition;
                targetTransform = null;
                needsNormalZoom = true;
                canOverWorldZoomIn = true;
                atUniverse = false;
                OnCameraZoomContinue?.Invoke();
            }
        }
    }

    void CheckZoomOut()
    {
        if (!canOverWorldZoom && !canOverWorldZoomIn)
        {
            myCamera.orthographicSize = Mathf.Lerp(cameraNormalZoomSize, cameraZoomedOutSize, cameraZoomCurrentTimerTime);

            cameraZoomCurrentTimerTime += 0.5f * Time.deltaTime;
            if (cameraZoomCurrentTimerTime > 1f)
            {
                cameraZoomCurrentTimerTime = 0f;
                canOverWorldZoom = true;
                OnNeedZoomInfo?.Invoke();
            }
        }
    }

    void CheckNormalize()
    {
        if (needsNormalZoom)
        {
            if (canOverWorldZoomIn)
            {
                myCamera.orthographicSize = Mathf.Lerp(cameraZoomedOutSize, cameraNormalZoomSize, cameraZoomCurrentTimerTime);
                cameraZoomCurrentTimerTime += 0.5f * Time.deltaTime;
                myTransform.position = cameraNormalPosition;
            }
            else
            {
                myCamera.orthographicSize = Mathf.Lerp(cameraZoomedInSize, cameraNormalZoomSize, cameraZoomCurrentTimerTime);
                cameraZoomCurrentTimerTime += 0.5f * Time.deltaTime;
                myTransform.position = Vector3.Lerp(cameraStartingZoomPosition, cameraEndingZoomPosition, cameraZoomCurrentTimerTime);
            }

            if (cameraZoomCurrentTimerTime > cameraZoomTime)
            {
                needsNormalZoom = false;
                myTransform.position = cameraNormalPosition;
                myCamera.orthographicSize = 5f;
                canOverWorldZoom = true;
                targetTransform = null;
                isZooming = false;
            }
        }
    }

    private void ZoomIntoSpaceobject(GameObject obj)
    {
        targetTransform = obj.transform;
        cameraEndingZoomPosition = targetTransform.position;
        cameraStartingZoomPosition = myTransform.position;
        cameraEndingZoomPosition.z = cameraStartingZoomPosition.z;
        journeyLength = Vector3.Distance(cameraStartingZoomPosition, cameraEndingZoomPosition);
        cameraZoomTimeReference = Time.time;
        cameraZoomCurrentTimerTime = 0f;
        isZooming = true;
    }

    private void ZoomFromGivenObject(GameObject obj)
    {
        targetTransform = obj.transform;
        cameraStartingZoomPosition = targetTransform.position;
        cameraEndingZoomPosition = cameraNormalPosition;
        cameraStartingZoomPosition.z = cameraEndingZoomPosition.z;
        journeyLength = Vector3.Distance(cameraStartingZoomPosition, cameraEndingZoomPosition);
        cameraZoomTimeReference = Time.time;
        cameraZoomCurrentTimerTime = 0f;
        needsNormalZoom = true;
    }
    #endregion

    private void TransitionCameraSettings(bool inOverWorld)
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

    #region Planetary Camera Controls
    void LateUpdate()
    {
        if (inMap)
        {
            CheckMouseWheelPress();

            MoveCameraOnPlanet();

            ZoomCameraOnPlanet();
        }
    }

    private void FindStartingPosition(Transform tran)
    {
        Vector3 pos = tran.position;
        pos.y += 10;
        pos.z -= 10;
        myTransform.position = pos;
    }

    private void CheckMouseWheelPress()
    {
        if (Input.GetMouseButtonDown(2))
        {
            canMoveOnMap = true;
        }

        if (Input.GetMouseButtonUp(2))
        {
            canMoveOnMap = false;
        }
    }

    private void ZoomCameraOnPlanet()
    {
        float f = Input.mouseScrollDelta.y;
        if (f != 0 && !planetaryCameraIsFrozen)
        {
            float val = f * -1 * 0.25f;
            float size = myCamera.orthographicSize;
            if (size + val < cameraPlanetaryZoomOutBounds && size + val > cameraZoomedInSize)
            {
                myCamera.orthographicSize += val;
            }
            if (size < cameraZoomedInSize)
            {
                myCamera.orthographicSize = cameraZoomedInSize;
            }
        }
    }

    private void MoveCameraOnPlanet()
    {
        if (canMoveOnMap && !planetaryCameraIsFrozen)
        {
            Vector3 pos = myTransform.position;
            pos.z += Input.GetAxis("Mouse Y") * cameraPlanetaryZoomRate * -1;
            pos.x += Input.GetAxis("Mouse X") * cameraPlanetaryZoomRate * -1;
            myTransform.position = pos;
        }
    }

    public void TogglePlanetaryCameraMovement()
    {
        planetaryCameraIsFrozen = !planetaryCameraIsFrozen;
    }
    #endregion

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
