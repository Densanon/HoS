using System;
using UnityEngine;

public class Depthinteraction : MonoBehaviour
{
    public static Action<GameObject> SpaceInteractionHover = delegate { };

    bool Hovering;
    [SerializeField]
    GameObject UIElements;

    private void OnMouseEnter()
    {
        Hovering = true;

        UIElements.SetActive(true);
    }

    private void OnMouseExit()
    {
        Hovering = false;

        UIElements.SetActive(false);
    }

    private void OnMouseDown()
    {
        SpaceInteractionHover?.Invoke(transform.parent.gameObject); 
    }
}
