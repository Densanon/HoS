using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResourceDisplayInfo : MonoBehaviour
{
    ItemData myItemData;
    [SerializeField]
    TMP_Text nameText;
    [SerializeField]
    TMP_Text displayText;
    [SerializeField]
    TMP_Text currentText;
    [SerializeField]
    TMP_Text autoAmountText;
    [SerializeField]
    TMP_Text autoTimeText;

    #region UnityEngine
    private void OnEnable()
    {
        Item.OnClicked += UpdateInfo;
        Item.OnUpdate += UpdateInfo;
    }
    private void OnDisable()
    {
        Item.OnClicked -= UpdateInfo;
        Item.OnUpdate -= UpdateInfo;
    }
    #endregion

    #region Setup
    public void Initialize(ItemData data)
    {
        myItemData = data;
        nameText.text = data.itemName;
        displayText.text = data.displayName;
        UpdateInfo(data);
    }
    #endregion

    #region UI Mnagement
    void UpdateInfo(ItemData source)
    {
        if(source == myItemData)
        {
            currentText.text = myItemData.currentAmount.ToString();
            autoAmountText.text = myItemData.autoAmount.ToString();
            autoTimeText.text = myItemData.craftTime.ToString();
        }
    }
    #endregion
}
