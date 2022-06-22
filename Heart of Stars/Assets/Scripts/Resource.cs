using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Resource : MonoBehaviour
{
    private Action<ResourceData> OnClicked = delegate { };

    Main main;

    [SerializeField]
    ResourceData myResource;
    [SerializeField]
    ResourceData[] myImediateDependence;
    [SerializeField]
    ResourceData[] allMyDependence;
    [SerializeField]
    int[] dependencyAmounts;
    [SerializeField]
    bool[] dependenciesAble;

    public GameObject pivot;
    public GameObject buttonPrefab;
    bool doneWithDependencyCheck = false;
    bool Alpha = false;

    private void Start()
    {
        main = GameObject.Find("Brain").GetComponent<Main>();
    }

    public void AssignResource(ResourceData data, bool alpha)
    {
        Alpha = alpha;
        myResource = data;
        Debug.Log($"My Resource is: {myResource}");

        if(data.requirements != "nothing=0")
        {
            List<ResourceData> temp = new List<ResourceData>();
            List<int> otherTemp = new List<int>();
            string[] str = data.requirements.Split("-");
            if(str[0] != "nothing=0")
            {
                foreach(string s in str)
                {
                    temp.Add(main.ReturnData(s.Remove(s.Length-2,2)));
                    otherTemp.Add(int.Parse(s.Remove(0, s.Length-1)));
                    Debug.Log($"My imediate resoure dependency is: {temp[temp.Count - 1]}");
                }
            }

            
            myImediateDependence = temp.ToArray();
            dependenciesAble = new bool[myImediateDependence.Length];
            dependencyAmounts = otherTemp.ToArray();

            if (alpha)
            {
                GetAllDependencies(temp);
            }
        }
    }

    void GetAllDependencies(List<ResourceData> dependencies)
    {
        List<ResourceData> extendedList = dependencies;
        List<ResourceData> temp = new List<ResourceData>();
        List<ResourceData> dump = new List<ResourceData>();
        string[] str;


        while (!doneWithDependencyCheck)
        {
            if (extendedList == null)
            {
                doneWithDependencyCheck = true;
            }
            foreach (ResourceData dt in extendedList)
            {
                str = dt.requirements.Split("-");
                if (str[0] != "nothing=0")
                {
                    foreach (string s in str)
                    {
                        ResourceData TD = main.ReturnData(s.Remove(s.Length - 2, 2));
                        dump.Add(TD);
                        temp.Add(TD);
                        Debug.Log($"In extensions: {dt.itemName} : dependency {TD.itemName}");
                    }
                }
            }
            extendedList = dump;
            dump.Clear();
        }

        allMyDependence = temp.ToArray();
        SetupButtonLayout();
    }

    void SetupButtonLayout()
    {
        for(int i = allMyDependence.Length-1; i > 0; i--)
        {
            GameObject Obj = Instantiate(buttonPrefab, new Vector3(0f, 115f, 0f), Quaternion.identity, pivot.transform);
            Obj.GetComponent<Resource>().AssignResource(allMyDependence[i], false);
            pivot.transform.Rotate(0f, 0f, 15f);
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
