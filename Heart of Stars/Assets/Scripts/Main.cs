using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class Main : MonoBehaviour
{
    public static Action<string, string> OnSendMessage = delegate { };
    public static Action<bool> OnWorldMap = delegate { };
    public static Action<GameObject> OnGoingToHigherLevel = delegate { };
    public static Action<string> SendCameraState = delegate { };

    public enum UniverseDepth {Universe, SuperCluster, Galaxy, Nebula, GlobularCluster, StarCluster, Constellation, SolarSystem, PlanetMoon, Planet, Moon}
    static UniverseDepth currentDepth = UniverseDepth.Universe;

    [SerializeField]
    TMP_Text areaText;

    public int landStartingPointsForSpawning;
    public float frequencyForLandSpawning;

    [SerializeField]
    GameObject[] depthPrefabs;
    [SerializeField]
    Transform universeTransform;
    [SerializeField]
    GameObject[] depthLocations;
    bool fromMemoryOfLocation = false;
    bool isPlanet = false;
    bool canZoomContinue = false;
    int objectIndex = 0;
    [SerializeField]
    GameObject planetPrefab;
    [SerializeField]
    List<LocationManager> planetContainer;
    [SerializeField]
    LocationManager activeBrain;

    [SerializeField]
    HexTileInfo[] tileInfoList;

    List<string[]> SheetData;
    List<string[]> LoadedData;
    List<string> itemNames;
    [SerializeField]
    ResourceData[] ResourceLibrary;
    ResourceData[] LocationResources;
    List<string> LocationAddresses;

    public string universeAdress;

    string[] ResourceNameReferenceIndex;
    int[] QuedResourceAmount;

    public GameObject ResourePanelPrefab;
    public Transform ResourcePanelTransform;
    public List<GameObject> resourcePanelInfoPieces;

    public GameObject BuildableResourceButtonPrefab;
    public GameObject Canvas;

    int buttonsCreated = 0;
    public string searchInputField = "";
    bool needResetAllBuildables = false;

    WeightedRandomBag<ResourceData> dropTypes = new WeightedRandomBag<ResourceData>();
    
    #region Debug Values
    [SerializeField]
    GameObject debugPanel;

    ResourceData dat;
    public string debugField = "";
    int amount;

    [SerializeField]
    TMP_Text ResourceName;
    [SerializeField]
    TMP_Text ResourceCur;
    [SerializeField]
    TMP_Text ResourceAut;
    [SerializeField]
    TMP_Text ResourceT;
    #endregion
    
    #region Debugging
    public void UpdateDebugField(string s)
    {
        debugField = s;
    }
    public void UpdateCurrentDebugFields()
    {
        ResourceName.text = dat.displayName;
        ResourceCur.text = dat.currentAmount.ToString();
        ResourceAut.text = dat.autoAmount.ToString();
        ResourceT.text = dat.craftTime.ToString();
    }
    public void SubmitDebugField()
    {
        dat = FindResourceFromString(debugField);
        UpdateCurrentDebugFields();
    }
    public void SubmitDebugIncrease()
    {
        dat.AdjustCurrentAmount(int.Parse(debugField));
        UpdateCurrentDebugFields();
    }
    public void SubmitDebugDecrease()
    {
        dat.AdjustCurrentAmount(int.Parse(debugField));
        UpdateCurrentDebugFields();
    }
    public void SubmitDebugSetCurrent()
    {
        dat.SetCurrentAmount(int.Parse(debugField));
        UpdateCurrentDebugFields();
    }
    public void SubmitDebugSetVisible()
    {
        dat.AdjustVisibility("true" == debugField.ToLower());
        Debug.Log($"Visible: {dat.visible} ");
    }
    public void ToTheTop()
    {
        SetLocationDepthByInt(1);
        GenerateUniverseLocation(currentDepth, 42);
    }
    public void DeleteAllSaveData()
    {
        SaveSystem.SeriouslyDeleteAllSaveFiles();
    }
    public void ForceBuildDataFromSheet() // used this to reforce everything to be built from the data sheet
    {
        for (int j = 0; j < SheetData.Count; j++)
        {
            Debug.Log($"Sheetdata at [1]: {SheetData[j][1]}");
            ResourceLibrary[j] = new ResourceData(SheetData[j][0], SheetData[j][8],
                SheetData[j][9], SheetData[j][10], SheetData[j][1], SheetData[j][2],
                SheetData[j][3], false, 0, 0, float.Parse(SheetData[j][4]), SheetData[j][5],
                SheetData[j][6], SheetData[j][7], SheetData[j][11], SheetData[j][12],
                SheetData[j][13], 0, "");

            if (SheetData[j][3] == "nothing=0" && SheetData[j][4] == "nothing")
            {
                ResourceLibrary[j].AdjustVisibility(true);
            }

            if (ResourceLibrary[j].groups == "tool")
            {
                string[] ra = SheetData[j][7].Split(" ");
                string[] k = ra[1].Split("=");
                ResourceLibrary[j].SetAtMost(int.Parse(k[1]));
            }

            ResourceNameReferenceIndex[j] = ResourceLibrary[j].displayName;
        }

        CreateOrResetAllBuildableStrings();

        CreateResourcePanelInfo("all", "");

        SaveResourceLibrary();
        SaveSystem.SaveFile("/resource_shalom");
        LoadedData.Clear();
    }
    #endregion

    #region Unity Methods
    void Awake()
    {
        UnityEngine.Random.InitState(42);
        
        SheetData = SheetReader.GetSheetData();
        resourcePanelInfoPieces = new List<GameObject>();

        //This will build the template we will use for planets
        if(SheetData != null)
        {
            BuildGenericResourceInformation();
        }
        else
        {
            BuildGenericResourceInformationFromMemory();
        }

        string s = SaveSystem.LoadFile("/address_nissi");
        if (s != null)
        {
            universeAdress = s;
            fromMemoryOfLocation = true;
        }

        LoadWhereWeHaveBeen();

        DontDestroyOnLoad(this.gameObject);

        Depthinteraction.SpaceInteractionHover += CheckForSpaceObject;
        CameraController.OnCameraZoomContinue += SetZoomContinueTrue;
        CameraController.OnNeedZoomInfo += GoBackAStep;
        CameraController.SaveCameraState += SaveCamState;
    }

    private void SetZoomContinueTrue()
    {
        canZoomContinue = true;
    }

    private void Start()
    {
        //Get Daily message from online
        //OnSendMessage?.Invoke("Welcome!", "Thank you for starting the game and participating in all the fun!");

        GenerateUniverseLocation(UniverseDepth.Universe, 42);

        LoadCamState();
    }

    private void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.B) && buttonsCreated == 0)
        {
            buttonsCreated++;
            GameObject obj = Instantiate(BuildableResourceButtonPrefab, Canvas.transform);
            obj.GetComponent<Resource>().SetUpResource(ResourceLibrary[UnityEngine.Random.Range(0, ResourceLibrary.Length - 1)], true, this);
        }*/

        if (Input.GetKeyDown(KeyCode.Escape)) debugPanel.SetActive(!debugPanel.activeSelf);
    }
    #endregion

    #region Qeue
    public void StartQueUpdate(ResourceData data)
    {
        Debug.Log($"I have been told to start the que for {data.displayName}");
        StartCoroutine(UpdateQue(data));
    }

    IEnumerator UpdateQue(ResourceData data)
    {
        bool addedNormal = false;
        yield return new WaitForSeconds(data.craftTime);
        if (data.visible  && (QuedResourceAmount[System.Array.IndexOf(ResourceNameReferenceIndex, data.displayName)] > 0 || data.autoAmount > 0))
        {
            Debug.Log($"Starting {data.displayName} Que Update process.");
            if (QuedResourceAmount[System.Array.IndexOf(ResourceNameReferenceIndex, data.displayName)] > 0)
            {
                QuedResourceAmount[System.Array.IndexOf(ResourceNameReferenceIndex, data.displayName)] -= 1;
                addedNormal = true;
                Debug.Log($"{data.displayName} has something in Que.");
            }

            if (addedNormal)
            {
                data.AdjustCurrentAmount(1 + data.autoAmount);
                Debug.Log($"Adding {data.displayName} auto amount and que.");
            }
            else
            {
                data.AdjustCurrentAmount(data.autoAmount);
                Debug.Log($"Adding {data.displayName} sinlge que.");
            }

            StartCoroutine(UpdateQue(data));
        }
    }

    public void AddToQue(ResourceData data, int amount)
    {
        Debug.Log($"I have been told to add {amount} to the {data.displayName} que");
        Debug.Log($"{data.displayName} before addition: {QuedResourceAmount[System.Array.IndexOf(ResourceNameReferenceIndex, data.displayName)]}");
        QuedResourceAmount[System.Array.IndexOf(ResourceNameReferenceIndex, data.displayName)] += amount;
        Debug.Log($"{data.displayName} after addition: {QuedResourceAmount[System.Array.IndexOf(ResourceNameReferenceIndex, data.displayName)]}");

        StartQueUpdate(data);
    }
    #endregion

    #region Data Calls
    void CreateOrResetAllBuildableStrings()
    {
        for (int k = 0; k < ResourceLibrary.Length; k++)
        {
            if (needResetAllBuildables) ResourceLibrary[k].SetBuildablesString("");

            if (ResourceLibrary[k].buildables == "")
            {
                ResourceData[] res = FindBuildablesForResourceData(ResourceLibrary[k]);
                for (int i = 0; i < res.Length; i++)
                {
                    if (i != res.Length - 1)
                    {
                        ResourceLibrary[k].SetBuildablesString(ResourceLibrary[k].buildables + res[i].itemName + "-");
                        continue;
                    }

                    ResourceLibrary[k].SetBuildablesString(ResourceLibrary[k].buildables + res[i].itemName);
                }
            }
        }
        needResetAllBuildables = false;
    }
    void CompareIndividualResourceValues(ResourceData data)
    {
        if (data == null) return;
        foreach(string[] ar in SheetData)
        {
            if (ar[0] == data.itemName)
            {
                if(data.displayName != ar[8]) data.SetDisplayName(ar[8]);
                
                if(data.description != ar[9]) data.SetDescription(ar[9]);
                
                if(data.groups != ar[10]) data.SetGroups(ar[10]);
                
                if(data.gameElementType != ar[1]) data.SetGameElementType(ar[1]);
                
                if(data.consumableRequirements != ar[2])
                {
                    data.SetConsumableRequirements(ar[2]);
                    needResetAllBuildables = true;
                }
                if(data.nonConsumableRequirements != ar[3])
                {
                    data.SetNonConsumableRequirements(ar[3]);
                    needResetAllBuildables = true;
                }
                if(data.craftTime != float.Parse(ar[4])) data.SetCraftTimer(float.Parse(ar[4]));
                
                if (data.itemsToGain != ar[5]) data.SetItemsToGain(ar[5]);
                
                if(data.commandsOnPressed != ar[6]) data.SetCommandsOnPressed(ar[6]);
                
                if (data.commandsOnCreated != ar[7]) data.SetCommandsOnCreated(ar[7]);
                
                if (data.imageName != ar[11]) data.SetImageName(ar[11]);

                if (data.soundName != ar[12]) data.SetSoundName(ar[12]);

                if (data.achievement != ar[13]) data.SetAchievementName(ar[13]);
                return;
            }
        }
    }
    public ResourceData FindResourceFromString(string itemName)
    {
        foreach(ResourceData data in activeBrain.myResources)
        {
            if(data.itemName == itemName)
            {
                return data;
            }
        }

        Debug.LogError($"FindResourceFromString: Could not find itemName : {itemName}");
        return null;
    }
    public ResourceData[] FindBuildablesForResourceData(ResourceData data)
    {
        List<ResourceData> temp = new List<ResourceData>();
        foreach(ResourceData rd in ResourceLibrary)
        {
            if((rd.consumableRequirements != "nothing=0" &&
                CheckStringForResource(data.itemName, rd.consumableRequirements)) ||
                (rd.nonConsumableRequirements != "nothing" &&
                CheckStringForResource(data.itemName, rd.nonConsumableRequirements))) 
                temp.Add(rd);
        }
        return temp.ToArray();
    }
    bool CheckStringForResource(string itemName, string dependencyListString)
    {
        string[] checkResource = dependencyListString.Split("-");
        foreach(string s in checkResource)
        {
            string[] stAr = s.Split("=");
            if(stAr[0] == itemName)
            {
                return true;
            }
        }
        return false;
    }
    public ResourceData[] FindDependenciesFromResourceData(ResourceData data)
    {
        List<ResourceData> deps = new List<ResourceData>();
        string[] ar = data.consumableRequirements.Split("-");
        if(ar[0] != "nothing=0")
        {
            foreach(string s in ar)
            {
                string[] tAr = s.Split('=');
                ResourceData TD = FindResourceFromString(tAr[0]);
                if (!deps.Contains(TD)) deps.Add(TD);
            }
        }
        return deps.ToArray();
    }
    public int[] FindDependencyAmountsFromResourceData(ResourceData data)
    {
        List<int> deps = new List<int>();
        string[] ar = data.consumableRequirements.Split("-");
        if (ar[0] != "nothing=0")
        {
            foreach (string s in ar)
            {
                string[] tAr = s.Split('=');
                int i = int.Parse(tAr[1]);
                if (!deps.Contains(i)) deps.Add(i);
            }
        }
        return deps.ToArray();
    }
    #endregion

    #region Resource Panel
    public void CreateResourcePanelInfo(string group, string type) // currently only works for the ResourceLibrary
    {
        RemovePreviousPanelInformation();

        if (group == "all")
        {
            foreach (ResourceData data in activeBrain.myResources)
            {
                CreateInfoPanel(data);
            }
            return;
        }

        if (group == "allVisible")
        {
            foreach (ResourceData data in activeBrain.myResources)
            {
                if (data.visible) CreateInfoPanel(data);
            }
            return;
        }

        if (type == "Groups")
        {
            foreach (ResourceData data in activeBrain.myResources)//checking group name
            {
                string[] ar = data.groups.Split(" ");
                foreach(string s in ar)
                {
                    if (s.ToLower() == group.ToLower() && data.visible)
                    {
                        CreateInfoPanel(data);
                    }
                }
            }

            if(resourcePanelInfoPieces.Count == 0)
            {
                foreach (ResourceData dt in activeBrain.myResources)
                {
                    string[] ar = dt.displayName.Split(" ");
                    foreach(string s in ar)
                    {
                        if(s.ToLower() == group.ToLower() && dt.visible)
                        {
                            CreateInfoPanel(dt);
                            break;
                        }
                    }
                }
            }
            return;
        }

        if (type == "Game Element")
        {
            foreach (ResourceData data in activeBrain.myResources)
            {
                if (data.gameElementType == group)
                {
                    CreateInfoPanel(data);
                }
            }
        }
    }
    private void CreateInfoPanel(ResourceData data)
    {
        GameObject obj = Instantiate(ResourePanelPrefab, ResourcePanelTransform);
        obj.GetComponent<ResourceDisplayInfo>().Initialize(data);
        resourcePanelInfoPieces.Add(obj);
    }
    public void CreateResourcePanelInfo(string resourceByName) 
    {
        RemovePreviousPanelInformation();
        Debug.Log($"Looking to create: {resourceByName}");
        foreach(ResourceData data in activeBrain.myResources)
        {
            if(resourceByName.ToLower() == data.displayName.ToLower())
            {
                if (data.visible)
                {
                    CreateInfoPanel(data);
                }
                return;
            }
        }
    }
    public void ChangeSearchField(string search)
    {
        searchInputField = search;
    }
    public void SubmitSearchForPanelInformation()
    {
        foreach(string s in ResourceNameReferenceIndex) //check by displayName
        {
            if(s.ToLower() == searchInputField.ToLower())
            {
                CreateResourcePanelInfo(searchInputField);
                return;
            }
        }

        if(searchInputField.ToLower() == "crafters")
        {
            CreateResourcePanelInfo("crafters", "Game Element");
            return;
        }
        if (searchInputField.ToLower() == "*all")
        {
            CreateResourcePanelInfo("all", "");
            return;
        }
        if (searchInputField.ToLower() == "all")
        {
            Debug.Log("Should be finding all visible.");
            CreateResourcePanelInfo("allVisible", "");
            return;
        }

        CreateResourcePanelInfo(searchInputField, "Groups");
    }
    void RemovePreviousPanelInformation()
    {
        foreach(Transform child in ResourcePanelTransform)
        {
            GameObject.Destroy(child.gameObject);
        }

        resourcePanelInfoPieces.Clear();
    }
    #endregion

    #region Save Load
    void SaveCamState(string state)
    {
        SaveSystem.SaveCameraSettings(state);
        SaveSystem.SaveFile("/camera_rohi");
    }
    void LoadCamState()
    {
        SendCameraState?.Invoke(SaveSystem.LoadFile("/camera_rohi"));
    }
    public void SaveResourceLibrary()
    {
        for (int i = 0; i < ResourceLibrary.Length; i++)
        {
            if (i != (ResourceLibrary.Length - 1))
            {
                SaveSystem.SaveResource(ResourceLibrary[i], false);
                continue;
            }
            SaveSystem.SaveResource(ResourceLibrary[i], true);
            Debug.Log("Finished saving the last resource to the library.");
        }
    }
    public void SaveLocationAddressBook()
    {
        SaveSystem.WipeString();
        string s = "";
        for(int i = 0; i < LocationAddresses.Count; i++)
        {
            if(i == LocationAddresses.Count-1)
            {
                s += LocationAddresses[i];
                continue;
            }
            s += LocationAddresses[i] + ";";
        }
        SaveSystem.SaveLocationList(s);
    }
    public void SaveUniverseLocation()
    {
        SaveSystem.WipeString();
        SaveSystem.SaveCurrentAddress(universeAdress);
    }
    void LoadWhereWeHaveBeen()
    {
        LocationAddresses = new List<string>();
        string s = SaveSystem.LoadFile("/Locations_Jireh");
        if(s != null)
        {
            string[] ar = s.Split(";");
            foreach(string st in ar)
            {
                LocationAddresses.Add(st);
            }
        }
    }
    void BuildGenericResourceInformation()
    {
        ResourceLibrary = new ResourceData[SheetData.Count];
        LocationResources = new ResourceData[SheetData.Count];
        ResourceNameReferenceIndex = new string[SheetData.Count];
        QuedResourceAmount = new int[SheetData.Count];
        LoadedData = new List<string[]>();
        itemNames = new List<string>();

        BuildLoadedData(SaveSystem.LoadFile("/resource_shalom"));

        for (int j = 0; j < SheetData.Count; j++)
        {
            if (itemNames.Contains(SheetData[j][0]))
            {
                BuildResourceLibraryFromMemoryAtGivenIndex(j);

                try
                {
                    if (SheetData[j][14] == "TRUE")
                        CompareIndividualResourceValues(ResourceLibrary[j]);
                }
                catch (IndexOutOfRangeException e) { }

                continue;
            }

            CreateResourceForLibraryAtIndex(j);

            //If it is a basic resource we need it to start visible
            if (SheetData[j][2] == "nothing=0" && SheetData[j][3] == "nothing") ResourceLibrary[j].AdjustVisibility(true);

            //If it is a tool, we need to set it's max amount to whatever the given max amount will be
            if (ResourceLibrary[j].groups == "tool")
            {
                string[] ra = SheetData[j][7].Split(" ");
                string[] k = ra[1].Split("=");
                ResourceLibrary[j].SetAtMost(int.Parse(k[1]));
            }

            ResourceNameReferenceIndex[j] = ResourceLibrary[j].displayName;
            StartCoroutine(UpdateQue(ResourceLibrary[j]));
        }

        CreateOrResetAllBuildableStrings();

        SaveResourceLibrary();
        SaveSystem.SaveResourceLibrary();
        LoadedData.Clear();
    }
    void BuildGenericResourceInformationFromMemory()
    {
        LoadedData = new List<string[]>();
        itemNames = new List<string>();

        BuildLoadedData(SaveSystem.LoadFile("/resource_shalom"));

        List<ResourceData> temp = new List<ResourceData>();
        ResourceLibrary = new ResourceData[LoadedData.Count];
        LocationResources = new ResourceData[LoadedData.Count];
        ResourceNameReferenceIndex = new string[LoadedData.Count];
        QuedResourceAmount = new int[LoadedData.Count];

        for (int j = 0; j < LoadedData.Count; j++)
        {
            BuildResourceLibraryFromMemoryAtGivenIndex(j);
        }

        CreateOrResetAllBuildableStrings();
        LoadedData.Clear();
    }
    private void BuildLoadedData(string fileInfo)
    {
        if (fileInfo != null)
        {
            string[] ar = fileInfo.Split(";");
            foreach (string str in ar)
            {
                string[] final = str.Split(',');
                LoadedData.Add(final);
                itemNames.Add(final[0]);
            }
        }
    }
    private void BuildLoadedData(string[] fileInfo)
    {
        if (fileInfo != null)
        {
            foreach (string str in fileInfo)
            {
                string[] final = str.Split(',');
                LoadedData.Add(final);
                itemNames.Add(final[0]);
            }
        }
    }
    private void BuildResourceLibraryFromMemoryAtGivenIndex(int j)
    {
        ResourceLibrary[j] = new ResourceData(LoadedData[j][0], LoadedData[j][1], LoadedData[j][2], LoadedData[j][3], LoadedData[j][4], LoadedData[j][5], LoadedData[j][6],
                        (LoadedData[j][7] == "True") ? true : false, int.Parse(LoadedData[j][8]), int.Parse(LoadedData[j][9]), float.Parse(LoadedData[j][10]), LoadedData[j][11], LoadedData[j][12],
                        LoadedData[j][13], LoadedData[j][14], LoadedData[j][15], LoadedData[j][16], int.Parse(LoadedData[j][17]), LoadedData[j][18]);

        ResourceNameReferenceIndex[j] = ResourceLibrary[j].displayName;
        StartCoroutine(UpdateQue(ResourceLibrary[j]));
    }
    private void CreateResourceForLibraryAtIndex(int j)
    {
        //name, desc, dis, gr, eType, req, nonReq, vis, cur, autA, timer, created,
        //coms, createComs, im, snd, ach, most
        ResourceLibrary[j] = new ResourceData(SheetData[j][0], SheetData[j][8],
                        SheetData[j][9], SheetData[j][10], SheetData[j][1], SheetData[j][2],
                        SheetData[j][3], false, 0, 0, float.Parse(SheetData[j][4]), SheetData[j][5],
                        SheetData[j][6], SheetData[j][7], SheetData[j][11], SheetData[j][12],
                        SheetData[j][13], 0, "");
    }
    void LoadAndBuildGameStats(string[] resources)
    {
        LoadedData = new List<string[]>();
        itemNames = new List<string>();

        if(resources != null)
        {
            BuildLoadedData(resources);
        }
        else
        {
            BuildLoadedData(SaveSystem.LoadFile("/resource_shalom"));
        }

        for (int j = 0; j < SheetData.Count; j++)
        {
            //Load data from previous data on drive
            if (itemNames.Contains(SheetData[j][0]))
            {
                BuildResourceLibraryFromMemoryAtGivenIndex(j);

                try
                {
                    if(SheetData[j][14] == "TRUE")
                        CompareIndividualResourceValues(LocationResources[j]);
                }
                catch (IndexOutOfRangeException e){}
                
                continue;
            }
        }

        CreateOrResetAllBuildableStrings();
    }
    void LoadDataFromSave(string[] resource)
    {
        LoadedData = new List<string[]>();
        itemNames = new List<string>();

        if(resource != null) //Checking whether the location had any data at first
        {
            BuildLoadedData(resource);
        }
        else //If not then we need to grab the universal resource list
        {
            string s = SaveSystem.LoadFile("/resource_shalom");
            BuildLoadedData(s);
        }
        
        LocationResources = new ResourceData[LoadedData.Count];

        for (int j = 0; j < LoadedData.Count; j++)
        {
            LocationResources[j] = new ResourceData(LoadedData[j][0], LoadedData[j][1], LoadedData[j][2], LoadedData[j][3], LoadedData[j][4], LoadedData[j][5], LoadedData[j][6],
                    (LoadedData[j][7] == "True") ? true : false, int.Parse(LoadedData[j][8]), int.Parse(LoadedData[j][9]), float.Parse(LoadedData[j][10]), LoadedData[j][11], LoadedData[j][12],
                    LoadedData[j][13], LoadedData[j][14], LoadedData[j][15], LoadedData[j][16], int.Parse(LoadedData[j][17]), LoadedData[j][18]);
        }
    }
    string[] TryLoadLevel()
    {
        string s = SaveSystem.LoadFile("/" + universeAdress);
        if(s != null) // Checking for a save of the planet
        {
            string[] ar = s.Split("|");
            string[] resources = ar[1].Split(";");

            LoadDataFromSave(resources);
            return ar[0].Split(";");
        }

        //No planetary info was stored so build new stuff
        if (SheetData != null)//This grabs all the resource data for this particular planet
        {
            LoadAndBuildGameStats(null);
        }
        LoadDataFromSave(null);//This is here if we are offline from when we started
        return null;
    }
    private void OnApplicationQuit()
    {
        SaveUniverseLocation();
        SaveLocationAddressBook();
    }
    #endregion
    
    #region Map
    public void GenerateUniverseLocation(UniverseDepth depth, int index)
    {
        UniverseDepth dp = depth;

        if (depthLocations != null) WipeUniversePieces();
        
        List<HexTileInfo> temp = new List<HexTileInfo>();

        if (!fromMemoryOfLocation)
        {
            if (depth == UniverseDepth.Universe)
            {
                universeAdress = $"{index}";
            }
            else
            {
                universeAdress += $",{index}";
                UnityEngine.Random.InitState(universeAdress.GetHashCode());
            }
        }
        else
        {
            Debug.Log($"UniverseAddress being set up from memory: {universeAdress}");

            UnityEngine.Random.InitState(universeAdress.GetHashCode());

            string[] ar = universeAdress.Split(",");
            SetLocationDepthByInt(ar.Length);
            dp = currentDepth;
            fromMemoryOfLocation = false;
        }

        int SpawnAmount = NormalizeRandom(-4, 17);

        switch (dp)
        {
            case UniverseDepth.Universe:
                SetUpSpaceEncounter(0, 10);
                Camera.main.transform.GetComponent<CameraController>().atUniverse = true;
                break;
            case UniverseDepth.SuperCluster:
                SetUpSpaceEncounter(1, SpawnAmount);
                break;
            case UniverseDepth.Galaxy:
                SetUpSpaceEncounter(2, SpawnAmount);
                break;
            case UniverseDepth.Nebula:
                SetUpSpaceEncounter(3, SpawnAmount);
                break;
            case UniverseDepth.GlobularCluster:
                SetUpSpaceEncounter(4, SpawnAmount);
                break;
            case UniverseDepth.StarCluster:
                SetUpSpaceEncounter(5,SpawnAmount);
                break;
            case UniverseDepth.Constellation:
                SpawnAmount = NormalizeRandom(-14, 16);
                SetUpSpaceEncounter(6, SpawnAmount);
                break;
            case UniverseDepth.SolarSystem:
                SpawnAmount = NormalizeRandom(-14, 16);
                SetUpSpaceEncounter(7, SpawnAmount);
                break;
            case UniverseDepth.PlanetMoon:
                OnWorldMap?.Invoke(true);
                SpawnAmount = NormalizeRandom(-14, 16);
                SetUpSpaceEncounter(9, 8, SpawnAmount);               
                break;
            case UniverseDepth.Planet:
                OnWorldMap?.Invoke(false);
                SetUpPlanetaryEncounter();
                break;
            case UniverseDepth.Moon:
                OnWorldMap?.Invoke(false);
                SetUpPlanetaryEncounter();
                break;
        }

        SetAreaText();
    }
    private void SetUpSpaceEncounter(int prefabIndex, int spawnAmount)
    {
        List<GameObject> objs = new List<GameObject>();
        
        if(depthPrefabs[prefabIndex] != null)
        {
            for (int i = 0; i < spawnAmount; i++)
            {
                GameObject obj = Instantiate(depthPrefabs[prefabIndex], new Vector3(UnityEngine.Random.Range(-8.3f, 8.3f), UnityEngine.Random.Range(-4.4f, 4.4f), 0f), Quaternion.identity, universeTransform);
                foreach (GameObject ob in objs) //Probably some great way to ensure this way better than what I did, but it works, I wouldn't try this with a huge list
                {
                    if ((obj.transform.position.x > ob.transform.position.x - 1.5f && obj.transform.position.x < ob.transform.position.x + 1.5f) &&
                        (obj.transform.position.y > ob.transform.position.y - 1.5f && obj.transform.position.y < ob.transform.position.y + 1.5f))
                    {
                        bool makeThrough = false;
                        while (!makeThrough) // this ensures that objects won't be stacked on each other
                        {
                            makeThrough = true;
                            foreach (GameObject j in objs)
                            {
                                while ((obj.transform.position.x > j.transform.position.x - 1.5f && obj.transform.position.x < j.transform.position.x + 1.5f) &&
                                    (obj.transform.position.y > j.transform.position.y - 1.5f && obj.transform.position.y < j.transform.position.y + 1.5f))
                                {
                                    obj.transform.localPosition = new Vector3(UnityEngine.Random.Range(-8.3f, 8.3f), UnityEngine.Random.Range(-4.4f, 4.4f), 0f);
                                    makeThrough = false;
                                }
                            }
                        }
                    }
                }
                objs.Add(obj);
            }
        }

        SetupDepthLocationsArray(objs);
    }
    private void SetUpSpaceEncounter(int planetPrefabIndex, int moonPrefabIndex,int spawnAmount)
    {
        List<GameObject> objs = new List<GameObject>();

        if (depthPrefabs[planetPrefabIndex] != null)
        {
            GameObject obj = Instantiate(depthPrefabs[planetPrefabIndex], new Vector3(0f, 0f, 0f), Quaternion.identity, universeTransform);
            obj.GetComponentInChildren<MeshCollider>().transform.localScale = new Vector3(8f, 8f, 8f);
            objs.Add(obj);
        }
        if (depthPrefabs[moonPrefabIndex] != null)
        {
            for (int i = 0; i < spawnAmount; i++)
            {
                GameObject obj = Instantiate(depthPrefabs[moonPrefabIndex], new Vector3(4f, 0f, 0f), Quaternion.identity, objs[0].transform);
                objs.Add(obj);
                objs[0].transform.Rotate(0f, 0f, 360f / spawnAmount);
            }
        }

        SetupDepthLocationsArray(objs);
    }
    private void SetUpPlanetaryEncounter()
    {
        if (!LocationAddresses.Contains(universeAdress)) LocationAddresses.Add(universeAdress);
        
        if (planetContainer == null) planetContainer = new List<LocationManager>();
        
        bool found = false;
        foreach (LocationManager brain in planetContainer)
        {
            if (brain.myAddress == universeAdress)
            {
                activeBrain = brain;
                found = true;
                activeBrain.TurnOnVisibility();
                break;
            }
        }
        if (!found)
        {
            GameObject obj = Instantiate(planetPrefab, universeTransform);
            LocationManager Ego = obj.GetComponent<LocationManager>();
            planetContainer.Add(Ego);
            activeBrain = Ego;
            string[] ar = TryLoadLevel();
            activeBrain.BuildPlanetData(ar, universeAdress, LocationResources);
        }

        #region Diamond Map
        /*float zOffSet = 0f;
        int zLast = 0;
        int zRow = 0;
        for(int x = 0; x < 19; x++)
        {
            if(x < 9)
            {
                zRow = 1;
                for (int y = zLast+1; y > 0; y--)
                {
                    GameObject obj = Instantiate(tilePrefab, new Vector3(x*0.78f, 0f, (y * -1f)+zOffSet), Quaternion.identity, universeTransform);
                    objs.Add(obj);
                    HexTileInfo tf = obj.GetComponent<HexTileInfo>();
                    tf.SetUpTileLocation(x, zRow);
                    temp.Add(tf);
                    zRow++;
                }
                zOffSet += 0.5f;
                zLast++;
            }else if(x > 8)
            {
                zRow = 1;
                for (int y = zLast+1; y > 0 ; y--)
                {
                    GameObject obj = Instantiate(tilePrefab, new Vector3(x*0.78f, 0f, (y * -1f) + zOffSet), Quaternion.identity, universeTransform);
                    objs.Add(obj);
                    HexTileInfo tf = obj.GetComponent<HexTileInfo>();
                    tf.SetUpTileLocation(x, zRow);
                    temp.Add(tf);
                    zRow++;
                }
                zOffSet -= 0.5f;
                zLast--;
            }

        }
        tileInfoList = new HexTileInfo[temp.Count];
        for (int i = 0; i < tileInfoList.Length; i++)
        {
            tileInfoList[i] = temp[i];
        }*/
        #endregion
    }
    private void SetupDepthLocationsArray(List<GameObject> objs)
    {
        depthLocations = new GameObject[objs.Count];
        for (int j = 0; j < objs.Count; j++)
        {
            depthLocations[j] = objs[j];
        }
    }
    private void SetAreaText()
    {
        if (currentDepth != UniverseDepth.Planet)
        {
            areaText.text = $"{universeAdress} : {currentDepth}";
            return;
        }

        areaText.text = $"{universeAdress} : {currentDepth} : NamedOrNot";
    }
    void WipeUniversePieces()
    {
        int j = depthLocations.Length;
        for(int i = 0; i < j; i++)
        {
            Destroy(depthLocations[i]);
        }
    }
    void CheckForSpaceObject(GameObject obj)
    {
        areaText.text = "";
        for(int i = 0; i < depthLocations.Length; i++)
        {
            if(ReferenceEquals(obj,depthLocations[i]))
            {
                if(currentDepth == UniverseDepth.PlanetMoon) isPlanet = (i == 0);

                StartCoroutine(ZoomWait(i));
                break;
            }
        }
    }
    IEnumerator ZoomWait(int index)
    {
        yield return new WaitUntil(() => canZoomContinue);
        canZoomContinue = false;
        DeeperIntoUniverseLocationDepth();
        GenerateUniverseLocation(currentDepth, index);
    }
    void DeeperIntoUniverseLocationDepth()
    {
        switch (currentDepth)
        {
            case UniverseDepth.Universe:
                currentDepth = UniverseDepth.SuperCluster;
                break;
            case UniverseDepth.SuperCluster:
                currentDepth = UniverseDepth.Galaxy;
                break;
            case UniverseDepth.Galaxy:
                currentDepth = UniverseDepth.Nebula;
                break;
            case UniverseDepth.Nebula:
                currentDepth = UniverseDepth.GlobularCluster;
                break;
            case UniverseDepth.GlobularCluster:
                currentDepth = UniverseDepth.StarCluster;
                break;
            case UniverseDepth.StarCluster:
                currentDepth = UniverseDepth.Constellation;
                break;
            case UniverseDepth.Constellation:
                currentDepth = UniverseDepth.SolarSystem;
                break;
            case UniverseDepth.SolarSystem:
                currentDepth = UniverseDepth.PlanetMoon;
                break;
            case UniverseDepth.PlanetMoon:
                currentDepth = isPlanet == true ? UniverseDepth.Planet : UniverseDepth.Moon;
                break;
        }
        areaText.text = $"{universeAdress} : {currentDepth}";
    }
    void SetLocationDepthByInt(int depth)
    {
        switch (depth)
        {
            case 1:
                UnityEngine.Random.InitState(42);
                currentDepth = UniverseDepth.Universe;
                break;
            case 2:
                currentDepth = UniverseDepth.SuperCluster;
                break;
            case 3:
                currentDepth = UniverseDepth.Galaxy;
                break;
            case 4:
                currentDepth = UniverseDepth.Nebula;
                break;
            case 5:
                currentDepth = UniverseDepth.GlobularCluster;
                break;
            case 6:
                currentDepth = UniverseDepth.StarCluster;
                break;
            case 7:
                currentDepth = UniverseDepth.Constellation;
                break;
            case 8:
                currentDepth = UniverseDepth.SolarSystem;
                break;
            case 9:
                currentDepth = UniverseDepth.PlanetMoon;
                break;
            case 10:
                string[] ar = universeAdress.Split(",");
                currentDepth = int.Parse(ar[ar.Length - 1]) != 0 ? UniverseDepth.Moon : UniverseDepth.Planet;
                break;
        }
    }
    public void GoBackAStep()
    {
        string[] ar = universeAdress.Split(",");
        int index = 42;
        if(ar.Length != 1)
        {
            SetLocationDepthByInt(ar.Length-1);
            universeAdress = "";
            for(int i = 0; i < ar.Length-1; i++)
            {
                if(i != 0)
                {
                    if (i == ar.Length - 2)
                    {
                        index = int.Parse(ar[i]);
                        continue;
                    }
                    universeAdress += "," + ar[i];
                    continue;
                }
                universeAdress += ar[i];
            }
        }

        if(index == 42) SetLocationDepthByInt(1);
        
        GenerateUniverseLocation(currentDepth, index);
        OnGoingToHigherLevel?.Invoke(depthLocations[int.Parse(ar[ar.Length - 1])]);
    }
    int NormalizeRandom(int minValue, int maxValue) //Normal distribution
    {
        //-4&17 = 12 average
        //-14&16 = 8 average

        float mean = (minValue + maxValue) / 2;
        float sigma = (maxValue - mean) ;
        int ret = Mathf.RoundToInt(UnityEngine.Random.value * sigma + mean);
        if(ret > maxValue -1) ret = maxValue -1;
        
        return ret;
    }
    #endregion

    #region Misc
    void ReceiveDropForExtraction(string drop)
    {
        string[] items = drop.Split("-");
        foreach(string s in items)
        {
            string[] ar = s.Split("=");
            FindResourceFromString(ar[0]).AdjustCurrentAmount(int.Parse(ar[1]));
        }
    }

    IEnumerator PushMessage(string type, string message)
    {
        Debug.Log("Pushing message wait.");
        yield return new WaitForSeconds(5f);
        Debug.Log("Pushing message");
        OnSendMessage?.Invoke(type, message);
    }
    #endregion
}
