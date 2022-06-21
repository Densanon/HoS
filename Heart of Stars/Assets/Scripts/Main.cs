using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    List<string[]> SheetData;
    List<int[]> PlayerData;

    // Start is called before the first frame update
    void Start()
    {

        SheetData = SheetReader.GetSheetData();
        PlayerData = new List<int[]>();

        foreach(string[] item in SheetData)
        {
            int[] n = new int[3];
            if (!PlayerPrefs.HasKey(item[0]))
            {
                PlayerPrefs.SetInt(item[0], 0);
                n[0] = 0;
                PlayerPrefs.SetInt($"{item[0]}Visible", 0);
                n[1] = 0;
                PlayerPrefs.SetInt($"{item[0]}Auto", 0);
                n[2] = 0;
            }
            else
            {
                n[0] = PlayerPrefs.GetInt(item[0]);
                n[1] = PlayerPrefs.GetInt($"{item[0]}Visible");
                n[2] = PlayerPrefs.GetInt($"{item[0]}Auto");
            }
            PlayerData.Add(n);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
