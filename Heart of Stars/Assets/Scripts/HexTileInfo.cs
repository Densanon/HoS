using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class HexTileInfo : MonoBehaviour
{
    public static Action<Transform> OnStartingTile = delegate { };
    public static Action OnLanded = delegate { };
    public static Action OnLeaving = delegate { };
    public static Action<Vector2> OnTakeover = delegate { };
    public static Action<Vector2> TypeTransform = delegate { };
    public static Action<Vector2> LookingForNeighbors = delegate { };
    public static Action<HexTileInfo> OnNeedUIElementsForTile = delegate { };

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
    [SerializeField]
    GameObject dependenceButtonPrefab;

    public GameObject canvasContainerPrefab;
    public Transform myCanvasContainer;

    public Vector2 myPositionInTheArray;

    public float frequencyOfLandDistribution;

    public enum TileStates { UnClickable, Clickable, Conquered, UnPlayable}
    TileStates myState = TileStates.UnPlayable;

    public int myTileType = 0;
    LocationManager myManager;
    Main main;
    Camera camera;
    UIResourceManager myResourceManager;

    bool isInitializingLandingSequence = false;
    bool isInitializingLeavingSequence = false;
    Vector3 startPosition;
    Vector3 endPosition;
    float spaceshipSequenceTimer;
    float spaceshipSequenceDesiredTime;
    public bool isStartingPoint;
    bool isMousePresent;

    #region Unity Methods
    private void OnEnable()
    {
        OnTakeover += CheckForPlayability;
        TypeTransform += TryToTransformToLand;
        OnNeedUIElementsForTile += CheckDeactivateOptions;
        CameraController.OnZoomRelocateUI += SetButtonsOnScreenPosition;
        CameraController.OnZoomedOutTurnOffUI += DeactivateTileOptions;
    }
    private void OnDisable()
    {
        OnTakeover -= CheckForPlayability;
        TypeTransform -= TryToTransformToLand;
        OnNeedUIElementsForTile -= CheckDeactivateOptions;
        CameraController.OnZoomRelocateUI -= SetButtonsOnScreenPosition;
        CameraController.OnZoomedOutTurnOffUI -= DeactivateTileOptions;

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
            return;
        }
        myTileType = resourceSprites.Length+1; // blank space
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
    void CreateResources()
    {
        List<ResourceData> tempToPermanent = new List<ResourceData>();
        List<ResourceData> locationTypeOptionList = new List<ResourceData>();

        foreach(ResourceData data in main.GetResourceLibrary())
        {
            if (data.itemName == "soldier") //UniversalResource
            {
                tempToPermanent.Add(new ResourceData(data));
            }else if(myTileType == 1 && data.groups == "metal")
            {
                locationTypeOptionList.Add(data);
            }else if(myTileType == 2 && data.itemName == "food")
            {
                locationTypeOptionList.Add(data);
            }
        }

        if(locationTypeOptionList.Count > 0)
            tempToPermanent.Add(new ResourceData(locationTypeOptionList[UnityEngine.Random.Range(0, locationTypeOptionList.Count)]));
        
        myResources = tempToPermanent.ToArray();
    }
    public void SetMain(Main m)
    {
        main = m;
    }
    public void SetManager(LocationManager manager)
    {
        myManager = manager;
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
        OnTakeover -= CheckForPlayability;
        OnTakeover?.Invoke(myPositionInTheArray);
        StartLandingSequenceAnimation();
    }
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

        if (isStart) SetAsStartingPoint();

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
        }
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
        if (isMousePresent && camera.orthographicSize < 4.25f)
        {
            if(myState == TileStates.Clickable)
            {
                myState = TileStates.Conquered;
                myRenderers[0].material.mainTexture = tileTextures[3];
                OnTakeover -= CheckForPlayability;
                OnTakeover?.Invoke(myPositionInTheArray);
            }else if(myState == TileStates.Conquered)
            {
                ActivateTileOptions();
                OnNeedUIElementsForTile?.Invoke(this);
            }
            return;
        }
    }
    public void OnMouseEnter()
    {
        isMousePresent = true;
        Vector3 v = transform.position;
        v.y += .1f;
        transform.position = v;
        //if (myState == TileStates.Conquered) return;
        if (myState == TileStates.UnClickable) return;
    }
    private void OnMouseExit()
    {
        isMousePresent = false;
        Vector3 v = transform.position;
        v.y -= .1f;
        transform.position = v;
        //if (myState == TileStates.Conquered) return;
        if (myState == TileStates.UnClickable) return;
    }
    #endregion
    private void CheckDeactivateOptions(HexTileInfo tile)
    {
        if (tile.GetInstanceID() != this.GetInstanceID())
        {
            DeactivateTileOptions();
        }
    }
    private void ActivateTileOptions()
    {
        if (myCanvasContainer == null)
        {
            CreateTileOptions();
            return;
        }else if (myCanvasContainer.gameObject.activeInHierarchy)
        {
            Debug.Log("Container was active.");
            if (myResourceManager.CheckMouseOnUI()) return;
            Debug.Log("Container is active.");
            myCanvasContainer.gameObject.SetActive(false);
            return;
        }
        Debug.Log("Container was off.");
        myCanvasContainer.gameObject.SetActive(true);
        myResourceManager.ResetUI();
        SetButtonsOnScreenPosition();
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
        foreach(ResourceData resource in myResources)
        {
            if (resource.itemName == "soldier")
            {
                return resource.currentAmount;
            }
        }
        return 0;
    }

    void DeactivateTileOptions()
    {
        if(myCanvasContainer != null) myCanvasContainer.gameObject.SetActive(false);
    }

    public ResourceData CheckIfAndUseOwnResources(ResourceData item)
    {
        foreach(ResourceData data in myResources)
        {
            if(data != item && data.itemName == item.itemName)
            {
                return data;
            }
        }
        return item;
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
