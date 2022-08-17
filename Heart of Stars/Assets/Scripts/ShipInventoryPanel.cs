using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class ShipInventoryPanel : MonoBehaviour
{
    public static Action OnUpdateSliderForShip = delegate { };

    Spacecraft myShip;
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

    private void OnEnable()
    {
        OnUpdateSliderForShip += AdjustSlider;
    }
    private void OnDisable()
    {
        OnUpdateSliderForShip -= AdjustSlider;
    }

    public void Setup(Spacecraft ship, ResourceData shipRes, ResourceData tileRes)
    {
        myShip = ship;
        shipResource = shipRes;
        tileResource = tileRes;
        resourceNameText.text = shipResource.displayName;
        
        SetValues();
    }
    void SetValues()
    {
        shipResourceAmountText.text = shipResource.currentAmount.ToString();
        tileResourceAmountText.text = tileResource.currentAmount.ToString();
        if (shipResource.itemName == "soldier")
        {
            resourceSlider.maxValue = myShip.troopMaximum;
            resourceSlider.minValue = 0;
            resourceSlider.value = shipResource.currentAmount;
            OnUpdateSliderForShip -= AdjustSlider;
        }
        else
        {
            resourceSlider.maxValue = shipResource.currentAmount + tileResource.currentAmount;
            resourceSlider.value = shipResource.currentAmount;
        }
    }
    void AdjustSlider()
    {
        int value = shipResource.currentAmount + tileResource.currentAmount;
        if (value > myShip.storageMax)
        {
            value -= value - myShip.storageMax;
            resourceSlider.maxValue = value;
            return;
        }
        resourceSlider.maxValue = value;
    }

    public void ShowUIFromSlider()// Accessed via slider
    {
        int value = (int)resourceSlider.value;
        int total = shipResource.currentAmount + tileResource.currentAmount;
        if(shipResource.itemName == "soldier")
        {
            value = myShip.GetTroopsOnBoard(value);
            shipResource.SetCurrentAmount(value);
            shipResourceAmountText.text = value.ToString();
            shipResourceAmountText.color =(myShip.troops == myShip.troopMaximum) ?Color.red : Color.black;

            SetTileAmount(total - value);
            return;
        }
        shipResource.SetCurrentAmount(value);
        shipResourceAmountText.text = value.ToString();
        SetTileAmount(total-value);
        myShip.UpdateStorage();

        OnUpdateSliderForShip?.Invoke();
    }

    void SetTileAmount(int value)
    {
        tileResource.SetCurrentAmount(value);
        tileResourceAmountText.text = tileResource.currentAmount.ToString();
    }
}
