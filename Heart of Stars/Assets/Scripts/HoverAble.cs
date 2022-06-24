using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HoverAble : MonoBehaviour
{
    public static Action<ResourceData> OnHoverUpdate = delegate { };

    bool Hovering = false;
    bool panelActive = false;
    bool panelStall = false;

    [SerializeField]
    GameObject myPanel;

    Resource myResource;

    public void Assignment(Resource data)
    {
        myResource = data;
        //Debug.Log($"Hover Panel: {data}");
        myPanel.GetComponent<ResourceInfoPanel>().Assignment(data);
    }

    private void Update()
    {
        if(Hovering && Input.GetKeyDown(KeyCode.Space) && panelActive && !panelStall)
        {
            panelStall = true;
        }
    }

    public void HidePanel()
    {
        panelStall = false;
        panelActive = false;
        if (myPanel != null)
            myPanel.SetActive(false);
    }

    public void MouseEnter()
    {
        //Debug.Log($"Hovering over {myResource.myResource.displayName}");
        Hovering = true;
        if (!panelActive)
        {
            panelActive = true;
            myPanel.SetActive(true);
            transform.SetSiblingIndex(transform.parent.childCount-1);
            OnHoverUpdate?.Invoke(myResource.myResource);
        }
    }

    public void MouseExit()
    {
        Hovering = false;
        if (!panelStall)
        {
            HidePanel();
        }
    }
}
