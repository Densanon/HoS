using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    List<string[]> SheetData;
    List<int[]> PlayerData;
    ResourceData[] ResourceLibrary;
    List<ResourceData> StatChanges;

    // Start is called before the first frame update
    void Start()
    {

        SheetData = SheetReader.GetSheetData();
        PlayerData = new List<int[]>();
        ResourceLibrary = new ResourceData[SheetData.Count];
        StatChanges = new List<ResourceData>();

        LoadGameStats();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StatUpdateQue(ResourceData data)
    {
        if (!StatChanges.Contains(data))
        {
            StatChanges.Add(data);
        }
    }

    void LoadGameStats()
    {
        for (int i = 0; i < SheetData.Count; i++)
        {
            ResourceData d = SaveSystem.LoadResource(SheetData[i][0]);
            if(d != null)
            {
                ResourceLibrary[i] = d;
                continue;
            }
            ResourceLibrary[i] = new ResourceData(SheetData[i][0], SheetData[i][2], SheetData[i][4], false, 0, 0, 0f);
            if(SheetData[i][4] == "nothing=0")
            {
                ResourceLibrary[i].visibile = true;
            }
            SaveSystem.SaveResource(ResourceLibrary[i]);
        }
    }

    void SaveStats()
    {
        foreach(ResourceData data in StatChanges)
        {
            SaveSystem.SaveResource(data);
        }
        StatChanges.Clear();
    }
}
