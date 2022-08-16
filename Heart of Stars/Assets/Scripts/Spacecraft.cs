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
    public string currentLocation { private set; get; }
    public string targetLocation { private set; get; }
    public int myAccessLevel { private set; get; }
    public float myTravelTime { private set; get; }
    public float myTravelTimer { private set; get; }
    public bool isTraveling { private set; get; }
    bool cancel = false;

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
        statusText.text = "Waiting";
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
    void AssignLocation(string location)
    {
        currentLocation = location;
        currentLocationText.text = currentLocation;
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
        Debug.Log($"Loading {currentAmount} of {itemName} in the ship.");
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
                    Debug.Log($"Troops before loading: {troops}");
                    if(troops + currentAmount <= troopMaximum)
                    {
                        troops += currentAmount;
                        Debug.Log($"Troops after loading: {troops}");
                        continue;
                    }
                    Main.PushMessage($"Ship {Name}", "You are trying to fit too many troops into the ship.");
                    troops = troopMaximum;
                    Debug.Log($"Troops after loading: {troops}");
                    continue;
                }

                Debug.Log($"Storage before loading: {storageCount}");
                if(storageCount + currentAmount <= storageMax)
                {
                    data.AdjustCurrentAmount(currentAmount);
                    storageCount += currentAmount;
                    Debug.Log($"Storage after loading: {storageCount}");   
                }
            }

        }
        if (!alreadyHave)
        {
            Debug.Log($"I didn't have {itemName}");
            ResourceData data = new ResourceData(main.FindResourceFromString(itemName));
            if (data.itemName == "soldier")
            {
                Debug.Log($"Troops before loading: {troops}");
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
                Debug.Log($"Troops after loading: {troops}");
            }
            else
            {
                Debug.Log($"Storage before loading: {storageCount}");
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
                Debug.Log($"Storage after loading: {storageCount}");
            }
            temp.Add(data);
        }
        myResources = temp.ToArray();
        foreach(ResourceData dt in myResources)
        {
            Debug.Log($"{dt.itemName} with {dt.currentAmount}");
        }
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
            Main.PushMessage($"Ship {Name}", "We don't have enough troops to crew the ship.");
            return;
        }
        myTravelTime = 0;
        cancel = false;
        isTraveling = true;
        statusText.text = "In Transit";
    }
    private void ReadyForArrival()
    {
        statusText.text = "Arrived";
        //possibly give main the spacecraft and generate landing sequence
        myUIImage.color = Color.green;
        //Allow the player to interact with it and go to the new location
        Main.PushMessage($"Ship {Name}", $"Ready to land on location {targetLocation}.");
    }
    public void GoToNewLocation()
    {
        OnGoToPlanetForShip?.Invoke(this);
    }
    public void Cancel()
    {
        cancel = true;
        myTravelTime = myTravelTimer - myTravelTime;
    }
    private void ReturnedToPlanet()
    {
        Debug.Log($"{Name} has returned.");
        statusText.text = "Returned";
        cancel = false;
        //need to express the ship has returned
        //land the ship somewhere
    }
    public void OffloadResources()
    {
        foreach(ResourceData resource in myResources)
        {
            resource.SetCurrentAmount(0);
        }
        storageCount = 0;
    }
    #endregion
}
