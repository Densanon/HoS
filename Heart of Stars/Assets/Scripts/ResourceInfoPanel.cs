using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResourceInfoPanel : MonoBehaviour
{
    ResourceData myResource;

    [SerializeField]
    TMP_Text myTitle_text;
    [SerializeField]
    TMP_Text myDetails_text;
    [SerializeField]
    TMP_Text myDependencies_text;
    [SerializeField]
    TMP_Text myDependenciesNeededAmounts_text;
    [SerializeField]
    TMP_Text myDependenciesCurAmount_text;
    [SerializeField]
    TMP_Text amountOwned_text;

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
        myTitle_text.text = data.myResource.displayName;
        myDetails_text.text = data.myResource.discription;
        amountOwned_text.text = $"Amount Owned: {data.myResource.currentAmount}";

        myResourceNeeds = data.GetImediateDependencyNames();
        int[] oTemp = data.GetDependencyAmounts();

        myDependencies_text.text = "";
        myDependenciesNeededAmounts_text.text = "";
        myDependenciesCurAmount_text.text = "";

        Debug.Log($"ResourceInfoPanel for {myResource.displayName} : {myResourceNeeds.Length}");

        if(myResourceNeeds != null)
        {
            for(int i = 0; i < myResourceNeeds.Length; i++)
            {
                GameObject obj = Instantiate(buttonPrefab, buttonContainer);
                Resource r = obj.GetComponent<Resource>();
                r.panelButton = true;
                r.AssignResource(myResourceNeeds[i], false, main);
                r.ResetRotation();
                myDependencies_text.text = myDependencies_text.text + myResourceNeeds[i].displayName + "\n";
                myDependenciesNeededAmounts_text.text = myDependenciesNeededAmounts_text.text + oTemp[i].ToString() + "\n";
                myDependenciesCurAmount_text.text = myDependenciesCurAmount_text.text + myResourceNeeds[i].currentAmount.ToString() + "\n";
            }
        }
    }

    public void Assignment(ResourceData data, Main main)
    {
        myResource = data;
        myTitle_text.text = data.displayName;
        myDetails_text.text = data.discription;
        amountOwned_text.text = $"Amount Owned: {data.currentAmount}";

        myResourceNeeds = main.ReturnDependencies(data);
        int[] oTemp = main.ReturnDependencyAmounts(data);

        myDependencies_text.text = "";
        myDependenciesNeededAmounts_text.text = "";
        myDependenciesCurAmount_text.text = "";

        Debug.Log($"ResourceInfoPanel for {myResource.displayName} : {myResourceNeeds.Length}");

        if (myResourceNeeds != null)
        {
            for (int i = 0; i < myResourceNeeds.Length; i++)
            {
                GameObject obj = Instantiate(buttonPrefab, buttonContainer);
                Resource r = obj.GetComponent<Resource>();
                r.panelButton = true;
                r.AssignResource(data, false, main);
                r.ResetRotation();

                myDependencies_text.text = myDependencies_text.text + myResourceNeeds[i].displayName + "\n";
                myDependenciesNeededAmounts_text.text = myDependenciesNeededAmounts_text.text + oTemp[i].ToString() + "\n";
                myDependenciesCurAmount_text.text = myDependenciesCurAmount_text.text + myResourceNeeds[i].currentAmount.ToString() + "\n";
            }
        }
    }

    public void UpdateInfo(ResourceData source)
    {
        if(source == myResource)
        {
            amountOwned_text.text = $"Amount Owned: {myResource.currentAmount}";
            myDependenciesCurAmount_text.text = "";
            for (int i = 0; i < myResourceNeeds.Length; i++)
            {
                myDependenciesCurAmount_text.text = myDependenciesCurAmount_text.text + myResourceNeeds[i].currentAmount.ToString() + "\n";
            }
        }
    }
}
