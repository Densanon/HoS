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
    public SpaceshipType MyShipType { private set; get; }
    public string Name { private set; get; }
    public ItemData[] MyItemsData { private set; get; }
    public int Units {private set; get;}
    public int UnitMinimum { private set; get; }
    public int UnitMaximum { private set; get; }
    public int StorageCount { private set; get; }
    public int StorageMax { private set; get; }
    public string CurrentPlanetLocation { private set; get; }
    public string TargetLocation { private set; get; }
    public Vector2 MyTileLocation { private set; get; }
    public int MyAccessLevel { private set; get; }
    public float MyTravelTime { private set; get; }
    public float MyTravelTimer { private set; get; }
    public bool IsTraveling { private set; get; }
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
        MyTravelTimer = speed;
    }
    #endregion

    #region UnityEngine
    private void Update()
    {
        if (IsTraveling)
        {
            MyTravelTime += Time.deltaTime;
            if(!cancel && MyTravelTime > MyTravelTimer)
            {
                IsTraveling = false;
                ReadyForArrival();
            }

            if (cancel && MyTravelTime > MyTravelTimer)
            {
                IsTraveling = false;
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
        UnitMaximum = int.Parse(ar[3]);
        UnitMinimum = int.Parse(ar[4]);
        StorageMax = int.Parse(ar[5]);
        AssignLocation(ar[6]);
        AssignDestination(ar[7]);
        AssignTileLocation(ar[8]);
        MyAccessLevel = int.Parse(ar[9]);
        MyTravelTimer = float.Parse(ar[10]);
        MyTravelTime = float.Parse(ar[11]);
        IsTraveling = ar[12] == "True";
        cancel = ar[13] == "True";
        if (!IsTraveling)
        {
            SetStatus("Waiting");
        }
        else if (IsTraveling && !cancel)
        {
            SetStatus("In Transit");
        }
        else if (IsTraveling && cancel)
        {
            SetStatus("Returning");
        }

        foreach(ItemData resource in MyItemsData)
        {
            if(resource.itemName == "soldier")
            {
                Units = resource.currentAmount;
                continue;
            }
            StorageCount += resource.currentAmount;
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
        MyItemsData = new ItemData[1];
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
        IsTraveling = false;
        SetStatus("Waiting");
        TargetLocation = "";
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
        System.Random rand = new();
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
                MyShipType = type;
                MyAccessLevel = 8;
                MyTravelTimer = 60f;
                CreateBasicResourceHolders();
                UnitMinimum = 5;
                UnitMaximum = 20;
                StorageCount = 0;
                StorageMax = 160;
                break;
            case SpaceshipType.NonBasic:
                MyShipType = type;
                MyAccessLevel = 8;
                MyTravelTimer = 50f;
                break;
        }
    }
    void AssignSpacecraftType(string memory)
    {
        switch (memory)
        {
            case "Basic":
                MyShipType = SpaceshipType.Basic;
                break;
            case "NonBasic":
                MyShipType = SpaceshipType.NonBasic;
                break;
        }
    }
    void AssignLocation(string location)
    {
        CurrentPlanetLocation = location;
        currentLocationText.text = CurrentPlanetLocation;
    }
    public void AssignTileLocation(Vector2 tile)
    {
        MyTileLocation = tile; 
    }
    public void AssignTileLocation(string memory)
    {
        string s = memory;
        char[] charToTrim = { '(', ')'};
        s = s.Trim(charToTrim);
        string[] ar = s.Split(",");
        MyTileLocation = new Vector2(float.Parse(ar[0]), float.Parse(ar[1]));
    }
    #endregion

    #region Item Management
    void CreateBasicResourceHolders()
    {
        List<ItemData> temp = new();
        foreach(ItemData resource in main.GetItemLibrary("ItemLibrary"))
        {
            if(resource.itemName == "soldier")
            {
                temp.Add(new ItemData(resource));
            }else if (resource.itemName == "food")
            {
                temp.Add(new ItemData(resource));
            }
        }
        MyItemsData = temp.ToArray();
    }
    public void CreateMyItems(ItemData[] resources)
    {
        MyItemsData = new ItemData[resources.Length];
        for(int i = 0; i < resources.Length; i++)
        {
            ItemData data = resources[i];
            MyItemsData[i] = new ItemData(data);
            data.AdjustCurrentAmount(data.currentAmount);
        }
    }
    public void CreateMyItems(string memory)
    {
        List<ItemData> temp = new();
        string[] ar = memory.Split(";");
        foreach (string s in ar)
        {
            string[] st = s.Split(",");
            //string name, string display, string dis, string gr, string eType, string reqs, string nonReqs, bool vis, int cur, int autoA, float craft,
            //string created, string coms, string createComs, string im, string snd, string ach, int mos, string build
            temp.Add(new ItemData(st[0], st[1], st[2], st[3], st[4], st[5], st[6],
                    st[7] == "True", int.Parse(st[8]), int.Parse(st[9]),
                    float.Parse(st[10]), st[11], st[12], st[13], st[14], st[15], st[16],
                    int.Parse(st[17]), st[18]));
        }

        MyItemsData = temp.ToArray();
        foreach (ItemData data in MyItemsData)
        {
            if (Main.needCompareForUpdatedValues) Main.CompareIndividualItemValues(main, data);
        }
    }
    public void LoadShipWithResource(ItemData resource)
    {
        List<ItemData> temp = new();
        bool alreadyHave = false;
        foreach(ItemData data in MyItemsData)
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

        MyItemsData = temp.ToArray();
    }
    public void LoadShipWithResource(string itemName, int currentAmount)
    {
        List<ItemData> temp = new();
        bool alreadyHave = false;
        foreach (ItemData data in MyItemsData)
        {
            if (data == null) break;
            temp.Add(data);
            if (data.itemName == itemName)
            {
                alreadyHave = true;
                if(data.itemName == "soldier")
                {
                    if(Units + currentAmount <= UnitMaximum)
                    {
                        Units += currentAmount;
                        continue;
                    }
                    Main.PushMessage($"Ship {Name}", "You are trying to fit too many Units into the ship.");
                    Units = UnitMaximum;
                    continue;
                }

                if(StorageCount + currentAmount <= StorageMax)
                {
                    data.AdjustCurrentAmount(currentAmount);
                    StorageCount += currentAmount;
                }
            }

        }
        if (!alreadyHave)
        {
            ItemData data = new(main.FindItemFromString(itemName));
            if (data.itemName == "soldier")
            {
                if (Units + currentAmount <= UnitMaximum)
                {
                    data.SetCurrentAmount(currentAmount);
                    Units += currentAmount;
                }
                else
                {
                    Main.PushMessage($"Ship {Name}", "You are trying to fit too many Units into the ship.");
                    Units = UnitMaximum;
                    data.SetCurrentAmount(StorageMax - StorageCount);
                }
            }
            else
            {
                if (StorageCount + currentAmount <= StorageMax)
                {
                    data.SetCurrentAmount(currentAmount);
                    StorageCount += currentAmount;
                }
                else
                {
                    data.SetCurrentAmount(StorageMax - StorageCount);
                    StorageCount = StorageMax;
                    Main.PushMessage($"Ship {Name}", "You have attempted to store more resources than you have capacity for.");
                }
            }
            temp.Add(data);
        }
        MyItemsData = temp.ToArray();
    }
    public ItemData[] GetShipStorage()
    {
        return MyItemsData;
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
        if(value > UnitMaximum)
        {
            Units = UnitMaximum;
            return Units;
        }
        Units = value;
        return Units;
    }
    public void UpdateStorage()
    {
        StorageCount = 0;
        foreach(ItemData data in MyItemsData)
        {
            StorageCount += data.currentAmount;
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
        OnRequestingShipDestination?.Invoke(MyAccessLevel);
    }
    public void AssignDestination(string target)
    {
        TargetLocation = target;
        targetLocationText.text = TargetLocation;
        Main.OnSendPlanetLocationAsTarget -= AssignDestination;
    }
    public void StartJourney() //Accessed via button
    {
        if(TargetLocation == "")
        {
            Main.PushMessage($"Ship {Name}", "We don't have a location to travel to.");
            return;
        }else if(Units < UnitMinimum)
        {
            Main.PushMessage($"Ship {Name}", $"We don't have enough Units to crew the ship. We currently have {Units} of {UnitMinimum} needed. " +
                $"Load some of the Units from the tile into the ship by accessing the ship inventory on the tile.");
            return;
        }
        if (!IsTraveling)
        {
            MyTravelTime = 0;
            OnLaunchSpaceCraft?.Invoke(MyTileLocation);
        }
        journeyButton.SetActive(false);
        cancelJourneyButton.SetActive(true);
        cancel = false;
        IsTraveling = true;
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
        MyTravelTime = MyTravelTimer - MyTravelTime;
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
        Main.PushMessage($"Ship {Name}", $"Ready to land on location {((statusText.text == "Arrived")?TargetLocation:CurrentPlanetLocation)}.");
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
        OnLocalLanding?.Invoke(MyTileLocation);
        SetStatus("Waiting");
    }
    public void OffloadItems()
    {
        foreach(ItemData item in MyItemsData)
        {
            item.SetCurrentAmount(0);
        }
        StorageCount = 0;
        Units = 0;
    }
    public void SwitchLocationToCurrent()
    {
        CurrentPlanetLocation = TargetLocation;
        currentLocationText.text = CurrentPlanetLocation;
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
        if (MyItemsData.Length != 0)
        {
            foreach (ItemData item in MyItemsData)
            {
                s += item.DigitizeForSerialization();
                if (item == MyItemsData[^1]) s = s.Remove(s.Length - 1);
            }
        }

        return $"{Name}:{MyShipType}:{s}:{UnitMaximum}:{UnitMinimum}:{StorageMax}:{CurrentPlanetLocation}:{TargetLocation}:{MyTileLocation}:{MyAccessLevel}" +
            $":{MyTravelTimer}:{MyTravelTime}:{IsTraveling}:{cancel}|";
    }
    #endregion
}