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
    [SerializeField]
    ResourceData[] myResources; // need to implement
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
    }
    private void OnDisable()
    {
        OnTakeover -= CheckForPlayability;
        TypeTransform -= TryToTransformToLand;
        OnNeedUIElementsForTile -= CheckDeactivateOptions;

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

        foreach(ResourceData data in myManager.myResources)
        {
            if (data.itemName == "soldier") //UniversalResource
            {
                tempToPermanent.Add(new ResourceData(data));
            }else if(myTileType == 1 && data.groups == "metal")
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
    public void SetAllTileInfoFromMemory(string state, int tileType, string neighbors, bool isStart)
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
        if (isMousePresent)
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
            myCanvasContainer.gameObject.SetActive(false);
            return;
        }

        myCanvasContainer.gameObject.SetActive(true);
        SetButtonsOnScreenPosition();
    }
    private void SetButtonsOnScreenPosition()
    {
        myCanvasContainer.position = Camera.main.WorldToScreenPoint(transform.position);
    }
    private void CreateTileOptions()
    {
        myCanvasContainer = GameObject.Find("Canvas").transform;
        GameObject obj = Instantiate(canvasContainerPrefab, myCanvasContainer);
        myCanvasContainer = obj.transform;
        SetButtonsOnScreenPosition();
        foreach (ResourceData data in myResources)
        {
            Debug.Log($"Resource: {data.itemName}");
            GameObject obs = Instantiate(dependenceButtonPrefab, myCanvasContainer);
            obs.transform.position = new Vector3(obs.transform.position.x, obs.transform.position.y + 10f);
            myCanvasContainer.Rotate(0f, 0f, 360f / myResources.Length);
            obs.GetComponent<Resource>().SetUpResource(data, false, main);
        }
    }
    void DeactivateTileOptions()
    {
        if(myCanvasContainer != null) myCanvasContainer.gameObject.SetActive(false);
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
        return $"{myState}:{myTileType}:{s}:{isStartingPoint};";
    }
}
