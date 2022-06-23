using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class Resource : MonoBehaviour
{
    private Action<ResourceData> OnClicked = delegate { };

    Main main;

    [SerializeField]
    string myNamedresource;
    public ResourceData myResource;
    [SerializeField]
    string[] myNamedimediateDependencies;
    ResourceData[] myImediateDependence;
    [SerializeField]
    string[] myNamedAllDependencies;
    ResourceData[] allMyDependence;
    [SerializeField]
    int[] dependencyAmounts;
    [SerializeField]
    bool[] dependenciesAble;

    public TMP_Text myText;

    public GameObject pivot;
    public GameObject buttonPrefab;
    bool doneWithDependencyCheck = false;
    bool Alpha = false;

    private void OnEnable()
    {
        main = GameObject.Find("Brain").GetComponent<Main>();
    }

    public void AssignResource(ResourceData data, bool alpha)
    {
        Alpha = alpha;
        myResource = data;
        myNamedresource = myResource.itemName;
        //Debug.Log($"My Resource is: {myResource.itemName}");

        List<string> tpDs = new List<string>();

        if(data.requirements != "nothing=0")
        {
            List<ResourceData> temp = new List<ResourceData>();
            List<int> otherTemp = new List<int>();
            string[] str = data.requirements.Split("-");
            if(str[0] != "nothing=0")
            {
                foreach(string s in str)
                {
                    string[] tAr = s.Split('=');
                    //Debug.Log($"Looking for {tAr[0]}");
                    temp.Add(main.ReturnData(tAr[0]));
                    otherTemp.Add(int.Parse(tAr[1]));
                    //Debug.Log($"My imediate resource dependency is: {temp[temp.Count - 1].itemName}");
                    tpDs.Add(tAr[0]);
                }
            }

            
            myImediateDependence = temp.ToArray();
            dependenciesAble = new bool[myImediateDependence.Length];
            dependencyAmounts = otherTemp.ToArray();

            myNamedimediateDependencies = tpDs.ToArray();
            myText.text = myResource.displayName;

            if (alpha)
            {
                GetAllDependencies(temp);
            }
        }
    }

    void GetAllDependencies(List<ResourceData> dependencies)
    {
        Debug.Log(dependencies.Count);
        List<ResourceData> extendedList = dependencies;
        List<ResourceData> temp = new List<ResourceData>();
        foreach(ResourceData r in dependencies)
        {
            temp.Add(r);
        }
        //Debug.Log($"Temp has {temp.Count} for all at start.");
        List<ResourceData> dump = new List<ResourceData>();
        string[] str;

        List<string> tempDependenceListNames = new List<string>();

        while (!doneWithDependencyCheck)
        {
            //Debug.Log($"extendedList has {extendedList.Count}");
            if (extendedList.Count == 0)
            {
                //Debug.Log("I found the end of the list.");
                doneWithDependencyCheck = true;
            }
            foreach (ResourceData dt in extendedList)
            {
                //Debug.Log("Going into the foreach loop.");
                str = dt.requirements.Split("-");
                //Debug.Log($"Looking at the new string array str {str[0]}");
                if (str[0] != "nothing=0")
                {
                    foreach (string s in str)
                    {
                        //Debug.Log($"Resource:GetAllDependencyCheck:Dependency: {s}");
                        string[] tAr = s.Split('=');
                        //Debug.Log($"Resource:GetAllDependencyCheck:Dependency: {tAr[0]}");
                        ResourceData TD = main.ReturnData(tAr[0]);
                        dump.Add(TD);
                        temp.Add(TD);
                        //Debug.Log($"In extensions: {dt.itemName} : dependency {TD.itemName}");
                        tempDependenceListNames.Add(TD.itemName);
                    }
                }
            }
            //Debug.Log($"Handing over {dump.Count} to extendedList.");
            extendedList.Clear();
            foreach(ResourceData d in dump)
            {
                extendedList.Add(d);
            }
            //Debug.Log($"extendedList now has {extendedList.Count}.");
            dump.Clear();
            //Debug.Log($"extendedList now has {extendedList.Count}.");
        }

        foreach(string st in myNamedimediateDependencies)
        {
            tempDependenceListNames.Add(st);
        }
        myNamedAllDependencies = tempDependenceListNames.ToArray();

        //Debug.Log($"Temp now has {temp.Count} at the end.");
        allMyDependence = temp.ToArray();
        SetupButtonLayout();
    }

    void SetupButtonLayout()
    {
        Debug.Log("Setting up dependence UI");
        for(int i = allMyDependence.Length-1; i > 0; i--)
        {
            GameObject Obj = Instantiate(buttonPrefab, new Vector3(pivot.transform.position.x, pivot.transform.position.y + 100f, pivot.transform.position.z), Quaternion.identity, pivot.transform);
            Resource source = Obj.GetComponent<Resource>();
            source.AssignResource(allMyDependence[i], false);
            Debug.Log($"Making {source.myResource.itemName} button.");
            if(i!=1)
                pivot.transform.Rotate(0f, 0f, -45f);
        }
    }

    public void ClickedButton()
    {
        for(int i = 0; i < myImediateDependence.Length; i++)
        {
            if (myImediateDependence[i].currentAmount >= dependencyAmounts[i]) {
                dependenciesAble[i] = true;
            }
            else{
                dependenciesAble[i] = false;
                return;
            }
        }

        for(int i = 0; i < myImediateDependence.Length; i++)
        {
            myImediateDependence[i].AdjustCurrentAmount(dependencyAmounts[i] * -1);
        }
        myResource.AdjustCurrentAmount(1);

        OnClicked?.Invoke(myResource);
    }
}
