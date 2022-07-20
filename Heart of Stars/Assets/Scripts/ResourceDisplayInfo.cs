using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResourceDisplayInfo : MonoBehaviour
{
    ResourceData myResource;
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

    private void OnEnable()
    {
        Resource.OnClicked += UpdateInfo;
        Resource.OnUpdate += UpdateInfo;
    }

    private void OnDisable()
    {
        Resource.OnClicked -= UpdateInfo;
        Resource.OnUpdate -= UpdateInfo;
    }

    public void Initialize(ResourceData data)
    {
        myResource = data;
        nameText.text = data.itemName;
        displayText.text = data.displayName;
        UpdateInfo(data);
    }

    void UpdateInfo(ResourceData source)
    {
        if(source == myResource)
        {
            currentText.text = myResource.currentAmount.ToString();
            autoAmountText.text = myResource.autoAmount.ToString();
            autoTimeText.text = myResource.craftTime.ToString();
        }
    }
}
