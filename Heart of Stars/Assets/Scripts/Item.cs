using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class Item : MonoBehaviour
{
    public static Action<ItemData> OnClicked = delegate { };
    public static Action<ItemData> OnUpdate = delegate { };

    Main main;
    HexTileInfo tile;

    public ItemData myItemData;
    List<ItemData> myImediateDependence;
    ItemData[] allMyDependence;
    [SerializeField]
    int[] dependencyAmounts;
    [SerializeField]
    bool[] dependenciesAble;

    public TMP_Text myText;

    public GameObject pivot;
    public GameObject buttonPrefab;
    public GameObject originalItemButton;
    bool doneWithDependencyCheck = false;
    public bool isOriginalResource = false;

    #region Setup
    public void SetUpItem(ItemData source, bool alpha, Main theMain)
    {
        isOriginalResource = alpha;
        myItemData = source;
        main = theMain;

        SetUpDependencyLists(source);

        if (isOriginalResource != true) // setting non-alpha information
        {
            if (myText != null) myText.text = myItemData.displayName;

            if (transform.GetComponent<HoverAbleItemButton>() != null)
            {
                HoverAbleItemButton h = transform.GetComponent<HoverAbleItemButton>();
                h.Assignment(this, main);
            }
        }
    }
    public void AssignTile(HexTileInfo hexTile)
    {
        tile = hexTile;
    }
    private void SetUpDependencyLists(ItemData source)
    {
        myImediateDependence = new List<ItemData>();

        List<string> tempNames = new List<string>();
        List<ItemData> temp = new List<ItemData>();
        List<int> tempIndices = new List<int>();

        //check for consumable dependencies
        string[] str = source.consumableRequirements.Split("-");
        if (source.consumableRequirements != "nothing=0")
        {
            foreach (string s in str)
            {
                string[] tAr = s.Split('=');
                temp.Add(main.FindItemFromString(tAr[0]));
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
                temp.Add(main.FindItemFromString(tAr[0]));
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

        for(int k = 0; k < myImediateDependence.Count; k++) //Setting to my tile resources if there are any
        {
            myImediateDependence[k] = tile.CheckIfAndUseOwnItems(myImediateDependence[k]);
        }

        if (isOriginalResource)
        {
            GetAllDependencies(temp);
        }
    }
    void GetAllDependencies(List<ItemData> dependencies)
    {
        List<ItemData> extendedList = dependencies;
        List<ItemData> temp = new List<ItemData>();
        foreach(ItemData r in dependencies)
        {
            temp.Add(r);
        }
        List<ItemData> dump = new List<ItemData>();
        string[] str;

        while (!doneWithDependencyCheck)
        {
            if (extendedList.Count == 0)
            {
                doneWithDependencyCheck = true;
            }
            foreach (ItemData dt in extendedList)
            {
                str = dt.consumableRequirements.Split("-");
                if (str[0] != "nothing=0")
                {
                    foreach (string s in str)
                    {
                        string[] tAr = s.Split('=');
                        ItemData TD = main.FindItemFromString(tAr[0]);
                        if (!temp.Contains(TD))
                        {
                            dump.Add(TD);
                            temp.Add(TD);
                        }
                    }
                }

                str = dt.nonConsumableRequirements.Split("-");
                if (str[0] != "nothing")
                {
                    foreach (string s in str)
                    {
                        string[] tAr = s.Split('=');
                        ItemData TD = main.FindItemFromString(tAr[0]);
                        if (!temp.Contains(TD))
                        {
                            dump.Add(TD);
                            temp.Add(TD);
                        }
                    }
                }
            }
            extendedList.Clear();
            foreach(ItemData d in dump)
            {
                extendedList.Add(d);
            }
            dump.Clear();
        }

        allMyDependence = temp.ToArray();
        SetupButtonLayout();
    }
    #endregion

    #region Buttons
    void SetupButtonLayout()
    {
        List<Item> temp = new List<Item>();

        if (isOriginalResource)
        {
            Item item = originalItemButton.GetComponent<Item>();

            item.SetUpItem(myItemData, false, main);
            temp.Add(item);
        }

        for(int i = allMyDependence.Length-1; i > -1; i--)
        {
            GameObject Obj = Instantiate(buttonPrefab, new Vector3(pivot.transform.position.x, pivot.transform.position.y + 100f, pivot.transform.position.z), Quaternion.identity, pivot.transform);
            Item source = Obj.GetComponent<Item>();
            source.SetUpItem(allMyDependence[i], false, main);
            temp.Add(source);
            if (i != 0)
            {
                pivot.transform.Rotate(0f, 0f, -45f);  
                continue;
            }

            foreach (Item item in temp)
            {
                item.ResetRotation();
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
            if (myItemData.groups == "tool" && myItemData.currentAmount == myItemData.atMostAmount) return; // if I am a tool and already have it we don't need to be here

            for (int i = 0; i < myImediateDependence.Count; i++) // checking amounts are enough for each dependency
            {
                if (myImediateDependence[i].currentAmount >= dependencyAmounts[i]) {
                    dependenciesAble[i] = true;
                }
                else
                {
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

            string[] ar = myItemData.itemsToGain.Split("=");           
            tile.AddToQue(tile.GetResourceByString(ar[0]), int.Parse(ar[1]));
            OnClicked?.Invoke(myItemData);
            StartCoroutine(UpdateMyInformation());
        }
        else
        {
            if (myItemData.groups == "tool" && myItemData.currentAmount == myItemData.atMostAmount) return;

            string[] ar = myItemData.itemsToGain.Split("=");
            tile.AddToQue(tile.GetResourceByString(ar[0]), int.Parse(ar[1]));
            OnClicked?.Invoke(myItemData);
            StartCoroutine(UpdateMyInformation());
        }
    }
    #endregion

    #region Dependency
    public ItemData[] GetImediateDependencyNames()
    {
        return myImediateDependence.ToArray(); ;
    }
    public int[] GetDependencyAmounts()
    {
        return dependencyAmounts;
    }
    #endregion

    #region Visibility
    public void BecomeVisible()
    {
        myItemData.AdjustVisibility(true);
        tile.StartQueUpdate(myItemData);
    }
    #endregion

    #region IEnumerators
    IEnumerator UpdateMyInformation()
    {
        yield return new WaitForSeconds(myItemData.craftTime + 0.1f);
        OnUpdate?.Invoke(myItemData);
    }
    #endregion
}
