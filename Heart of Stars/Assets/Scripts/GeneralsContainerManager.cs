using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralsContainerManager : MonoBehaviour
{
    public static Action OnNeedTileForGeneral = delegate { };

    [SerializeField]
    Main main;
    [SerializeField]
    GameObject mainContainer;
    [SerializeField]
    GameObject contentContainer;
    [SerializeField]
    GameObject generalPrefab;
    [SerializeField]
    GameObject generalPanelButton;

    Dictionary<string, List<General>> GeneralsDictionary;
    List<General> allGenerals;
    List<General> boardGenerals;
    General activeGeneral;

    List<LocationManager> locations;
    LocationManager activeLocation;

    #region UnityEngine
    private void Awake()
    {
        LocationManager.OnGreetManagers += ReceiveNewLocationManager;
        LocationManager.OnTurnActiveManagerForGenerals += SetActiveManager;
        Main.OnWorldMap += StopAllLocationGenerals;
        HexTileInfo.OnReturnPositionToGeneralManager += SetGeneralLocation;

        GeneralsDictionary = new Dictionary<string, List<General>>();
        locations = new List<LocationManager>();
        allGenerals = new List<General>();
        boardGenerals = new List<General>();
    }
    private void Start()
    {
        activeLocation = main.GetActiveLocation();

        mainContainer.gameObject.SetActive(false);
    }
    #endregion

    #region Location Management
    void ReceiveNewLocationManager(LocationManager manager)
    {
        SetActiveManager(manager);
        if (GeneralsDictionary.ContainsKey(manager.myAddress)) return;
        locations.Add(manager);
        GeneralsDictionary.Add(manager.myAddress, new List<General>());
    }
    void SetActiveManager(LocationManager manager)
    {
        activeLocation = manager;
    }
    #endregion

    #region UIManagement
    public void TurnOffPanel()
    {
        foreach(General g in boardGenerals)
        {
            g.TurnOffUI();
            g.transform.SetParent(transform);
        }
        mainContainer.SetActive(false);
    }
    public void TurnOnPanel()
    {
        mainContainer.SetActive(true);
        foreach (General g in boardGenerals)
        {
            g.TurnOnUI();
            g.transform.SetParent(contentContainer.transform);
        }
    }
    public void WipeBoard()
    {
        foreach (General g in boardGenerals)
        {
            g.gameObject.SetActive(false);
        }
        boardGenerals.Clear();
    }
    public void ShowLocationGenerals()
    {
        WipeBoard();
        foreach(General g in GeneralsDictionary[activeLocation.myAddress])
        {
            g.gameObject.SetActive(true);
            boardGenerals.Add(g);
        }
    }
    public void ShowAllGenerals()
    {
        WipeBoard();
        foreach (General g in allGenerals)
        {
            g.gameObject.SetActive(true);
            boardGenerals.Add(g);
        }
    }
    #endregion

    #region GeneralManagement
    public void CreateAGeneral()
    {
        if (!mainContainer.activeInHierarchy) TurnOnPanel();

        GameObject go = Instantiate(generalPrefab, contentContainer.transform);
        General g = go.GetComponent<General>();
        g.BasicSetup(this, activeLocation, General.GeneralType.Basic);
        GeneralsDictionary[activeLocation.myAddress].Add(g);
        allGenerals.Add(g);
        boardGenerals.Add(g);
        Main.PushMessage("Hurray!", "You have a new general that can work for you! Give it a name and a troop location to start!");
    }
    public void AssignTileToGeneral(General general)
    {
        activeGeneral = general;
        TurnOffPanel();
        OnNeedTileForGeneral?.Invoke();
    }
    public void SetActiveGeneral(General general)
    {
        activeGeneral = general;
    }
    public void SetActiveGeneralName(string name)
    {
        activeGeneral.SetGeneralName(name);
    }
    public void SetGeneralLocation(Vector2 location)
    {
        activeGeneral.SetGeneralTroopLocation(location);
        TurnOnPanel();
    }
    public void StartAllGenerals()
    {
        foreach(General g in allGenerals)
        {
            g.ActivateGeneral();
        }
    }
    public void StartAllLocationGenerals()
    {
        foreach (General g in GeneralsDictionary[activeLocation.myAddress])
        {
            g.ActivateGeneral();
        }
    }
    public void StartActiveGeneral()
    {
        activeGeneral.ActivateGeneral();
    }
    public void StopAllGenerals()
    {
        foreach(General g in allGenerals)
        {
            g.Stop();
        }
    }
    public void StopAllLocationGenerals(bool onWorldMap)
    {
        if(activeLocation != null)
        {
            foreach (General g in GeneralsDictionary[activeLocation.myAddress])
            {
                if(!onWorldMap) g.GetTimeStamp();
                g.Stop();
            }
        }
    }
    public void StopActiveGeneral()
    {
        activeGeneral.Stop();
    }
    #endregion
}
