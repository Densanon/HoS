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
    public static Action OnInitializeVeryFirstInteraction = delegate { };
    public static Action<Spacecraft> OnInitializeRegularFirstPlanetaryInteraction = delegate { };
    public static Action<string> OnSendPlanetLocationAsTarget = delegate { };
    public static Action OnPausePlanetFunction = delegate { };

    public enum UniverseDepth {Universe, SuperCluster, Galaxy, Nebula, GlobularCluster, StarCluster, Constellation, SolarSystem, PlanetMoon, Planet, Moon}
    public static UniverseDepth currentDepth = UniverseDepth.Universe;

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
    GeneralsContainerManager generalManager;
    [SerializeField]
    ShipContainerManager shipsManager;
    Spacecraft activeSpacecraft;

    [SerializeField]
    HexTileInfo[] tileInfoList;

    List<string[]> SheetData;
    List<string[]> LoadedData;
    List<string> itemNames;
    [SerializeField]
    ItemData[] ItemLibrary;
    ItemData[] LocationResources;
    List<string> LocationAddresses;

    public string universeAdress;
    string activePlanetAddress;

    string[] ItemNameReferenceIndex;
    int[] QuedItemAmount;

    public GameObject ResourePanelPrefab;
    public Transform ItemPanelTransform;
    public List<GameObject> itemPanelInfoPieces;

    public GameObject BuildableResourceButtonPrefab;
    public GameObject Canvas;

    public static int highestLevelOfView = 7; //0 = universe 8 = planet/moon
    public static bool canSeeIntoPlanets = false;
    public static bool isVisitedPlanet = false;
    public static bool isViewingPlanetOnly = false;
    public static bool isGettingPlanetLocation = false;
    bool isLanding = false;

    public string searchInputField = "";
    static bool needResetAllBuildables = false;

    public static bool isInitialized = false;

    WeightedRandomBag<ItemData> dropTypes = new WeightedRandomBag<ItemData>(); //Needs to be implemented for item drops

    #region Debug Values
    public static Action OnRevealTileLocations = delegate { };
    public static Action<Vector2> OnRevealTileSpecificInformation = delegate { };
    public static Action OnRevealEnemies = delegate { };
    public static Action OnDestroyLevel = delegate { };

    [Header("Debug Values")]
    [SerializeField]
    GameObject debugPanel;
    HexTileInfo debugTile;

    public static float camCancelUI = 4f;
    
    [Tooltip("Percentage to spawn on tiles.")]
    [SerializeField][Range(0f, 1.0f)]
    float spawnEnemyRatio;
    [SerializeField]
    int spawnEnemyDensityMin;
    [SerializeField]
    int spawnEnemyDensityMax;
    [SerializeField]
    int testAmounts;

    ItemData dat;
    public string debugField = "";
    int buildLevelAmount = 0;

    public static bool usingGeneral;
    public static bool cantLose;
    public static bool canSeeAllEnemies;
    public static bool needCompareForUpdatedValues;

    [SerializeField]
    TMP_Text ItemNameText;
    [SerializeField]
    TMP_Text ItemCurrentAmountText;
    [SerializeField]
    TMP_Text ItemAutoAmountText;
    [SerializeField]
    TMP_Text ItemTimeText;
    #endregion
    
    #region Debugging
    public void ToggleCanSeeInPlanets() //Allows access into a planet/moon without saving things
    {
        canSeeIntoPlanets = !canSeeIntoPlanets;
    }
    public void SetHighestLevelOfView() //Allows access to farther out zooms
    {
        highestLevelOfView = int.Parse(debugField);
    }
    public void ToTheTop() //Pulls you to the Universe Level
    {
        SetLocationDepthByInt(1);
        GenerateUniverseLocation(currentDepth, 42);
    }
    public void UpdateDebugField(string s) //Sets the universal Debug Field
    {
        debugField = s;
    }
    public void SubmitDebugSetTileActive() //Takes x,y from Universal Debug Field so set active tile
    {
        string[] ar = debugField.Split(",");
        debugTile = activeBrain.GetTile(new Vector2(float.Parse(ar[0]), float.Parse(ar[1])));
    }
    public void SubmitDebugField() //With a tile active, when done editting the highest field it attemps to access the tile item by itemName
    {
        dat = debugTile.GetItem(debugField);
        if (dat == null)
        {
            Debug.Log("Didn't get a legitamate item.");
            return;
        }
        UpdateCurrentDebugFields();
    }
    public void UpdateCurrentDebugFields() //Populates all item fields with it's values
    {
        ItemNameText.text = dat.displayName;
        ItemCurrentAmountText.text = dat.currentAmount.ToString();
        ItemAutoAmountText.text = dat.autoAmount.ToString();
        ItemTimeText.text = dat.craftTime.ToString();
    }
    public void SubmitDebugIncrease() //If active tile and some integer value in universal debug, adds to the current amount
    {
        dat.AdjustCurrentAmount(int.Parse(debugField));
        UpdateCurrentDebugFields();
    }
    public void SubmitDebugDecrease() //If active tile and some integer value with a -inFront in universal debug, subtract from the current amount
    {
        dat.AdjustCurrentAmount(int.Parse(debugField));
        UpdateCurrentDebugFields();
    }
    public void SubmitDebugSetCurrent() //If active tile and some integer value in universal debug, sets the current amount
    {
        dat.SetCurrentAmount(int.Parse(debugField));
        UpdateCurrentDebugFields();
    }
    public void SubmitDebugSetVisible() //If active tile and item and some True or False value in universal debug, sets the visibility
    {
        dat.AdjustVisibility("true" == debugField.ToLower());
    }
    public void DeleteAllSaveData() //Deletes all saved data, game reset
    {
        SaveSystem.SeriouslyDeleteAllSaveFiles();
    }
    public void ForceBuildDataFromSheet() //Use this to reforce everything to be built from the data sheet
    {
        for (int j = 0; j < SheetData.Count; j++)
        {
            Debug.Log($"Sheetdata at [1]: {SheetData[j][1]}");
            ItemLibrary[j] = new ItemData(SheetData[j][0], SheetData[j][8],
                SheetData[j][9], SheetData[j][10], SheetData[j][1], SheetData[j][2],
                SheetData[j][3], false, 0, 0, float.Parse(SheetData[j][4]), SheetData[j][5],
                SheetData[j][6], SheetData[j][7], SheetData[j][11], SheetData[j][12],
                SheetData[j][13], 0, "");

            if (SheetData[j][3] == "nothing=0" && SheetData[j][4] == "nothing")
            {
                ItemLibrary[j].AdjustVisibility(true);
            }

            if (ItemLibrary[j].groups == "tool")
            {
                string[] ra = SheetData[j][7].Split(" ");
                string[] k = ra[1].Split("=");
                ItemLibrary[j].SetAtMost(int.Parse(k[1]));
            }

            ItemNameReferenceIndex[j] = ItemLibrary[j].displayName;
        }

        CreateOrResetAllBuildableStrings();

        CreateItemPanelInfo("all", "");

        SaveItemLibrary();
        SaveSystem.SaveFile("/item_shalom");
        LoadedData.Clear();
    }
    public void RevealTileLocation() //Shows all tile vector2 positions on screen, may take a second to create all UI
    {
        OnRevealTileLocations?.Invoke();
    }
    public void DebugTileInformation() //Active Tile, in console it gives the tiles save data string in the console
    {
        string[] s = debugField.Split(",");
        Vector2 v = new Vector2(int.Parse(s[0]), int.Parse(s[1]));
        OnRevealTileSpecificInformation(v);
    }
    public void ToggleCantLose() //Prevents you or generals from losing any units
    {
        cantLose = !cantLose;
    }
    public void TestNormalizeNumbers() //Values are set in the inspector, it shows min, max and average of the set(for finding a spaceobject/enemy spread)
    {
        int average = 0;
        int min = 100;
        int max = 0;
        for(int i = 0; i < testAmounts; i++)
        {
            int j = NormalizeRandom(spawnEnemyDensityMin, spawnEnemyDensityMax);
            average += j;
            if (j < min) min = j;
            if (j > max) max = j;
        }
        Debug.Log("Min: " + min);
        Debug.Log("Max: " + max);
        Debug.Log("Average: " + average / testAmounts);
    }
    public void RevealEnemies() //Shows all enemies and their amounts in the map
    {
        OnRevealEnemies?.Invoke();
    }
    public void ChangeCameraZoomToggleUI() //Sets the distance the camera can zoom before the UI turn off
    {
        camCancelUI = float.Parse(debugField);
    }
    public void ResetLevelWithNewValues() //Actually resets the game to initial values and destroys all things
    {
        OnDestroyLevel?.Invoke();
        ResetTheInitialEncounter();
        universeAdress = universeAdress.Remove(universeAdress.Length - 2);
        foreach(Transform manager in universeTransform)
        {
            Destroy(manager.gameObject);
        }
        planetContainer.Clear();
        GenerateUniverseLocation(UniverseDepth.Planet, 0);
    }
    public void ResetTheInitialEncounter() //The rest of the reset code from above^
    {
        isInitialized = false;
        PlayerPrefs.SetInt("Initialized", 0);
        shipsManager.RemoveAllShips();
        SaveSystem.SeriouslyDeleteAllSaveFiles();
        Destroy(activeBrain.gameObject);
        activeBrain = null;
        planetContainer.Clear();
        LocationAddresses.Clear();
    }
    public void ToggleGeneral() //Turns off all generals or allows generals to be used
    {
        usingGeneral = !usingGeneral;
        generalManager.StartActiveGeneral();
    }
    public void SubmitGeneralStart() //Puts the active general at tile location set by Universal Debug Field x,y
    {
        string[] s = debugField.Split(",");
        Vector2 tile = new Vector2(float.Parse(s[0]), float.Parse(s[1]));
        generalManager.SetGeneralLocation(tile);
    }
    public void SubmitGeneralName() //Changes the name of active general set by Universal Debug Field
    {
        Debug.Log("Sending name submition.");
        generalManager.SetActiveGeneralName(debugField);
    }
    public void CreateGeneral() //Creates a general which needs to be assigned
    {
        generalManager.CreateAGeneral();
    }
    public void GenerateMapLocation() //Using the Universal Debug Field type in the universe address and it will create and take you there
    {
        //42,7,1,1,2,7,10,2,8,0  Current planet working inside of
        fromMemoryOfLocation = true;
        universeAdress = debugField;
        OnWorldMap?.Invoke(true);
        GenerateUniverseLocation(UniverseDepth.Planet, 42);
    }
    public void CreateABasicShip() //Creates a basic ship, this ship must be set with a tile to start by Universal Debug Field x,y
    {
        string[] ar = debugField.Split(",");
        Vector2 v = new Vector2(float.Parse(ar[0]), float.Parse(ar[1]));
        shipsManager.BuildABasicShip(v);
    }
    public void SetAllShipSpeeds() //Sets all ships flying delays to whatever float value set by Universal Debug Field
    {
        shipsManager.SetAllShipSpeeds(float.Parse(debugField));
    }
    #endregion

    #region Unity Methods
    void Awake()
    {
        if (PlayerPrefs.HasKey("Initialized"))
        {
            isInitialized = 1 == PlayerPrefs.GetInt("Initialized");
        }

        UnityEngine.Random.InitState(42);
        
        SheetData = SheetReader.GetSheetData();
        itemPanelInfoPieces = new List<GameObject>();

        //This will build the template we will use for all items
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

        Depthinteraction.SpaceInteractionHover += CheckForSpaceObject;
        Depthinteraction.CheckIfCanZoomToPlanetaryLevel += CheckIfWeCanZoomToPlanet;
        CameraController.OnCameraZoomContinue += SetZoomContinueTrue;
        CameraController.OnNeedZoomInfo += GoBackAStep;
        CameraController.SaveCameraState += SaveCamState;
        HexTileInfo.OnLeaving += GoBackAStep;
        Spacecraft.OnRequestingShipDestination += StartShipDestinationMode;
        Spacecraft.OnGoToPlanetForShip += GoToPlanetWithNewSpacecraft;

        DontDestroyOnLoad(this.gameObject);
    }
    private void Start()
    {
        GenerateUniverseLocation(UniverseDepth.Universe, 42); //If there is no loaded location we will start at the universe level

        LoadCamState();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            debugPanel.SetActive(!debugPanel.activeSelf);
            debugPanel.transform.SetAsLastSibling();
        }
    }
    #endregion

    #region Data Calls
    void CreateOrResetAllBuildableStrings()
    {
        for (int k = 0; k < ItemLibrary.Length; k++)
        {
            if (needResetAllBuildables) ItemLibrary[k].SetBuildablesString("");

            if (ItemLibrary[k].buildables == "")
            {
                ItemData[] res = FindBuildablesForItem(ItemLibrary[k]);
                for (int i = 0; i < res.Length; i++)
                {
                    if (i != res.Length - 1)
                    {
                        ItemLibrary[k].SetBuildablesString(ItemLibrary[k].buildables + res[i].itemName + "-");
                        continue;
                    }

                    ItemLibrary[k].SetBuildablesString(ItemLibrary[k].buildables + res[i].itemName);
                }
            }
        }
        needResetAllBuildables = false;
    }
    public static void CompareIndividualItemValues(Main main, ItemData item)
    {
        if (item == null) return;
        foreach(string[] ar in main.SheetData)
        {
            if (ar[0] == item.itemName)
            {
                Debug.Log($"{item.itemName} is gettting checked for value changes.");
                if(item.displayName != ar[8]) item.SetDisplayName(ar[8]);      
                if(item.description != ar[9]) item.SetDescription(ar[9]);              
                if(item.groups != ar[10]) item.SetGroups(ar[10]);                
                if(item.gameElementType != ar[1]) item.SetGameElementType(ar[1]);               
                if(item.consumableRequirements != ar[2])
                {
                    item.SetConsumableRequirements(ar[2]);
                    needResetAllBuildables = true;
                }
                if(item.nonConsumableRequirements != ar[3])
                {
                    item.SetNonConsumableRequirements(ar[3]);
                    needResetAllBuildables = true;
                }
                if(item.craftTime != float.Parse(ar[4])) item.SetCraftTimer(float.Parse(ar[4]));               
                if (item.itemsToGain != ar[5]) item.SetItemsToGain(ar[5]);                
                if(item.commandsOnPressed != ar[6]) item.SetCommandsOnPressed(ar[6]);                
                if (item.commandsOnCreated != ar[7]) item.SetCommandsOnCreated(ar[7]);                
                if (item.imageName != ar[11]) item.SetImageName(ar[11]);
                if (item.soundName != ar[12]) item.SetSoundName(ar[12]);
                if (item.achievement != ar[13]) item.SetAchievementName(ar[13]);
                return;
            }
        }
    }
    public ItemData FindItemFromString(string itemName)
    {
        foreach(ItemData item in ItemLibrary)
        {
            if(item.itemName == itemName)
            {
                return item;
            }
        }

        Debug.LogError($"FindItemFromString: Could not find itemName : {itemName}");
        return null;
    }
    public ItemData[] FindBuildablesForItem(ItemData item)
    {
        List<ItemData> temp = new List<ItemData>();
        foreach(ItemData rd in ItemLibrary)
        {
            if((rd.consumableRequirements != "nothing=0" &&
                CheckStringForItem(item.itemName, rd.consumableRequirements)) ||
                (rd.nonConsumableRequirements != "nothing" &&
                CheckStringForItem(item.itemName, rd.nonConsumableRequirements))) 
                temp.Add(rd);
        }
        return temp.ToArray();
    }
    bool CheckStringForItem(string itemName, string dependencyListString)
    {
        string[] checkItem = dependencyListString.Split("-");
        foreach(string s in checkItem)
        {
            string[] stAr = s.Split("=");
            if(stAr[0] == itemName)
            {
                return true;
            }
        }
        return false;
    }
    public ItemData[] FindDependenciesFromItem(ItemData item)
    {
        List<ItemData> deps = new List<ItemData>();
        string[] ar = item.consumableRequirements.Split("-");
        if(ar[0] != "nothing=0")
        {
            foreach(string s in ar)
            {
                string[] tAr = s.Split('=');
                ItemData TD = FindItemFromString(tAr[0]);
                if (!deps.Contains(TD)) deps.Add(TD);
            }
        }
        return deps.ToArray();
    }
    public int[] FindDependencyAmountsFromItem(ItemData item)
    {
        List<int> deps = new List<int>();
        string[] ar = item.consumableRequirements.Split("-");
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

    #region Item Panel
    public void CreateItemPanelInfo(string group, string type) // currently only works for the ItemLibrary
    {
        RemovePreviousPanelInformation();

        if (group == "all")
        {
            foreach (ItemData item in ItemLibrary)
            {
                CreateInfoPanel(item);
            }
            return;
        }

        if (group == "allVisible")
        {
            foreach (ItemData item in ItemLibrary)
            {
                if (item.visible) CreateInfoPanel(item);
            }
            return;
        }

        if (type == "Groups")
        {
            foreach (ItemData item in ItemLibrary)//checking group name
            {
                string[] ar = item.groups.Split(" ");
                foreach(string s in ar)
                {
                    if (s.ToLower() == group.ToLower() && item.visible)
                    {
                        CreateInfoPanel(item);
                    }
                }
            }

            if(itemPanelInfoPieces.Count == 0)
            {
                foreach (ItemData item in ItemLibrary)
                {
                    string[] ar = item.displayName.Split(" ");
                    foreach(string s in ar)
                    {
                        if(s.ToLower() == group.ToLower() && item.visible)
                        {
                            CreateInfoPanel(item);
                            break;
                        }
                    }
                }
            }
            return;
        }

        if (type == "Game Element")
        {
            foreach (ItemData item in ItemLibrary)
            {
                if (item.gameElementType == group)
                {
                    CreateInfoPanel(item);
                }
            }
        }
    }
    private void CreateInfoPanel(ItemData item)
    {
        GameObject obj = Instantiate(ResourePanelPrefab, ItemPanelTransform);
        obj.GetComponent<ResourceDisplayInfo>().Initialize(item);
        itemPanelInfoPieces.Add(obj);
    }
    public void CreateItemPanelInfo(string itemByName) 
    {
        RemovePreviousPanelInformation();
        foreach(ItemData item in ItemLibrary)
        {
            if(itemByName.ToLower() == item.displayName.ToLower())
            {
                if (item.visible)
                {
                    CreateInfoPanel(item);
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
        foreach(string s in ItemNameReferenceIndex) //check by displayName
        {
            if(s.ToLower() == searchInputField.ToLower())
            {
                CreateItemPanelInfo(searchInputField);
                return;
            }
        }

        if(searchInputField.ToLower() == "crafters")
        {
            CreateItemPanelInfo("crafters", "Game Element");
            return;
        }
        if (searchInputField.ToLower() == "*all")
        {
            CreateItemPanelInfo("all", "");
            return;
        }
        if (searchInputField.ToLower() == "all")
        {
            Debug.Log("Should be finding all visible.");
            CreateItemPanelInfo("allVisible", "");
            return;
        }

        CreateItemPanelInfo(searchInputField, "Groups");
    }
    void RemovePreviousPanelInformation()
    {
        foreach(Transform child in ItemPanelTransform)
        {
            GameObject.Destroy(child.gameObject);
        }

        itemPanelInfoPieces.Clear();
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
    public void SaveItemLibrary()
    {
        for (int i = 0; i < ItemLibrary.Length; i++)
        {
            if (i != (ItemLibrary.Length - 1))
            {
                SaveSystem.SaveResource(ItemLibrary[i], false);
                continue;
            }
            SaveSystem.SaveResource(ItemLibrary[i], true);
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
        ItemLibrary = new ItemData[SheetData.Count];
        LocationResources = new ItemData[SheetData.Count];
        ItemNameReferenceIndex = new string[SheetData.Count];
        QuedItemAmount = new int[SheetData.Count];
        LoadedData = new List<string[]>();
        itemNames = new List<string>();

        BuildLoadedData(SaveSystem.LoadFile("/item_shalom"));

        for (int j = 0; j < SheetData.Count; j++)
        {
            if (itemNames.Contains(SheetData[j][0]))
            {
                BuildItemLibraryFromMemoryAtIndex(j);

                try
                {
                    if (SheetData[j][14] == "TRUE")
                    {
                        needCompareForUpdatedValues = true;
                        CompareIndividualItemValues(this,ItemLibrary[j]);
                    }
                }
                catch (IndexOutOfRangeException e) { }

                continue;
            }

            CreateItemForLibraryAtIndex(j);

            //If it is a basic resource we need it to start visible
            if ((SheetData[j][2] == "nothing=0" && SheetData[j][3] == "nothing")|| SheetData[j][0] == "barracks") ItemLibrary[j].AdjustVisibility(true);

            //If it is a tool, we need to set it's max amount to whatever the given max amount will be
            if (ItemLibrary[j].groups == "tool")
            {
                string[] ra = SheetData[j][7].Split(" ");
                string[] k = ra[1].Split("=");
                ItemLibrary[j].SetAtMost(int.Parse(k[1]));
            }

            ItemNameReferenceIndex[j] = ItemLibrary[j].displayName;
        }

        CreateOrResetAllBuildableStrings();

        SaveItemLibrary();
        SaveSystem.SaveItemLibrary();
        LoadedData.Clear();
    }
    void BuildGenericResourceInformationFromMemory()
    {
        LoadedData = new List<string[]>();
        itemNames = new List<string>();

        BuildLoadedData(SaveSystem.LoadFile("/item_shalom"));

        List<ItemData> temp = new List<ItemData>();
        ItemLibrary = new ItemData[LoadedData.Count];
        LocationResources = new ItemData[LoadedData.Count];
        ItemNameReferenceIndex = new string[LoadedData.Count];
        QuedItemAmount = new int[LoadedData.Count];

        for (int j = 0; j < LoadedData.Count; j++)
        {
            BuildItemLibraryFromMemoryAtIndex(j);
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
    private void BuildItemLibraryFromMemoryAtIndex(int j)
    {
        ItemLibrary[j] = new ItemData(LoadedData[j][0], LoadedData[j][1], LoadedData[j][2], LoadedData[j][3], LoadedData[j][4], LoadedData[j][5], LoadedData[j][6],
                        (LoadedData[j][7] == "True") ? true : false, int.Parse(LoadedData[j][8]), int.Parse(LoadedData[j][9]), float.Parse(LoadedData[j][10]), LoadedData[j][11], LoadedData[j][12],
                        LoadedData[j][13], LoadedData[j][14], LoadedData[j][15], LoadedData[j][16], int.Parse(LoadedData[j][17]), LoadedData[j][18]);

        ItemNameReferenceIndex[j] = ItemLibrary[j].displayName;
    }
    private void CreateItemForLibraryAtIndex(int j)
    {
        //name, desc, dis, gr, eType, req, nonReq, vis, cur, autA, timer, created,
        //coms, createComs, im, snd, ach, most
        ItemLibrary[j] = new ItemData(SheetData[j][0], SheetData[j][8],
                        SheetData[j][9], SheetData[j][10], SheetData[j][1], SheetData[j][2],
                        SheetData[j][3], false, 0, 0, float.Parse(SheetData[j][4]), SheetData[j][5],
                        SheetData[j][6], SheetData[j][7], SheetData[j][11], SheetData[j][12],
                        SheetData[j][13], 0, "");
    }
    void LoadDataFromSave()
    {
        LoadedData = new List<string[]>();
        itemNames = new List<string>();

        string s = SaveSystem.LoadFile("/item_shalom");
        BuildLoadedData(s);
        
        LocationResources = new ItemData[LoadedData.Count];

        for (int j = 0; j < LoadedData.Count; j++)
        {
            LocationResources[j] = new ItemData(LoadedData[j][0], LoadedData[j][1], LoadedData[j][2], LoadedData[j][3], LoadedData[j][4], LoadedData[j][5], LoadedData[j][6],
                    (LoadedData[j][7] == "True") ? true : false, int.Parse(LoadedData[j][8]), int.Parse(LoadedData[j][9]), float.Parse(LoadedData[j][10]), LoadedData[j][11], LoadedData[j][12],
                    LoadedData[j][13], LoadedData[j][14], LoadedData[j][15], LoadedData[j][16], int.Parse(LoadedData[j][17]), LoadedData[j][18]);
        }
    }
    string[] TryLoadLevel()
    {
        string s = SaveSystem.LoadFile("/" + universeAdress);
        if(s != null) // Checking for a save of the planet
        {
            return s.Split("|");
        }

        //No planetary info was stored so build new stuff
        LoadDataFromSave();//This is here if we are offline from when we started
        return null;
    }
    #endregion
    
    #region Map
    public void GenerateUniverseLocation(UniverseDepth depth, int index)
    {
        Debug.Log("Generating Location.");
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
                SaveLocationAddressBook();
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
        Debug.Log("Generating Space Encounter.");
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
        if (buildLevelAmount > 0) return;
        bool isBrandNew = false;
        isViewingPlanetOnly = !CheckIfVisitedPlanet(); //need a better check system
        if (!isInitialized) isViewingPlanetOnly = false;
        if (!LocationAddresses.Contains(universeAdress) && (isLanding ? true : !isViewingPlanetOnly)) 
        {
            LocationAddresses.Add(universeAdress);
            isBrandNew = true;        
        }
        
        if (planetContainer == null) planetContainer = new List<LocationManager>();
        foreach (LocationManager brain in planetContainer)
        {
            if (brain.myAddress == universeAdress)
            {
                activeBrain = brain;
                if (isLanding) activeBrain.GiveATileAShip(activeSpacecraft, activeBrain.FindSuitableLandingSpace(), true);
                activeBrain.TurnOnVisibility();
                return;
            }
        }


        GameObject obj = Instantiate(planetPrefab, universeTransform);
        LocationManager Ego = obj.GetComponent<LocationManager>();
        planetContainer.Add(Ego);
        activeBrain = Ego;
        activeBrain.AssignMain(this);
        activeBrain.SetEnemyNumbers(spawnEnemyRatio, spawnEnemyDensityMin, spawnEnemyDensityMax);
        if (isBrandNew && isInitialized)
        {
            Debug.Log("Setting up first encounter.");
            activeBrain.FirstPlanetaryEncounter(activeSpacecraft);
        }
        else if (!isInitialized)
        {
            Debug.Log("Setting up very first encounter.");
            isInitialized = true;
            PlayerPrefs.SetInt("Initialized", 1);
            activeBrain.FirstPlanetaryEncounter(shipsManager.GetStarterShip());
        }
        activeBrain.BuildPlanetData(TryLoadLevel(), universeAdress,(isLanding) ? false:isViewingPlanetOnly);
        isLanding = false;

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
        areaText.text = (currentDepth != UniverseDepth.Planet) ? $"{universeAdress} : {currentDepth}" :
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
        if (isGettingPlanetLocation && currentDepth == UniverseDepth.PlanetMoon)
        {
            OnSendPlanetLocationAsTarget?.Invoke(universeAdress + $",{GetSpaceObjectIndex(obj)}");
            ReturnToPlanet();
            isGettingPlanetLocation = false;
            return;
        }
        areaText.text = "";
        StartCoroutine(ZoomWait(GetSpaceObjectIndex(obj))) ;
    }
    IEnumerator ZoomWait(int index)
    {
        if (currentDepth == UniverseDepth.PlanetMoon)
        {
            isPlanet = (index == 0);
            if (!canSeeIntoPlanets && !isVisitedPlanet && isInitialized) yield break;
        }

        yield return new WaitUntil(() => canZoomContinue);

        canZoomContinue = false;
        DeeperIntoUniverseLocationDepth();
        GenerateUniverseLocation(currentDepth, index);
    }
    private void CheckIfWeCanZoomToPlanet(GameObject obj)
    {
        isVisitedPlanet = CheckIfVisitedPlanet(GetSpaceObjectIndex(obj));
    }
    private bool CheckIfVisitedPlanet(int index)
    {
        return LocationAddresses.Contains(universeAdress + $",{index}");
    }
    private bool CheckIfVisitedPlanet()
    {
        return LocationAddresses.Contains(universeAdress);
    }
    private int GetSpaceObjectIndex(GameObject obj)
    {
        for (int i = 0; i < depthLocations.Length; i++)
        {
            if (ReferenceEquals(obj, depthLocations[i])) return i;
        }

        Debug.Log("Object wasn't in the array");
        return 0;
    }
    public void RemovePlanetBrainAndDestroy(LocationManager brain)
    {
        planetContainer.Remove(brain);
        Destroy(brain.gameObject);
    }
    private void SetZoomContinueTrue()
    {
        canZoomContinue = true;
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
    public static int NormalizeRandom(int minValue, int maxValue) //Normal distribution
    {
        //-4&17 = 12 average
        //-14&16 = 8 average

        float mean = (minValue + maxValue) / 2;
        float sigma = (maxValue - mean) ;
        int ret = Mathf.RoundToInt(UnityEngine.Random.value * sigma + mean);
        if(ret > maxValue -1) ret = maxValue -1;
        
        return ret;
    }
    public ItemData[] GetItemLibrary()
    {
        return ItemLibrary;
    }
    public LocationManager GetActiveLocation()
    {
        return activeBrain;
    }
    #endregion

    #region Ship Management
    private void StartShipDestinationMode(int access)
    {
        highestLevelOfView = access;
        isGettingPlanetLocation = true;
        activePlanetAddress = universeAdress;
        GoBackAStep();
    }
    private void ReturnToPlanet()
    {
        debugField = activePlanetAddress;
        GenerateMapLocation();
        shipsManager.TurnOnPanel();
    }
    private void GoToPlanetWithNewSpacecraft(Spacecraft ship)
    {
        activeSpacecraft = ship;
        OnPausePlanetFunction?.Invoke();
        debugField = ship.targetLocation;
        isLanding = true;
        buildLevelAmount = 0;
        GenerateMapLocation();
        activeSpacecraft.SwitchLocationToCurrent();
    }
    public void GetShipMenu()
    {
        shipsManager.TurnOnPanel();
    }
    #endregion

    #region Misc
    void ReceiveDropForExtraction(string drop)
    {
        string[] items = drop.Split("-");
        foreach(string s in items)
        {
            string[] ar = s.Split("=");
            FindItemFromString(ar[0]).AdjustCurrentAmount(int.Parse(ar[1]));
        }
    }
    public IEnumerator PushMessage(string type, string message, float delay) //Delayed Message
    {
        yield return new WaitForSeconds(delay);
        OnSendMessage?.Invoke(type, message);
    }
    public static void PushMessage(string type, string message) //Instant Universal Message System
    {
        OnSendMessage?.Invoke(type, message);
    }
    #endregion

    #region Life Cycle
    private void OnApplicationQuit()
    {
        SaveUniverseLocation();
        SaveLocationAddressBook();
    }
    #endregion
}
