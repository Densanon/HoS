using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipContainerManager : MonoBehaviour
{
    [SerializeField]
    Main main;
    [SerializeField]
    GameObject mainContainer;
    [SerializeField]
    GameObject contentContainer;
    [SerializeField]
    GameObject shipPrefab;
    [SerializeField]
    GameObject shipsPanelButton;

    List<Spacecraft> ships;
    Spacecraft activeShip;
    LocationManager activeLocationManager;

    #region Debuging
    public void SetAllShipSpeeds(float speed)
    {
        foreach(Spacecraft ship in ships)
        {
            ship.SetShipSpeed(speed);
        }
    }
    public void AssignShipToTile(Spacecraft ship)//Accessed via button
    {
        activeLocationManager.GiveATileAShip(ship, ship.MyTileLocation, false);
    }
    public void RemoveAllShips()
    {
        foreach(Spacecraft ship in ships)
        {
            Destroy(ship.gameObject);
        }
        ships.Clear();
    }
    #endregion

    #region UnityEngine
    private void Awake()
    {
        LocationManager.OnGreetManagers += SetActiveLocationManager;
        LocationManager.OnTurnActiveManager += SetActiveLocationManager;

        ships = new List<Spacecraft>();

        TryLoadShipData();
    }
    private void Start()
    {
        mainContainer.SetActive(false);
    }
    #endregion

    #region Ship Creation
    public void BuildABasicShip(Vector2 location)
    {
        CreateAShip(Spacecraft.SpaceshipType.Basic, "");
        activeLocationManager.GiveATileAShip(activeShip, location, false);
    }
    public void CreateAShip(string memory)
    {
        GameObject go = Instantiate(shipPrefab, contentContainer.transform);
        Spacecraft sp = go.GetComponent<Spacecraft>();
        sp.BuildShip(main, this, memory);
        ships.Add(sp);
        SimilarSetup(sp);
    }
    public void CreateAShip(Spacecraft.SpaceshipType spaceshipType, string name)
    {
        GameObject go = Instantiate(shipPrefab, contentContainer.transform);
        Spacecraft sp = go.GetComponent<Spacecraft>();
        sp.BuildShip(main,this, spaceshipType, name, main.universeAdress);
        ships.Add(sp);
        SimilarSetup(sp);
        Main.PushMessage("Ship Created!", "You have a new ship that you can manage! Check out the ships menu to look at all the " +
            "ship related information.");
    }
    void SimilarSetup(Spacecraft ship)
    {
        activeShip = ship;
        ship.AssignMain(main);
        shipsPanelButton.SetActive(true);
    }
    #endregion

    #region Board Management
    public void TurnOnPanel()
    {
        mainContainer.SetActive(true);
        foreach(Spacecraft ship in ships)
        {
            ship.transform.SetParent(contentContainer.transform);
            ship.TurnOnUI();
        }
    }
    public void TurnOffPanel()
    {
        foreach (Spacecraft ship in ships)
        {
            ship.TurnOffUI();
            ship.transform.SetParent(transform);
        }
        mainContainer.SetActive(false);
    }
    #endregion

    #region Ship Management
    private void SetActiveLocationManager(LocationManager manager)
    {
        activeLocationManager = manager;
        foreach (Spacecraft ship in ships)
        {
            if (activeLocationManager.myAddress == ship.CurrentPlanetLocation) activeLocationManager.GiveATileAShip(ship, ship.MyTileLocation, false);
        }
    }
    public void SetAtiveShip(Spacecraft ship)
    {
        activeShip = ship;
    }
    #endregion

    #region Life Cycle
    public void TryLoadShipData()
    {
        string s = SaveSystem.LoadFile("/ships_Raah");
        if(s != null)
        {
            string[] ar = s.Split("|");
            foreach(string str in ar)
            {
                CreateAShip(str);
            }
        }
    }
    public void SaveShipStatus()
    {
        SaveSystem.WipeString();
        string s = "";
        foreach(Spacecraft ship in ships)
        {
            if(ReferenceEquals(ship, ships[^1]))
            {
                s += ship.DigitizeForSerialization();
                s = s.Remove(s.Length - 1);
                continue;
            }
            s += ship.DigitizeForSerialization();
        }
        SaveSystem.SaveShips(s);
    }
    private void OnApplicationQuit()
    {
        SaveShipStatus();
    }
    #endregion
}
