using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

public class HexTileInfo : MonoBehaviour
{
    public static Action<Transform> OnStartingTile = delegate { };
    public static Action OnLanded = delegate { };
    public static Action OnLeaving = delegate { };
    public static Action<Vector2> OnTakeover = delegate { };
    public static Action<Vector2> TypeTransform = delegate { };
    public static Action<Vector2> LookingForNeighbors = delegate { };
    public static Action<HexTileInfo> OnNeedUIElementsForTile = delegate { };
    public static Action OnResetStateToPreviousFromInteraction = delegate { };
    public static Action OnReinstateInteractability = delegate { };
    public static Action<Vector2, string, int> OnTradeWithPartnerTile = delegate { };
    public static Action<Vector2> OnReturnPositionToGeneralManager = delegate { };
    public static Action OnLandToConquer = delegate { };

    public enum TileStates { UnClickable, Clickable, Conquered, UnPlayable, Pickable, ReturnTile}
    public TileStates myState = TileStates.UnPlayable;
    TileStates previousState = TileStates.UnPlayable;

    // References
    LocationManager myLocationManager;
    Main main;
    Camera myCamera;
    public UIItemManager myUIManager;
    public Spacecraft myShip;
    
    // Objects
    [SerializeField]
    Texture2D[] tileTextures;
    [SerializeField]
    Sprite[] topographySprites;
    [SerializeField]
    Sprite[] buildingSprites;
    [SerializeField]
    Sprite[] armySprites;
    [SerializeField]
    Renderer[] myRenderers;
    [SerializeField]
    TMP_Text FloatingText;
    [SerializeField]
    GameObject dependenceButtonPrefab;
    [SerializeField]
    LineRenderer lineRenderer;
    public GameObject canvasContainerPrefab;
    public Transform myCanvasContainer;

    // Basic Values
    public int myTileType = 0;
    float frequencyOfLandDistribution;
    int oddsOfTerraFormation; //always 1 in number set
    public Vector2 myPositionInTheArray;
    Vector3 tileTruePosition;
    public bool isStartingPoint;
    Vector2[] myNeighbors;

    // Mouse Interactions
    public bool isMousePresent;
    public bool isInteractable = true;

    // Starter Spaceship Values
    Transform spaceship;
    Vector3 shipTruePosition;
    Vector3 shipStartPosition;
    Vector3 shipEndPosition;
    float spaceshipSequenceTimer;
    float spaceshipSequenceDesiredTime;
    bool isInitializingLandingSequence = false;
    bool isInitializingLeavingSequence = false;
    bool launched = false;

    public bool hasShip = false;

    // Item References
    public ItemData[] myItemData;
    ItemData units;
    public ItemData enemies;
    int enemyCount;

    // Trade Information
    public Vector2 itemTradingBuddy;
    public int potentialAmountToReceive;
    
    // Line
    bool isDrawingRayForPicking;

    // Que values
    int[] QuedItemAmount;
    string[] ItemNameReferenceIndex;

    // Enemy Values
    float enemyRatio;
    int enemyDensityMin;
    int enemyDensityMax;

    //Traits
    AtmosphericTraits PlanetTraits;
    TerrainTraits MyGroundTraits;
    string TopographicalName;

    #region Debugging
    bool generalsTimerOverride;
    float generalsTimer;

    private void RevealTileInfoInConsole(Vector2 tile)
    {
        if (tile == myPositionInTheArray) Debug.Log(DigitizeForSerialization());
    }
    private void RevealTileLocation()
    {
        CheckNullUIManager();
        if (FloatingText.gameObject.activeInHierarchy)
        {
            FloatingText.gameObject.SetActive(false);
            return;
        }
        FloatingText.gameObject.SetActive(true);
        FloatingText.text = $"{myPositionInTheArray}";
    }
    private void RevealEnemies()
    {
        if (CheckEnemyAmount() > 0)
        {
            CheckNullUIManager();
            if (!myRenderers[4].enabled)
            {
                ShowEnemyOnTile();
                return;
            }
            myRenderers[4].enabled = false;
            FloatingText.gameObject.SetActive(false);
        }        
    }
    int CheckEnemyAmount()
    {
        return enemies.currentAmount;
    }
    public ItemData GetItem(string itemName)
    {
        foreach(ItemData item in myItemData)
        {
            if(item.itemName == itemName)
            {
                return item;
            }
        }
        return null;
    }
    private void TurnLandConquered(Vector2 tile)
    {
        if (myPositionInTheArray != tile && (myState == TileStates.Clickable || myState == TileStates.UnClickable))
        {
            myRenderers[0].material.mainTexture = tileTextures[3];
            myState = TileStates.Conquered;
            OnTakeover -= CheckForPlayability;
            OnTakeover(myPositionInTheArray);
        }
    }
    #endregion

    #region Unity Methods
    private void OnEnable()
    {
        OnTakeover += CheckForPlayability;
        TypeTransform += TryToTransformToLand;
        OnReinstateInteractability += ReinstateInteractability;
        OnLeaving += TurnOffEnemyFloatingText;
        OnReturnPositionToGeneralManager += ResetFromGeneralsInteraction;
        CameraController.OnZoomRelocateUI += SetButtonsOnScreenPosition;
        CameraController.OnZoomedOutTurnOffUI += DeactivateTileOptions;
        UIItemManager.OnTilePickInteraction += CheckTileInteratability;
        GeneralsContainerManager.OnNeedTileForGeneral += PrepareForReturnTileLocation;
        Spacecraft.OnLocalLanding += ReceiveLandingShip;
        Spacecraft.OnRequestingShipDestination += TurnOffEnemyFloatingText;
        Spacecraft.OnGoToPlanetForShip += TurnOffEnemyFloatingText;
        if(hasShip) Spacecraft.OnLaunchSpaceCraft += LaunchShip;

        Main.OnRevealTileLocations += RevealTileLocation;
        Main.OnRevealTileSpecificInformation += RevealTileInfoInConsole;
        Main.OnRevealEnemies += RevealEnemies;
        LocationManager.OnTurnAllLandConquered += TurnLandConquered;
    }
    private void OnDisable()
    {
        OnTakeover -= CheckForPlayability;
        TypeTransform -= TryToTransformToLand;
        OnReinstateInteractability -= ReinstateInteractability;
        OnLeaving -= TurnOffEnemyFloatingText;
        OnReturnPositionToGeneralManager -= ResetFromGeneralsInteraction;
        CameraController.OnZoomRelocateUI -= SetButtonsOnScreenPosition;
        CameraController.OnZoomedOutTurnOffUI -= DeactivateTileOptions;
        UIItemManager.OnTilePickInteraction -= CheckTileInteratability;
        GeneralsContainerManager.OnNeedTileForGeneral -= PrepareForReturnTileLocation;
        Spacecraft.OnLocalLanding -= ReceiveLandingShip;
        Spacecraft.OnRequestingShipDestination -= TurnOffEnemyFloatingText;
        Spacecraft.OnGoToPlanetForShip -= TurnOffEnemyFloatingText;
        Spacecraft.OnLaunchSpaceCraft -= LaunchShip;

        Main.OnRevealTileLocations -= RevealTileLocation;
        Main.OnRevealTileSpecificInformation -= RevealTileInfoInConsole;
        Main.OnRevealEnemies -= RevealEnemies;
        LocationManager.OnTurnAllLandConquered -= TurnLandConquered;
    }
    private void Update()
    {
        if (isInitializingLandingSequence)
        {
            LandShip();
        }
        if (isInitializingLeavingSequence)
        {
            LaunchShip();
        }
        if (isDrawingRayForPicking)
        {
            DrawPickingLine();
        }
        if(UIOverrideListener.isOverUI && transform.position != tileTruePosition)
        {
            transform.position = tileTruePosition;
        }
    }
    #endregion

    #region Setup
    #region Basic
    public void SetMain(Main m)
    {
        main = m;
    }
    public void SetManager(LocationManager manager)
    {
        myLocationManager = manager;
    }
    public void SetFloatingText(TMP_Text text)
    {
        FloatingText = text;
    }
    public void SetUpTileLocation(int column, int row)
    {
        myPositionInTheArray = new Vector2(column, row);

        if(myPositionInTheArray.x == 0 || myPositionInTheArray.x == myLocationManager.locationXBounds-1 ||
           myPositionInTheArray.y == 0 || myPositionInTheArray.y == myLocationManager.locationYBounds-1)
        {
            //This makes sure the edges cannot be a land piece
            TypeTransform -= TryToTransformToLand;
        }

        DeactivateDetailLayers();
        myCamera = Camera.main;
        tileTruePosition = transform.position;
        shipTruePosition = myRenderers[4].transform.position;
        shipEndPosition = shipTruePosition;
        shipStartPosition = new Vector3(shipEndPosition.x, shipEndPosition.y + 6f, shipEndPosition.z);
    }
    public void SetNeighbors(Vector2[] locations)
    {
        myNeighbors = locations;
    }
    public int GetItemSpritesLengthForStartPoint()
    {
        return topographySprites.Length + 1;
    }
    public void SetLandConfiguration(AtmosphericTraits aTraits, float landDistribution, int formationOdds, float ratio, int densityMin, int densityMax)
    {
        PlanetTraits = aTraits;
        frequencyOfLandDistribution = landDistribution;
        oddsOfTerraFormation = formationOdds;
        enemyRatio = ratio;
        enemyDensityMin = densityMin;
        enemyDensityMax = densityMax;
    }
    #endregion

    #region Build Tiles
    private void TryToTransformToLand(Vector2 pos)
    {
        if ((myState == TileStates.UnPlayable && pos.x == myPositionInTheArray.x && (pos.y + 1 == myPositionInTheArray.y || pos.y - 1 == myPositionInTheArray.y)) || //Same column +- row
            (myState == TileStates.UnPlayable && pos.x % 2 == 0 && pos.x + 1 == myPositionInTheArray.x && (pos.y == myPositionInTheArray.y || pos.y - 1 == myPositionInTheArray.y)) || //Even +Column +=row
            (myState == TileStates.UnPlayable && pos.x % 2 == 1 && pos.x + 1 == myPositionInTheArray.x && (pos.y == myPositionInTheArray.y || pos.y + 1 == myPositionInTheArray.y)) || //Odd +Column -=row 
            (myState == TileStates.UnPlayable && pos.x % 2 == 0 && pos.x - 1 == myPositionInTheArray.x && (pos.y == myPositionInTheArray.y || pos.y - 1 == myPositionInTheArray.y)) || //Even -Column +=row 
            (myState == TileStates.UnPlayable && pos.x % 2 == 1 && pos.x - 1 == myPositionInTheArray.x && (pos.y == myPositionInTheArray.y || pos.y + 1 == myPositionInTheArray.y))) //Odd -Column -=row 
        {
            if (Mathf.RoundToInt(UnityEngine.Random.Range(0f, 2f) * frequencyOfLandDistribution) == 1)
            {
                    TurnLand();
            }
        }
    }
    public void TurnLand()
    {
        TypeTransform -= TryToTransformToLand;
        TypeTransform?.Invoke(myPositionInTheArray);
        OnLandToConquer?.Invoke();
        myState = TileStates.UnClickable;
        myRenderers[0].material.mainTexture = tileTextures[1];
        CreateTerrainTraits();
        CreateTerrain();        
    }

    #region Topography Visuals
    private void CreateTerrainTraits()
    {
        MyGroundTraits = new();
        MyGroundTraits.SetGeneralTraits(PlanetTraits);
        MyGroundTraits.SetTerrainTraits(UnityEngine.Random.Range(0, 4), UnityEngine.Random.Range(0, 4), UnityEngine.Random.Range(0, 4),
            UnityEngine.Random.Range(0, 4), UnityEngine.Random.Range(0, 4), UnityEngine.Random.Range(0, 4), UnityEngine.Random.Range(0, 4),
            UnityEngine.Random.Range(0, 4), UnityEngine.Random.Range(0, 4), UnityEngine.Random.Range(0, 4), UnityEngine.Random.Range(0, 4),
            UnityEngine.Random.Range(0, 4));
    }
    private void CreateTerrain()
    {
        //Adjective order:
        //quantity, opinion, size, physical quality, age, shape, colour, pattern, origin, material, type, purpose
        //Ex:12,    unusual, big,        thin,     young, round, blue, checkered, American, metal,four-sided, hammer
        CreateMyItems();
        AssembleTemperature(); //physical quality
        AssembleToxicity(); //physical quality
        AssembleVegitaion(); //physical quality
        AssembleItems();//material
        AssembleTopography(); //purpose
        SetTopographyGraphic();
    }

    private void AssembleTemperature()
    {
        switch (MyGroundTraits.Temperature)
        {
            case 0:
                TopographicalName += "Frozen ";
                myRenderers[0].material.mainTexture = tileTextures[6];
                break;
            case 1:
                TopographicalName += "Frosted ";
                break;
            case 2:
                TopographicalName += "Warm ";
                break;
            case 3:
                TopographicalName += "Blistering ";
                myRenderers[0].material.mainTexture = tileTextures[4];
                break;
        }
    }
    private void AssembleToxicity()
    {
        if(MyGroundTraits.SolidityLiquidity > 0)
        {
            switch (MyGroundTraits.LiquidToxicity)
            {
                case 1:
                    TopographicalName += "Noxious ";
                    break;
                case 2:
                    TopographicalName += "Toxic ";
                    break;
                case 3:
                    TopographicalName += "Deadly ";
                    break;
            }
        }
    }
    private void AssembleVegitaion()
    {        
        switch (MyGroundTraits.Vegitation)
        {
            case 0:
                TopographicalName += "Barren";
                break;
            case 1:
                TopographicalName += "Lush";
                break;
            case 2:
                TopographicalName += "Dense";
                break;
            case 3:
                TopographicalName += "Overgrown";
                break;
        }
    }
    private void AssembleItems()
    {
        if(myItemData.Length > 2)
        {
            foreach(ItemData item in myItemData)
            {
                if(item.itemName != "soldier" || item.itemName != "enemy")
                {
                    TopographicalName += $" {item.displayName} filled";
                    break;
                }
            }
        }
    }
    private void AssembleTopography()
    {
        //River, Lava, Oasis, Valley, FloatingIslands,
        switch (MyGroundTraits.SteepnessOfTopography)
        {
            case 0:
                if (MyGroundTraits.SolidityLiquidity == 0 && MyGroundTraits.Vegitation == 0)
                {
                    if(MyGroundTraits.FluctuationOfSteepness == 0)
                    {
                        TopographicalName += " Flats";
                        break;
                    }
                    TopographicalName += " Desert";
                    break;
                }
                else if (MyGroundTraits.SolidityLiquidity == 0 && MyGroundTraits.Vegitation == 1)
                {
                    TopographicalName += " Grasslands";
                    break;
                }
                else if (MyGroundTraits.SolidityLiquidity == 0 && MyGroundTraits.Vegitation > 1)
                {
                    TopographicalName += " Forest";
                    break;
                }
                else if (MyGroundTraits.SolidityLiquidity == 1 && MyGroundTraits.Vegitation > 1)
                {
                    if(MyGroundTraits.Vegitation == 3)
                    {
                        TopographicalName += " Jungle";
                        break;
                    }
                    TopographicalName += " Marshlands";
                    break;
                }
                else if (MyGroundTraits.SolidityLiquidity == 2 && MyGroundTraits.Vegitation > 1)
                {
                    TopographicalName += " Swamps";
                    break;
                }
                else if (MyGroundTraits.SolidityLiquidity == 3)
                {
                    TopographicalName += " Lakes";
                    break;
                }
                else if (MyGroundTraits.Temperature < 1)
                {
                    TopographicalName += " Tundra";
                    break;
                }
                else
                {
                    TopographicalName += " Plains";
                    break;
                }
            case 1:
                if (MyGroundTraits.FluctuationOfSteepness == 1 && MyGroundTraits.SolidityLiquidity > 1 && MyGroundTraits.Temperature == 0)
                {
                    TopographicalName += " Glaciers";
                    break;
                }
                else if (MyGroundTraits.FluctuationOfSteepness == 1)
                {
                    TopographicalName += " Mounds";
                    break;
                }
                else if (MyGroundTraits.FluctuationOfSteepness == 2)
                {
                    if(MyGroundTraits.Vegitation == 0)
                    {
                        TopographicalName += " Dunes";
                        break;
                    }
                    TopographicalName += " Hills";
                    break;
                }
                else if (MyGroundTraits.FluctuationOfSteepness == 3)
                {
                    TopographicalName += " Rolling Hills";
                    break;
                }
                else
                {
                    TopographicalName += " Butte";
                    break;
                }
            case 2:
                if(MyGroundTraits.SolidityLiquidity == 1)
                {
                    TopographicalName += " Waterfalls";
                    break;
                }else if (MyGroundTraits.SolidityLiquidity == 2)
                {
                    TopographicalName += " Mudslides";
                    break;
                }
                else if (MyGroundTraits.SolidityLiquidity < 2 && MyGroundTraits.FluctuationOfSteepness > 1)
                {
                    TopographicalName += " Canyons";
                    break;
                }
                else
                {
                    TopographicalName += " Cliffs";
                    break;
                }
            case 3:
                if (MyGroundTraits.Vegitation == 0 && MyGroundTraits.Temperature > 1)
                {
                    TopographicalName += " Valcano";
                    break;
                }
                else if (MyGroundTraits.Vegitation == 3)
                {
                    TopographicalName += " Forest covered Mountains";
                    break;
                }
                else
                {
                    TopographicalName += " Mountains";
                    break;
                }
        }
    }
    public bool CheckTileIsEmpty()
    {
        if (MyGroundTraits == null) return false;
        return MyGroundTraits.SteepnessOfTopography == 0 && MyGroundTraits.SolidityLiquidity == 0 && MyGroundTraits.Vegitation == 0;
    }
    private void SetTopographyGraphic()
    {
        myRenderers[2].enabled = true;
        SpriteRenderer s = myRenderers[2].GetComponent<SpriteRenderer>();
        switch (MyGroundTraits.SteepnessOfTopography)
        {
            case 0:
                switch (MyGroundTraits.Vegitation)
                {
                    case 0:
                        myRenderers[2].enabled = false;
                        myTileType = GetBlankTileIndex();
                        break;
                    case 1:
                        s.sprite = topographySprites[3];
                        break;
                    case 2:
                        s.sprite = topographySprites[4];
                        break;
                    case 3:
                        s.sprite = topographySprites[5];
                        break;
                }
                break;
            case 1:
                s.sprite = topographySprites[0];
                break;
            case 2:
                s.sprite = topographySprites[1];
                break;
            case 3:
                s.sprite = topographySprites[2];
                break;
        }

        myRenderers[1].enabled = true;
        Material m = myRenderers[1].material;
        switch (MyGroundTraits.LiquidToxicity)
        {
            case 0:
                myRenderers[1].enabled = false;
                break;
            case 1:
                m.mainTexture = tileTextures[7];
                break;
            case 2:
                m.mainTexture = tileTextures[9];
                break;
            case 3:
                m.mainTexture = tileTextures[18];
                break;
        }
    }
    #endregion

    void TrySpawnEnemy()
    {
        if(UnityEngine.Random.Range(0,1f) > 1 - enemyRatio)
        {
            int x = Main.NormalizeRandom(enemyDensityMin, enemyDensityMax);
            enemies.SetCurrentAmount(x);
            enemyCount = x;
        }
    }
    public void SetAsStartingPoint()
    {
        isStartingPoint = true;
        myRenderers[0].material.mainTexture = tileTextures[3];
        myState = TileStates.Conquered;
    }
    public void SetAsStartingPoint(Spacecraft ship)
    {
        isStartingPoint = true;
        myRenderers[0].material.mainTexture = tileTextures[3];
        myState = TileStates.Conquered;
        OnTakeover -= CheckForPlayability;
        OnTakeover?.Invoke(myPositionInTheArray);

        if(ship != null)
        {
            myShip = ship;
            OffloadItemsFromShip(ship.MyItemsData);
            ship.OffloadItems();
            ship.AssignTileLocation(myPositionInTheArray);
            hasShip = true;
            if (!CheckIfItemIsInMyArray("barracks")) AddItemToMyItems("barracks");
            StartLandingAnimation();
            return;
        }
        AddItemToMyItems("soldier");
        foreach(ItemData item in myItemData)
        {
            if(item.itemName == "soldier") units = item;
            else if(item.itemName == "enemy") item.SetCurrentAmount(0);
        }
        units.SetCurrentAmount(5);
        CheckUnitVisual();
    }
    #endregion

    #region Setup Tiles From Memory
    public void SetAllTileInfoFromMemory(string state, int tileType, string neighbors, bool isStart, string resources, string topographicVisual, string terrainValues)
    {
        SetTileStateFromString(state);

        myTileType = tileType;

        SetNeighborsFromString(neighbors);

        BuildItemsFromMemory(resources);

        CreateQueElements();

        if (isStart) SetAsStartingPoint();

        MyGroundTraits = new(terrainValues);
        TopographicalName = topographicVisual;
        SetTopographyGraphic();
    }
    void SetTileStateFromString(string state)
    {
        switch (state)
        {
            case "UnPlayable":
                myRenderers[0].material.mainTexture = tileTextures[0];
                myState = TileStates.UnPlayable;
                break;
            case "UnClickable":
                myRenderers[0].material.mainTexture = tileTextures[1];
                myState = TileStates.UnClickable;
                OnLandToConquer?.Invoke();
                break;
            case "Clickable":
                myRenderers[0].material.mainTexture = tileTextures[2];
                myState = TileStates.Clickable;
                OnTakeover -= CheckForPlayability;
                OnLandToConquer?.Invoke();
                break;
            case "Conquered":
                myRenderers[0].material.mainTexture = tileTextures[3];
                myState = TileStates.Conquered;
                OnTakeover -= CheckForPlayability;
                break;
        }
    }
    void SetNeighborsFromString(string neighbors)
    {
        List<Vector2> temp = new();
        string[] ar = neighbors.Split("'");
        foreach (string s in ar)
        {
            string[] st = s.Split(",");
            Vector2 v = new(float.Parse(st[0]), float.Parse(st[1].Remove(0, 1)));
            temp.Add(v);
        }

        myNeighbors = new Vector2[temp.Count];
        for (int i = 0; i < myNeighbors.Length; i++)
        {
            myNeighbors[i] = temp[i];
        }
    }
    void BuildItemsFromMemory(string items)
    {
        if (items != "")
        {
            List<ItemData> temp = new();
            string[] ar = items.Split(";");
            foreach (string s in ar)
            {
                string[] st = s.Split(",");
                //string name, string display, string dis, string gr, string eType, string reqs, string nonReqs, bool vis, int cur, int autoA, float craft,
                //string created, string coms, string createComs, string im, string snd, string ach, int mos, string build
                temp.Add(new ItemData(st[0], st[1], st[2], st[3], st[4], st[5], st[6],
                        st[7] == "True", int.Parse(st[8]), int.Parse(st[9]),
                        float.Parse(st[10]), st[11], st[12], st[13], st[14], st[15], st[16],
                        int.Parse(st[17]), st[18]));
            }

            myItemData = temp.ToArray();
            foreach (ItemData item in myItemData)
            {
                if (Main.needCompareForUpdatedValues) Main.CompareIndividualItemValues(main, item);

                if (item.itemName == "soldier")
                {
                    units = item;
                    if (item.currentAmount > 0)
                    {
                        myRenderers[4].enabled = true;
                    }
                    continue;
                }
                if (item.itemName == "enemy")
                {
                    enemies = item;
                    enemyCount = enemies.currentAmount;
                }
            }
        }
    }
    #endregion
    #endregion

    #region Animations
    public void StartLandingAnimation()
    {        
        spaceship = myRenderers[4].transform;
        OnStartingTile?.Invoke(spaceship);
        SpawnShipVisual();
        spaceshipSequenceTimer = 0;
        spaceshipSequenceDesiredTime = 6f;
        isInitializingLandingSequence = true;
    }
    void LandShip()
    {
        spaceship.position = Vector3.Lerp(shipStartPosition, shipEndPosition, spaceshipSequenceTimer / spaceshipSequenceDesiredTime);

        spaceshipSequenceTimer += Time.deltaTime;
        if (spaceshipSequenceTimer > spaceshipSequenceDesiredTime)
        {
            isInitializingLandingSequence = false;
            spaceship.position = shipEndPosition;
            OnLanded?.Invoke();
        }
    }
    public void StartLaunchAnimation()
    {
        spaceship = myRenderers[4].transform;
        spaceshipSequenceTimer = 0;
        spaceshipSequenceDesiredTime = 6f;
        isInitializingLeavingSequence = true;
        Spacecraft.OnGoToPlanetForShip += RemoveShip;
    }
    void LaunchShip()
    {
        spaceship.position = Vector3.Lerp(shipEndPosition, shipStartPosition, spaceshipSequenceTimer / spaceshipSequenceDesiredTime);

        spaceshipSequenceTimer += Time.deltaTime;
        if (spaceshipSequenceTimer > spaceshipSequenceDesiredTime)
        {
            isInitializingLeavingSequence = false;
            myRenderers[4].enabled = false;
            spaceship.position = shipEndPosition;
            CheckUnitVisual();
        }
    }
    void DrawPickingLine()
    {
        Vector3 myPos = transform.position;
        myPos.y += 1f;
        myPos.z -= 1f;
        lineRenderer.SetPosition(0, myPos);
        lineRenderer.SetPosition(1, myCamera.ScreenToWorldPoint(Input.mousePosition));
    }  
    #endregion

    #region Ship Management
    public void SetShip(Spacecraft ship)
    {
        myShip = ship;
        SpawnShipVisual();
    }
    public void SpawnShipVisual()
    {
        Spacecraft.OnLaunchSpaceCraft += LaunchShip;
        SpriteRenderer rend = myRenderers[4].GetComponent<SpriteRenderer>();
        rend.enabled = true;
        rend.sprite = armySprites[^1];
        hasShip = true;
    }
    void LaunchShip(Vector2 location)
    {
        if(location == myPositionInTheArray && !launched)
        {
            launched = true;
            Spacecraft.OnLaunchSpaceCraft -= LaunchShip;
            StartLaunchAnimation();
            RemoveItemAndRebuildItems("barracks");
        }
    }
    void ReceiveLandingShip(Vector2 location)
    {
        if(location == myPositionInTheArray)
        {
            StartLandingAnimation();
            Spacecraft.OnLaunchSpaceCraft += LaunchShip; 
            OnTakeover -= CheckForPlayability;
            OnTakeover?.Invoke(myPositionInTheArray);
            AddItemToMyItems("barracks");
        }
    }
    private void RemoveShip(Spacecraft ship)
    {
        if(ReferenceEquals(ship, myShip))
        {
            hasShip = false;
            myShip = null;
            Spacecraft.OnGoToPlanetForShip -= RemoveShip;
        }
    }
    public int GetBlankTileIndex()
    {
        return topographySprites.Length + 1;
    }
    #endregion

    #region Qeue
    public void CreateQueElements()
    {
        QuedItemAmount = new int[myItemData.Length];
        ItemNameReferenceIndex = new string[myItemData.Length];
        
        for(int i = 0; i < myItemData.Length; i++)
        {
            ItemNameReferenceIndex[i] = myItemData[i].displayName;
        }
    }
    public void AddToQue(ItemData item, int amount)
    {
        if (item == null || !item.visible) return;

        QuedItemAmount[System.Array.IndexOf(ItemNameReferenceIndex, item.displayName)] += amount;

        if(QuedItemAmount[System.Array.IndexOf(ItemNameReferenceIndex, item.displayName)] == 1)
            StartQueUpdate(item);
    }
    public void StartQueUpdate(ItemData item)
    {
        StartCoroutine(UpdateQue(item));
    }
    IEnumerator UpdateQue(ItemData item)
    {
        bool addedNormal = false;
        myUIManager.SetTimerAndStart(item.craftTime);

        yield return new WaitForSeconds(item.craftTime);

        int queIndex = System.Array.IndexOf(ItemNameReferenceIndex, item.displayName);
        int queAmount = QuedItemAmount[queIndex];
        if (queAmount > 0 || item.autoAmount > 0)
        {
            if (queAmount > 0)
            {
                QuedItemAmount[queIndex] -= 1;
                addedNormal = true;
            }

            item.AdjustCurrentAmount(addedNormal ? 1 + item.autoAmount : item.autoAmount);

            if (item.itemName == "soldier") myUIManager.ResetUnitText();

            if (QuedItemAmount[queIndex] > 0 || item.autoAmount > 0)
                StartCoroutine(UpdateQue(item));
        }
    }
    #endregion

    #region Mouse Interactions
    public void OnMouseEnter()
    {
        if (!UIOverrideListener.isOverUI) 
        {
            isMousePresent = true;
            Vector3 v = transform.position;
            v.y += .1f;
            transform.position = v;
        }
    }
    private void OnMouseExit()
    {
        isMousePresent = false;
        if(transform.position != tileTruePosition) transform.position = tileTruePosition;
    }
    private void OnMouseDown()
    {
        if (isMousePresent && isInteractable && !UIOverrideListener.isOverUI)
        {
            GetNameAndValues();
            if(myState == TileStates.Conquered && myCamera.orthographicSize < 4.25f)
            {
                CheckCanvasContainerAndCreateTileOptions();
                OnNeedUIElementsForTile?.Invoke(this);
            }else if(myState == TileStates.Pickable)
            {
                if(enemies.currentAmount == 0)
                {
                    StartCoroutine(Move());
                    return;
                }
                StartCoroutine(BattleSequence());
            }else if(myState == TileStates.ReturnTile)
            {
                OnReturnPositionToGeneralManager?.Invoke(myPositionInTheArray);
            }
        }
    }
    #endregion

    void GetNameAndValues()
    {
        Debug.Log($"{TopographicalName} Steep: {MyGroundTraits.SteepnessOfTopography}, Veg: {MyGroundTraits.Vegitation}, Rad: {MyGroundTraits.Radioactivity}, Water: {MyGroundTraits.SolidityLiquidity}");
        foreach(ItemData item in myItemData)
        {
            if(item.itemName != "soldier" && item.itemName != "enemy")
            {
                Debug.Log(item.itemName);
            }
        }
    }

    #region Unit Management
    public void AdjustUnits(int amount)
    {
        if(units.currentAmount + amount <= 0)
        {
            units.SetCurrentAmount(0);
        }
        else
        {
            units.AdjustCurrentAmount(amount);
        }
        CheckUnitVisual();
    }
    void CheckUnitVisual()
    {
        if (hasShip && myShip.status == "Waiting")
        {
            SpriteRenderer rend = myRenderers[4].GetComponent<SpriteRenderer>();
            rend.enabled = true;
            rend.sprite = armySprites[^1];
        }
        else if(units.currentAmount > 0)
        {
            SpriteRenderer rend = myRenderers[4].GetComponent<SpriteRenderer>();
            rend.enabled = true;
            rend.sprite = armySprites[0];

        }else if (units.currentAmount == 0 && myRenderers[4].enabled)
        {
            myRenderers[4].enabled = false;
        }
    }
    public int GetUnitCount()
    {
        return units.currentAmount;
    }
    public IEnumerator Move()
    {
        OnReinstateInteractability?.Invoke();
        OnResetStateToPreviousFromInteraction?.Invoke();
        float timer =(generalsTimerOverride) ? generalsTimer : 2f;
        CheckNullUIManager();
        myUIManager.SetTimerAndStart(timer);

        yield return new WaitForSeconds(timer);

        generalsTimerOverride = false;
        AdjustUnits(potentialAmountToReceive);
        myRenderers[0].material.mainTexture = tileTextures[3];
        OnTakeover -= CheckForPlayability;
        OnTakeover?.Invoke(myPositionInTheArray);
        myState = TileStates.Conquered;
    }
    public IEnumerator BattleSequence()
    {
        float timer = 2f;
        //create some more interesting timer;
        SetTimerAndStart(timer);
        OnReinstateInteractability?.Invoke();
        OnResetStateToPreviousFromInteraction?.Invoke();

        yield return new WaitForSeconds(timer);

        int difference = enemyCount - potentialAmountToReceive;
        if (difference < 0)
        {
            AdjustUnits(potentialAmountToReceive);
            enemies.SetCurrentAmount(0);
            enemyCount = 0;
            myRenderers[0].material.mainTexture = tileTextures[3];
            OnTakeover -= CheckForPlayability;
            OnTakeover?.Invoke(myPositionInTheArray);
            myState = TileStates.Conquered;
            CheckNullUIManager();
            FloatingText.gameObject.SetActive(false);
            potentialAmountToReceive = 0;
        }
        else if (difference == 0)
        {
            OnTradeWithPartnerTile?.Invoke(itemTradingBuddy, "soldier", potentialAmountToReceive);
            ShowEnemyOnTile();
        }else if (difference > 0)
        {
            if (Main.cantLose) OnTradeWithPartnerTile?.Invoke(itemTradingBuddy, "soldier", potentialAmountToReceive);
            ShowEnemyOnTile();
        }

    }
    void CheckTileInteratability(string type, Vector2 tile, int amount)
    {
        potentialAmountToReceive = amount;

        if((myState == TileStates.Clickable || myState == TileStates.Conquered) && type == "soldier" && CheckForNeighbor(tile))
        {
            myRenderers[0].material.color = Color.red;
            isInteractable = true;
            previousState = myState;
            myState = TileStates.Pickable;
            OnResetStateToPreviousFromInteraction += ResetTileStateFromInteraction;
            itemTradingBuddy = tile;
            return;
        }
        //Otherwise don't let them interact
        isInteractable = false;
    }
    bool CheckForNeighbor(Vector2 pos)
    {
        foreach(Vector2 v in myNeighbors)
        {
            if (v == pos) return true;
        }
        return false;
    }
    public void SetupReceivingOfUnitsForOriginator()
    {
        OnTradeWithPartnerTile += ReceiveUnitsFromTrade;
    }
    void ReceiveUnitsFromTrade(Vector2 tile, string item, int amount)
    {
        if (tile != myPositionInTheArray) return;

        OnTradeWithPartnerTile -= ReceiveUnitsFromTrade; 

        foreach(ItemData data in myItemData)
        {
            if(data.itemName == item)
            {
                data.AdjustCurrentAmount(amount);
                if(data.itemName == "soldier")
                {
                    CheckUnitVisual();
                }
                return;
            }
        }

        foreach(ItemData data in main.GetItemLibrary("ItemLibrary"))
        {
            if (data.itemName == item)
            {
                List<ItemData> l = new();
                foreach(ItemData dt in myItemData)
                {
                    l.Add(dt);
                }
                ItemData d = new(data);
                l.Add(d);
                myItemData = l.ToArray();
                return;
            }
        }
    }

    #region Generals Management
    private void PrepareForReturnTileLocation()
    {
        previousState = myState;
        myState = TileStates.ReturnTile;
        myRenderers[0].material.color = Color.blue;
    }
    private void ResetFromGeneralsInteraction(Vector2 tile)
    {
        myState = previousState;
        myRenderers[0].material.color = Color.white;
    }
    public void ReceiveGeneralMove(int units, float timer)
    {
        potentialAmountToReceive = units;
        generalsTimerOverride = true;
        generalsTimer = timer;
        StartCoroutine(Move());
    }
    public void SetResourceTradingBuddy(Vector2 tile)
    {
        itemTradingBuddy = tile;
    }
    #endregion
    #endregion

    #region Item Specific Methods
    public ItemData CheckIfAndUseOwnItems(ItemData item)
    {
        foreach(ItemData data in myItemData)
        {
            if(!ReferenceEquals(data, item) && item.itemName == data.itemName) return data;
        }
        return item;
    }
    public ItemData GetResourceByString(string itemName)
    {
        foreach(ItemData item in myItemData)
        {
            if(itemName == item.itemName) return item;
        }
        return null;
    }
    private void OffloadItemsFromShip(ItemData[] items)
    {
        foreach (ItemData item in items)
        {
            AddItemToMyItems(item);
        }
    }
    private bool CheckIfItemIsInMyArray(string itemName)
    {
        foreach (ItemData item in myItemData)
        {
            if (item.itemName == itemName)
            {
                return true;
            }
        }
        return false;
    }
    void AddItemToMyItems(string itemName)
    {
        List<ItemData> temp = new();
        foreach (ItemData item in myItemData)
        {
            temp.Add(item);
            if (item.itemName == itemName) return;
        }

        foreach (ItemData item in main.GetItemLibrary("ItemLibrary"))
        {
            if (itemName == item.itemName)
            {
                ItemData d = new(item);
                if(itemName == "barracks") d.SetCurrentAmount(1);
                temp.Add(d);
                break;
            }
        }

        myItemData = temp.ToArray();
    }
    void AddItemToMyItems(ItemData item)
    {
        List<ItemData> temp = new();
        bool isAlreadyHere = false;
        foreach (ItemData data in myItemData)
        {
            temp.Add(data);
            if (item.itemName == data.itemName)
            {
                data.AdjustCurrentAmount(item.currentAmount);
                isAlreadyHere = true;
            }
        }
        if (!isAlreadyHere) temp.Add(new ItemData(item));

        myItemData = temp.ToArray();
    }
    void CreateMyItems()
    {
        List<ItemData> tempToPermanent = new();
        List<ItemData> locationTypeOptionList = new();

        foreach (ItemData item in main.GetItemLibrary("BasicItemLibrary")) // Need all terrain types outlined
        {
            string[] ar = item.nonConsumableRequirements.Split(" ");
            bool needAdd = true;
            for(int i = 0; i < ar.Length; i++)
            {
                if (ar[i].Contains("="))
                {
                    string[] a = ar[i].Split("=");
                    if(!CheckTerrainValue(a[0], a[1], '='))
                    {
                        needAdd = false;
                        break;
                    }
                }
                else if (ar[i].Contains(">"))
                {
                    string[] a = ar[i].Split(">");
                    if (!CheckTerrainValue(a[0], a[1], '>'))
                    {
                        needAdd = false;
                        break;
                    }
                }
                else if (ar[i].Contains("<"))
                {
                    string[] a = ar[i].Split("<");
                    if (!CheckTerrainValue(a[0], a[1], '<'))
                    {
                        needAdd = false;
                        break;
                    }
                }
            }
            if (needAdd)
            {
                locationTypeOptionList.Add(new(item));
                continue;
            }
        }
        foreach (ItemData item in main.GetItemLibrary("ItemLibrary"))
        {
            if (item.itemName == "soldier") //Universal item
            {
                units = new ItemData(item);
                tempToPermanent.Add(units);
                continue;
            }
            if (item.itemName == "enemy") //Universal item  && myTileType != 0
            {
                enemies = new ItemData(item);
                tempToPermanent.Add(enemies);
                TrySpawnEnemy();
                continue;
            }
        }

        if (locationTypeOptionList.Count > 0)//Getting a random resource that we can have.
        {
            tempToPermanent.Add(locationTypeOptionList[UnityEngine.Random.Range(0, locationTypeOptionList.Count)]);
        }

        myItemData = tempToPermanent.ToArray();

        if (Main.needCompareForUpdatedValues)
        {
            foreach (ItemData item in myItemData)
            {
                Main.CompareIndividualItemValues(main, item);
            }
        }

        CreateQueElements();
    }
    private bool CheckTerrainValue(string name, string value, char valueAccessor)
    {
        return valueAccessor switch
        {
            '=' => name switch
            {
                "Acidity" => MyGroundTraits.Acidity == int.Parse(value),
                "AcousticAbsorbtion" => MyGroundTraits.AcousticAbsorption == int.Parse(value),
                "Compressability" => MyGroundTraits.Compressability == int.Parse(value),
                "Density" => MyGroundTraits.Density == int.Parse(value),
                "ElectricalCharge" => MyGroundTraits.ElectricalCharge == int.Parse(value),
                "FrequencyEmission" => MyGroundTraits.FrequencyEmission == int.Parse(value),
                "Flammability" => MyGroundTraits.Flammability == int.Parse(value),
                "Luminescence" => MyGroundTraits.Luminescence == int.Parse(value),
                "Temperature" => MyGroundTraits.Temperature == int.Parse(value),
                "Magnetism" => MyGroundTraits.Magnetism == int.Parse(value),
                "Pressure" => MyGroundTraits.Pressure == int.Parse(value),
                "Radioactivity" => MyGroundTraits.Radioactivity == int.Parse(value),
                "Refelctivity" => MyGroundTraits.Reflectivity == int.Parse(value),
                "Smell" => MyGroundTraits.Smell == int.Parse(value),
                "Transparency" => MyGroundTraits.Transparency == int.Parse(value),
                "BoilingPoint" => MyGroundTraits.BoilingPoint == int.Parse(value),
                "Brittleness" => MyGroundTraits.Brittleness == int.Parse(value),
                "Elasticity" => MyGroundTraits.Elasticity == int.Parse(value),
                "FluctuationOfSteepness" => MyGroundTraits.FluctuationOfSteepness == int.Parse(value),
                "Gravity" => MyGroundTraits.Gravity == int.Parse(value),
                "Hardness" => MyGroundTraits.Hardness == int.Parse(value),
                "LiquidToxicity" => MyGroundTraits.LiquidToxicity == int.Parse(value),
                "Sharpness" => MyGroundTraits.Sharpness == int.Parse(value),
                "SolidityLiquidity" => MyGroundTraits.SolidityLiquidity == int.Parse(value),
                "SteepnessOfTopography" => MyGroundTraits.SteepnessOfTopography == int.Parse(value),
                "Structure" => MyGroundTraits.Structure == int.Parse(value),
                "Vegitation" => MyGroundTraits.Vegitation == int.Parse(value),
                _ => false,
            },
            //Greater than
            '>' => name switch
            {
                "Acidity" => MyGroundTraits.Acidity > int.Parse(value),
                "AcousticAbsorbtion" => MyGroundTraits.AcousticAbsorption > int.Parse(value),
                "Compressability" => MyGroundTraits.Compressability > int.Parse(value),
                "Density" => MyGroundTraits.Density > int.Parse(value),
                "ElectricalCharge" => MyGroundTraits.ElectricalCharge > int.Parse(value),
                "FrequencyEmission" => MyGroundTraits.FrequencyEmission > int.Parse(value),
                "Flammability" => MyGroundTraits.Flammability > int.Parse(value),
                "Luminescence" => MyGroundTraits.Luminescence > int.Parse(value),
                "Temperature" => MyGroundTraits.Temperature > int.Parse(value),
                "Magnetism" => MyGroundTraits.Magnetism > int.Parse(value),
                "Pressure" => MyGroundTraits.Pressure > int.Parse(value),
                "Radioactivity" => MyGroundTraits.Radioactivity > int.Parse(value),
                "Refelctivity" => MyGroundTraits.Reflectivity > int.Parse(value),
                "Smell" => MyGroundTraits.Smell > int.Parse(value),
                "Transparency" => MyGroundTraits.Transparency > int.Parse(value),
                "BoilingPoint" => MyGroundTraits.BoilingPoint > int.Parse(value),
                "Brittleness" => MyGroundTraits.Brittleness > int.Parse(value),
                "Elasticity" => MyGroundTraits.Elasticity > int.Parse(value),
                "FluctuationOfSteepness" => MyGroundTraits.FluctuationOfSteepness > int.Parse(value),
                "Gravity" => MyGroundTraits.Gravity > int.Parse(value),
                "Hardness" => MyGroundTraits.Hardness > int.Parse(value),
                "LiquidToxicity" => MyGroundTraits.LiquidToxicity > int.Parse(value),
                "Sharpness" => MyGroundTraits.Sharpness > int.Parse(value),
                "SolidityLiquidity" => MyGroundTraits.SolidityLiquidity > int.Parse(value),
                "SteepnessOfTopography" => MyGroundTraits.SteepnessOfTopography > int.Parse(value),
                "Structure" => MyGroundTraits.Structure > int.Parse(value),
                "Vegitation" => MyGroundTraits.Vegitation > int.Parse(value),
                _ => false,
            },
            //Less than
            '<' => name switch
            {
                "Acidity" => MyGroundTraits.Acidity < int.Parse(value),
                "AcousticAbsorbtion" => MyGroundTraits.AcousticAbsorption < int.Parse(value),
                "Compressability" => MyGroundTraits.Compressability < int.Parse(value),
                "Density" => MyGroundTraits.Density < int.Parse(value),
                "ElectricalCharge" => MyGroundTraits.ElectricalCharge < int.Parse(value),
                "FrequencyEmission" => MyGroundTraits.FrequencyEmission < int.Parse(value),
                "Flammability" => MyGroundTraits.Flammability < int.Parse(value),
                "Luminescence" => MyGroundTraits.Luminescence < int.Parse(value),
                "Temperature" => MyGroundTraits.Temperature < int.Parse(value),
                "Magnetism" => MyGroundTraits.Magnetism < int.Parse(value),
                "Pressure" => MyGroundTraits.Pressure < int.Parse(value),
                "Radioactivity" => MyGroundTraits.Radioactivity < int.Parse(value),
                "Refelctivity" => MyGroundTraits.Reflectivity < int.Parse(value),
                "Smell" => MyGroundTraits.Smell < int.Parse(value),
                "Transparency" => MyGroundTraits.Transparency < int.Parse(value),
                "BoilingPoint" => MyGroundTraits.BoilingPoint < int.Parse(value),
                "Brittleness" => MyGroundTraits.Brittleness < int.Parse(value),
                "Elasticity" => MyGroundTraits.Elasticity < int.Parse(value),
                "FluctuationOfSteepness" => MyGroundTraits.FluctuationOfSteepness < int.Parse(value),
                "Gravity" => MyGroundTraits.Gravity < int.Parse(value),
                "Hardness" => MyGroundTraits.Hardness < int.Parse(value),
                "LiquidToxicity" => MyGroundTraits.LiquidToxicity < int.Parse(value),
                "Sharpness" => MyGroundTraits.Sharpness < int.Parse(value),
                "SolidityLiquidity" => MyGroundTraits.SolidityLiquidity < int.Parse(value),
                "SteepnessOfTopography" => MyGroundTraits.SteepnessOfTopography < int.Parse(value),
                "Structure" => MyGroundTraits.Structure < int.Parse(value),
                "Vegitation" => MyGroundTraits.Vegitation < int.Parse(value),
                _ => false,
            },
            _ => false,
        };
    }
    public void RemoveItemAndRebuildItems(string itemName)
    {
        List<ItemData> temp = new();
        foreach(ItemData item in myItemData)
        {
            if (item.itemName == itemName) continue;
            temp.Add(item);
        }
        myItemData = temp.ToArray();
    }
    #endregion

    #region UI Management
    void CheckForPlayability(Vector2 pos)
    {
        if((myState == TileStates.UnClickable && pos.x == myPositionInTheArray.x && (pos.y +1 == myPositionInTheArray.y || pos.y - 1 == myPositionInTheArray.y)) || //Same column +- row
            (myState == TileStates.UnClickable && pos.x % 2 == 0 && pos.x + 1 == myPositionInTheArray.x && (pos.y == myPositionInTheArray.y || pos.y - 1 == myPositionInTheArray.y)) || //Even +Column +=row
            (myState == TileStates.UnClickable && pos.x % 2 == 1 && pos.x + 1 == myPositionInTheArray.x && (pos.y == myPositionInTheArray.y || pos.y + 1 == myPositionInTheArray.y)) || //Odd +Column -=row 
            (myState == TileStates.UnClickable && pos.x % 2 == 0 && pos.x - 1 == myPositionInTheArray.x && (pos.y == myPositionInTheArray.y || pos.y - 1 == myPositionInTheArray.y)) || //Even -Column +=row 
            (myState == TileStates.UnClickable && pos.x % 2 == 1 && pos.x - 1 == myPositionInTheArray.x && (pos.y == myPositionInTheArray.y || pos.y + 1 == myPositionInTheArray.y))) //Odd -Column -=row 
        {
            myState = TileStates.Clickable;
            myRenderers[0].material.mainTexture = tileTextures[2];
            OnTakeover -= CheckForPlayability;
        }
    }
    void SetTimerAndStart(float timer)
    {
        CheckNullUIManager();
        myUIManager.SetTimerAndStart(timer);
    }
    void CheckNullUIManager()
    {
        if (myUIManager == null)
        {
            CheckCanvasContainerAndCreateTileOptions();
            myUIManager.DeactivateSelf();
        }
    }
    void CheckCanvasContainerAndCreateTileOptions()
    {
        if (myCanvasContainer == null) CreateTileOptions();
    }
    void CreateTileOptions()
    {
        myCanvasContainer = GameObject.Find("Canvas").transform;
        GameObject obj = Instantiate(canvasContainerPrefab, myCanvasContainer);
        myCanvasContainer = obj.transform;
        myUIManager = obj.GetComponent<UIItemManager>();
        myUIManager.SetMyTileAndMain(this, main);
        myUIManager.CreateItemButtons();
        SetButtonsOnScreenPosition();
    }
    void TurnOffEnemyFloatingText() //Accessed via action
    {
        FloatingText.gameObject.SetActive(false);
    }
    void TurnOffEnemyFloatingText(Spacecraft ship)
    {
        if(FloatingText != null)
        FloatingText.gameObject.SetActive(false);
    }
    void TurnOffEnemyFloatingText(int access)
    {
        if(FloatingText != null)
        FloatingText.gameObject.SetActive(false);
    }
    void ShowEnemyOnTile()
    {
        CheckNullUIManager();
        FloatingText.gameObject.SetActive(true);
        FloatingText.text = enemyCount.ToString();
        SpriteRenderer rend = myRenderers[4].GetComponent<SpriteRenderer>();
        rend.enabled = true;
        rend.sprite = armySprites[1];
    }
    void DeactivateTileOptions()
    {
        if (myUIManager != null) myUIManager.DeactivateSelf();
    }
    public void SetButtonsOnScreenPosition()
    {
        if (myCanvasContainer != null)
        {
            myCanvasContainer.position = myCamera.WorldToScreenPoint(transform.position);
            myCanvasContainer.localScale = new Vector3(1.75f / myCamera.orthographicSize, 1.75f / myCamera.orthographicSize, 1f);
        }
    }
    public void StartDrawingRayForPicking()
    {
        isDrawingRayForPicking = true;
    }
    void ResetTileStateFromInteraction()
    {
        myRenderers[0].material.color = Color.white;
        myState = previousState;
        OnResetStateToPreviousFromInteraction -= ResetTileStateFromInteraction;
    }
    public void ReinstateInteractability()
    {
        isInteractable = true;
        isDrawingRayForPicking = false;
        lineRenderer.SetPosition(0, Vector3.zero);
        lineRenderer.SetPosition(1, Vector3.zero);
    }
    #endregion

    #region Life Cycle
    public void DeactivateSelf()
    {
        OnTakeover -= CheckForPlayability;
        TypeTransform -= TryToTransformToLand;
        gameObject.SetActive(false);
    }
    void DeactivateDetailLayers()
    {
        myRenderers[1].enabled = false;
        myRenderers[2].enabled = false;
        myRenderers[3].enabled = false;
        myRenderers[4].enabled = false;
    }
    public string DigitizeForSerialization()
    {
        if (myState == TileStates.ReturnTile) ResetFromGeneralsInteraction(myPositionInTheArray);

        string s = "";
        bool first = true;
        foreach(Vector2 vec in myNeighbors)
        {
            string st = $"{vec}";
            st = st.Remove(0, 1);
            st = st.Remove(st.Length - 1);
            s = (first) ? s + st : s + "'" + st;
            first = false;
        }

        string str = "";
        if(myItemData.Length != 0)
        {
            foreach(ItemData item in myItemData)
            {
                str += item.DigitizeForSerialization();
                if (item == myItemData[^1])
                {
                    str = str.Remove(str.Length - 1);
                }
            }
        }

        if (MyGroundTraits == null) MyGroundTraits = new();

        TopographicalName = string.IsNullOrEmpty(TopographicalName) ? "Ocean" : TopographicalName;

        return $"{myState}:{myTileType}:{s}:{isStartingPoint}:{str}:{TopographicalName}:{MyGroundTraits.DigitizeForSerialization()}|";
    }
    #endregion
}
