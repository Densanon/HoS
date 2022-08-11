using System;
using System.Collections;
using UnityEngine;

public class Depthinteraction : MonoBehaviour
{
    public static Action<GameObject> SpaceInteractionHover = delegate { };
    public static Action<GameObject> CheckIfCanZoomToPlanetaryLevel = delegate { };
    public static Action OnDeactivate = delegate { };

    [SerializeField]
    GameObject UIElements;

    bool isInteractable = false;

    private void OnEnable()
    {
        OnDeactivate += TurnOffInteractables;
        StartCoroutine(Reactivate());
    }

    private void OnDisable()
    {
        OnDeactivate += TurnOffInteractables;
    }

    IEnumerator Reactivate()
    {
        yield return new WaitForSeconds(2f);
        TurnOnInteractable();
    }

    void TurnOnInteractable()
    {
        isInteractable = true;
    }

    void TurnOffInteractables()
    {
        isInteractable = false;
    }

    private void OnMouseEnter()
    {
        if(isInteractable)
            UIElements.SetActive(true);
    }

    private void OnMouseExit()
    {
        if(isInteractable)
            UIElements.SetActive(false);
    }

    private void OnMouseDown()
    {
        Debug.Log("Mouse was pressed on a space object.");
        CheckIfCanZoomToPlanetaryLevel?.Invoke(transform.parent.gameObject);
        if (isInteractable && (Main.isVisitedPlanet || Main.canSeeIntoPlanets))
        {
            Debug.Log("I am interactable and either visited or can see.");
            SpaceInteractionHover?.Invoke(transform.parent.gameObject);
            OnDeactivate?.Invoke();
        }
    }
}
