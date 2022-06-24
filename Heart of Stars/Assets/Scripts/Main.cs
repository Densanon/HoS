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

    public GameObject ResourePanelPrefab;
    public Transform ResourcePanel;

    public GameObject BuildableResourceButtonPrefab;
    public GameObject Canvas;

    int created = 0;

    void Awake()
    {

        SheetData = SheetReader.GetSheetData();
        ResourceLibrary = new ResourceData[SheetData.Count];

        LoadGameStats();
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

        for(int j = 0; j < SheetData.Count;j++)
        {
            GameObject obj;

            if (itemNames.Contains(SheetData[j][0])){
                ResourceLibrary[j] = new ResourceData(LoadedData[j][0], LoadedData[j][1], LoadedData[j][2], (LoadedData[j][3] == "true")?true:false,
                    int.Parse(LoadedData[j][4]), int.Parse(LoadedData[j][5]), float.Parse(LoadedData[j][6]));
                obj = Instantiate(ResourePanelPrefab, ResourcePanel);
                obj.GetComponent<ResourceDisplayInfo>().Initialize(ResourceLibrary[j]);
                SaveSystem.SaveResource(ResourceLibrary[j]);
                continue;
            }
            
            ResourceLibrary[j] = new ResourceData(SheetData[j][0], SheetData[j][2], SheetData[j][4], false, 0, 0, 0f);
            if(SheetData[j][4] == "nothing=0")
            {
                ResourceLibrary[j].AdjustVisibility(true);
                Debug.Log($"{ResourceLibrary[j].itemName} is a basic resource.");
            }
            obj = Instantiate(ResourePanelPrefab, ResourcePanel);
            obj.GetComponent<ResourceDisplayInfo>().Initialize(ResourceLibrary[j]);
            Debug.Log($"There wasn't {ResourceLibrary[j].itemName}, so I made a new one.");
            SaveSystem.SaveResource(ResourceLibrary[j]);
        }

        SaveSystem.SaveFile();
        LoadedData.Clear();
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
