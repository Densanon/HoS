using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIOverrideListener : MonoBehaviour
{
    public static bool isOverUI;

    void Update()
    {
        isOverUI = EventSystem.current.IsPointerOverGameObject();
    }
}
