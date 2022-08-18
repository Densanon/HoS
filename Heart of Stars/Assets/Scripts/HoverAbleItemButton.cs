using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HoverAbleItemButton : MonoBehaviour
{
    public static Action<ItemData> OnHoverUpdate = delegate { };

    [SerializeField]
    GameObject myPanel;
    ItemData myItemData;

    public bool isHovering = false;
    bool panelIsActive = false;
    bool panelIsStalled = false;

    #region UnityEngine
    private void Update()
    {
        if(isHovering && Input.GetKeyDown(KeyCode.Space) && panelIsActive && !panelIsStalled) panelIsStalled = true;
    }
    
    #region Mouse Interactions
    public void MouseEnter()
    {
        isHovering = true;
        if (!panelIsActive)
        {
            panelIsActive = true;
            myPanel.SetActive(true);
            transform.SetSiblingIndex(transform.parent.childCount-1);

            OnHoverUpdate?.Invoke(myItemData);
        }
    }
    public void MouseExit()
    {
        isHovering = false;
        if (!panelIsStalled) HidePanel();
    }
    #endregion
    #endregion

    #region Setup
    public void Assignment(Item data, Main main)
    {
        myItemData = data.myItemData;
        myPanel.GetComponent<ItemInfoPanel>().Assignment(data, main);
    }
    public void Assignment(ItemData data, Main main)
    {
        myItemData = data;
        myPanel.GetComponent<ItemInfoPanel>().Assignment(data, main);
    }
    #endregion

    #region Panel Management
    public void HidePanel()
    {
        panelIsStalled = false;
        panelIsActive = false;
        if (myPanel != null) myPanel.SetActive(false);
    }
    #endregion
}
