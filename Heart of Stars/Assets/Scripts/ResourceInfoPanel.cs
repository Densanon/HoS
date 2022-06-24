using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResourceInfoPanel : MonoBehaviour
{
    Resource myResource;

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

    ResourceData[] myResourceNeeds;

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

    public void Assignment(Resource data)
    {
        myResource = data;
        myTitle_text.text = data.myResource.displayName;
        myDetails_text.text = "Jordan you need to make sure that the resourcedata has a slot for details on each item.";

        myResourceNeeds = data.GetImediateDependencyNames();
        int[] oTemp = data.GetDependencyAmounts();

        myDependencies_text.text = "";
        myDependenciesNeededAmounts_text.text = "";
        myDependenciesCurAmount_text.text = "";

        Debug.Log($"ResourceInfoPanel for {myResource.myResource.displayName} : {myResourceNeeds.Length}");

        if(myResourceNeeds != null)
        {
            for(int i = 0; i < myResourceNeeds.Length; i++)
            {
                myDependencies_text.text = myDependencies_text.text + myResourceNeeds[i].displayName + "\n";
                myDependenciesNeededAmounts_text.text = myDependenciesNeededAmounts_text.text + oTemp[i].ToString() + "\n";
                myDependenciesCurAmount_text.text = myDependenciesCurAmount_text.text + myResourceNeeds[i].currentAmount.ToString() + "\n";
            }
        }
    }

    public void UpdateInfo(ResourceData source)
    {
        if(source == myResource.myResource)
        {
            myDependenciesCurAmount_text.text = "";
            for (int i = 0; i < myResourceNeeds.Length; i++)
            {
                myDependenciesCurAmount_text.text = myDependenciesCurAmount_text.text + myResourceNeeds[i].currentAmount.ToString() + "\n";
            }
        }
    }
}
