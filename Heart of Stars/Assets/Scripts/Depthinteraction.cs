using System;
using UnityEngine;

public class Depthinteraction : MonoBehaviour
{
    public static Action<GameObject> SpaceInteractionHover = delegate { };

    [SerializeField]
    GameObject UIElements;

    private void OnMouseEnter()
    {
        UIElements.SetActive(true);
    }

    private void OnMouseExit()
    {
        UIElements.SetActive(false);
    }

    private void OnMouseDown()
    {
        SpaceInteractionHover?.Invoke(transform.parent.gameObject);
    }
}
