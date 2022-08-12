using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationManager : MonoBehaviour
{
    public static Action<LocationManager> OnGreetGeneralManager = delegate { };
    public static Action<LocationManager> OnTurnActiveManagerForGenerals = delegate { };

    Main main;
    UIResourceManager activeManager;
    Transform canvas;

    public string myAddress;
    public float frequencyForLandSpawning; //set in inspector
    public int landStartingPointsForSpawning; //set in inspector
    public GameObject tilePrefab;
    public HexTileInfo[][] tileInfoList;
    HexTileInfo starter;
    Vector2[] TileLocations;
    public int locationXBounds;
    public int locationYBounds;

    List<General> myGenerals;
    [SerializeField]
    GameObject GeneralPrefab;

    #region Debugging
    float enemyRatio;
    int enemyDensityMin;
    int enemyDensityMax;

    bool isViewing;

    public void SetEnemyNumbers(float ratio, int densityMin, int densityMax)
    {
        enemyRatio = ratio;
        enemyDensityMin = densityMin;
        enemyDensityMax = densityMax;
    }
    #endregion

    #region Unity Methods
    private void Awake()
    {
        Main.OnWorldMap += TurnOffVisibility;
        Main.OnInitializeVeryFirstInteraction += VeryFirstEncounterSetup;
        Main.OnInitializeRegularFirstPlanetaryInteraction += FirstRegularPlanetaryEncounter;
        canvas = GameObject.Find("Canvas").transform;
        myGenerals = new List<General>();
    }

    private void OnEnable()
    {
        HexTileInfo.OnNeedUIElementsForTile += CheckNewTileOptions;
    }

    private void OnDisable()
    {
        HexTileInfo.OnNeedUIElementsForTile -= CheckNewTileOptions;
    }

    private void OnDestroy()
    {
        Main.OnWorldMap -= TurnOffVisibility;
        Main.OnInitializeVeryFirstInteraction -= VeryFirstEncounterSetup;
        Main.OnInitializeRegularFirstPlanetaryInteraction -= FirstRegularPlanetaryEncounter;
    } 
    #endregion

    #region Setup
    public void AssignMain(Main m)
    {
        main = m;
    }
    private void VeryFirstEncounterSetup()
    {
        //Do some stuff for the firstencounter.
        foreach(ResourceData data in starter.myResources)
        {
            if(data.itemName == "enemy")
            {
                data.SetCurrentAmount(0);
                continue;
            }
            if(data.itemName == "soldier")
            {
                data.SetCurrentAmount(100);
                continue;
            }
            if (data.itemName == "food")
            {
                data.SetCurrentAmount(100);
                continue;
            }
            if(data.itemName == "barracks")
            {
                data.SetCurrentAmount(1);
                continue;
            }
        }
    }
    private void FirstRegularPlanetaryEncounter(int amount)//will actually need resource names and amounts
    {
        if (starter == null) return;
        foreach (ResourceData data in starter.myResources)
        {
            if (data.itemName == "soldier")
            {
                data.SetCurrentAmount(amount);
                continue;
            }
        }
    }
    public void BuildPlanetData(string[] hextiles, string address, bool viewing)
    {
        //Debug.Log($"Is viewing only: {viewing}");
        myAddress = address;
        isViewing = viewing;

        BuildTileBase();

        if (hextiles != null)
        {
            SetHexTileInformationFromMemory(hextiles);
            return;
        }

        OrganizePieces();

        if (!viewing)
        {
            //Debug.Log("Not viewing.");
            SaveLocationInfo();
            main.SaveLocationAddressBook();
            OnGreetGeneralManager?.Invoke(this);
        }
    }
    void BuildTileBase()
    {
        Debug.Log($"Building with Ration: {enemyRatio}; Min: {enemyDensityMin}, Max: {enemyDensityMax}");

        List<HexTileInfo[]> mainTemp = new List<HexTileInfo[]>();
        List<Vector2> locs = new List<Vector2>();

        locationXBounds = UnityEngine.Random.Range(30, 45);
        locationYBounds = UnityEngine.Random.Range(20, 35);

        for (int x = 0; x < locationXBounds; x++)
        {
            List<HexTileInfo> temp = new List<HexTileInfo>();
            bool odd = (x % 2 == 1) ? true : false;
            for (int y = 0; y < locationYBounds; y++)
            {
                GameObject obj = Instantiate(tilePrefab, new Vector3(x * 0.75f, 0f, (odd) ? y * .87f + .43f : y * .87f), Quaternion.identity, transform);
                HexTileInfo tf = obj.GetComponent<HexTileInfo>();
                tf.SetManager(this);
                tf.SetMain(main);
                tf.SetUpTileLocation(x, y);
                tf.SetEnemyNumbers(enemyRatio, enemyDensityMin, enemyDensityMax);
                tf.frequencyOfLandDistribution = frequencyForLandSpawning;
                temp.Add(tf);
                locs.Add(tf.myPositionInTheArray);
            }
            mainTemp.Add(temp.ToArray());
        }

        tileInfoList = mainTemp.ToArray();
        TileLocations = new Vector2[locs.Count];

        for (int i = 0; i < TileLocations.Length; i++)
        {
            TileLocations[i] = locs[i];
        }
    }
    private void SetHexTileInformationFromMemory(string[] hextileslist)
    {
        Debug.Log("Using memory to build level.");
        int x = 0, y = 0;
        foreach(string s in hextileslist)
        {
            if (y == locationYBounds)
            {
                x++;
                y = 0;
            }
            string[] ar = s.Split(":");
            tileInfoList[x][y].SetAllTileInfoFromMemory(ar[0], int.Parse(ar[1]), ar[2], (ar[3] == "True"), ar[4]);
            if (ar[3] == "True")
            {
                starter = tileInfoList[x][y];
            }
            y++;
        }

        OnGreetGeneralManager?.Invoke(this);
    }
    public void OrganizePieces()
    {
        for (int p = 0; p < landStartingPointsForSpawning; p++)
        {
            float l = locationXBounds*locationYBounds;
            float q = (float)p / (float)landStartingPointsForSpawning * l;
            float f = (float)(p + 1) / (float)landStartingPointsForSpawning * l;
            int k = 0,d = 0, r = 0;
            bool suitable = false;
            while (!suitable)
            {
                k = UnityEngine.Random.Range(Mathf.RoundToInt(q), Mathf.RoundToInt(f));
                d = Mathf.FloorToInt(k / locationYBounds);
                r = k % locationYBounds;
                Vector2 target = tileInfoList[d][r].myPositionInTheArray;
                if(target.x != 0 && target.x != locationXBounds-1 &&
                   target.y != 0 && target.y != locationYBounds - 1 &&
                   tileInfoList[d][r].myTileType == 0)
                {
                    suitable = true;
                }
            }

            tileInfoList[d][r].TurnLand();
        }

        bool start = false;
        foreach(HexTileInfo[] tileArray in tileInfoList)
        {
            foreach(HexTileInfo tile in tileArray)
            {
                tile.SetNeighbors(FindNeighbors(tile.myPositionInTheArray));
                if (!start && tile.myTileType == tile.GetResourceSpritesLengthForStartPoint() && !isViewing)
                {
                    //Debug.Log("Setting a starting point.");
                    tile.SetAsStartingPoint();
                    start = true;
                    starter = tile;
                }
            }
        }
    }
    public void StartLeaveSequence()
    {
        foreach(HexTileInfo[] ar in tileInfoList)
        {
            foreach(HexTileInfo tile in ar)
            {
                if (tile.isStartingPoint)
                {
                    tile.StartLeavingSequenceAnimation();
                    return;
                }
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

    public Vector2 CheckLeftDownLocation(Vector2 location)
    {
        Vector2 v = new Vector2(location.x - 1, location.y - 1);
        if (CheckLocationMatch(v))
        {
            return v;
        }
        return new Vector2(-1, -1);
    }

    public Vector2 CheckLeftUpLocation(Vector2 location)
    {
        Vector2 v = new Vector2(location.x - 1, location.y + 1);
        if (CheckLocationMatch(v))
        {
            return v;
        }
        return new Vector2(-1, -1);
    }

    public Vector2 CheckLeftEqualLocation(Vector2 location)
    {
        Vector2 v = new Vector2(location.x - 1, location.y);
        if (CheckLocationMatch(v))
        {
            return v;
        }
        return new Vector2(-1, -1);
    }

    public Vector2 CheckRightDownLocation(Vector2 location)
    {
        Vector2 v = new Vector2(location.x + 1, location.y - 1);
        if (CheckLocationMatch(v))
        {
            return v;
        }
        return new Vector2(-1, -1);
    }

    public Vector2 CheckRightUpLocation(Vector2 location)
    {
        Vector2 v = new Vector2(location.x + 1, location.y + 1);
        if (CheckLocationMatch(v))
        {
            return v;
        }
        return new Vector2(-1, -1);
    }

    public Vector2 CheckRightEqualLocation(Vector2 location)
    {
        Vector2 v = new Vector2(location.x + 1, location.y);
        if (CheckLocationMatch(v))
        {
            return v;
        }
        return new Vector2(-1, -1);
    }

    public Vector2 CheckUpLocation(Vector2 location)
    {
        Vector2 v = new Vector2(location.x, location.y + 1);
        if (CheckLocationMatch(v)){
            return v;
        }
        return new Vector2(-1, -1);
    }

    public Vector2 CheckDownLocation(Vector2 location)
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
        if (inOverWorld && gameObject.activeInHierarchy) //leaving planet
        {
            gameObject.SetActive(false);
            if(starter != null) SaveLocationInfo();
            return;
        }

        if (starter == null)//comming to a new one and this isn't one we want to keep open.
            main.RemovePlanetBrainAndDestroy(this);       
    }

    public void TurnOnVisibility()
    {
        OnTurnActiveManagerForGenerals?.Invoke(this);
        gameObject.SetActive(true);
        if(starter != null)
            starter.StartLandingSequenceAnimation();
    }

    private void CheckNewTileOptions(HexTileInfo tile)
    {
        UIResourceManager res = tile.myUIManager;
        if (activeManager == null) activeManager = res;
        if (res != activeManager && activeManager.activeMouseHoverInteractions == 0)
        {
            activeManager.activeMouseHoverInteractions = 0;
            activeManager.ResetUI();
            activeManager.DeactivateSelf();
            activeManager = res;
            activeManager.ActivateSelf();
            activeManager.ResetUI();
        }
        else if (res != activeManager && activeManager.activeMouseHoverInteractions > 0)
        {
            res.DeactivateSelf();
        }else if (res == activeManager && !activeManager.interactiblesContainer.activeInHierarchy)
        {
            activeManager.ActivateSelf();
            activeManager.ResetUI();
        }
        activeManager.transform.SetSiblingIndex(canvas.childCount-1); //Ensures that it is the highest in the hierarchy
    }
    #endregion

    void SaveLocationInfo()
    {
        SaveSystem.WipeString();

        if (tileInfoList != null)
        {
            foreach(Vector2 tile in TileLocations)
            {
                if(tile == new Vector2(locationXBounds-1, locationYBounds - 1))
                {
                    SaveSystem.SaveTile(tileInfoList[Mathf.RoundToInt(tile.x)][Mathf.RoundToInt(tile.y)], true);
                    continue;
                }
                SaveSystem.SaveTile(tileInfoList[Mathf.RoundToInt(tile.x)][Mathf.RoundToInt(tile.y)], false);
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
