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

    public void Initialize(ResourceData data)
    {
        myResource = data;
        nameText.text = data.itemName;
        displayText.text = data.displayName;
        curText.text = data.currentAmount.ToString();
        autoAText.text = data.autoAmount.ToString();
        autoTText.text = data.autoTime.ToString();
    }
}
