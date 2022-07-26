using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class Resource : MonoBehaviour
{
    public static Action<ResourceData> OnClicked = delegate { };
    public static Action<ResourceData> OnUpdate = delegate { };

    Main main;

    [SerializeField]
    string myNamedResource;
    public ResourceData myResource;
    [SerializeField]
    string[] myNamedimediateDependencies;
    List<ResourceData> myImediateDependence;
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
    public GameObject originalResourceButton;
    bool doneWithDependencyCheck = false;
    public bool isOriginalResource = false;
    public bool panelButton = false;

    public void SetUpResource(ResourceData source, bool alpha, Main theMain)
    {
        isOriginalResource = alpha;
        myResource = source;
        main = theMain;
        myNamedResource = myResource.itemName;

        SetUpDependencyLists(source);

        if (isOriginalResource != true) // setting non-alpha information
        {
            if (myText != null) myText.text = myResource.displayName;

            if (transform.GetComponent<HoverAbleResourceButton>() != null)
            {
                HoverAbleResourceButton h = transform.GetComponent<HoverAbleResourceButton>();
                h.Assignment(this, main);
                if (panelButton)  h.panelButton = true;
            }
        }
    }

    private void SetUpDependencyLists(ResourceData source)
    {
        myImediateDependence = new List<ResourceData>();

        List<string> tempNames = new List<string>();
        List<ResourceData> temp = new List<ResourceData>();
        List<int> tempIndices = new List<int>();

        //check for consumable dependencies
        string[] str = source.consumableRequirements.Split("-");
        if (source.consumableRequirements != "nothing=0")
        {
            foreach (string s in str)
            {
                string[] tAr = s.Split('=');
                temp.Add(main.FindResourceFromString(tAr[0]));
                tempIndices.Add(int.Parse(tAr[1]));
                tempNames.Add(tAr[0]);
            }
        }

        //check for nonconsumable dependencies
        str = source.nonConsumableRequirements.Split("-");
        if (source.nonConsumableRequirements != "nothing")
        {
            foreach (string s in str)
            {
                string[] tAr = s.Split('=');
                temp.Add(main.FindResourceFromString(tAr[0]));
                tempIndices.Add(1);
                tempNames.Add(tAr[0]);
            }
        }

        for (int i = 0; i < temp.Count; i++)
        {
            myImediateDependence.Add(temp[i]);
        }

        dependenciesAble = new bool[myImediateDependence.Count];
        dependencyAmounts = tempIndices.ToArray();
        myNamedimediateDependencies = tempNames.ToArray();


        if (isOriginalResource)
        {
            GetAllDependencies(temp);
        }
    }

    void GetAllDependencies(List<ResourceData> dependencies)
    {
        List<ResourceData> extendedList = dependencies;
        List<ResourceData> temp = new List<ResourceData>();
        foreach(ResourceData r in dependencies)
        {
            temp.Add(r);
        }
        List<ResourceData> dump = new List<ResourceData>();
        string[] str;

        List<string> tempDependenceListNames = new List<string>();

        while (!doneWithDependencyCheck)
        {
            if (extendedList.Count == 0)
            {
                doneWithDependencyCheck = true;
            }
            foreach (ResourceData dt in extendedList)
            {
                str = dt.consumableRequirements.Split("-");
                if (str[0] != "nothing=0")
                {
                    foreach (string s in str)
                    {
                        string[] tAr = s.Split('=');
                        ResourceData TD = main.FindResourceFromString(tAr[0]);
                        if (!temp.Contains(TD))
                        {
                            dump.Add(TD);
                            temp.Add(TD);
                            tempDependenceListNames.Add(TD.itemName);
                        }
                    }
                }

                str = dt.nonConsumableRequirements.Split("-");
                if (str[0] != "nothing")
                {
                    foreach (string s in str)
                    {
                        string[] tAr = s.Split('=');
                        ResourceData TD = main.FindResourceFromString(tAr[0]);
                        if (!temp.Contains(TD))
                        {
                            dump.Add(TD);
                            temp.Add(TD);
                            tempDependenceListNames.Add(TD.itemName);
                        }
                    }
                }
            }
            extendedList.Clear();
            foreach(ResourceData d in dump)
            {
                extendedList.Add(d);
            }
            dump.Clear();
        }

        foreach(string st in myNamedimediateDependencies)
        {
            tempDependenceListNames.Add(st);
        }
        myNamedAllDependencies = tempDependenceListNames.ToArray();

        allMyDependence = temp.ToArray();
        SetupButtonLayout();
    }

    void SetupButtonLayout()
    {
        List<Resource> deps = new List<Resource>();

        if (isOriginalResource)
        {
            Resource res = originalResourceButton.GetComponent<Resource>();

            res.SetUpResource(myResource, false, main);
            deps.Add(res);
        }

        for(int i = allMyDependence.Length-1; i > -1; i--)
        {
            GameObject Obj = Instantiate(buttonPrefab, new Vector3(pivot.transform.position.x, pivot.transform.position.y + 100f, pivot.transform.position.z), Quaternion.identity, pivot.transform);
            Resource source = Obj.GetComponent<Resource>();
            source.SetUpResource(allMyDependence[i], false, main);
            deps.Add(source);
            if (i != 0)
            {
                pivot.transform.Rotate(0f, 0f, -45f);  
                continue;
            }

            foreach (Resource r in deps)
            {
                r.ResetRotation();
            }
        }
    }

    public void ResetRotation()
    {
        transform.rotation = Quaternion.identity;
    }

    public void ClickedButton()
    {
        if(myImediateDependence.Count != 0)
        {
            if (myResource.groups == "tool" && myResource.currentAmount == myResource.atMostAmount)
            {
                return; // if I am a tool and already have it we don't need to be here
            }

            for (int i = 0; i < myImediateDependence.Count; i++) // checking amounts are enough for each dependency
            {
                if (myImediateDependence[i].currentAmount >= dependencyAmounts[i]) {
                    dependenciesAble[i] = true;
                }
                else{
                    dependenciesAble[i] = false; // if one is not able we will stop the method
                    return;
                }
            }

            for(int i = 0; i < myImediateDependence.Count; i++) // Adjusting all dependencies
            {
                if(myImediateDependence[i].groups != "tool")
                {
                    myImediateDependence[i].AdjustCurrentAmount(dependencyAmounts[i] * -1);
                    OnUpdate?.Invoke(myImediateDependence[i]);
                }
            }

            main.AddToQue(myResource, int.Parse(myResource.itemsToGain.Split("=")[1]));
            OnClicked?.Invoke(myResource);
            StartCoroutine(UpdateMyInformation());
        }
        else
        {
            if (myResource.groups == "tool" && myResource.currentAmount == myResource.atMostAmount)
            {
                return;
            }

            main.AddToQue(myResource, int.Parse(myResource.itemsToGain.Split("=")[1]));
            OnClicked?.Invoke(myResource);
            StartCoroutine(UpdateMyInformation());
        }
    }

    public ResourceData[] GetImediateDependencyNames()
    {
        return myImediateDependence.ToArray(); ;
    }

    public int[] GetDependencyAmounts()
    {
        return dependencyAmounts;
    }

    public void BecomeVisible()
    {
        myResource.AdjustVisibility(true);
        main.StartQueUpdate(myResource);

    }

    IEnumerator UpdateMyInformation()
    {
        yield return new WaitForSeconds(myResource.craftTime + 0.1f);
        OnUpdate?.Invoke(myResource);
    }
}
