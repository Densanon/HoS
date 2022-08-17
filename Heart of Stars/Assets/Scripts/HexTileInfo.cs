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

    public enum TileStates { UnClickable, Clickable, Conquered, UnPlayable, Pickable, ReturnTile}
    public TileStates myState = TileStates.UnPlayable;
    TileStates previousState = TileStates.UnPlayable;

    // References
    LocationManager myLocationManager;
    Main main;
    Camera camera;
    public UIResourceManager myUIManager;
    public Spacecraft myShip;
    
    // Objects
    [SerializeField]
    Texture2D[] tileTextures;
    [SerializeField]
    Sprite[] resourceSprites;
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
    public float frequencyOfLandDistribution;
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

    // Resource References
    public ResourceData[] myResources;
    ResourceData soldiers;
    public ResourceData enemies;
    int enemyCount;

    // Trade Information
    public Vector2 resourceTradingBuddy;
    public int potentialAmountToReceive;
    
    // Line
    bool isDrawingRayForPicking;

    // Que values
    int[] QuedResourceAmount;
    string[] ResourceNameReferenceIndex;

    #region Debugging
    float enemyRatio;
    int enemyDensityMin;
    int enemyDensityMax;

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
    public ResourceData GetResource(string itemName)
    {
        foreach(ResourceData resource in myResources)
        {
            if(resource.itemName == itemName)
            {
                return resource;
            }
        }
        return null;
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
        UIResourceManager.OnTilePickInteraction += CheckTileInteratability;
        GeneralsContainerManager.OnNeedTileForGeneral += PrepareForReturnTileLocation;
        Spacecraft.OnLocalLanding += ReceiveLandingShip;
        Spacecraft.OnRequestingShipDestination += TurnOffEnemyFloatingText;
        Spacecraft.OnGoToPlanetForShip += TurnOffEnemyFloatingText;
        if(hasShip) Spacecraft.OnLaunchSpaceCraft += LaunchShip;

        Main.OnRevealTileLocations += RevealTileLocation;
        Main.OnRevealTileSpecificInformation += RevealTileInfoInConsole;
        Main.OnRevealEnemies += RevealEnemies;
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
        UIResourceManager.OnTilePickInteraction -= CheckTileInteratability;
        GeneralsContainerManager.OnNeedTileForGeneral -= PrepareForReturnTileLocation;
        Spacecraft.OnLocalLanding -= ReceiveLandingShip;
        Spacecraft.OnRequestingShipDestination -= TurnOffEnemyFloatingText;
        Spacecraft.OnGoToPlanetForShip -= TurnOffEnemyFloatingText;
        Spacecraft.OnLaunchSpaceCraft -= LaunchShip;

        Main.OnRevealTileLocations -= RevealTileLocation;
        Main.OnRevealTileSpecificInformation -= RevealTileInfoInConsole;
        Main.OnRevealEnemies -= RevealEnemies;
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
            TypeTransform -= TryToTransformToLand;
        }

        DeactivateDetailLayers();
        camera = Camera.main;
        tileTruePosition = transform.position;
        shipTruePosition = myRenderers[4].transform.position;
        shipEndPosition = shipTruePosition;
        shipStartPosition = new Vector3(shipEndPosition.x, shipEndPosition.y + 6f, shipEndPosition.z);
    }
    public void SetNeighbors(Vector2[] locations)
    {
        myNeighbors = locations;
    }
    public int GetResourceSpritesLengthForStartPoint()
    {
        return resourceSprites.Length + 1;
    }
    public void SetEnemyNumbers(float ratio, int densityMin, int densityMax)
    {
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
        myState = TileStates.UnClickable;
        myRenderers[0].material.mainTexture = tileTextures[1];

        if (UnityEngine.Random.Range(0, 4) == 0)
        { 
            myTileType = UnityEngine.Random.Range(1, resourceSprites.Length+1);
            myRenderers[2].enabled = true;
            myRenderers[2].GetComponent<SpriteRenderer>().sprite = resourceSprites[myTileType-1];
            CreateResources();
            return;
        }
        myTileType = resourceSprites.Length+1; // ensures blank space is always more than sprites
        CreateResources();
        ///1 mountain
        ///2 tree
        ///3 mine
        ///4 wheat
        /// tent
        /// building
        /// trap?
        /// chest
        /// guy
        /// robot spider
    }
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
        myShip = ship;
        OffLoadResourcesFromShip(ship.myResources);
        ship.OffloadResources();
        ship.AssignTileLocation(myPositionInTheArray);
        hasShip = true;

        isStartingPoint = true;
        myRenderers[0].material.mainTexture = tileTextures[3];
        myState = TileStates.Conquered;
        if (!CheckIfResourceIsInMyArray("barracks")) AddResourceToMyResources("barracks");
        OnTakeover -= CheckForPlayability;
        OnTakeover?.Invoke(myPositionInTheArray);
        StartLandingAnimation();
    }
    #endregion

    #region Setup Tiles From Memory
    public void SetAllTileInfoFromMemory(string state, int tileType, string neighbors, bool isStart, string resources)
    {
        SetTileStateFromString(state);

        SetTileType(tileType);

        SetNeighborsFromString(neighbors);

        BuildResourcesFromMemory(resources);

        CreateQueElements();

        if (isStart) SetAsStartingPoint();
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
                break;
            case "Clickable":
                myRenderers[0].material.mainTexture = tileTextures[2];
                myState = TileStates.Clickable;
                OnTakeover -= CheckForPlayability;
                break;
            case "Conquered":
                myRenderers[0].material.mainTexture = tileTextures[3];
                myState = TileStates.Conquered;
                OnTakeover -= CheckForPlayability;
                break;
        }
    }
    void SetTileType(int tileType)
    {
        myTileType = tileType;
        if (myTileType < 5 && myTileType != 0)
        {
            myRenderers[2].GetComponent<SpriteRenderer>().sprite = resourceSprites[myTileType - 1];
            myRenderers[2].enabled = true;
        }
    }
    void SetNeighborsFromString(string neighbors)
    {
        List<Vector2> temp = new List<Vector2>();
        string[] ar = neighbors.Split("'");
        foreach (string s in ar)
        {
            string[] st = s.Split(",");
            Vector2 v = new Vector2(float.Parse(st[0]), float.Parse(st[1].Remove(0, 1)));
            temp.Add(v);
        }

        myNeighbors = new Vector2[temp.Count];
        for (int i = 0; i < myNeighbors.Length; i++)
        {
            myNeighbors[i] = temp[i];
        }
    }
    void BuildResourcesFromMemory(string resources)
    {
        if (resources != "")
        {
            List<ResourceData> temp = new List<ResourceData>();
            string[] ar = resources.Split(";");
            foreach (string s in ar)
            {
                string[] st = s.Split(",");
                //string name, string display, string dis, string gr, string eType, string reqs, string nonReqs, bool vis, int cur, int autoA, float craft,
                //string created, string coms, string createComs, string im, string snd, string ach, int mos, string build
                temp.Add(new ResourceData(st[0], st[1], st[2], st[3], st[4], st[5], st[6],
                        (st[7] == "True") ? true : false, int.Parse(st[8]), int.Parse(st[9]),
                        float.Parse(st[10]), st[11], st[12], st[13], st[14], st[15], st[16],
                        int.Parse(st[17]), st[18]));
            }

            myResources = temp.ToArray();
            foreach (ResourceData data in myResources)
            {
                if (Main.needCompareForUpdatedValues) Main.CompareIndividualResourceValues(main, data);

                if (data.itemName == "soldier")
                {
                    soldiers = data;
                    if (data.currentAmount > 0)
                    {
                        myRenderers[4].enabled = true;
                    }
                    continue;
                }
                if (data.itemName == "enemy")
                {
                    enemies = data;
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
        SpawnAShip();
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
            CheckSoldierVisual();
        }
    }
    void DrawPickingLine()
    {
        Vector3 myPos = transform.position;
        myPos.y += 1f;
        myPos.z -= 1f;
        lineRenderer.SetPosition(0, myPos);
        lineRenderer.SetPosition(1, camera.ScreenToWorldPoint(Input.mousePosition));
    }  
    #endregion

    #region Ship Management
    public void CreateShip(Spacecraft ship)
    {
        myShip = ship;
        SpawnAShip();
    }
    public void SpawnAShip()
    {
        Spacecraft.OnLaunchSpaceCraft += LaunchShip;
        SpriteRenderer rend = myRenderers[4].GetComponent<SpriteRenderer>();
        rend.enabled = true;
        rend.sprite = armySprites[armySprites.Length - 1];
        hasShip = true;
    }
    void LaunchShip(Vector2 location)
    {
        if(location == myPositionInTheArray && !launched)
        {
            launched = true;
            Spacecraft.OnLaunchSpaceCraft -= LaunchShip;
            StartLaunchAnimation();
            RemoveResourceAndRebuildResources("barracks");
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
            AddResourceToMyResources("barracks");
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
        return resourceSprites.Length + 1;
    }
    #endregion

    #region Qeue
    public void CreateQueElements()
    {
        QuedResourceAmount = new int[myResources.Length];
        ResourceNameReferenceIndex = new string[myResources.Length];
        
        for(int i = 0; i < myResources.Length; i++)
        {
            ResourceNameReferenceIndex[i] = myResources[i].displayName;
        }
    }
    public void AddToQue(ResourceData data, int amount)
    {
        if (data == null || !data.visible) return;

        QuedResourceAmount[System.Array.IndexOf(ResourceNameReferenceIndex, data.displayName)] += amount;

        if(QuedResourceAmount[System.Array.IndexOf(ResourceNameReferenceIndex, data.displayName)] == 1)
            StartQueUpdate(data);
    }
    public void StartQueUpdate(ResourceData data)
    {
        StartCoroutine(UpdateQue(data));
    }
    IEnumerator UpdateQue(ResourceData data)
    {
        bool addedNormal = false;
        myUIManager.SetTimerAndStart(data.craftTime);

        yield return new WaitForSeconds(data.craftTime);

        int queIndex = System.Array.IndexOf(ResourceNameReferenceIndex, data.displayName);
        int queAmount = QuedResourceAmount[queIndex];
        if (queAmount > 0 || data.autoAmount > 0)
        {
            if (queAmount > 0)
            {
                QuedResourceAmount[queIndex] -= 1;
                addedNormal = true;
            }

            data.AdjustCurrentAmount(addedNormal ? 1 + data.autoAmount : data.autoAmount);

            if (data.itemName == "soldier") myUIManager.ResetTroopText();

            if (QuedResourceAmount[queIndex] > 0 || data.autoAmount > 0)
                StartCoroutine(UpdateQue(data));
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
            if(myState == TileStates.Conquered && camera.orthographicSize < 4.25f)
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

    #region Troop Management
    public void AdjustSoldiers(int amount)
    {
        if(soldiers.currentAmount + amount <= 0)
        {
            soldiers.SetCurrentAmount(0);
        }
        else
        {
            soldiers.AdjustCurrentAmount(amount);
        }
        CheckSoldierVisual();
    }
    void CheckSoldierVisual()
    {
        if (hasShip && !myShip.isTraveling && !myShip.arrived)
        {
            SpriteRenderer rend = myRenderers[4].GetComponent<SpriteRenderer>();
            rend.enabled = true;
            rend.sprite = armySprites[armySprites.Length - 1];
        }
        else if(soldiers.currentAmount > 0)
        {
            SpriteRenderer rend = myRenderers[4].GetComponent<SpriteRenderer>();
            rend.enabled = true;
            rend.sprite = armySprites[0];

        }else if (soldiers.currentAmount == 0 && myRenderers[4].enabled)
        {
            myRenderers[4].enabled = false;
        }
    }
    public int GetSoldierCount()
    {
        return soldiers.currentAmount;
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
        AdjustSoldiers(potentialAmountToReceive);
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
            AdjustSoldiers(potentialAmountToReceive);
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
            OnTradeWithPartnerTile?.Invoke(resourceTradingBuddy, "soldier", potentialAmountToReceive);
            ShowEnemyOnTile();
        }else if (difference > 0)
        {
            if (Main.cantLose) OnTradeWithPartnerTile?.Invoke(resourceTradingBuddy, "soldier", potentialAmountToReceive);
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
            resourceTradingBuddy = tile;
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
    public void SetupReceivingOfTroopsForOriginator()
    {
        OnTradeWithPartnerTile += ReceiveResourcesFromTrade;
    }
    void ReceiveResourcesFromTrade(Vector2 tile, string resource, int amount)
    {
        if (tile != myPositionInTheArray) return;

        OnTradeWithPartnerTile -= ReceiveResourcesFromTrade; 

        foreach(ResourceData data in myResources)
        {
            if(data.itemName == resource)
            {
                data.AdjustCurrentAmount(amount);
                if(data.itemName == "soldier")
                {
                    CheckSoldierVisual();
                }
                return;
            }
        }

        foreach(ResourceData dt in main.GetResourceLibrary())
        {
            if (dt.itemName == resource)
            {
                List<ResourceData> l = new List<ResourceData>();
                foreach(ResourceData res in myResources)
                {
                    l.Add(res);
                }
                ResourceData d = new ResourceData(dt);
                l.Add(d);
                myResources = l.ToArray();
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
    public void ReceiveGeneralMove(int troops, float timer)
    {
        potentialAmountToReceive = troops;
        generalsTimerOverride = true;
        generalsTimer = timer;
        StartCoroutine(Move());
    }
    public void SetResourceTradingBuddy(Vector2 tile)
    {
        resourceTradingBuddy = tile;
    }
    #endregion
    #endregion

    #region Resource Specific Methods
    public ResourceData CheckIfAndUseOwnResources(ResourceData item)
    {
        foreach(ResourceData data in myResources)
        {
            if(data != item && data.itemName == item.itemName) return data;
        }
        return item;
    }
    public ResourceData GetResourceByString(string itemName)
    {
        foreach(ResourceData data in myResources)
        {
            if(itemName == data.itemName) return data;
        }
        return null;
    }
    private void OffLoadResourcesFromShip(ResourceData[] resources)
    {
        foreach (ResourceData resource in resources)
        {
            AddResourceToMyResources(resource);
        }
    }
    private bool CheckIfResourceIsInMyArray(string itemName)
    {
        foreach (ResourceData data in myResources)
        {
            if (data.itemName == itemName)
            {
                return true;
            }
        }
        return false;
    }
    void AddResourceToMyResources(string itemName)
    {
        List<ResourceData> temp = new List<ResourceData>();
        foreach (ResourceData data in myResources)
        {
            temp.Add(data);
            if (data.itemName == itemName) return;
        }

        foreach (ResourceData dt in main.GetResourceLibrary())
        {
            if (itemName == dt.itemName)
            {
                ResourceData d = new ResourceData(dt);
                if(itemName == "barracks") d.SetCurrentAmount(1);
                temp.Add(d);
                break;
            }
        }

        myResources = temp.ToArray();
    }
    void AddResourceToMyResources(ResourceData resource)
    {
        List<ResourceData> temp = new List<ResourceData>();
        bool isAlreadyHere = false;
        foreach (ResourceData data in myResources)
        {
            temp.Add(data);
            if (resource.itemName == data.itemName)
            {
                data.AdjustCurrentAmount(resource.currentAmount);
                isAlreadyHere = true;
            }
        }
        if (!isAlreadyHere) temp.Add(new ResourceData(resource));

        myResources = temp.ToArray();
    }
    void CreateResources()
    {
        List<ResourceData> tempToPermanent = new List<ResourceData>();
        List<ResourceData> locationTypeOptionList = new List<ResourceData>();

        foreach (ResourceData data in main.GetResourceLibrary())
        {
            if (myTileType == 1 && data.groups == "metal") //Mountain resources
            {
                locationTypeOptionList.Add(new ResourceData(data));
                continue;
            }
            if (myTileType == 2 && data.itemName == "food") //Forest resource
            {
                locationTypeOptionList.Add(new ResourceData(data));
                continue;
            }
            if (data.itemName == "soldier") //UniversalResource
            {
                soldiers = new ResourceData(data);
                tempToPermanent.Add(soldiers);
                continue;
            }
            if (data.itemName == "enemy" && myTileType != 0) //UniversalResource
            {
                enemies = new ResourceData(data);
                tempToPermanent.Add(enemies);
                TrySpawnEnemy();
                continue;
            }
        }

        if (locationTypeOptionList.Count > 0)//Getting a random resource that we can have.
            tempToPermanent.Add(new ResourceData(locationTypeOptionList[UnityEngine.Random.Range(0, locationTypeOptionList.Count)]));

        myResources = tempToPermanent.ToArray();

        if (Main.needCompareForUpdatedValues)
        {
            foreach (ResourceData data in myResources)
            {
                Main.CompareIndividualResourceValues(main, data);
            }
        }

        CreateQueElements();
    }
    public void RemoveResourceAndRebuildResources(string itemName)
    {
        List<ResourceData> temp = new List<ResourceData>();
        foreach(ResourceData resource in myResources)
        {
            if (resource.itemName == itemName) continue;
            temp.Add(resource);
        }
        myResources = temp.ToArray();
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
        myUIManager = obj.GetComponent<UIResourceManager>();
        myUIManager.SetMyTileAndMain(this, main);
        myUIManager.CreateResourceButtons();
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
    void SetButtonsOnScreenPosition()
    {
        if (myCanvasContainer != null)
        {
            myCanvasContainer.position = camera.WorldToScreenPoint(transform.position);
            myCanvasContainer.localScale = new Vector3(1.75f / camera.orthographicSize, 1.75f / camera.orthographicSize, 1f);
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
        if(myResources.Length != 0)
        {
            foreach(ResourceData data in myResources)
            {
                str = str + data.DigitizeForSerialization();
                if (data == myResources[myResources.Length - 1])
                {
                    str = str.Remove(str.Length - 1);
                }
            }
        }
        return $"{myState}:{myTileType}:{s}:{isStartingPoint}:{str}|";
    }
    #endregion
}
