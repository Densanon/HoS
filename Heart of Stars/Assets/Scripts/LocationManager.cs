using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationManager : MonoBehaviour
{
    public static Action<LocationManager> OnGreetManagers = delegate { };
    public static Action<LocationManager> OnTurnActiveManager = delegate { };
    public static Action<Transform> OnCameraLookAtStarter = delegate { };

    public GameObject tilePrefab;

    Main main;
    UIItemManager activeManager;
    Spacecraft mySpaceship;

    public string myAddress;
    public HexTileInfo[][] tileInfoList;
    HexTileInfo starter;
    Vector2[] TileLocations;
    public int locationXBounds;
    public int locationYBounds;
    public int landStartingPointsForSpawning;
    public float frequencyForLandSpawning;
    float enemyRatio;
    int enemyDensityMin;
    int enemyDensityMax;
    bool isViewing;
    int landLeftToConquer;
    public Dictionary<string, Trait> PlanetaryTraits;

    #region Debuging
    public static Action<Vector2> OnTurnAllLandConquered = delegate { };
    public HexTileInfo GetTile(Vector2 tileLocation)
    {
        return tileInfoList[Mathf.RoundToInt(tileLocation.x)][Mathf.RoundToInt(tileLocation.y)];
    }
    public void TurnAllLandConqueredButOne(Vector2 tile)
    {
        OnTurnAllLandConquered?.Invoke(tile);
    }
    #endregion

    #region Unity Methods
    private void Awake()
    {
        Main.OnWorldMap += TurnOffVisibility;
        PlanetaryTraits = new();
        foreach(Trait trait in Main.PlanetaryTraits)
        {
            PlanetaryTraits.Add(trait.UniqueName, new(trait));
        }
        PlanetaryTraits.Add("temperature", new (Main.TraitLibrary[Array.IndexOf(Main.TraitNameReference, "temperature")]));
        PlanetaryTraits.Add("groundCoverType", new (Main.TraitLibrary[Array.IndexOf(Main.TraitNameReference, "groundCoverType")]));

        foreach(KeyValuePair<string, Trait>trait in PlanetaryTraits)
        {
            trait.Value.Randomize();
        }
    }
    private void OnEnable()
    {
        HexTileInfo.OnNeedUIElementsForTile += CheckNewTileOptions;
        HexTileInfo.OnLandToConquer += AddToLandCount;
        HexTileInfo.OnTakeover += RemoveLandCount;
    }
    private void OnDisable()
    {
        HexTileInfo.OnNeedUIElementsForTile -= CheckNewTileOptions;
        HexTileInfo.OnLandToConquer -= AddToLandCount;
        HexTileInfo.OnTakeover -= RemoveLandCount;
    }
    private void OnDestroy()
    {
        Main.OnWorldMap -= TurnOffVisibility;
    } 
    #endregion

    #region Setup
    public void AssignMain(Main m)
    {
        main = m;
    }
    public void SetLandConfiguration(int landStarters, float landFrequency, float ratio, int densityMin, int densityMax)
    {
        landStartingPointsForSpawning = landStarters;
        frequencyForLandSpawning = landFrequency;
        enemyRatio = ratio;
        enemyDensityMin = densityMin;
        enemyDensityMax = densityMax;
    }
    public void FirstPlanetaryEncounter(Spacecraft ship)
    {
        mySpaceship = ship;
    }
    public void BuildPlanetData(string[] hextiles, string address, bool viewing)
    {
        myAddress = address;
        isViewing = viewing;

        BuildTileBase();

        OrganizePieces();

        if(hextiles != null)
        {
            SetHexTileInformationFromMemory(hextiles);
        }
        else
        {
            GetStarter();
        }

        if (!viewing)
        {
            SaveLocationInfo();
            main.SaveLocationAddressBook();
            OnGreetManagers?.Invoke(this);
            if(starter != null)OnCameraLookAtStarter?.Invoke(starter.transform);
        }
    }
    void BuildTileBase()
    {
        List<HexTileInfo[]> mainTemp = new();
        List<Vector2> locs = new();

        if (main.isPlanet)
        {
            locationXBounds = UnityEngine.Random.Range(30, 45);
            locationYBounds = UnityEngine.Random.Range(25, 35);
        }
        else
        {
            locationXBounds = UnityEngine.Random.Range(20, 35);
            locationYBounds = UnityEngine.Random.Range(10, 25);
            PlanetaryTraits["groundCoverType"].SetValue(4);
        }

        int tempVariation = UnityEngine.Random.Range(1, 4);
        for (int x = 0; x < locationXBounds; x++)
        {
            List<HexTileInfo> temp = new();
            bool odd = x % 2 == 1;
            for (int y = 0; y < locationYBounds; y++)
            {
                GameObject obj = Instantiate(tilePrefab, new Vector3(x * 0.75f, 0f, (odd) ? y * .87f + .43f : y * .87f), Quaternion.identity, transform);
                HexTileInfo tf = obj.GetComponent<HexTileInfo>();
                tf.SetManager(this);
                tf.SetMain(main);
                tf.SetUpTileLocation(x, y);
                tf.SetLandConfiguration(GetIdUsingPerlin(x, y), frequencyForLandSpawning, enemyRatio, enemyDensityMin, enemyDensityMax);
                tf.SetTemperatureDistribution(tempVariation);
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
        int x = 0, y = 0;
        foreach(string s in hextileslist)
        {
            if (y == locationYBounds)
            {
                x++;
                y = 0;
            }
            string[] ar = s.Split(":");
            tileInfoList[x][y].SetAllTileInfoFromMemory(ar[0], ar[1]);
            y++;
        }
    }
    public void OrganizePieces()
    {
        SetNeighbors();
        SetVegitation();
        SetIdenticalEdges();
        SetAllTopographics();
        SetUpItems();
    }
    private void SetNeighbors()
    {
        foreach (HexTileInfo[] tileArray in tileInfoList)
        {
            foreach (HexTileInfo tile in tileArray)
            {
                tile.SetNeighbors(FindNeighbors(tile.myPositionInTheArray));
            }
        }
    }
    private void SetVegitation()
    {
        int planetVeg = PlanetaryTraits["groundCoverType"].RangeValue;
        for (int i = 0; i < planetVeg*2; i++){
            bool suitable = false;
            while (!suitable)
            {
                int j = UnityEngine.Random.Range(0, TileLocations.Length);
                int[] ar = new int[]{Mathf.RoundToInt(TileLocations[j].x), Mathf.RoundToInt(TileLocations[j].y)};
                HexTileInfo tile = tileInfoList[ar[0]][ar[1]];
                if (tile.GetTerrainHeight() > 1)
                {
                    int Veg = UnityEngine.Random.Range(1, planetVeg+1);
                    tile.SetVegitation(Veg, true);
                    suitable = true;
                }
            }
        }
    }
    private void SetIdenticalEdges()
    {
        for (int i = 0; i < tileInfoList.Length; i++)
        {
            tileInfoList[i][locationYBounds - 1].SetTopographicLevel();
        }
        for (int i = 0; i < tileInfoList[0].Length; i++)
        {
            tileInfoList[locationXBounds - 1][i].SetTopographicLevel();
        }
    }
    private void SetAllTopographics()
    {
        foreach (HexTileInfo[] tileArray in tileInfoList)
        {
            foreach (HexTileInfo tile in tileArray)
            {
                tile.SetTopographyGraphic();
            }
        }
    }
    private void SetUpItems()
    {
        foreach (HexTileInfo[] tileArray in tileInfoList)
        {
            foreach (HexTileInfo tile in tileArray)
            {
                tile.CreateMyItems();
            }
        }
    }
    private void GetStarter()
    {
        if (!isViewing)
        {
            bool suitable = false;
            while (!suitable)
            {
                int tileIndex = UnityEngine.Random.Range(0, TileLocations.Length + 1);
                Vector2 ar = TileLocations[tileIndex];
                HexTileInfo tile = tileInfoList[Mathf.RoundToInt(ar.x)][Mathf.RoundToInt(ar.y)];
                if(tile.GetTerrainHeight() == 2 && tile.GetTerrainVegitation() == 0)
                {
                    tile.SetAsStartingPoint(mySpaceship);
                    starter = tile;
                    suitable = true;
                }
            }
        }
    }
    int GetIdUsingPerlin(int x, int y)
    {
        float rawPerlin = Mathf.PerlinNoise(
            (x+main.xOffset)/main.magnification,
            (y+main.yOffset)/main.magnification
            );
        float clampPerlin = Mathf.Clamp(rawPerlin, 0.0f, 1.0f);
        float scalePerlin = clampPerlin * Main.TraitLibrary[Array.IndexOf(Main.TraitNameReference,"steepness")].RangeMaximum;//steepness level
        if(scalePerlin == 5) scalePerlin = 4;
        return Mathf.FloorToInt(scalePerlin);
    }
    public void GiveATileAShip(Spacecraft ship, Vector2 tile, bool startLanding)
    {
        mySpaceship = ship;
        HexTileInfo info = tileInfoList[Mathf.RoundToInt(tile.x)][Mathf.RoundToInt(tile.y)];
        info.SetShip(ship);
        if (startLanding)
        {
            mySpaceship.AssignTileLocation(tile);
            info.StartLandingAnimation();
        } 
    }
    public Vector2 FindSuitableLandingSpace()
    {
        foreach(HexTileInfo[] ar in tileInfoList)
        {
            foreach(HexTileInfo tile in ar)
            {
                if(tile.CheckTileIsEmpty() && !tile.hasShip && tile.enemies.currentAmount == 0)
                {
                    return tile.myPositionInTheArray;
                }
            }
        }
        return  starter.myPositionInTheArray;
    }
    #endregion

    #region Location Checks For Neighbors
    private Vector2[] FindNeighbors(Vector2 location)
    {
        List<Vector2> neg = new();
        Vector2 vectorNull = new(-1, -1);

        Vector2 v = CheckUpLocation(location);
        if(v != vectorNull) neg.Add(v);

        v = CheckDownLocation(location);
        if (v != vectorNull) neg.Add(v);

        v = CheckLeftEqualLocation(location);
        if (v != vectorNull) neg.Add(v);

        v = CheckRightEqualLocation(location);
        if (v != vectorNull) neg.Add(v);

        if(location.x % 2 == 1)
        {
            v = CheckLeftUpLocation(location);
            if (v != vectorNull) neg.Add(v);

            v = CheckRightUpLocation(location);
            if (v != vectorNull) neg.Add(v);

        }
        else
        {
            v = CheckLeftDownLocation(location);
            if (v != vectorNull) neg.Add(v);

            v = CheckRightDownLocation(location);
            if (v != vectorNull) neg.Add(v);
        }
        return neg.ToArray();
    }
    public Vector2 CheckLeftDownLocation(Vector2 location)
    {
        Vector2 v = new(location.x - 1, location.y - 1);
        return (CheckLocationMatch(v)) ? v : new(-1, -1);
    }
    public Vector2 CheckLeftUpLocation(Vector2 location)
    {
        Vector2 v = new(location.x - 1, location.y + 1);
        return (CheckLocationMatch(v)) ? v : new(-1, -1);
    }
    public Vector2 CheckLeftEqualLocation(Vector2 location)
    {
        Vector2 v = new(location.x - 1, location.y);
        return (CheckLocationMatch(v)) ? v : new(-1, -1);
    }
    public Vector2 CheckRightDownLocation(Vector2 location)
    {
        Vector2 v = new(location.x + 1, location.y - 1);
        return (CheckLocationMatch(v)) ? v : new(-1, -1);
    }
    public Vector2 CheckRightUpLocation(Vector2 location)
    {
        Vector2 v = new(location.x + 1, location.y + 1);
        return (CheckLocationMatch(v)) ? v : new(-1, -1);
    }
    public Vector2 CheckRightEqualLocation(Vector2 location)
    {
        Vector2 v = new(location.x + 1, location.y);
        return (CheckLocationMatch(v)) ? v : new(-1, -1);
    }
    public Vector2 CheckUpLocation(Vector2 location)
    {
        Vector2 v = new(location.x, location.y + 1);
        return (CheckLocationMatch(v)) ? v : new(-1, -1);
    }
    public Vector2 CheckDownLocation(Vector2 location)
    {
        Vector2 v = new(location.x, location.y - 1);
        return (CheckLocationMatch(v)) ? v : new(-1, -1);
    }
    bool CheckLocationMatch(Vector2 location)
    {
        return Array.Exists(TileLocations, loc => loc.x == location.x && loc.y == location.y);
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
        OnTurnActiveManager?.Invoke(this);
        gameObject.SetActive(true);
        OnCameraLookAtStarter?.Invoke(starter.transform);
    }
    private void CheckNewTileOptions(HexTileInfo tile)
    {
        UIItemManager i = tile.myUIManager;
        if (activeManager == null) activeManager = i;
        if (i != activeManager)
        {
            activeManager.ResetUI();
            activeManager.DeactivateSelf();
            activeManager = i;
            activeManager.ActivateSelf();
            activeManager.ResetUI();
        }
        else if (i != activeManager)
        {
            i.DeactivateSelf();
        }else if (i == activeManager && !activeManager.interactiblesContainer.activeInHierarchy)
        {
            activeManager.ActivateSelf();
            activeManager.ResetUI();
        }
        activeManager.transform.SetAsLastSibling(); //Ensures that it is the highest in the hierarchy
    }
    #endregion

    #region Life Cycle
    private void RemoveLandCount(Vector2 tile)
    {
        landLeftToConquer--;
        if (landLeftToConquer == 0) Main.PushMessage("Land Conquered!", $"Congradulations you have conquered all of {myAddress}!" +
            $" There are many more locations out there to conquer.");
        //We will need to check if the Planet has been named or not and allow naming.
    }
    private void AddToLandCount()
    {
        landLeftToConquer++;
    }
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
    #endregion
}
