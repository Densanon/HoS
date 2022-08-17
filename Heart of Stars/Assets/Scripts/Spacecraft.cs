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
    public ResourceData[] myResources { private set; get; }
    public int troops {private set; get;}
    public int troopMinimum { private set; get; }
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
        CreateResources(ar[2]);
        troopMaximum = int.Parse(ar[3]);
        troopMinimum = int.Parse(ar[4]);
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

        foreach(ResourceData resource in myResources)
        {
            if(resource.itemName == "soldier")
            {
                troops = resource.currentAmount;
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
        myResources = new ResourceData[1];
    }
    public void BuildShip(Main m, ShipContainerManager manager, SpaceshipType type, string name, string curLocation, ResourceData[] resources)
    {
        
        AssignBasics(m, manager, type, name, curLocation);
        CreateResources(resources);       
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
                troopMinimum = 5;
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

    #region Resource Management
    void CreateBasicResourceHolders()
    {
        List<ResourceData> temp = new List<ResourceData>();
        foreach(ResourceData resource in main.GetResourceLibrary())
        {
            if(resource.itemName == "soldier")
            {
                temp.Add(new ResourceData(resource));
            }else if (resource.itemName == "food")
            {
                temp.Add(new ResourceData(resource));
            }
        }
        myResources = temp.ToArray();
    }
    public void CreateResources(ResourceData[] resources)
    {
        myResources = new ResourceData[resources.Length];
        for(int i = 0; i < resources.Length; i++)
        {
            ResourceData data = resources[i];
            myResources[i] = new ResourceData(data);
            data.AdjustCurrentAmount(data.currentAmount);
        }
    }
    public void CreateResources(string memory)
    {
        List<ResourceData> temp = new List<ResourceData>();
        string[] ar = memory.Split(";");
        foreach (string s in ar)
        {
            string[] st = s.Split(",");
            //string name, string display, string dis, string gr, string eType, string reqs, string nonReqs, bool vis, int cur, int autoA, float craft,
            //string created, string coms, string createComs, string im, string snd, string ach, int mos, string build
            temp.Add(new ResourceData(st[0], st[1], st[2], st[3], st[4], st[5], st[6],
                    (st[7] == "True") ? true : false, int.Parse(st[8]), int.Parse(st[9]),
                    float.Parse(st[10]), st[11], st[12], st[13], st[14], st[15], st[16],
                    int.Parse(st[17]), st[18]));
        }

        myResources = temp.ToArray();
        foreach (ResourceData data in myResources)
        {
            if (Main.needCompareForUpdatedValues) Main.CompareIndividualResourceValues(main, data);
        }
    }
    public void LoadShipWithResource(ResourceData resource)
    {
        List<ResourceData> temp = new List<ResourceData>();
        bool alreadyHave = false;
        foreach(ResourceData data in myResources)
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
            temp.Add(new ResourceData(resource));
            resource.AdjustCurrentAmount(resource.currentAmount * -1);
        }

        myResources = temp.ToArray();
    }
    public void LoadShipWithResource(string itemName, int currentAmount)
    {
        List<ResourceData> temp = new List<ResourceData>();
        bool alreadyHave = false;
        foreach (ResourceData data in myResources)
        {
            if (data == null) break;
            temp.Add(data);
            if (data.itemName == itemName)
            {
                alreadyHave = true;
                if(data.itemName == "soldier")
                {
                    if(troops + currentAmount <= troopMaximum)
                    {
                        troops += currentAmount;
                        continue;
                    }
                    Main.PushMessage($"Ship {Name}", "You are trying to fit too many troops into the ship.");
                    troops = troopMaximum;
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
            ResourceData data = new ResourceData(main.FindResourceFromString(itemName));
            if (data.itemName == "soldier")
            {
                if (troops + currentAmount <= troopMaximum)
                {
                data.SetCurrentAmount(currentAmount);
                troops += currentAmount;
                }
                else
                {
                    Main.PushMessage($"Ship {Name}", "You are trying to fit too many troops into the ship.");
                    troops = troopMaximum;
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
        myResources = temp.ToArray();
    }
    public ResourceData[] GetShipStorage()
    {
        return myResources;
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
    public int GetTroopsOnBoard(int value)
    {
        if(value > troopMaximum)
        {
            troops = troopMaximum;
            return troops;
        }
        troops = value;
        return troops;
    }
    public void UpdateStorage()
    {
        storageCount = 0;
        foreach(ResourceData data in myResources)
        {
            storageCount += data.currentAmount;
        }
    }
    #endregion

    #region Ship Actions
    public void SetActiveShip()
    {
        myManager.SetAtiveShip(this);
    }
    public void RequestFindDestination()
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
    public void StartJourney()
    {
        if(targetLocation == "")
        {
            Main.PushMessage($"Ship {Name}", "We don't have a location to travel to.");
            return;
        }else if(troops < troopMinimum)
        {
            Main.PushMessage($"Ship {Name}", $"We don't have enough troops to crew the ship. We currently have {troops} of {troopMinimum} needed. " +
                $"Load some of the troops from the tile into the ship by accessing the ship inventory on the tile.");
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
    public void Cancel()
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
    public void Land()
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
    public void OffloadResources()
    {
        foreach(ResourceData resource in myResources)
        {
            resource.SetCurrentAmount(0);
        }
        storageCount = 0;
        troops = 0;
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
        if (myResources.Length != 0)
        {
            foreach (ResourceData data in myResources)
            {
                s = s + data.DigitizeForSerialization();
                if (data == myResources[myResources.Length - 1])
                {
                    s = s.Remove(s.Length - 1);
                }
            }
        }

        return $"{Name}:{myShipType}:{s}:{troopMaximum}:{troopMinimum}:{storageMax}:{currentPlanetLocation}:{targetLocation}:{myTileLocation}:{myAccessLevel}" +
            $":{myTravelTimer}:{myTravelTime}:{isTraveling}:{cancel}|";
    }
    #endregion
}