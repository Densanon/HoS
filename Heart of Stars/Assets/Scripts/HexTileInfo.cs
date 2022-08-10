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

    [SerializeField]
    GameObject BattleObject;
    [SerializeField]
    int enemyCount;
    [SerializeField]
    TMP_Text FloatingText;

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
    Transform spaceship;
    [SerializeField]
    Vector2[] myNeighbors;
    public ResourceData[] myResources;
    ResourceData soldiers;
    public ResourceData enemies;
    
    [SerializeField]
    GameObject dependenceButtonPrefab;

    public GameObject canvasContainerPrefab;
    public Transform myCanvasContainer;

    public Vector2 myPositionInTheArray;
    public Vector2 resourceTradingBuddy;

    public float frequencyOfLandDistribution;

    public enum TileStates { UnClickable, Clickable, Conquered, UnPlayable, Pickable}
    [SerializeField]
    public TileStates myState = TileStates.UnPlayable;
    TileStates previousState = TileStates.UnPlayable;

    public int myTileType = 0;
    LocationManager myManager;
    Main main;
    Camera camera;

    public UIResourceManager myResourceManager;
    int[] QuedResourceAmount;
    string[] ResourceNameReferenceIndex;

    bool isInitializingLandingSequence = false;
    bool isInitializingLeavingSequence = false;
    Vector3 startPosition;
    Vector3 endPosition;
    float spaceshipSequenceTimer;
    float spaceshipSequenceDesiredTime;
    public bool isStartingPoint;
    public bool isMousePresent;
    public bool isInteractable = true;
    public int potentialAmountToReceive;

    [SerializeField]
    LineRenderer lineRenderer;
    bool isDrawingRayForPicking;

    #region Debugging
    float enemyRatio;
    int enemyDensityMin;
    int enemyDensityMax;

    bool generalTimerOverride;
    float generalTimer;

    private void RevealTileInfoInConsole(Vector2 tile)
    {
        if (tile == myPositionInTheArray) Debug.Log(DigitizeForSerialization());
    }
    private void RevealTileLocation()
    {
        CheckNullFloatingText();
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
            CheckNullFloatingText();
            if (!myRenderers[4].enabled)
            {
                ShowEnemyOnTile();
                return;
            }
            myRenderers[4].enabled = false;
            FloatingText.gameObject.SetActive(false);
        }        
    }
    public void SetEnemyNumbers(float ratio, int densityMin, int densityMax)
    {
        enemyRatio = ratio;
        enemyDensityMin = densityMin;
        enemyDensityMax = densityMax;
    }
    int CheckEnemyAmount()
    {
        return enemies.currentAmount;
    }

    public void ReceiveGeneralMove(int troops, float timer)
    {
        potentialAmountToReceive = troops;
        generalTimerOverride = true;
        generalTimer = timer;
        StartCoroutine(Move());
    }
    #endregion

    #region Unity Methods
    private void OnEnable()
    {
        OnTakeover += CheckForPlayability;
        TypeTransform += TryToTransformToLand;
        OnReinstateInteractability += ReinstateInteractability;
        OnLeaving += TurnOffAllFloatingText;
        CameraController.OnZoomRelocateUI += SetButtonsOnScreenPosition;
        CameraController.OnZoomedOutTurnOffUI += DeactivateTileOptions;
        UIResourceManager.OnTilePickInteraction += CheckTileInteratability;

        Main.OnRevealTileLocations += RevealTileLocation;
        Main.OnRevealTileSpecificInformation += RevealTileInfoInConsole;
        Main.OnRevealEnemies += RevealEnemies;
    }
    private void OnDisable()
    {
        OnTakeover -= CheckForPlayability;
        TypeTransform -= TryToTransformToLand;
        OnReinstateInteractability -= ReinstateInteractability;
        OnLeaving -= TurnOffAllFloatingText;
        CameraController.OnZoomRelocateUI -= SetButtonsOnScreenPosition;
        CameraController.OnZoomedOutTurnOffUI -= DeactivateTileOptions;
        UIResourceManager.OnTilePickInteraction -= CheckTileInteratability;

        Main.OnRevealTileLocations -= RevealTileLocation;
        Main.OnRevealTileSpecificInformation -= RevealTileInfoInConsole;
        Main.OnRevealEnemies -= RevealEnemies;
    }
    private void Update()
    {
        if (isInitializingLandingSequence)
        {
            spaceship.position = Vector3.Lerp(startPosition, endPosition, spaceshipSequenceTimer/spaceshipSequenceDesiredTime);

            spaceshipSequenceTimer += Time.deltaTime;
            if(spaceshipSequenceTimer > spaceshipSequenceDesiredTime)
            {
                isInitializingLandingSequence = false;
                spaceship.position = endPosition;
                OnLanded?.Invoke();
            }
        }
        if (isInitializingLeavingSequence)
        {
            spaceship.position = Vector3.Lerp(endPosition, startPosition, spaceshipSequenceTimer / spaceshipSequenceDesiredTime);

            spaceshipSequenceTimer += Time.deltaTime;
            if (spaceshipSequenceTimer > spaceshipSequenceDesiredTime)
            {
                isInitializingLeavingSequence = false;
                OnLeaving?.Invoke();
                spaceship.position = endPosition;
            }
        }       
    }
    private void FixedUpdate()
    {
        if (isDrawingRayForPicking)
        {
            //Debug.Log("Drawing.");
            Vector3 myPos = transform.position;
            myPos.y += 1f;
            myPos.z -= 1f;
            lineRenderer.SetPosition(0, myPos);
            lineRenderer.SetPosition(1, camera.ScreenToWorldPoint(Input.mousePosition));
        }
    }
    #endregion

    #region Qeue
    public void StartQueUpdate(ResourceData data)
    {
        Debug.Log($"I have been told to start the que for {data.displayName}");
        StartCoroutine(UpdateQue(data));
    }
    IEnumerator UpdateQue(ResourceData data)
    {
        bool addedNormal = false;
        yield return new WaitForSeconds(data.craftTime);
        Debug.Log($"Created: {data.itemName}");
        if (QuedResourceAmount[System.Array.IndexOf(ResourceNameReferenceIndex, data.displayName)] > 0 || data.autoAmount > 0)
        {
            Debug.Log($"Starting {data.displayName} Que Update process.");
            if (QuedResourceAmount[System.Array.IndexOf(ResourceNameReferenceIndex, data.displayName)] > 0)
            {
                QuedResourceAmount[System.Array.IndexOf(ResourceNameReferenceIndex, data.displayName)] -= 1;
                addedNormal = true;
                Debug.Log($"{data.displayName} has something in Que.");
            }

            if (addedNormal)
            {
                data.AdjustCurrentAmount(1 + data.autoAmount);
                Debug.Log($"Adding {data.displayName} auto amount and que.");
            }
            else
            {
                data.AdjustCurrentAmount(data.autoAmount);
                Debug.Log($"Adding {data.displayName} sinlge que.");
            }

            StartCoroutine(UpdateQue(data));
        }
    }
    public void AddToQue(ResourceData data, int amount)
    {
        if (data == null || !data.visible)
        {
            Debug.Log("Didn't get a legitamate resource.");
            return;
        }

        Debug.Log($"I have been told to add {amount} to the {data.displayName} que");
        Debug.Log($"{data.displayName} before que addition: {QuedResourceAmount[System.Array.IndexOf(ResourceNameReferenceIndex, data.displayName)]}");
        QuedResourceAmount[System.Array.IndexOf(ResourceNameReferenceIndex, data.displayName)] += amount;
        Debug.Log($"{data.displayName} after que addition: {QuedResourceAmount[System.Array.IndexOf(ResourceNameReferenceIndex, data.displayName)]}");

        StartQueUpdate(data);
    }
    #endregion

    #region Initial Setup Methods
    public void SetUpTileLocation(int column, int row)
    {
        myPositionInTheArray = new Vector2(column, row);

        if(myPositionInTheArray.x == 0 || myPositionInTheArray.x == myManager.locationXBounds-1 ||
           myPositionInTheArray.y == 0 || myPositionInTheArray.y == myManager.locationYBounds-1)
        {
            TypeTransform -= TryToTransformToLand;
        }

        DeactivateDetailLayers();
        camera = Camera.main;
    }
    public void SetNeighbors(Vector2[] locations)
    {
        myNeighbors = locations;
    }
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
            //TrySpawnEnemy();
            return;
        }
        myTileType = resourceSprites.Length+1; // blank space
        CreateResources();
        //TrySpawnEnemy();
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
    void CreateResources()
    {
        //Debug.Log($"Creating Resource List For: {myPositionInTheArray}");
        List<ResourceData> tempToPermanent = new List<ResourceData>();
        List<ResourceData> locationTypeOptionList = new List<ResourceData>();

        foreach(ResourceData data in main.GetResourceLibrary())
        {
            if (data.itemName == "soldier") //UniversalResource
            {
                soldiers = new ResourceData(data);
                tempToPermanent.Add(soldiers);
                continue;
            }
            if(myTileType == 1 && data.groups == "metal")
            {
                locationTypeOptionList.Add(new ResourceData(data));
                continue;
            }
            if(myTileType == 2 && data.itemName == "food")
            {
                locationTypeOptionList.Add(new ResourceData(data));
                continue;
            }
            if(data.itemName == "enemy" && myTileType != 0)
            {
                enemies = new ResourceData(data);
                tempToPermanent.Add(enemies);
                TrySpawnEnemy();
                continue;
            }
        }

        if(locationTypeOptionList.Count > 0)
            tempToPermanent.Add(new ResourceData(locationTypeOptionList[UnityEngine.Random.Range(0, locationTypeOptionList.Count)]));
        
        myResources = tempToPermanent.ToArray();
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
    public void SetMain(Main m)
    {
        main = m;
    }
    public void SetManager(LocationManager manager)
    {
        myManager = manager;
    }
    public void SetFloatingText(TMP_Text text)
    {
        FloatingText = text;
    }
    public int GetResourceSpritesLengthForStartPoint()
    {
        return resourceSprites.Length + 1;
    }
    private void CheckForPlayability(Vector2 pos)
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
    public void SetAsStartingPoint()
    {
        isStartingPoint = true;
        myRenderers[0].material.mainTexture = tileTextures[3];
        myState = TileStates.Conquered;
        if (!CheckIfResourceIsInMyArray("barracks"))
        {
            AddResourceToMyResources("barracks");
            AddResourceToMyResources("food");
        }
        OnTakeover -= CheckForPlayability;
        OnTakeover?.Invoke(myPositionInTheArray);
        StartLandingSequenceAnimation();

        //foreach(ResourceData data in myResources)
        //{
        //    Debug.Log($"Checking all resources: {data.itemName}");
        //}
    }
    private bool CheckIfResourceIsInMyArray(string itemName)
    {
        foreach(ResourceData data in myResources)
        {
            if(data.itemName == itemName)
            {
                return true;
            }
        }
        return false;
    }
    void AddResourceToMyResources(string itemName)
    {
        List<ResourceData> temp = new List<ResourceData>();
        foreach(ResourceData data in myResources)
        {
            temp.Add(data);
        }

        foreach(ResourceData dt in main.GetResourceLibrary())
        {
            if(itemName == dt.itemName)
            {
                ResourceData d = new ResourceData(dt);
                d.SetCurrentAmount(1);
                temp.Add(d);
                break;
            }
        }

        myResources = temp.ToArray();
    }

    #region TileShipAnimation
    public void StartLandingSequenceAnimation()
    {
        spaceship = myRenderers[4].transform;
        OnStartingTile?.Invoke(spaceship);
        SpriteRenderer rend = myRenderers[4].GetComponent<SpriteRenderer>();
        rend.enabled = true;
        rend.sprite = armySprites[armySprites.Length - 1];
        endPosition = spaceship.position;
        startPosition = new Vector3(endPosition.x, endPosition.y + 6f, endPosition.z);
        spaceshipSequenceTimer = 0;
        spaceshipSequenceDesiredTime = 6f;
        isInitializingLandingSequence = true;
    }
    public void StartLeavingSequenceAnimation()
    {
        spaceshipSequenceTimer = 0;
        spaceshipSequenceDesiredTime = 6f;
        isInitializingLeavingSequence = true;
    }
    #endregion
    #endregion

    #region Setup Tiles From Memory
    public void SetAllTileInfoFromMemory(string state, int tileType, string neighbors, bool isStart, string resources)
    {
        SetTileStateFromString(state);

        myTileType = tileType;
        if(myTileType < 5 && myTileType != 0)
        {
            myRenderers[2].GetComponent<SpriteRenderer>().sprite = resourceSprites[myTileType-1];
            myRenderers[2].enabled = true;
        }

        SetNeighborsFromString(neighbors);

        if(resources != "")
        {
            List<ResourceData> temp = new List<ResourceData>();
            string[] ar = resources.Split(";");
            foreach(string s in ar)
            {
                string[] st = s.Split(",");
                //string name, string display, string dis, string gr, string eType, string reqs, string nonReqs, bool vis, int cur, int autoA, float craft,
                //string created, string coms, string createComs, string im, string snd, string ach, int mos, string build
                temp.Add(new ResourceData(st[0], st[1], st[2], st[3], st[4], st[5], st[6],
                        (st[7] == "True") ? true : false, int.Parse(st[8]), int.Parse(st[9]), 
                        float.Parse(st[10]), st[11], st[12],st[13], st[14], st[15], st[16], 
                        int.Parse(st[17]), st[18]));
            }

            myResources = temp.ToArray();
            foreach(ResourceData data in myResources)
            {
                if(data.itemName == "soldier")
                {
                    //Debug.Log($"Soldiers: {data.currentAmount} on {myPositionInTheArray}");
                    soldiers = data;
                    if(data.currentAmount > 0)
                    {
                        myRenderers[4].enabled = true;
                    }
                    continue;
                }
                if(data.itemName == "enemy")
                {
                    enemies = data;
                    enemyCount = enemies.currentAmount;
                }
            }
        }

        if (isStart) SetAsStartingPoint();
    }
    private void SetNeighborsFromString(string neighbors)
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
    private void SetTileStateFromString(string state)
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
    #endregion

    #region Mouse Interactions
    private void OnMouseDown()
    {
        if (isMousePresent && isInteractable && !Main.isDebugging && !Main.isBlockingMapInteractions)
        {
            if(myState == TileStates.Conquered && camera.orthographicSize < 4.25f)
            {
                ActivateTileOptions();
                //Debug.Log("Clicked on a Conquered location.");
                OnNeedUIElementsForTile?.Invoke(this);
            }else if(myState == TileStates.Pickable)
            {
                //Debug.Log($"{myPositionInTheArray} has been chosen.");
                if(enemies.currentAmount == 0)
                {
                    StartCoroutine(Move());
                    return;
                }
                StartCoroutine(BattleSequence());
            }
        }
    }
    public void OnMouseEnter()
    {
        if (!Main.isDebugging && !Main.isBlockingMapInteractions)
        {
            isMousePresent = true;
            Vector3 v = transform.position;
            v.y += .1f;
            transform.position = v;
            //if (myState == TileStates.Conquered) return;
            if (myState == TileStates.UnClickable) return;
        }
    }
    private void OnMouseExit()
    {
        if (!Main.isDebugging && !Main.isBlockingMapInteractions)
        {
            isMousePresent = false;
            Vector3 v = transform.position;
            v.y -= .1f;
            transform.position = v;
            //if (myState == TileStates.Conquered) return;
            if (myState == TileStates.UnClickable) return;
        }
    }
    #endregion

    public void SetResourceTradingBuddy(Vector2 tile)
    {
        resourceTradingBuddy = tile;
    }
    public IEnumerator Move()
    {
        OnReinstateInteractability?.Invoke();
        OnResetStateToPreviousFromInteraction?.Invoke();
        float timer =(generalTimerOverride) ? generalTimer : 2f;
        CheckNullFloatingText();
        myResourceManager.SetBattleTimerAndStart(timer);

        yield return new WaitForSeconds(timer);

        generalTimerOverride = false;
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
        CheckNullFloatingText();
        myResourceManager.SetBattleTimerAndStart(timer);
        OnReinstateInteractability?.Invoke();
        OnResetStateToPreviousFromInteraction?.Invoke();

        yield return new WaitForSeconds(timer);

        int difference = enemyCount - potentialAmountToReceive;
        if (difference < 0)
        {
            //Debug.Log($"Potential Amount: {potentialAmountToReceive}");
            AdjustSoldiers(potentialAmountToReceive);
            enemies.SetCurrentAmount(0);
            enemyCount = 0;
            myRenderers[0].material.mainTexture = tileTextures[3];
            OnTakeover -= CheckForPlayability;
            OnTakeover?.Invoke(myPositionInTheArray);
            myState = TileStates.Conquered;
            CheckNullFloatingText();
            FloatingText.gameObject.SetActive(false);
            //Debug.Log("Soldiers current: " + soldiers.currentAmount);
            //Debug.Log("I won!");
            potentialAmountToReceive = 0;
        }
        else if (difference == 0)
        {
            OnTradeWithPartnerTile?.Invoke(resourceTradingBuddy, "soldier", potentialAmountToReceive);
            ShowEnemyOnTile();
            //Debug.Log("We Tied!");
        }else if (difference > 0)
        {
            if (Main.cantLose)
            {
                Debug.Log("Can't Lose is on.");
                OnTradeWithPartnerTile?.Invoke(resourceTradingBuddy, "soldier", potentialAmountToReceive);
            }
            ShowEnemyOnTile();
            //Debug.Log("I lost.");
        }

    }
    private void CheckNullFloatingText()
    {
        if (FloatingText == null)
        {
            ActivateTileOptions();
            myResourceManager.DeactivateSelf();
        }
    }
    private void ShowEnemyOnTile()
    {
        CheckNullFloatingText();
        FloatingText.gameObject.SetActive(true);
        FloatingText.text = enemyCount.ToString();
        SpriteRenderer rend = myRenderers[4].GetComponent<SpriteRenderer>();
        rend.enabled = true;
        rend.sprite = armySprites[1];
    }
    private void ReceiveResourcesFromTrade(Vector2 tile, string resource, int amount)
    {
        Debug.Log($"{myPositionInTheArray} receiving {resource}:{amount} from {tile}");
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
    private void ActivateTileOptions()
    {
        if (myCanvasContainer == null)
        {
            CreateTileOptions();
            return;
        }
    }
    private void SetButtonsOnScreenPosition()
    {
        if (myCanvasContainer != null)
        {
            myCanvasContainer.position = camera.WorldToScreenPoint(transform.position);
            myCanvasContainer.localScale = new Vector3(1.75f / camera.orthographicSize, 1.75f / camera.orthographicSize, 1f);
        }
    }
    private void CreateTileOptions()
    {
        myCanvasContainer = GameObject.Find("Canvas").transform;
        GameObject obj = Instantiate(canvasContainerPrefab, myCanvasContainer);
        myCanvasContainer = obj.transform;
        myResourceManager = obj.GetComponent<UIResourceManager>();
        myResourceManager.SetMyTileAndMain(this, main);
        myResourceManager.CreateResourceButtons();
        SetButtonsOnScreenPosition();
    }

    public int GetSoldierCount()
    {
        return soldiers.currentAmount;
    }
    public void AdjustSoldiers(int amount)
    {
        if(soldiers.currentAmount + amount <= 0)
        {
            //Debug.Log($"{soldiers.currentAmount} + {amount} = {soldiers.currentAmount + amount}");
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
        if(soldiers.currentAmount > 0 && !myRenderers[4].enabled)
        {
            SpriteRenderer rend = myRenderers[4].GetComponent<SpriteRenderer>();
            rend.enabled = true;
            rend.sprite = armySprites[0];

        }else if (soldiers.currentAmount == 0 && myRenderers[4].enabled)
        {
            myRenderers[4].enabled = false;
        }
    }
    private void CheckTileInteratability(string type, Vector2 tile, int amount)
    {
        potentialAmountToReceive = amount;

        if((myState == TileStates.Clickable || myState == TileStates.Conquered) && type == "soldier" && CheckForNeighbor(tile))
        {
            //Debug.Log("It is a neighbor.");
            myRenderers[0].material.color = Color.red;
            isInteractable = true;
            previousState = myState;
            myState = TileStates.Pickable;
            OnResetStateToPreviousFromInteraction += ResetTileStateFromInteraction;
            resourceTradingBuddy = tile;
        }
        else
        {
            //Otherwise don't let them interact
            isInteractable = false;
        }
    }
    public void SetupReceivingOfTroopsForOriginator()
    {
        OnTradeWithPartnerTile += ReceiveResourcesFromTrade;
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

    bool CheckForNeighbor(Vector2 pos)
    {
        foreach(Vector2 v in myNeighbors)
        {
            if (v == pos) return true;
        }

        return false;
    }

    void DeactivateTileOptions()
    {
        if (myResourceManager != null)
        {
           myResourceManager.DeactivateSelf();
        }
    }

    public ResourceData CheckIfAndUseOwnResources(ResourceData item)
    {
        foreach(ResourceData data in myResources)
        {
            if(data != item && data.itemName == item.itemName)
            {
                //Debug.Log($"Using local resource: {data.itemName}");
                return data;
            }
        }
        return item;
    }

    public ResourceData GetResourceByString(string itemName)
    {
        foreach(ResourceData data in myResources)
        {
            if(itemName == data.itemName)
            {
                return data;
            }
        }

        Debug.Log($"No item by the given string: {itemName}");
        return null;
    }

    private void TurnOffAllFloatingText()
    {
        FloatingText.gameObject.SetActive(false);
    }
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
}
