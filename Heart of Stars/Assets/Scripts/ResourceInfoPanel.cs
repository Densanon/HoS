using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResourceInfoPanel : MonoBehaviour
{
    ResourceData myResource;

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

    ResourceData[] myResourceNeeds;

    [SerializeField]
    GameObject buttonPrefab;
    [SerializeField]
    Transform buttonContainer;

    private void OnEnable()
    {
        Resource.OnClicked += UpdateInfo;
        Resource.OnUpdate += UpdateInfo;
        HoverAble.OnHoverUpdate += UpdateInfo;
    }

    private void OnDisable()
    {
        Resource.OnClicked -= UpdateInfo;
        Resource.OnUpdate -= UpdateInfo;
        HoverAble.OnHoverUpdate -= UpdateInfo;
    }

    public void Assignment(Resource data, Main main)
    {
        myResource = data.myResource;
        myTitle.text = data.myResource.displayName;
        myDetails.text = data.myResource.description;
        amountOwned.text = $"Amount Owned: {data.myResource.currentAmount}";

        myResourceNeeds = data.GetImediateDependencyNames();
        int[] oTemp = data.GetDependencyAmounts();

        myDependencies.text = "";
        myDependenciesNeededAmounts.text = "";
        myDependenciesCurAmount.text = "";

        //Debug.Log($"ResourceInfoPanel for {myResource.displayName} : {myResourceNeeds.Length}");

        if(myResourceNeeds != null)
        {
            for(int i = 0; i < myResourceNeeds.Length; i++)
            {
                GameObject obj = Instantiate(buttonPrefab, buttonContainer);
                Resource r = obj.GetComponent<Resource>();
                r.panelButton = true;
                r.SetUpResource(myResourceNeeds[i], false, main);
                r.ResetRotation();
                myDependencies.text = myDependencies.text + myResourceNeeds[i].displayName + "\n";
                myDependenciesNeededAmounts.text = myDependenciesNeededAmounts.text + oTemp[i].ToString() + "\n";
                myDependenciesCurAmount.text = myDependenciesCurAmount.text + myResourceNeeds[i].currentAmount.ToString() + "\n";
            }
        }
    }

    public void Assignment(ResourceData data, Main main)
    {
        myResource = data;
        myTitle.text = data.displayName;
        myDetails.text = data.description;
        amountOwned.text = $"Amount Owned: {data.currentAmount}";

        myResourceNeeds = main.FindDependenciesFromResourceData(data);
        int[] oTemp = main.FindDependencyAmountsFromResourceData(data);

        myDependencies.text = "";
        myDependenciesNeededAmounts.text = "";
        myDependenciesCurAmount.text = "";

        //Debug.Log($"ResourceInfoPanel for {myResource.displayName} : {myResourceNeeds.Length}");

        if (myResourceNeeds != null)
        {
            for (int i = 0; i < myResourceNeeds.Length; i++)
            {
                GameObject obj = Instantiate(buttonPrefab, buttonContainer);
                Resource r = obj.GetComponent<Resource>();
                r.panelButton = true;
                r.SetUpResource(data, false, main);
                r.ResetRotation();

                myDependencies.text = myDependencies.text + myResourceNeeds[i].displayName + "\n";
                myDependenciesNeededAmounts.text = myDependenciesNeededAmounts.text + oTemp[i].ToString() + "\n";
                myDependenciesCurAmount.text = myDependenciesCurAmount.text + myResourceNeeds[i].currentAmount.ToString() + "\n";
            }
        }
    }

    public void UpdateInfo(ResourceData source)
    {
        if(source == myResource)
        {
            amountOwned.text = $"Amount Owned: {myResource.currentAmount}";
            myDependenciesCurAmount.text = "";
            for (int i = 0; i < myResourceNeeds.Length; i++)
            {
                myDependenciesCurAmount.text = myDependenciesCurAmount.text + myResourceNeeds[i].currentAmount.ToString() + "\n";
            }
        }
    }
}
