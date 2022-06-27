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
    TMP_Text curText;
    [SerializeField]
    TMP_Text autoAText;
    [SerializeField]
    TMP_Text autoTText;

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
            //Debug.Log($"Resource Display: {myResource.displayName} received an update for my info.");
            curText.text = myResource.currentAmount.ToString();
            autoAText.text = myResource.autoAmount.ToString();
            autoTText.text = myResource.craftTime.ToString();
        }
    }
}
