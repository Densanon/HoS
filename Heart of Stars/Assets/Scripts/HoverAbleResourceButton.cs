using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HoverAbleResourceButton : MonoBehaviour
{
    public static Action<ResourceData> OnHoverUpdate = delegate { };

    public bool isHovering = false;
    bool panelIsActive = false;
    bool panelIsStalled = false;

    public bool panelButton = false;

    [SerializeField]
    GameObject myPanel;

    ResourceData myResource;

    public void Assignment(Resource data, Main main)
    {
        myResource = data.myResource;
        myPanel.GetComponent<ResourceInfoPanel>().Assignment(data, main);
    }

    public void Assignment(ResourceData data, Main main)
    {
        myResource = data;
        myPanel.GetComponent<ResourceInfoPanel>().Assignment(data, main);
    }

    private void Update()
    {
        if(isHovering && Input.GetKeyDown(KeyCode.Space) && panelIsActive &&
            !panelIsStalled) panelIsStalled = true;
    }

    public void HidePanel()
    {
        panelIsStalled = false;
        panelIsActive = false;
        if (myPanel != null) myPanel.SetActive(false);
    }

    public void MouseEnter()
    {
        isHovering = true;
        if (!panelIsActive)
        {
            panelIsActive = true;
            myPanel.SetActive(true);
            if (!panelButton) transform.SetSiblingIndex(transform.parent.childCount-1);

            OnHoverUpdate?.Invoke(myResource);
        }
    }

    public void MouseExit()
    {
        isHovering = false;
        if (!panelIsStalled) HidePanel();
    }
}
