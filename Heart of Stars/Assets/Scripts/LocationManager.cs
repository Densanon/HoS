using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationManager : MonoBehaviour
{
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

    #region General
    //enum GeneralState { Moving, Combat, Searching, Stop }
    //GeneralState myGeneralState = GeneralState.Searching;
    //enum Direction { North, NorthEast, SouthEast, South, SouthWest, NorthWest }
    //Direction generalDirection = Direction.North;
    //bool isSearching, isMoving, isFighting,directionIsPlayable,needCombat,isStopped;
    //bool northAvailable, northEastAvailable, southEastAvailable, southAvailable, southWestAvailable, northWestAvailable;
    //float generalSwitchStateTime = 4f;
    //Vector2 generalsTile;
    //Vector2 targetTile;
    //System.Random rand = new System.Random();
    //List<int> directionsAvailable;

    //void ResetDirectionsAvailable()
    //{
    //    if(directionsAvailable == null) directionsAvailable = new List<int>();
    //    directionsAvailable.Clear();
    //    for (int i = 0; i < 6; i++)
    //    {
    //        directionsAvailable.Add(i);
    //    }
    //}
    //void GetNewDirection()
    //{
    //    Debug.Log("Getting a new direction.");
    //    foreach(int i in directionsAvailable)
    //    {
    //        Debug.Log($"Directions still available to check {i}");
    //    }
    //    int x = directionsAvailable[rand.Next(0, directionsAvailable.Count)];
    //    switch (x)
    //    {
    //        case 0:
    //            Debug.Log("General is Picking North.");
    //            generalDirection = Direction.North;
    //            directionsAvailable.Remove(0);
    //            break;
    //        case 1:
    //            Debug.Log("General is Picking NorthEast.");
    //            generalDirection = Direction.NorthEast;
    //            directionsAvailable.Remove(1);
    //            break;
    //        case 2:
    //            Debug.Log("General is Picking SouthEast.");
    //            generalDirection = Direction.SouthEast;
    //            directionsAvailable.Remove(2);
    //            break;
    //        case 3:
    //            Debug.Log("General is Picking South.");
    //            generalDirection = Direction.South;
    //            directionsAvailable.Remove(3);
    //            break;
    //        case 4:
    //            Debug.Log("General is Picking SouthWest.");
    //            generalDirection = Direction.SouthWest;
    //            directionsAvailable.Remove(4);
    //            break;
    //        case 5:
    //            Debug.Log("General is Picking NorthWest.");
    //            generalDirection = Direction.NorthWest;
    //            directionsAvailable.Remove(5);
    //            break;
    //    }
    //}
    //void CheckDirectionIsPlayable()
    //{
    //    Debug.Log("Checking if the direction is playable.");
    //    switch (generalDirection)
    //    {
    //        case Direction.North:
    //            northAvailable = directionIsPlayable = CheckIfTileIsInNeedOfConquering(CheckUpLocation(generalsTile));
    //            Debug.Log($"North isPlayable = {northAvailable}");
    //            break;
    //        case Direction.NorthEast:
    //            if (generalsTile.x % 2 == 1)
    //            {
    //                northEastAvailable = directionIsPlayable = CheckIfTileIsInNeedOfConquering(CheckRightUpLocation(generalsTile));
    //                Debug.Log($"NorthEast isPlayable = {northEastAvailable}");
    //                break;
    //            }
    //            northEastAvailable = directionIsPlayable = CheckIfTileIsInNeedOfConquering(CheckRightEqualLocation(generalsTile));
    //            Debug.Log($"NorthEast isPlayable = {northEastAvailable}");
    //            break;
    //        case Direction.SouthEast:
    //            if (generalsTile.x % 2 == 1)
    //            {
    //                southEastAvailable = directionIsPlayable = CheckIfTileIsInNeedOfConquering(CheckRightEqualLocation(generalsTile));
    //                Debug.Log($"SouthEast isPlayable = {southEastAvailable}");
    //                break;
    //            }
    //            southEastAvailable = directionIsPlayable = CheckIfTileIsInNeedOfConquering(CheckRightDownLocation(generalsTile));
    //            Debug.Log($"SouthEast isPlayable = {southEastAvailable}");
    //            break;
    //        case Direction.South:
    //            southAvailable = directionIsPlayable = CheckIfTileIsInNeedOfConquering(CheckDownLocation(generalsTile));
    //            Debug.Log($"South isPlayable = {southAvailable}");
    //            break;
    //        case Direction.SouthWest:
    //            if (generalsTile.x % 2 == 1)
    //            {
    //                southWestAvailable = directionIsPlayable = CheckIfTileIsInNeedOfConquering(CheckLeftEqualLocation(generalsTile));
    //                Debug.Log($"Southwest isPlayable = {southWestAvailable}");
    //                break;
    //            }
    //            southWestAvailable = directionIsPlayable = CheckIfTileIsInNeedOfConquering(CheckLeftDownLocation(generalsTile));
    //            Debug.Log($"Southwest isPlayable = {southWestAvailable}");
    //            break;
    //        case Direction.NorthWest:
    //            if (generalsTile.x % 2 == 1)
    //            {
    //                northWestAvailable = directionIsPlayable = CheckIfTileIsInNeedOfConquering(CheckLeftUpLocation(generalsTile));
    //                Debug.Log($"Northwest isPlayable = {northWestAvailable}");
    //                break;
    //            }
    //            northWestAvailable = directionIsPlayable = CheckIfTileIsInNeedOfConquering(CheckLeftEqualLocation(generalsTile));
    //            Debug.Log($"Northwest isPlayable = {northWestAvailable}");
    //            break;
    //    }

    //    if (directionIsPlayable) StartCoroutine(SwitchGeneralState());

    //    if (!directionIsPlayable && directionsAvailable.Count == 0)
    //    {
    //        Debug.Log("General found a dead end.");
    //        main.PushMessage("General", "I have run out of places around me that aren't conquered. You shoud" +
    //            " continue on without me, or move the troops where I may start again.");
    //        directionIsPlayable = true;
    //        myGeneralState = GeneralState.Stop;
    //    }
    //}
    //bool CheckIfTileIsInNeedOfConquering(Vector2 tile)
    //{
    //    if (tile.x == -1) return false;
    //    targetTile = tile;
    //    HexTileInfo info = tileInfoList[Mathf.RoundToInt(tile.x)][Mathf.RoundToInt(tile.y)];
    //    needCombat = info.enemies.currentAmount > 0;
    //    return info.myState == HexTileInfo.TileStates.Clickable;
    //}
    //void ResetDirectionAvailabilityBools()
    //{
    //    northAvailable = true;
    //    northEastAvailable = true;
    //    southEastAvailable = true;
    //    southAvailable = true;
    //    southWestAvailable = true;
    //    northWestAvailable = true;
    //}
    //void ExecuteGeneralState()
    //{
    //    switch (myGeneralState)
    //    {
    //        case GeneralState.Searching:
    //            if (!isSearching)
    //            {
    //                isSearching = true;
    //                directionIsPlayable = false;
    //                ResetDirectionAvailabilityBools();
    //                ResetDirectionsAvailable();
    //                while (!directionIsPlayable)
    //                {
    //                    GetNewDirection();
    //                    CheckDirectionIsPlayable();
    //                }
    //            }
    //            break;
    //        case GeneralState.Moving:
    //            if (!isMoving)
    //            {
    //                isMoving = true;
    //                HexTileInfo current = tileInfoList[Mathf.RoundToInt(generalsTile.x)][Mathf.RoundToInt(generalsTile.y)];
    //                int soldierAmount = current.GetSoldierCount();
    //                current.AdjustSoldiers(soldierAmount * -1);
    //                HexTileInfo target = tileInfoList[Mathf.RoundToInt(targetTile.x)][Mathf.RoundToInt(targetTile.y)];
    //                target.ReceiveGeneralMove(soldierAmount, generalSwitchStateTime);
    //                StartCoroutine(SwitchGeneralState());
    //            }
    //            break;
    //        case GeneralState.Combat:
    //            if (!isFighting)
    //            {
    //                isFighting = true;
    //                HexTileInfo current = tileInfoList[Mathf.RoundToInt(generalsTile.x)][Mathf.RoundToInt(generalsTile.y)];
    //                int soldierAmount = current.GetSoldierCount();
    //                Debug.Log($"I have {soldierAmount} soldiers. On {generalsTile}");
    //                current.AdjustSoldiers(soldierAmount * -1);
    //                Debug.Log($"There are now {current.GetSoldierCount()} soldiers. On {generalsTile}");
    //                HexTileInfo target = tileInfoList[Mathf.RoundToInt(targetTile.x)][Mathf.RoundToInt(targetTile.y)];
    //                target.potentialAmountToReceive = soldierAmount;
    //                Debug.Log($"I will be facing {target.enemies.currentAmount}");
    //                if (target.enemies.currentAmount - soldierAmount < 0)
    //                {
    //                    Debug.Log("I will overcome the new direction.");
    //                    generalsTile = targetTile;
    //                    StartCoroutine(SwitchGeneralState());
    //                }
    //                else
    //                {
    //                    Debug.Log("I either tied or died.");
    //                    myGeneralState = GeneralState.Stop;
    //                }
    //                target.StartCoroutine(target.BattleSequence());
    //            }
    //            break;
    //        case GeneralState.Stop:
    //            if (!isStopped)
    //            {
    //                isStopped = true;
    //                Debug.Log("Stopped.");
    //            }
    //            break;
    //    }
    //}
    //IEnumerator SwitchGeneralState()
    //{
    //    Debug.Log($"Switching state and waiting {generalSwitchStateTime}");

    //    yield return new WaitForSeconds(generalSwitchStateTime);

    //    Debug.Log("Made it through the wait.");

    //    if (myGeneralState == GeneralState.Searching)
    //    {
    //        if (needCombat)
    //        {
    //            Debug.Log("Switching from searching to Combat.");
    //            isFighting = false;
    //            myGeneralState = GeneralState.Combat;
    //        }
    //        else
    //        {
    //            Debug.Log("Switching from searching to Moving.");
    //            isMoving = false;
    //            myGeneralState = GeneralState.Moving;
    //        }
    //    }
    //    else if (myGeneralState == GeneralState.Moving)
    //    {
    //        Debug.Log("Switching from Moving to Searching.");
    //        generalsTile = targetTile;
    //        isSearching = false;
    //        myGeneralState = GeneralState.Searching;

    //    }
    //    else if (myGeneralState == GeneralState.Combat)
    //    {
    //        Debug.Log("Switching from Combat to Searching.");
    //        isSearching = false;
    //        myGeneralState = GeneralState.Searching;
    //    }
    //}
    #endregion

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
        foreach (ResourceData data in starter.myResources)
        {
            if (data.itemName == "soldier")
            {
                data.SetCurrentAmount(amount);
                continue;
            }
        }
    }
    public void BuildPlanetData(string[] hextiles, string address)
    {
        myAddress = address;

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
                d = Mathf.FloorToInt(k / locationXBounds);
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
                if (!start && tile.myTileType == tile.GetResourceSpritesLengthForStartPoint())
                {
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

    #region GeneralManagement
    public void CreateAGeneral()
    {
        GameObject go = Instantiate(GeneralPrefab, transform);
        General g = go.GetComponent<General>();
        g.BasicSetup(this, General.GeneralType.Basic);
        myGenerals.Add(g);
        Main.PushMessage("Hurray!", "You have a new general that can work for you! Give it a name and a troop location to start!");
    }
    public void GiveAnUnNamedGeneralAName(string name)
    {
        foreach(General g in myGenerals)
        {
            if(g.Name == "")
            {
                g.SetGeneralName(name);
                Main.PushMessage($"General {name}", "Reporting for duty! Make sure I have a troop location otherwise I won't know what to do.");
                return;
            }
        }
    }
    public void StartGeneral()
    {
        foreach(General g in myGenerals)
        {
            g.ActivateGeneral();
        }
    }
    public void SetGeneralLocation(Vector2 location)
    {
        myGenerals[0].SetGeneralTroopLocation(location);
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
            main.SaveLocationAddressBook();
        }
    }

    private void OnApplicationQuit()
    {
        SaveLocationInfo();
    }
}
