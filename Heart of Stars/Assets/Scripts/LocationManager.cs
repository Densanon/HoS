using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationManager : MonoBehaviour
{
    Main main;

    public string myAddress;
    public float frequencyForLandSpawning; //set in inspector
    public int landStartingPointsForSpawning; //set in inspector
    public GameObject[] myPlanetPieces;
    public GameObject tilePrefab;
    public HexTileInfo[] tileInfoList;
    HexTileInfo starter;
    //public ResourceData[] myResources;
    Vector2[] TileLocations;
    public int locationXBounds;
    public int locationYBounds;

    #region Debugging
    public void DestroyAllScenePieces()
    {
        int j = myPlanetPieces.Length;
        for (int i = 0; i < j; i++)
        {
            Destroy(myPlanetPieces[i]);
        }
    }
    public void Rebuild()
    {
        DestroyAllScenePieces();
        BuildTileBase();
        OrganizePieces();
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
    public void AssignMain(Main m)
    {
        main = m;
    }
    private void FirstEncounterSetup()
    {
        //Do some stuff for the firstencounter.
        //foreach(ResourceData data in myResources)
        //{
        //    if(data.itemName == "soldier")
        //    {
        //        data.SetCurrentAmount(10);
        //        continue;
        //    }
        //    if(data.itemName == "food")
        //    {
        //        data.SetCurrentAmount(100);
        //        continue;
        //    }
        //}
    }
    public void BuildPlanetData(string[] hextiles, string address)
    {
        myAddress = address;
        //myResources = new ResourceData[resources.Length];
        //Array.Copy(resources, myResources, resources.Length);

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

        locationXBounds = UnityEngine.Random.Range(30, 45);
        locationYBounds = UnityEngine.Random.Range(20, 35);

        for (int x = 0; x < locationXBounds; x++)
        {
            bool odd = (x % 2 == 1) ? true : false;
            for (int y = 0; y < locationYBounds; y++)
            {
                GameObject obj = Instantiate(tilePrefab, new Vector3(x * 0.75f, 0f, (odd) ? y * .87f + .43f : y * .87f), Quaternion.identity, transform);
                objs.Add(obj);
                HexTileInfo tf = obj.GetComponent<HexTileInfo>();
                tf.SetManager(this);
                tf.SetMain(main);
                tf.SetUpTileLocation(x, y);
                tf.frequencyOfLandDistribution = frequencyForLandSpawning;
                temp.Add(tf);
                locs.Add(tf.myPositionInTheArray);
            }
        }

        myPlanetPieces = new GameObject[objs.Count];
        tileInfoList = new HexTileInfo[temp.Count];
        TileLocations = new Vector2[locs.Count];

        for (int i = 0; i < tileInfoList.Length; i++)
        {
            myPlanetPieces[i] = objs[i];
            tileInfoList[i] = temp[i];
            TileLocations[i] = locs[i];
        }
    }
    private void SetHexTileInformationFromMemory(string[] hextileslist)
    {
        for (int i = 0; i < hextileslist.Length; i++)
        {
            string[] ar = hextileslist[i].Split(":");
            tileInfoList[i].SetAllTileInfoFromMemory(ar[0], int.Parse(ar[1]), ar[2], (ar[3] == "True"), ar[4]);
            if (ar[3] == "True")
            {
                starter = tileInfoList[i];
            }
        }
    }
    public void OrganizePieces()
    {
        for (int p = 0; p < landStartingPointsForSpawning; p++)
        {
            float l = locationXBounds*locationYBounds;
            float q = (float)p / (float)landStartingPointsForSpawning * l;
            float f = (float)(p + 1) / (float)landStartingPointsForSpawning * l;
            int k = 0;
            bool suitable = false;
            while (!suitable)
            {
                k = UnityEngine.Random.Range(Mathf.RoundToInt(q), Mathf.RoundToInt(f));
                Vector2 target = tileInfoList[k].myPositionInTheArray;
                if(target.x != 0 && target.x != locationXBounds-1 &&
                   target.y != 0 && target.y != locationYBounds - 1 &&
                   tileInfoList[k].myTileType == 0)
                {
                    suitable = true;
                }
            }

            tileInfoList[k].TurnLand();
        }

        bool start = false;
        foreach(HexTileInfo tile in tileInfoList)
        {
            tile.SetNeighbors(FindNeighbors(tile.myPositionInTheArray));
            if (!start && tile.myTileType == tile.GetResourceSpritesLengthForStartPoint())
            {
                tile.SetAsStartingPoint();
                start = true;
                starter = tile;
            }
        }
    }
    public void StartLeaveSequence()
    {
        foreach(HexTileInfo tile in tileInfoList)
        {
            if (tile.isStartingPoint)
            {
                tile.StartLeavingSequenceAnimation();
                return;
            }
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
        starter.StartLandingSequenceAnimation();
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

            //for (int j = 0; j < myResources.Length; j++)
            //{
            //    if (j == myResources.Length - 1)
            //    {
            //        SaveSystem.SaveResource(myResources[j], true);
            //        continue;
            //    }
            //    SaveSystem.SaveResource(myResources[j], false);
            //}

            SaveSystem.SaveLocationData();
            SaveSystem.SaveFile("/" + myAddress);
        }
    }

    private void OnApplicationQuit()
    {
        SaveLocationInfo();
    }
}
