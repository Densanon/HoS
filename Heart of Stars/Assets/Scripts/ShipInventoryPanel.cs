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
    public ItemData shipItem;
    public ItemData tileItem;

    [SerializeField]
    TMP_Text itemNameText;
    [SerializeField]
    TMP_Text shipItemAmountText;
    [SerializeField]
    TMP_Text tileItemAmountText;
    [SerializeField]
    Slider itemSlider;

    #region UnityEngine
    private void OnEnable()
    {
        OnUpdateSliderForShip += AdjustSlider;
    }
    private void OnDisable()
    {
        OnUpdateSliderForShip -= AdjustSlider;
    }
    #endregion

    #region Setup
    public void Setup(Spacecraft ship, ItemData shipIte, ItemData tileIte)
    {
        myShip = ship;
        shipItem = shipIte;
        tileItem = tileIte;
        itemNameText.text = shipItem.displayName;
        
        SetValues();
    }
    #endregion

    #region UI Management
    void SetValues()
    {
        shipItemAmountText.text = shipItem.currentAmount.ToString();
        tileItemAmountText.text = tileItem.currentAmount.ToString();
        if (shipItem.itemName == "soldier")
        {
            itemSlider.maxValue = myShip.troopMaximum;
            itemSlider.minValue = 0;
            itemSlider.value = shipItem.currentAmount;
            OnUpdateSliderForShip -= AdjustSlider;
        }
        else
        {
            itemSlider.maxValue = shipItem.currentAmount + tileItem.currentAmount;
            itemSlider.value = shipItem.currentAmount;
        }
    }
    void AdjustSlider()
    {
        int value = shipItem.currentAmount + tileItem.currentAmount;
        if (value > myShip.storageMax)
        {
            value -= value - myShip.storageMax;
            itemSlider.maxValue = value;
            return;
        }
        itemSlider.maxValue = value;
    }
    public void ShowUIFromSlider()// Accessed via slider
    {
        int value = (int)itemSlider.value;
        int total = shipItem.currentAmount + tileItem.currentAmount;
        if(shipItem.itemName == "soldier")
        {
            value = myShip.GetUnitsOnBoard(value);
            shipItem.SetCurrentAmount(value);
            shipItemAmountText.text = value.ToString();
            shipItemAmountText.color =(myShip.units == myShip.troopMaximum) ?Color.red : Color.black;

            SetTileAmount(total - value);
            return;
        }
        shipItem.SetCurrentAmount(value);
        shipItemAmountText.text = value.ToString();
        SetTileAmount(total-value);
        myShip.UpdateStorage();

        OnUpdateSliderForShip?.Invoke();
    }
    void SetTileAmount(int value)
    {
        tileItem.SetCurrentAmount(value);
        tileItemAmountText.text = tileItem.currentAmount.ToString();
    }
    #endregion
}
