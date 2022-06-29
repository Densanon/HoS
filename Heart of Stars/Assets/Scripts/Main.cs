using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class Main : MonoBehaviour
{
    public static Action<string, string> OnSendMessage = delegate { };

    List<string[]> SheetData;
    List<string[]> LoadedData;
    List<string> itemNames;
    [SerializeField]
    ResourceData[] ResourceLibrary;

    string[] NameReferenceIndex;
    int[] QuedAmounts;

    public GameObject ResourePanelPrefab;
    public Transform ResourcePanel;
    public List<GameObject> resourcePanelInfoPieces;

    public GameObject BuildableResourceButtonPrefab;
    public GameObject Canvas;

    int created = 0;
    public string searchField = "";

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

    void Awake()
    {
        SaveSystem.SeriouslyDeleteAllSaveFiles();
        
        SheetData = SheetReader.GetSheetData();
        ResourceLibrary = new ResourceData[SheetData.Count];
        NameReferenceIndex = new string[SheetData.Count];
        QuedAmounts = new int[SheetData.Count];
        resourcePanelInfoPieces = new List<GameObject>();

        LoadGameStats();

        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        //StartCoroutine(PushMessage("Welcome!", "Thank you for starting the game and participating in all the fun!"));
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

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (debugPanel.active)
            {
                debugPanel.SetActive(false);
            }
            else
            {
                debugPanel.SetActive(true);
            }
        }
    }

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

    private void OnApplicationQuit()
    {
        //SaveStats();
        //SaveSystem.SaveFile();
    }

    void LoadGameStats()
    {
        LoadedData = new List<string[]>();
        itemNames = new List<string>();

        string s = SaveSystem.LoadFile();
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
            if (itemNames.Contains(SheetData[j][0])) {
                ResourceLibrary[j] = new ResourceData(LoadedData[j][0], LoadedData[j][1], LoadedData[j][2], LoadedData[j][3], LoadedData[j][4], LoadedData[j][5], LoadedData[j][6],
                    (LoadedData[j][7] == "True") ? true : false, int.Parse(LoadedData[j][8]), int.Parse(LoadedData[j][9]), float.Parse(LoadedData[j][10]), LoadedData[j][11], LoadedData[j][12],
                    LoadedData[j][13], LoadedData[j][14], LoadedData[j][15], LoadedData[j][16], int.Parse(LoadedData[j][17]));

                NameReferenceIndex[j] = ResourceLibrary[j].displayName;
                StartCoroutine(UpdateQue(ResourceLibrary[j]));
                continue;
            }

            //Create new data for non-existing info on drive
            //name, disp, dis, gr, eType, req, nonReq, vis, cur, autA, timer, created,
            //coms, createComs, im, snd, ach, most
            ResourceLibrary[j] = new ResourceData(SheetData[j][0], SheetData[j][8], 
                SheetData[j][9], SheetData[j][10], SheetData[j][1], SheetData[j][2], 
                SheetData[j][3], true, 0, 0, float.Parse(SheetData[j][4]), SheetData[j][5], 
                SheetData[j][6], SheetData[j][7], SheetData[j][11], SheetData[j][12], 
                SheetData[j][13], 0);

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

        CreateResourcePanelInfo("all", "");

        SaveStats();
        LoadedData.Clear();
    }

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

    public void SaveStats()
    {
        foreach(ResourceData data in ResourceLibrary)
        {
            SaveSystem.SaveResource(data);
        }
    }

    public void DeleteSaveFileData()
    {
        SaveSystem.SeriouslyDeleteAllSaveFiles();
    }

    public ResourceData ReturnData(string itemName)
    {
        //Debug.Log($"In Main:ReturnData:Looking for {itemName}");
        foreach(ResourceData data in ResourceLibrary)
        {
            if(data.itemName == itemName)
            {
                //Debug.Log($"Found a data type with name {data.itemName}");
                return data;
            }
        }

        Debug.LogError($"ReturnData: Could not find itemName : {itemName}");
        return null;
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
}
