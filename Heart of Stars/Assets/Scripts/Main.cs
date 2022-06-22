using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    List<string[]> SheetData;
    List<string[]> LoadedData;
    ResourceData[] ResourceLibrary;

    void Start()
    {

        SheetData = SheetReader.GetSheetData();
        ResourceLibrary = new ResourceData[SheetData.Count];

        LoadGameStats();

    }

    void LoadGameStats()
    {
        LoadedData = new List<string[]>();
        string s = SaveSystem.LoadFile();
        string[] ar = s.Split(';');
        foreach(string str in ar)
        {
            string[] final = str.Split(',');
            LoadedData.Add(final);
        }

        for (int i = 0; i < SheetData.Count; i++)
        {
            if(LoadedData[i][0] == SheetData[i][0])
            {
                ResourceLibrary[i] = new ResourceData(LoadedData[i][0], LoadedData[i][1], LoadedData[i][2], (LoadedData[i][3] == "true")?true:false,
                    int.Parse(LoadedData[i][4]), int.Parse(LoadedData[i][5]), float.Parse(LoadedData[i][6]));
                continue;
            }
            ResourceLibrary[i] = new ResourceData(SheetData[i][0], SheetData[i][2], SheetData[i][4], false, 0, 0, 0f);
            if(SheetData[i][4] == "nothing=0")
            {
                ResourceLibrary[i].AdjustVisibility(true);
            }
            SaveSystem.SaveResource(ResourceLibrary[i]);
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
}
