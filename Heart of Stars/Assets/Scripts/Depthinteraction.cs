using System;
using System.Collections;
using UnityEngine;

public class Depthinteraction : MonoBehaviour
{
    public static Action<GameObject> SpaceInteractionHover = delegate { };
    public static Action OnDeactivate = delegate { };

    [SerializeField]
    GameObject UIElements;

    bool b_interactable = false;

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
        b_interactable = true;
    }

    void TurnOffInteractables()
    {
        b_interactable = false;
    }

    private void OnMouseEnter()
    {
        if(b_interactable)
            UIElements.SetActive(true);
    }

    private void OnMouseExit()
    {
        if(b_interactable)
            UIElements.SetActive(false);
    }

    private void OnMouseDown()
    {
        if (b_interactable)
        {
            SpaceInteractionHover?.Invoke(transform.parent.gameObject);
            OnDeactivate?.Invoke();
        }
    }
}
