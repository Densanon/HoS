using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class Main : MonoBehaviour
{
    public static Action<string, string> OnSendMessage = delegate { };

    public enum UniverseDepth {Universe, SuperCluster, Galaxy, Nebula, GlobularCluster, StarCluster, Constellation, SolarSystem, PlanetMoon, Planet}
    UniverseDepth currentDepth = UniverseDepth.Universe;

    [SerializeField]
    GameObject[] depthPrefabs;
    [SerializeField]
    Transform universeTransform;
    GameObject[] depthLocations;
    bool fromMemoryOfLocation = false;

    List<string[]> SheetData;
    List<string[]> LoadedData;
    List<string> itemNames;
    [SerializeField]
    ResourceData[] ResourceLibrary;

    public string universeAdress;

    string[] NameReferenceIndex;
    int[] QuedAmounts;

    public GameObject ResourePanelPrefab;
    public Transform ResourcePanel;
    public List<GameObject> resourcePanelInfoPieces;

    public GameObject BuildableResourceButtonPrefab;
    public GameObject Canvas;

    int created = 0;
    public string searchField = "";
    bool resetAllBuildables = false;

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

    WeightedRandomBag<ResourceData> dropTypes = new WeightedRandomBag<ResourceData>();

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
        dat = ReturnData(debugField);
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
    #endregion

    #region Unity Methods
    void Awake()
    {
        UnityEngine.Random.InitState(42);

        //SaveSystem.SeriouslyDeleteAllSaveFiles();
        
        SheetData = SheetReader.GetSheetData();
        resourcePanelInfoPieces = new List<GameObject>();

        if(SheetData != null)
        {
            ResourceLibrary = new ResourceData[SheetData.Count];
            NameReferenceIndex = new string[SheetData.Count];
            QuedAmounts = new int[SheetData.Count];

            LoadAndBuildGameStats();
        }
        else
        {
            LoadDataFromSave();
        }

        DontDestroyOnLoad(this.gameObject);

        Depthinteraction.SpaceInteractionHover += CheckForSpaceObject;
    }

    private void Start()
    {
        //Get Daily message from online
        //OnSendMessage?.Invoke("Welcome!", "Thank you for starting the game and participating in all the fun!");

        GenerateUniverseLocation(UniverseDepth.Universe, 42);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B) && created == 0)
        {
            created++;
            GameObject obj = Instantiate(BuildableResourceButtonPrefab, Canvas.transform);
            obj.GetComponent<Resource>().AssignResource(ResourceLibrary[UnityEngine.Random.Range(0, ResourceLibrary.Length - 1)], true, this);
            //obj.GetComponent<RectTransform>().SetPositionAndRotation(new Vector3(-10f, 0f, 0f), Quaternion.identity);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SaveSystem.SeriouslyDeleteAllSaveFiles();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ForceBuildDataFromSheet();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (debugPanel.activeSelf)
            {
                debugPanel.SetActive(false);
            }
            else
            {
                debugPanel.SetActive(true);
            }
        }
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
        //Debug.Log($"Starting {data.displayName} Que Update, waiting for {data.craftTime}.");
        bool addedNormal = false;
        yield return new WaitForSeconds(data.craftTime);
        //Debug.Log($"Made it through wait time on {data.displayName}.");
        //Debug.Log($"Data visible? {data.visible}, Que > 0? {QuedAmounts[System.Array.IndexOf(NameReferenceIndex, data.displayName)]}, Auto > 0? {data.autoAmount}");
        if (data.visible  && (QuedAmounts[System.Array.IndexOf(NameReferenceIndex, data.displayName)] > 0 || data.autoAmount > 0))
        {
            Debug.Log($"Starting {data.displayName} Que Update process.");
            if (QuedAmounts[System.Array.IndexOf(NameReferenceIndex, data.displayName)] > 0)
            {
                QuedAmounts[System.Array.IndexOf(NameReferenceIndex, data.displayName)] -= 1;
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
        Debug.Log($"{data.displayName} before addition: {QuedAmounts[System.Array.IndexOf(NameReferenceIndex, data.displayName)]}");
        QuedAmounts[System.Array.IndexOf(NameReferenceIndex, data.displayName)] += amount;
        Debug.Log($"{data.displayName} after addition: {QuedAmounts[System.Array.IndexOf(NameReferenceIndex, data.displayName)]}");

        StartQueUpdate(data);
    }
    #endregion

    #region Data Calls
    public void ForceBuildDataFromSheet()
    {
        for (int j = 0; j < SheetData.Count; j++)
        {
            Debug.Log($"Sheetdata at [1]: {SheetData[j][1]}");
            ResourceLibrary[j] = new ResourceData(SheetData[j][0], SheetData[j][8],
                SheetData[j][9], SheetData[j][10], SheetData[j][1], SheetData[j][2],
                SheetData[j][3], true, 0, 0, float.Parse(SheetData[j][4]), SheetData[j][5],
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

            NameReferenceIndex[j] = ResourceLibrary[j].displayName;
        }

        CreateAllBuildableStrings();

        CreateResourcePanelInfo("all", "");

        SaveResourceLibrary();
        SaveSystem.SaveFile("/resource_shalom");
        LoadedData.Clear();
    }

    void CreateAllBuildableStrings()
    {
        for (int k = 0; k < ResourceLibrary.Length; k++)
        {
            if (resetAllBuildables)
            {
                ResourceLibrary[k].SetBuildablesString("");
            }

            if(ResourceLibrary[k].buildables == "")
            {
                Debug.Log($"{ResourceLibrary[k].itemName} needs to create a string for any buildables.");
                ResourceData[] res = ReturnMyBuildables(ResourceLibrary[k]);
                for (int i = 0; i < res.Length; i++)
                {
                    if (i != res.Length - 1)
                    {
                        ResourceLibrary[k].SetBuildablesString(ResourceLibrary[k].buildables + res[i].itemName + "-");
                    }
                    else
                    {
                        ResourceLibrary[k].SetBuildablesString(ResourceLibrary[k].buildables + res[i].itemName);
                    }
                }
            }

            resetAllBuildables = false;
        }
    }

    void CompareIndividualResourceValues(ResourceData data)
    {
        foreach(string[] ar in SheetData)
        {
            if (ar[0] == data.itemName)
            {
                if(data.displayName != ar[8])
                {
                    Debug.Log($"{data.itemName} is getting an updated display name.");
                    data.SetDisplayName(ar[8]);
                }
                if(data.description != ar[9])
                {
                    Debug.Log($"{data.itemName} is getting an updated description.");
                    data.SetDescription(ar[9]);
                }
                if(data.groups != ar[10])
                {
                    Debug.Log($"{data.itemName} is getting an updated groups.");
                    data.SetGroups(ar[10]);
                }
                if(data.gameElementType != ar[1])
                {
                    Debug.Log($"{data.itemName} is getting an updated game element type.");
                    data.SetGameElementType(ar[1]);
                }
                if(data.consumableRequirements != ar[2])
                {
                    Debug.Log($"{data.itemName} is getting an updated consumable list.");
                    data.SetConsumableRequirements(ar[2]);
                    resetAllBuildables = true;
                }
                if(data.nonConsumableRequirements != ar[3])
                {
                    Debug.Log($"{data.itemName} is getting an updated non-consumable list.");
                    data.SetNonConsumableRequirements(ar[3]);
                    resetAllBuildables = true;
                }
                if(data.craftTime != float.Parse(ar[4]))
                {
                    Debug.Log($"{data.itemName} is getting an updated craft time.");
                    data.SetCraftTimer(float.Parse(ar[4]));
                }
                if (data.itemsToGain != ar[5])
                {
                    Debug.Log($"{data.itemName} is getting an updated items to gain list.");
                    data.SetItemsToGain(ar[5]);
                }
                if(data.commandsOnPressed != ar[6])
                {
                    Debug.Log($"{data.itemName} is getting an updated commands on pressed list.");
                    data.SetCommandsOnPressed(ar[6]);
                }
                if (data.commandsOnCreated != ar[7])
                {
                    Debug.Log($"{data.itemName} is getting an updated commands on created list.");
                    data.SetCommandsOnCreated(ar[7]);
                }
                if (data.imageName != ar[11])
                {
                    Debug.Log($"{data.itemName} is getting an updated image name.");
                    data.SetImageName(ar[11]);
                }
                if (data.soundName != ar[12])
                {
                    Debug.Log($"{data.itemName} is getting an updated sound name.");
                    data.SetSoundName(ar[12]);
                }
                if (data.achievement != ar[13])
                {
                    Debug.Log($"{data.itemName} is getting an updated acheivement string.");
                    data.SetAchievementName(ar[13]);
                }
            }
        }
    }

    public ResourceData ReturnData(string itemName)
    {
        foreach(ResourceData data in ResourceLibrary)
        {
            if(data.itemName == itemName)
            {
                return data;
            }
        }

        Debug.LogError($"ReturnData: Could not find itemName : {itemName}");
        return null;
    }

    public ResourceData[] ReturnMyBuildables(ResourceData data)
    {
        List<ResourceData> temp = new List<ResourceData>();
        foreach(ResourceData rd in ResourceLibrary)
        {
            if(rd.consumableRequirements != "nothing=0")
            {
                if(CheckStringForResource(data.itemName, rd.consumableRequirements))
                {
                    temp.Add(rd);
                }
              
            }
            if (rd.nonConsumableRequirements != "nothing")
            {
                if (CheckStringForResource(data.itemName, rd.nonConsumableRequirements))
                {
                    temp.Add(rd);
                }
            }
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

    public ResourceData[] ReturnDependencies(ResourceData data)
    {
        List<ResourceData> deps = new List<ResourceData>();
        string[] ar = data.consumableRequirements.Split("-");
        if(ar[0] != "nothing=0")
        {
            foreach(string s in ar)
            {
                string[] tAr = s.Split('=');
                ResourceData TD = ReturnData(tAr[0]);
                if (!deps.Contains(TD))
                {
                    deps.Add(TD);
                }
            }
        }

        return deps.ToArray();
    }

    public int[] ReturnDependencyAmounts(ResourceData data)
    {
        List<int> deps = new List<int>();
        string[] ar = data.consumableRequirements.Split("-");
        if (ar[0] != "nothing=0")
        {
            foreach (string s in ar)
            {
                string[] tAr = s.Split('=');
                int i = int.Parse(tAr[1]);
                if (!deps.Contains(i))
                {
                    deps.Add(i);
                }
            }
        }

        return deps.ToArray();
    }
    #endregion

    #region Resource Panel
    public void CreateResourcePanelInfo(string group, string type)
    {
        RemovePreviousPanelInformation();

        if (group == "all")
        {
            foreach (ResourceData data in ResourceLibrary)
            {
                GameObject obj = Instantiate(ResourePanelPrefab, ResourcePanel);
                obj.GetComponent<ResourceDisplayInfo>().Initialize(data);
                resourcePanelInfoPieces.Add(obj);
            }
            return;
        }

        if (group == "allVisible")
        {
            foreach (ResourceData data in ResourceLibrary)
            {
                if (data.visible)
                {
                    GameObject obj = Instantiate(ResourePanelPrefab, ResourcePanel);
                    obj.GetComponent<ResourceDisplayInfo>().Initialize(data);
                    resourcePanelInfoPieces.Add(obj);
                }
            }
            return;
        }

        if (type == "Groups")
        {

            foreach (ResourceData data in ResourceLibrary)
            {
                string[] ar = data.groups.Split(" ");
                foreach(string s in ar)
                {
                    if (s.ToLower() == group.ToLower() && data.visible)
                    {
                        GameObject obj = Instantiate(ResourePanelPrefab, ResourcePanel);
                        obj.GetComponent<ResourceDisplayInfo>().Initialize(data);
                        resourcePanelInfoPieces.Add(obj);
                    }
                }
            }

            if(resourcePanelInfoPieces.Count == 0)
            {
                foreach (ResourceData dt in ResourceLibrary)
                {
                    string[] ar = dt.displayName.Split(" ");
                    foreach(string s in ar)
                    {
                        if(s.ToLower() == group.ToLower() && dt.visible)
                        {
                            GameObject obj = Instantiate(ResourePanelPrefab, ResourcePanel);
                            obj.GetComponent<ResourceDisplayInfo>().Initialize(dt);
                            resourcePanelInfoPieces.Add(obj);
                            break;
                        }
                    }
                }
            }

            return;
        }

        if (type == "Game Element")
        {
            foreach (ResourceData data in ResourceLibrary)
            {
                if (data.gameElementType == group)
                {
                    GameObject obj = Instantiate(ResourePanelPrefab, ResourcePanel);
                    obj.GetComponent<ResourceDisplayInfo>().Initialize(data);
                    resourcePanelInfoPieces.Add(obj);
                }
            }
        }
    }

    public void CreateResourcePanelInfo(string resourceByName)
    {
        RemovePreviousPanelInformation();

        foreach(ResourceData data in ResourceLibrary)
        {
            if(resourceByName.ToLower() == data.displayName.ToLower())
            {
                if (data.visible)
                {
                    GameObject obj = Instantiate(ResourePanelPrefab, ResourcePanel);
                    obj.GetComponent<ResourceDisplayInfo>().Initialize(data);
                    resourcePanelInfoPieces.Add(obj);
                }
                return;
            }
        }
    }

    public void ChangeSearchField(string search)
    {
        searchField = search;
    }

    public void SubmitSearchForPanelInformation()
    {
        foreach(string s in NameReferenceIndex)
        {
            if(s.ToLower() == searchField.ToLower())
            {
                CreateResourcePanelInfo(searchField);
                return;
            }
        }

        if(searchField.ToLower() == "crafters")
        {
            CreateResourcePanelInfo("crafters", "Game Element");
            return;
        }
        if (searchField.ToLower() == "all")
        {
            CreateResourcePanelInfo("all", "");
            return;
        }
        if (searchField.ToLower() == "allVisible")
        {
            CreateResourcePanelInfo("allVisible", "");
            return;
        }

        CreateResourcePanelInfo(searchField, "Groups");
    }

    void RemovePreviousPanelInformation()
    {
        foreach(Transform child in ResourcePanel)
        {
            GameObject.Destroy(child.gameObject);
        }

        resourcePanelInfoPieces.Clear();
    }
    #endregion

    #region Save Load
    public void SaveResourceLibrary()
    {
        for (int i = 0; i < ResourceLibrary.Length; i++)
        {
            if (i != (ResourceLibrary.Length - 1))
            {
                SaveSystem.SaveResource(ResourceLibrary[i], false);
            }
            else
            {
                SaveSystem.SaveResource(ResourceLibrary[i], true);

            }
        }
    }

    public void SaveUniverseLocation()
    {
        SaveSystem.WipeString();
        SaveSystem.SaveAddress(universeAdress);
        SaveSystem.SaveFile("/address_nissi");
    }

    void LoadAndBuildGameStats()
    {
        LoadedData = new List<string[]>();
        itemNames = new List<string>();

        string s = SaveSystem.LoadFile("/resource_shalom");
        if(s != null)
        {
            string[] ar = s.Split(';');
            foreach(string str in ar)
            {
                string[] final = str.Split(',');
                LoadedData.Add(final);
            }

            for (int i = 0; i < LoadedData.Count; i++)
            {
                itemNames.Add(LoadedData[i][0]);
            }
        }

        List<ResourceData> temp = new List<ResourceData>();

        for (int j = 0; j < SheetData.Count; j++)
        {
            

            //Load data from previous data on drive
            if (itemNames.Contains(SheetData[j][0]))
            {
                ResourceLibrary[j] = new ResourceData(LoadedData[j][0], LoadedData[j][1], LoadedData[j][2], LoadedData[j][3], LoadedData[j][4], LoadedData[j][5], LoadedData[j][6],
                (LoadedData[j][7] == "True") ? true : false, int.Parse(LoadedData[j][8]), int.Parse(LoadedData[j][9]), float.Parse(LoadedData[j][10]), LoadedData[j][11], LoadedData[j][12],
                LoadedData[j][13], LoadedData[j][14], LoadedData[j][15], LoadedData[j][16], int.Parse(LoadedData[j][17]), LoadedData[j][18]);

                try
                {
                    if(SheetData[j][14] == "TRUE")
                        CompareIndividualResourceValues(ResourceLibrary[j]);
                }
                catch (IndexOutOfRangeException e){}
                
                NameReferenceIndex[j] = ResourceLibrary[j].displayName;
                StartCoroutine(UpdateQue(ResourceLibrary[j]));
                continue;
            }
        
            //Create new data for non-existing info on drive
            //name, desc, dis, gr, eType, req, nonReq, vis, cur, autA, timer, created,
            //coms, createComs, im, snd, ach, most
            ResourceLibrary[j] = new ResourceData(SheetData[j][0], SheetData[j][8], 
                SheetData[j][9], SheetData[j][10], SheetData[j][1], SheetData[j][2], 
                SheetData[j][3], true, 0, 0, float.Parse(SheetData[j][4]), SheetData[j][5], 
                SheetData[j][6], SheetData[j][7], SheetData[j][11], SheetData[j][12], 
                SheetData[j][13], 0, "");

            //If it is a basic resource we need it to start visible
            if(SheetData[j][3] == "nothing=0" && SheetData[j][4] == "nothing")
            {
                ResourceLibrary[j].AdjustVisibility(true);

                //Debug.Log($"{ResourceLibrary[j].itemName} is a basic resource.");
            }
            //If it is a tool, we need to set it's max amount to 1, or whatever the given max amount will be
            if(ResourceLibrary[j].groups == "tool")
            {
                //Debug.Log($"Found that {ResourceLibrary[j].displayName} is a tool.");
                string[] ra = SheetData[j][7].Split(" ");
                string[] k = ra[1].Split("=");
                ResourceLibrary[j].SetAtMost(int.Parse(k[1]));
                //Debug.Log($"Found that {ResourceLibrary[j].displayName} is is now set to {k[1]}.");
            }

            NameReferenceIndex[j] = ResourceLibrary[j].displayName;
            StartCoroutine(UpdateQue(ResourceLibrary[j]));

            //Debug.Log($"There wasn't {ResourceLibrary[j].itemName}, so I made a new one.");
        }

        CreateAllBuildableStrings();

        CreateResourcePanelInfo("all", "");

        SaveResourceLibrary();
        SaveSystem.SaveFile("/resource_shalom");
        LoadedData.Clear();

        s = SaveSystem.LoadFile("/address_nissi");
        if(s != null)
        {
            universeAdress = s;
            fromMemoryOfLocation = true;
        }
    }

    void LoadDataFromSave()
    {
        LoadedData = new List<string[]>();
        itemNames = new List<string>();

        string s = SaveSystem.LoadFile("/resource_shalom");
        if (s != null)
        {
            string[] ar = s.Split(';');
            foreach (string str in ar)
            {
                string[] final = str.Split(',');
                LoadedData.Add(final);
            }

            for (int i = 0; i < LoadedData.Count; i++)
            {
                itemNames.Add(LoadedData[i][0]);
            }
        }

        ResourceLibrary = new ResourceData[LoadedData.Count];
        NameReferenceIndex = new string[LoadedData.Count];
        QuedAmounts = new int[LoadedData.Count];

        for (int j = 0; j < LoadedData.Count; j++)
        {
            ResourceLibrary[j] = new ResourceData(LoadedData[j][0], LoadedData[j][1], LoadedData[j][2], LoadedData[j][3], LoadedData[j][4], LoadedData[j][5], LoadedData[j][6],
                    (LoadedData[j][7] == "True") ? true : false, int.Parse(LoadedData[j][8]), int.Parse(LoadedData[j][9]), float.Parse(LoadedData[j][10]), LoadedData[j][11], LoadedData[j][12],
                    LoadedData[j][13], LoadedData[j][14], LoadedData[j][15], LoadedData[j][16], int.Parse(LoadedData[j][17]), LoadedData[j][18]);

            NameReferenceIndex[j] = ResourceLibrary[j].displayName;
            StartCoroutine(UpdateQue(ResourceLibrary[j]));
        }

        s = SaveSystem.LoadFile("/address_nissi");
        if (s != null)
        {
            universeAdress = s;
            fromMemoryOfLocation = true;
        }
    }

    public void DeleteSaveFileData()
    {
        SaveSystem.SeriouslyDeleteAllSaveFiles();
    }
    #endregion

    #region Map
    public void GenerateUniverseLocation(UniverseDepth depth, int index)
    {
        UniverseDepth dp = depth;

        if(depthLocations != null)
            WipeUniversePieces();


        List<GameObject> objs = new List<GameObject>();

        if (!fromMemoryOfLocation)
        {
            if(depth == UniverseDepth.Universe)
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
            Debug.Log($"UniverseAddress being set up again: {universeAdress}");

            UnityEngine.Random.InitState(universeAdress.GetHashCode());

            string[] ar = universeAdress.Split(",");
            SetLocationDepthByInt(ar.Length);
            dp = currentDepth;
            fromMemoryOfLocation = false;
        }
        //Debug.Log($"UniverseAddress: {universeAdress}");
        //Debug.Log($"HashCode: {universeAdress.GetHashCode()}");

        switch (dp)
        {
            case UniverseDepth.Universe:
                Debug.Log("Should be building SuperCluster Level.");
                if (depthPrefabs[0] != null)//builds SuperClusters
                {
                    for(int i = 0; i < 10; i++)
                    {
                        GameObject obj = Instantiate(depthPrefabs[0], new Vector3(UnityEngine.Random.Range(-8.3f, 8.3f), UnityEngine.Random.Range(-4.4f, 4.4f), 0f), Quaternion.identity);
                        objs.Add(obj);
                    }
                }
                break;
            case UniverseDepth.SuperCluster://builds Galaxies
                Debug.Log("Should be building Galaxy Level.");
                if (depthPrefabs[1] != null)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        GameObject obj = Instantiate(depthPrefabs[1], new Vector3(UnityEngine.Random.Range(-8.3f, 8.3f), UnityEngine.Random.Range(-4.4f, 4.4f), 0f), Quaternion.identity);
                        objs.Add(obj);
                    }
                }
                break;
            case UniverseDepth.Galaxy://builds Nebulas etc.
                Debug.Log("We haven't made Nebulas yet.");
                break;
            case UniverseDepth.Nebula:
                
                break;
            case UniverseDepth.GlobularCluster:
                
                break;
            case UniverseDepth.StarCluster:
                
                break;
            case UniverseDepth.Constellation:
                
                break;
            case UniverseDepth.SolarSystem:
                
                break;
            case UniverseDepth.PlanetMoon:
                
                break;
        }

        depthLocations = new GameObject[objs.Count];
        for(int j = 0; j < objs.Count; j++)
        {
            depthLocations[j] = objs[j];
        }
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
        for(int i = 0; i < depthLocations.Length; i++)
        {
            if(ReferenceEquals(obj,depthLocations[i]))
            {
                SwitchUniverseLocationDepth();
                GenerateUniverseLocation(currentDepth, i);
                break;
            }
        }
    }

    void SwitchUniverseLocationDepth()
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
                //This should create a planetary experience
                break;
        }
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
                currentDepth = UniverseDepth.Planet;
                break;
        }
    }

    public void GoBackAStep()
    {
        Debug.Log("I have been called to go back a step.");
        string[] ar = universeAdress.Split(",");
        int index = 42;
        if(ar.Length != 1)
        {

            SetLocationDepthByInt(ar.Length - 1);
            universeAdress = "";
            for(int i = 0; i < ar.Length-1; i++)
            {
                if(i != 0)
                {
                    universeAdress += ar[i];
                }
                else
                {
                    universeAdress += "," + ar[i];
                    if(i == ar.Length - 1)
                    {
                        index = int.Parse(ar[i]);
                    }
                }
            }
        }

        if(index == 42)
        {
            SetLocationDepthByInt(1);
        }
        GenerateUniverseLocation(currentDepth, index);
    }
    #endregion

    #region Misc
    void ReceiveDropForExtraction(string drop)
    {
        string[] items = drop.Split("-");
        foreach(string s in items)
        {
            string[] ar = s.Split("=");
            ReturnData(ar[0]).AdjustCurrentAmount(int.Parse(ar[1]));
        }
    }

    IEnumerator PushMessage(string type, string message)
    {
        Debug.Log("Pushing message wait.");
        yield return new WaitForSeconds(5f);
        Debug.Log("Pushing message");
        OnSendMessage?.Invoke(type, message);
    }

    private void OnApplicationQuit()
    {
        SaveResourceLibrary();
        SaveSystem.SaveFile("/resource_shalom");
        SaveUniverseLocation();
    }
    #endregion
}
