using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class Spacecraft : MonoBehaviour
{
    public static Action<int> OnRequestingShipDestination = delegate { };
    public static Action<Spacecraft> OnGoToPlanetForShip = delegate { };
    public static Action<Vector2> OnLaunchSpaceCraft = delegate { };
    public static Action<Vector2> OnLocalLanding = delegate { };

    Main main;
    ShipContainerManager myManager;

    public enum SpaceshipType { Basic, NonBasic }
    public SpaceshipType myShipType { private set; get; }
    public string Name { private set; get; }
    public ItemData[] myItemsData { private set; get; }
    public int units {private set; get;}
    public int unitMinimum { private set; get; }
    public int troopMaximum { private set; get; }
    public int storageCount { private set; get; }
    public int storageMax { private set; get; }
    public string currentPlanetLocation { private set; get; }
    public string targetLocation { private set; get; }
    public Vector2 myTileLocation { private set; get; }
    public int myAccessLevel { private set; get; }
    public float myTravelTime { private set; get; }
    public float myTravelTimer { private set; get; }
    public bool isTraveling { private set; get; }
    bool cancel = false;
    public bool arrived = false;
    public string status;

    [SerializeField]
    TMP_Text nameText;
    [SerializeField]
    TMP_Text currentLocationText;
    [SerializeField]
    TMP_Text targetLocationText;
    [SerializeField]
    TMP_Text statusText;
    [SerializeField]
    GameObject myUIContainer;
    [SerializeField]
    Image myUIImage;
    [SerializeField]
    GameObject journeyButton;
    [SerializeField]
    GameObject cancelJourneyButton;
    [SerializeField]
    GameObject landButton;

    #region Debug
    public void GetShipInfo()
    {
        Debug.Log(DigitizeForSerialization());
    }
    public void SetShipSpeed(float speed)
    {
        myTravelTimer = speed;
    }
    #endregion

    #region UnityEngine
    private void Update()
    {
        if (isTraveling)
        {
            myTravelTime += Time.deltaTime;
            if(!cancel && myTravelTime > myTravelTimer)
            {
                isTraveling = false;
                ReadyForArrival();
            }

            if (cancel && myTravelTime > myTravelTimer)
            {
                isTraveling = false;
                ReturnedToPlanet();
            }
        }
    }
    #endregion

    #region Setup
    public void BuildShip(Main m, ShipContainerManager manager,string memory)
    {
        main = m;
        myManager = manager;
        string[] ar = memory.Split(":");
        AssignName(ar[0]);
        AssignSpacecraftType(ar[1]);
        CreateMyItems(ar[2]);
        troopMaximum = int.Parse(ar[3]);
        unitMinimum = int.Parse(ar[4]);
        storageMax = int.Parse(ar[5]);
        AssignLocation(ar[6]);
        AssignDestination(ar[7]);
        AssignTileLocation(ar[8]);
        myAccessLevel = int.Parse(ar[9]);
        myTravelTimer = float.Parse(ar[10]);
        myTravelTime = float.Parse(ar[11]);
        isTraveling = ar[12] == "True";
        cancel = ar[13] == "True";
        if (!isTraveling)
        {
            SetStatus("Waiting");
        }
        else if (isTraveling && !cancel)
        {
            SetStatus("In Transit");
        }
        else if (isTraveling && cancel)
        {
            SetStatus("Returning");
        }

        foreach(ItemData resource in myItemsData)
        {
            if(resource.itemName == "soldier")
            {
                units = resource.currentAmount;
                continue;
            }
            storageCount += resource.currentAmount;
        }
    }
    /// <summary>
    /// Builds a basic ship without any resources and the lowest access level.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <param name="curLocation"></param>
    public void BuildShip(Main m, ShipContainerManager manager, SpaceshipType type, string name, string curLocation)
    {
        AssignBasics(m, manager, type, name, curLocation);
        myItemsData = new ItemData[1];
    }
    public void BuildShip(Main m, ShipContainerManager manager, SpaceshipType type, string name, string curLocation, ItemData[] resources)
    {
        
        AssignBasics(m, manager, type, name, curLocation);
        CreateMyItems(resources);       
    }
    public void AssignMain(Main m)
    {
        main = m;
    }
    void AssignBasics(Main m, ShipContainerManager manager, SpaceshipType type, string name, string location)
    {
        main = m;
        myManager = manager;
        AssignSpacecraftType(type);
        AssignName(name);
        AssignLocation(location);
        isTraveling = false;
        SetStatus("Waiting");
        targetLocation = "";
    }
    void AssignName(string name)
    {
        if(name == "")
        {
            GetRandomName();
            return;
        }
        Name = name;
        nameText.text = name;
    }
    private void GetRandomName()
    {
        System.Random rand = new System.Random();
        int i = rand.Next(0, 8);
        string name = "";
        switch (i)
        {
            case 0:
                name = "ISS";
                break;
            case 1:
                name = "BC";
                break;
            case 2:
                name = "BS";
                break;
            case 3:
                name = "CS";
                break;
            case 4:
                name = "HMS";
                break;
            case 5:
                name = "HWSS";
                break;
            case 6:
                name = "LWSS";
                break;
            case 7:
                name = "SC";
                break;
        }
        Name = name;
        nameText.text = name;
    }
    void AssignSpacecraftType(SpaceshipType type)
    {
        switch (type)
        {
            case SpaceshipType.Basic:
                myShipType = type;
                myAccessLevel = 8;
                myTravelTimer = 60f;
                CreateBasicResourceHolders();
                unitMinimum = 5;
                troopMaximum = 20;
                storageCount = 0;
                storageMax = 160;
                break;
            case SpaceshipType.NonBasic:
                myShipType = type;
                myAccessLevel = 8;
                myTravelTimer = 50f;
                break;
        }
    }
    void AssignSpacecraftType(string memory)
    {
        switch (memory)
        {
            case "Basic":
                myShipType = SpaceshipType.Basic;
                break;
            case "NonBasic":
                myShipType = SpaceshipType.NonBasic;
                break;
        }
    }
    void AssignLocation(string location)
    {
        currentPlanetLocation = location;
        currentLocationText.text = currentPlanetLocation;
    }
    public void AssignTileLocation(Vector2 tile)
    {
        myTileLocation = tile; 
    }
    public void AssignTileLocation(string memory)
    {
        string s = memory;
        char[] charToTrim = { '(', ')'};
        s = s.Trim(charToTrim);
        string[] ar = s.Split(",");
        myTileLocation = new Vector2(float.Parse(ar[0]), float.Parse(ar[1]));
    }
    #endregion

    #region Item Management
    void CreateBasicResourceHolders()
    {
        List<ItemData> temp = new List<ItemData>();
        foreach(ItemData resource in main.GetItemLibrary())
        {
            if(resource.itemName == "soldier")
            {
                temp.Add(new ItemData(resource));
            }else if (resource.itemName == "food")
            {
                temp.Add(new ItemData(resource));
            }
        }
        myItemsData = temp.ToArray();
    }
    public void CreateMyItems(ItemData[] resources)
    {
        myItemsData = new ItemData[resources.Length];
        for(int i = 0; i < resources.Length; i++)
        {
            ItemData data = resources[i];
            myItemsData[i] = new ItemData(data);
            data.AdjustCurrentAmount(data.currentAmount);
        }
    }
    public void CreateMyItems(string memory)
    {
        List<ItemData> temp = new List<ItemData>();
        string[] ar = memory.Split(";");
        foreach (string s in ar)
        {
            string[] st = s.Split(",");
            //string name, string display, string dis, string gr, string eType, string reqs, string nonReqs, bool vis, int cur, int autoA, float craft,
            //string created, string coms, string createComs, string im, string snd, string ach, int mos, string build
            temp.Add(new ItemData(st[0], st[1], st[2], st[3], st[4], st[5], st[6],
                    (st[7] == "True") ? true : false, int.Parse(st[8]), int.Parse(st[9]),
                    float.Parse(st[10]), st[11], st[12], st[13], st[14], st[15], st[16],
                    int.Parse(st[17]), st[18]));
        }

        myItemsData = temp.ToArray();
        foreach (ItemData data in myItemsData)
        {
            if (Main.needCompareForUpdatedValues) Main.CompareIndividualItemValues(main, data);
        }
    }
    public void LoadShipWithResource(ItemData resource)
    {
        List<ItemData> temp = new List<ItemData>();
        bool alreadyHave = false;
        foreach(ItemData data in myItemsData)
        {
            temp.Add(data);
            if(data.itemName == resource.itemName)
            {
                alreadyHave = true;
                data.AdjustCurrentAmount(resource.currentAmount);
                resource.AdjustCurrentAmount(resource.currentAmount * -1);
            }
        }

        if (!alreadyHave)
        {
            temp.Add(new ItemData(resource));
            resource.AdjustCurrentAmount(resource.currentAmount * -1);
        }

        myItemsData = temp.ToArray();
    }
    public void LoadShipWithResource(string itemName, int currentAmount)
    {
        List<ItemData> temp = new List<ItemData>();
        bool alreadyHave = false;
        foreach (ItemData data in myItemsData)
        {
            if (data == null) break;
            temp.Add(data);
            if (data.itemName == itemName)
            {
                alreadyHave = true;
                if(data.itemName == "soldier")
                {
                    if(units + currentAmount <= troopMaximum)
                    {
                        units += currentAmount;
                        continue;
                    }
                    Main.PushMessage($"Ship {Name}", "You are trying to fit too many units into the ship.");
                    units = troopMaximum;
                    continue;
                }

                if(storageCount + currentAmount <= storageMax)
                {
                    data.AdjustCurrentAmount(currentAmount);
                    storageCount += currentAmount;
                }
            }

        }
        if (!alreadyHave)
        {
            ItemData data = new ItemData(main.FindItemFromString(itemName));
            if (data.itemName == "soldier")
            {
                if (units + currentAmount <= troopMaximum)
                {
                    data.SetCurrentAmount(currentAmount);
                    units += currentAmount;
                }
                else
                {
                    Main.PushMessage($"Ship {Name}", "You are trying to fit too many units into the ship.");
                    units = troopMaximum;
                    data.SetCurrentAmount(storageMax - storageCount);
                }
            }
            else
            {
                if (storageCount + currentAmount <= storageMax)
                {
                    data.SetCurrentAmount(currentAmount);
                    storageCount += currentAmount;
                }
                else
                {
                    data.SetCurrentAmount(storageMax - storageCount);
                    storageCount = storageMax;
                    Main.PushMessage($"Ship {Name}", "You have attempted to store more resources than you have capacity for.");
                }
            }
            temp.Add(data);
        }
        myItemsData = temp.ToArray();
    }
    public ItemData[] GetShipStorage()
    {
        return myItemsData;
    }
    #endregion

    #region UI Management
    public void TurnOnUI()
    {
        myUIImage.enabled = true;
        myUIContainer.SetActive(true);
    }
    public void TurnOffUI()
    {
        myUIImage.enabled = false;
        myUIContainer.SetActive(false);
    }
    public int GetUnitsOnBoard(int value)
    {
        if(value > troopMaximum)
        {
            units = troopMaximum;
            return units;
        }
        units = value;
        return units;
    }
    public void UpdateStorage()
    {
        storageCount = 0;
        foreach(ItemData data in myItemsData)
        {
            storageCount += data.currentAmount;
        }
    }
    #endregion

    #region Ship Actions
    public void SetActiveShip() //Accessed via button
    {
        myManager.SetAtiveShip(this);
    }
    public void RequestFindDestination() //Accessed via button
    {
        myManager.SetAtiveShip(this);
        myManager.TurnOffPanel();
        Main.OnSendPlanetLocationAsTarget += AssignDestination;
        OnRequestingShipDestination?.Invoke(myAccessLevel);
    }
    public void AssignDestination(string target)
    {
        targetLocation = target;
        targetLocationText.text = targetLocation;
        Main.OnSendPlanetLocationAsTarget -= AssignDestination;
    }
    public void StartJourney() //Accessed via button
    {
        if(targetLocation == "")
        {
            Main.PushMessage($"Ship {Name}", "We don't have a location to travel to.");
            return;
        }else if(units < unitMinimum)
        {
            Main.PushMessage($"Ship {Name}", $"We don't have enough units to crew the ship. We currently have {units} of {unitMinimum} needed. " +
                $"Load some of the units from the tile into the ship by accessing the ship inventory on the tile.");
            return;
        }
        if (!isTraveling)
        {
            myTravelTime = 0;
            OnLaunchSpaceCraft?.Invoke(myTileLocation);
        }
        journeyButton.SetActive(false);
        cancelJourneyButton.SetActive(true);
        cancel = false;
        isTraveling = true;
        SetStatus("In Transit");
    }
    private void ReadyForArrival()
    {
        SetStatus("Arrived");
        arrived = true;
        LandingPreparation();
    }
    public void Cancel() //Accessed via button
    {
        cancelJourneyButton.SetActive(false);
        journeyButton.SetActive(true);
        cancel = true;
        myTravelTime = myTravelTimer - myTravelTime;
    }
    private void ReturnedToPlanet()
    {
        SetStatus("Returned");
        cancel = false;
        LandingPreparation();
    }
    void LandingPreparation()
    {
        myUIImage.color = Color.green;
        cancelJourneyButton.SetActive(false);
        journeyButton.SetActive(false);
        landButton.SetActive(true);
        Main.PushMessage($"Ship {Name}", $"Ready to land on location {((statusText.text == "Arrived")?targetLocation:currentPlanetLocation)}.");
    }
    public void Land() //Accessed via button
    {
        myUIImage.color = Color.white;
        landButton.SetActive(false);
        journeyButton.SetActive(true);
        myManager.TurnOffPanel();

        if(status == "Arrived")
        {
            Debug.Log("Landing on a new planet.");
            OnGoToPlanetForShip?.Invoke(this);
            SetStatus("Waiting");
            arrived = false;
            return;
        }
        Debug.Log("Landing in a local area.");
        OnLocalLanding?.Invoke(myTileLocation);
        SetStatus("Waiting");
    }
    public void OffloadItems()
    {
        foreach(ItemData item in myItemsData)
        {
            item.SetCurrentAmount(0);
        }
        storageCount = 0;
        units = 0;
    }
    public void SwitchLocationToCurrent()
    {
        currentPlanetLocation = targetLocation;
        currentLocationText.text = currentPlanetLocation;
    }
    void SetStatus(string stat)
    {
        status = stat;
        statusText.text = stat;
    }
    #endregion

    #region Life Cycle
    public string DigitizeForSerialization()
    {
        string s = "";
        if (myItemsData.Length != 0)
        {
            foreach (ItemData item in myItemsData)
            {
                s = s + item.DigitizeForSerialization();
                if (item == myItemsData[myItemsData.Length - 1])
                {
                    s = s.Remove(s.Length - 1);
                }
            }
        }

        return $"{Name}:{myShipType}:{s}:{troopMaximum}:{unitMinimum}:{storageMax}:{currentPlanetLocation}:{targetLocation}:{myTileLocation}:{myAccessLevel}" +
            $":{myTravelTimer}:{myTravelTime}:{isTraveling}:{cancel}|";
    }
    #endregion
}