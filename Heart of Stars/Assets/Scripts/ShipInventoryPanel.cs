using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShipInventoryPanel : MonoBehaviour
{
    public ResourceData shipResource;
    public ResourceData tileResource;

    [SerializeField]
    TMP_Text resourceNameText;
    [SerializeField]
    TMP_Text shipResourceAmountText;
    [SerializeField]
    TMP_Text tileResourceAmountText;
    [SerializeField]
    Slider resourceSlider;

    public void Setup(ResourceData ship, ResourceData tile)
    {
        shipResource = ship;
        tileResource = tile;
        resourceNameText.text = ship.displayName;
        shipResourceAmountText.text = ship.currentAmount.ToString();
        tileResourceAmountText.text = tile.currentAmount.ToString();
        resourceSlider.maxValue = ship.currentAmount + tile.currentAmount;
        resourceSlider.value = ship.currentAmount;
    }

    public void ShowUIFromSlider(int value)// Accessed via slider
    {
        shipResourceAmountText.text = value.ToString();
        tileResourceAmountText.text = (resourceSlider.maxValue - value).ToString();
        shipResource.SetCurrentAmount(value);
        tileResource.SetCurrentAmount(Mathf.RoundToInt(resourceSlider.maxValue - value));
    }
}
