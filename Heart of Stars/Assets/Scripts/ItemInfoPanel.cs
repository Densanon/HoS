using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ItemInfoPanel : MonoBehaviour
{
    ItemData myItemData;

    [SerializeField]
    TMP_Text myTitle;
    [SerializeField]
    TMP_Text myDetails;
    [SerializeField]
    TMP_Text myDependencies;
    [SerializeField]
    TMP_Text myDependenciesNeededAmounts;
    [SerializeField]
    TMP_Text myDependenciesCurAmount;
    [SerializeField]
    TMP_Text amountOwned;
    [SerializeField]
    TMP_Text myMakeables;

    ItemData[] myItemNeeds;

    #region UnityEngine
    private void OnEnable()
    {
        Item.OnClicked += UpdateInfo;
        Item.OnUpdate += UpdateInfo;
        HoverAbleItemButton.OnHoverUpdate += UpdateInfo;
    }
    private void OnDisable()
    {
        Item.OnClicked -= UpdateInfo;
        Item.OnUpdate -= UpdateInfo;
        HoverAbleItemButton.OnHoverUpdate -= UpdateInfo;
    }
    #endregion

    #region Setup
    public void Assignment(Item data, Main main)
    {
        myItemData = data.myItemData;
        myTitle.text = myItemData.displayName;
        myDetails.text = myItemData.description;
        amountOwned.text = $"Amount Owned: {myItemData.currentAmount}";
        myMakeables.text = myItemData.buildables;

        myItemNeeds = data.GetImediateDependencyNames();
        int[] oTemp = data.GetDependencyAmounts();

        myDependencies.text = "";
        myDependenciesNeededAmounts.text = "";
        myDependenciesCurAmount.text = "";

        if(myItemNeeds != null)
        {
            for(int i = 0; i < myItemNeeds.Length; i++)
            {
                myDependencies.text = myDependencies.text + myItemNeeds[i].displayName + "\n";
                myDependenciesNeededAmounts.text = myDependenciesNeededAmounts.text + oTemp[i].ToString() + "\n";
                myDependenciesCurAmount.text = myDependenciesCurAmount.text + myItemNeeds[i].currentAmount.ToString() + "\n";
            }
        }
    }
    public void Assignment(ItemData data, Main main)
    {
        myItemData = data;
        myTitle.text = data.displayName;
        myDetails.text = data.description;
        amountOwned.text = $"Amount Owned: {data.currentAmount}";
        myMakeables.text = data.buildables;

        myItemNeeds = main.FindDependenciesFromItem(data);
        int[] oTemp = main.FindDependencyAmountsFromItem(data);

        myDependencies.text = "";
        myDependenciesNeededAmounts.text = "";
        myDependenciesCurAmount.text = "";

        if (myItemNeeds != null)
        {
            for (int i = 0; i < myItemNeeds.Length; i++)
            {
                myDependencies.text = myDependencies.text + myItemNeeds[i].displayName + "\n";
                myDependenciesNeededAmounts.text = myDependenciesNeededAmounts.text + oTemp[i].ToString() + "\n";
                myDependenciesCurAmount.text = myDependenciesCurAmount.text + myItemNeeds[i].currentAmount.ToString() + "\n";
            }
        }
    }
    #endregion

    #region UI Management
    public void UpdateInfo(ItemData source)
    {
        if(source == myItemData)
        {
            amountOwned.text = $"Amount Owned: {myItemData.currentAmount}";
            myDependenciesCurAmount.text = "";
            for (int i = 0; i < myItemNeeds.Length; i++)
            {
                myDependenciesCurAmount.text = myDependenciesCurAmount.text + myItemNeeds[i].currentAmount.ToString() + "\n";
            }
        }
    }
    #endregion
}
