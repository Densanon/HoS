using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static Action OnCameraZoomContinue = delegate { };
    public static Action OnNeedZoomInfo = delegate { };
    public static Action<string> SaveCameraState = delegate { };

    Camera c_Camera;
    Transform t_Camera;
    bool b_InMap = false;
    public bool b_AtUniverse = false;

    Transform t_target;
    Vector3 v_NormalPos;
    Vector3 v_startPos;
    Vector3 v_endPos;
    float f_journeyLength;
    float f_startTime;
    float f_speed = 1f;
    float f_ZoomedOut = 15f;
    float f_ZoomedIn = 1.5f;
    float f_NormalizeZoom = 5f;
    float f_InterpolatedTime = 0f;
    bool b_zoom = true;
    bool b_Normalize = false;
    bool b_ZoomIn = false;
    bool b_zooming = false;

    float scrollRate = 0.25f;

    bool b_canMoveOnMap = false;

    private void Awake()
    {
        c_Camera = Camera.main;
        t_Camera = c_Camera.transform;
        v_NormalPos = t_Camera.position;
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
        if (!b_InMap)
        {
            CheckZoomIn();
            CheckZoomOut();
            CheckNormalize();
            if(!b_AtUniverse && Input.mouseScrollDelta.y < 0f && b_zoom && !b_zooming)
            {
                b_zoom = false;
                b_ZoomIn = false;
                f_InterpolatedTime = 0f;
            }
        }
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

            if(Input.mouseScrollDelta.y != 0)
            {
                c_Camera.orthographicSize += Input.mouseScrollDelta.y * -1 * 0.25f;
            }
        }
    }

    void CheckZoomIn()
    {
        if (t_target != null && b_zoom && b_zooming)
        {
            // Set our position as a fraction of the distance between the markers.
            t_Camera.position = Vector3.Lerp(v_startPos, v_endPos, f_InterpolatedTime);

            c_Camera.orthographicSize = Mathf.Lerp(f_NormalizeZoom, f_ZoomedIn, f_InterpolatedTime);

            f_InterpolatedTime += 0.5f * Time.deltaTime;
            if (f_InterpolatedTime > f_speed)
            {
                f_InterpolatedTime = 0f;
                t_Camera.position = v_endPos;
                t_target = null;
                b_Normalize = true;
                b_ZoomIn = true;
                b_AtUniverse = false;
                OnCameraZoomContinue?.Invoke();
            }
        }
    }

    void CheckZoomOut()
    {
        if(!b_zoom && !b_ZoomIn)
        {
            c_Camera.orthographicSize = Mathf.Lerp(f_NormalizeZoom, f_ZoomedOut, f_InterpolatedTime);

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
                c_Camera.orthographicSize = Mathf.Lerp(f_ZoomedOut, f_NormalizeZoom, f_InterpolatedTime);
                f_InterpolatedTime += 0.5f * Time.deltaTime;
                t_Camera.position = v_NormalPos;
            }
            else
            {
                c_Camera.orthographicSize = Mathf.Lerp(f_ZoomedIn, f_NormalizeZoom, f_InterpolatedTime);
                f_InterpolatedTime += 0.5f * Time.deltaTime;

                // Distance moved equals elapsed time times speed..
                //float distCovered = (Time.time - f_startTime) * f_speed;

                // Fraction of journey completed equals current distance divided by total distance.
                //float fractionOfJourney = distCovered / f_journeyLength;

                // Set our position as a fraction of the distance between the markers.
                t_Camera.position = Vector3.Lerp(v_startPos, v_endPos, f_InterpolatedTime);
            }

            if(f_InterpolatedTime > f_speed)
            {
                b_Normalize = false;
                t_Camera.position = v_NormalPos;
                c_Camera.orthographicSize = 5f;
                b_zoom = true;
                t_target = null;
                b_zooming = false;
            }
        }
    }

    private void ZoomIntoSpaceobject(GameObject obj)
    {
        t_target = obj.transform;
        v_endPos = t_target.position;
        v_startPos = t_Camera.position;
        v_endPos.z = v_startPos.z;
        f_journeyLength = Vector3.Distance(v_startPos, v_endPos);
        f_startTime = Time.time;
        f_InterpolatedTime = 0f;
        b_zooming = true;
    }

    private void ZoomFromGivenObject(GameObject obj)
    {
        t_target = obj.transform;
        v_startPos = t_target.position;
        v_endPos = v_NormalPos;
        v_startPos.z = v_endPos.z;
        f_journeyLength = Vector3.Distance(v_startPos, v_endPos);
        f_startTime = Time.time;
        f_InterpolatedTime = 0f;
        b_Normalize = true;
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
        t_Camera.position = new Vector3(0f, 10f, -.5f);
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

    string SaveCameraSettings()
    {
        return $"{transform.position};{transform.rotation};{c_Camera.orthographicSize}";
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

            c_Camera.orthographicSize = float.Parse(ar[2]);
        }
    }

    private void OnApplicationQuit()
    {
        SaveCameraState?.Invoke(SaveCameraSettings());
    }
}
