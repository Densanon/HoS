using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationManager : MonoBehaviour
{
    public string myAddress;
    public float frequencyForLandSpawning = 1.5f;
    public int landStartingPointsForSpawning = 5;
    public GameObject[] myPlanetPieces;
    public GameObject tilePrefab;
    public HexTileInfo[] tileInfoList;
    public ResourceData[] myResources;
    Vector2[] TileLocations;

    #region Debugging
    void DestroyAllScenePieces()
    {
        int j = myPlanetPieces.Length;
        for (int i = 0; i < j; i++)
        {
            Destroy(myPlanetPieces[i]);
        }
    }
    #endregion

    #region Unity Methods
    private void Awake()
    {
        Main.OnWorldMap += TurnOffVisibility;
        Main.OnInitializeFirstInteraction += FirstEncounterSetup;
    }

    private void OnDestroy()
    {
        Main.OnWorldMap -= TurnOffVisibility;
        Main.OnInitializeFirstInteraction -= FirstEncounterSetup;

    }
    #endregion

    #region Setup
    private void FirstEncounterSetup()
    {
        //Do some stuff for the firstencounter.
        foreach(ResourceData data in myResources)
        {
            if(data.itemName == "soldier")
            {
                data.SetCurrentAmount(10);
                continue;
            }
            if(data.itemName == "food")
            {
                data.SetCurrentAmount(100);
                continue;
            }
        }
    }
    public void BuildPlanetData(string[] hextiles, string address, ResourceData[] resources)
    {
        myAddress = address;
        myResources = new ResourceData[resources.Length];
        Array.Copy(resources, myResources, resources.Length);

        BuildTileBase();

        if (hextiles != null)
        {
            SetHexTileInformationFromMemory(hextiles);
            return;
        }

        OrganizePieces();

        SaveLocationInfo();
    }

    void BuildTileBase()
    {
        List<HexTileInfo> temp = new List<HexTileInfo>();
        List<GameObject> objs = new List<GameObject>();
        List<Vector2> locs = new List<Vector2>();

        for (int x = 0; x < 10; x++)
        {
            bool odd = (x % 2 == 1) ? true : false;
            for (int y = 0; y < 10; y++)
            {
                GameObject obj = Instantiate(tilePrefab, new Vector3(x * 0.75f, 0f, (odd) ? y * .87f + .43f : y * .87f), Quaternion.identity, transform);
                objs.Add(obj);
                HexTileInfo tf = obj.GetComponent<HexTileInfo>();
                tf.SetUpTileLocation(x, y);
                tf.frequencyOfLandDistribution = frequencyForLandSpawning;
                temp.Add(tf);
                locs.Add(tf.myPositionInTheArray);
            }
        }

        myPlanetPieces = new GameObject[objs.Count];
        for (int j = 0; j < objs.Count; j++)
        {
            myPlanetPieces[j] = objs[j];
        }

        tileInfoList = new HexTileInfo[temp.Count];
        for (int i = 0; i < tileInfoList.Length; i++)
        {
            tileInfoList[i] = temp[i];
        }

        TileLocations = new Vector2[locs.Count];
        for (int k = 0; k < TileLocations.Length; k++)
        {
            TileLocations[k] = locs[k];
        }
    }

    private void SetHexTileInformationFromMemory(string[] hextiles)
    {
        for (int i = 0; i < hextiles.Length; i++)
        {
            string[] ar = hextiles[i].Split(":");
            tileInfoList[i].SetAllTileInfoFromMemory(ar[0], int.Parse(ar[1]), ar[2]);
        }
    }

    public void OrganizePieces()
    {
        for (int p = 0; p < landStartingPointsForSpawning; p++)
        {
            float l = 100f;
            float q = (float)p / (float)landStartingPointsForSpawning * l;
            //Debug.Log($"Current low: {q}");
            float f = (float)(p + 1) / (float)landStartingPointsForSpawning * l;
            //Debug.Log($"Current high: {f}");
            int k = UnityEngine.Random.Range(Mathf.RoundToInt(q), Mathf.RoundToInt(f));
            //Debug.Log(k);
            tileInfoList[k].TurnLand();
        }

        HexTileInfo info = myPlanetPieces[UnityEngine.Random.Range(0, myPlanetPieces.Length)].GetComponent<HexTileInfo>();
        /*while (info.i_tileType != 1)
        {
            info = myPlanetPieces[UnityEngine.Random.Range(0, myPlanetPieces.Length)].GetComponent<HexTileInfo>();
        }*/
        if (info.myTileType == 1) info.SetAsStartingPoint();

        foreach (HexTileInfo hex in tileInfoList)
        {
            hex.SetNeighbors(FindNeighbors(hex.myPositionInTheArray));
        }
    }
    #endregion

    #region Location Checks For Neighbors
    private Vector2[] FindNeighbors(Vector2 location)
    {
        List<Vector2> neg = new List<Vector2>();

        Vector2 vectorNull = new Vector2(-1, -1);
        Vector2 v = CheckUpLocation(location);
        if(v != vectorNull)
        {
            neg.Add(v);
        }
        v = CheckDownLocation(location);
        if (v != vectorNull)
        {
            neg.Add(v);
        }
        v = CheckLeftEqualLocation(location);
        if (v != vectorNull)
        {
            neg.Add(v);
        }
        v = CheckRightEqualLocation(location);
        if (v != vectorNull)
        {
            neg.Add(v);
        }
        if(location.x % 2 == 1)
        {
            v = CheckLeftUpLocation(location);
            if (v != vectorNull)
            {
                neg.Add(v);
            }
            v = CheckRightUpLocation(location);
            if (v != vectorNull)
            {
                neg.Add(v);
            }
        }
        else
        {
            v = CheckLeftDownLocation(location);
            if (v != vectorNull)
            {
                neg.Add(v);
            }
            v = CheckRightDownLocation(location);
            if (v != vectorNull)
            {
                neg.Add(v);
            }
        }

        return neg.ToArray();
    }

    Vector2 CheckLeftDownLocation(Vector2 location)
    {
        Vector2 v = new Vector2(location.x - 1, location.y - 1);
        if (CheckLocationMatch(v))
        {
            return v;
        }
        return new Vector2(-1, -1);
    }

    Vector2 CheckLeftUpLocation(Vector2 location)
    {
        Vector2 v = new Vector2(location.x - 1, location.y + 1);
        if (CheckLocationMatch(v))
        {
            return v;
        }
        return new Vector2(-1, -1);
    }

    Vector2 CheckLeftEqualLocation(Vector2 location)
    {
        Vector2 v = new Vector2(location.x - 1, location.y);
        if (CheckLocationMatch(v))
        {
            return v;
        }
        return new Vector2(-1, -1);
    }

    Vector2 CheckRightDownLocation(Vector2 location)
    {
        Vector2 v = new Vector2(location.x + 1, location.y - 1);
        if (CheckLocationMatch(v))
        {
            return v;
        }
        return new Vector2(-1, -1);
    }

    Vector2 CheckRightUpLocation(Vector2 location)
    {
        Vector2 v = new Vector2(location.x + 1, location.y + 1);
        if (CheckLocationMatch(v))
        {
            return v;
        }
        return new Vector2(-1, -1);
    }

    Vector2 CheckRightEqualLocation(Vector2 location)
    {
        Vector2 v = new Vector2(location.x + 1, location.y);
        if (CheckLocationMatch(v))
        {
            return v;
        }
        return new Vector2(-1, -1);
    }

    Vector2 CheckUpLocation(Vector2 location)
    {
        Vector2 v = new Vector2(location.x, location.y + 1);
        if (CheckLocationMatch(v)){
            return v;
        }
        return new Vector2(-1, -1);
    }

    Vector2 CheckDownLocation(Vector2 location)
    {
        Vector2 v = new Vector2(location.x, location.y - 1);
        if (CheckLocationMatch(v)){
            return v;
        }
        return new Vector2(-1, -1);
    }

    bool CheckLocationMatch(Vector2 location)
    {
        if (Array.Exists(TileLocations, loc => loc.x == location.x && loc.y == location.y))
        {
            return true;
        }
        return false;
    }
    #endregion

    #region Visibility
    void TurnOffVisibility(bool inOverWorld)
    {
        if (inOverWorld && gameObject.activeInHierarchy)
        {
            gameObject.SetActive(false);
            SaveLocationInfo();
        }
    }

    public void TurnOnVisibility()
    {
        gameObject.SetActive(true);
    }
    #endregion

    void SaveLocationInfo()
    {
        SaveSystem.WipeString();

        if (tileInfoList != null)
        {
            for (int i = 0; i < tileInfoList.Length; i++)
            {
                if (i == tileInfoList.Length - 1)
                {
                    SaveSystem.SaveTile(tileInfoList[i], true);
                    continue;
                }
                SaveSystem.SaveTile(tileInfoList[i], false);
            }

            for (int j = 0; j < myResources.Length; j++)
            {
                if (j == myResources.Length - 1)
                {
                    SaveSystem.SaveResource(myResources[j], true);
                    continue;
                }
                SaveSystem.SaveResource(myResources[j], false);
            }

            SaveSystem.SaveLocationData();
            SaveSystem.SaveFile("/" + myAddress);
        }
    }

    private void OnApplicationQuit()
    {
        SaveLocationInfo();
    }
}
