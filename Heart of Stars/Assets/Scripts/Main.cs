using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    List<string[]> SheetData;
    List<string[]> LoadedData;
    List<string> itemNames;
    [SerializeField]
    ResourceData[] ResourceLibrary;

    string[] NameReferenceIndex;
    int[] QuedAmounts;

    public GameObject ResourePanelPrefab;
    public Transform ResourcePanel;

    public GameObject BuildableResourceButtonPrefab;
    public GameObject Canvas;

    int created = 0;

    void Awake()
    {
        //SaveSystem.SeriouslyDeleteAllSaveFiles();

        SheetData = SheetReader.GetSheetData();
        ResourceLibrary = new ResourceData[SheetData.Count];
        NameReferenceIndex = new string[SheetData.Count];
        QuedAmounts = new int[SheetData.Count];

        LoadGameStats();

        DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B) && created == 0)
        {
            created++;
            GameObject obj = Instantiate(BuildableResourceButtonPrefab, Canvas.transform);
            obj.GetComponent<Resource>().AssignResource(ResourceLibrary[Random.Range(0, ResourceLibrary.Length - 1)], true);
            //obj.GetComponent<RectTransform>().SetPositionAndRotation(new Vector3(-10f, 0f, 0f), Quaternion.identity);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SaveSystem.SeriouslyDeleteAllSaveFiles();
        }
    }

    public void StartQueUpdate(ResourceData data)
    {
        Debug.Log($"I have been told to start the que for {data.displayName}");
        StartCoroutine(UpdateQue(data));
    }

    IEnumerator UpdateQue(ResourceData data)
    {
        Debug.Log($"Starting {data.displayName} Que Update.");
        bool addedNormal = false;
        yield return new WaitForSeconds(data.craftTime);
        if (data.visible  && QuedAmounts[System.Array.IndexOf(NameReferenceIndex, data.displayName)] > 0 || data.autoAmount > 0)
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
        SaveStats();
        SaveSystem.SaveFile();
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
                ResourceLibrary[j] = new ResourceData(LoadedData[j][0], LoadedData[j][1], LoadedData[j][2], LoadedData[j][3], LoadedData[j][4], LoadedData[j][5],
                    (LoadedData[j][6] == "true") ? true : false, int.Parse(LoadedData[j][7]), int.Parse(LoadedData[j][8]), float.Parse(LoadedData[j][9]), LoadedData[j][10], LoadedData[j][11],
                    LoadedData[j][12], LoadedData[j][13], LoadedData[j][14], LoadedData[j][15]);

                NameReferenceIndex[j] = ResourceLibrary[j].displayName;
                CreateResourcePanelReferenceAndSave(ResourceLibrary[j]);
                StartCoroutine(UpdateQue(ResourceLibrary[j]));
                continue;
            }

            //Create new data for non-existing info on drive
            ResourceLibrary[j] = new ResourceData(SheetData[j][0], SheetData[j][8], SheetData[j][9], SheetData[j][10], SheetData[j][2], SheetData[j][3], true, 0, 0, float.Parse(SheetData[j][4]),
                SheetData[j][5], SheetData[j][6], SheetData[j][7], SheetData[j][11], SheetData[j][12], SheetData[j][13]);

            //If it is a basic resource we need it to start visible
            if(SheetData[j][2] == "nothing=0" && SheetData[j][3] == "nothing")
            {
                ResourceLibrary[j].AdjustVisibility(true);

                Debug.Log($"{ResourceLibrary[j].itemName} is a basic resource.");
            }
            NameReferenceIndex[j] = ResourceLibrary[j].displayName;
            CreateResourcePanelReferenceAndSave(ResourceLibrary[j]);
            StartCoroutine(UpdateQue(ResourceLibrary[j]));

            Debug.Log($"There wasn't {ResourceLibrary[j].itemName}, so I made a new one.");
        }

        SaveSystem.SaveFile();
        LoadedData.Clear();
    }

    void CreateResourcePanelReferenceAndSave(ResourceData data)
    {
        GameObject obj = Instantiate(ResourePanelPrefab, ResourcePanel);
        obj.GetComponent<ResourceDisplayInfo>().Initialize(data);
        SaveSystem.SaveResource(data);
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
}
