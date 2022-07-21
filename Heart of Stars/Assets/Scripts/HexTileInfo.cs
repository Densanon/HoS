using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class HexTileInfo : MonoBehaviour
{
    public static Action<Transform> OnStartingTile = delegate { };
    public static Action OnLanded = delegate { };
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

    [SerializeField]
    Vector2[] myNeighbors;

    public Vector2 myPositionInTheArray;

    public float frequencyOfLandDistribution;

    public enum TileStates { UnClickable, Clickable, Conquered, UnPlayable}
    TileStates myState = TileStates.UnPlayable;

    public int myTileType = 0;

    bool isInitializingLandingSequence = false;
    Vector3 startPosition;
    Vector3 endPosition;
    float landingSequenceTimer;
    float landingSequenceDesiredTime;
    bool isStartingPoint;

    #region Unity Methods
    private void OnEnable()
    {
        OnTakeover += CheckForPlayability;
        TypeTransform += TryToTransformToLand;
    }

    private void OnDisable()
    {
        OnTakeover -= CheckForPlayability;
        TypeTransform -= TryToTransformToLand;
    }
    #endregion

    #region Initial Setup Methods
    public void SetUpTileLocation(int column, int row)
    {
        myPositionInTheArray = new Vector2(column, row);

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
            if(Mathf.RoundToInt(UnityEngine.Random.Range(0f,2f)*frequencyOfLandDistribution) == 1)
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
            return;
        }
        myTileType = resourceSprites.Length+1; // blank space
        /// mountain
        /// tree
        /// mine
        /// wheat
        /// tent
        /// building
        /// trap?
        /// chest
        /// guy
        /// robot spider
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
        OnStartingTile?.Invoke(myRenderers[4].transform);
        OnTakeover -= CheckForPlayability;
        OnTakeover?.Invoke(myPositionInTheArray);
        LandingSequenceAnimation();
        Debug.Log("Sending Starting Point.");
    }

    void LandingSequenceAnimation()
    {
        myRenderers[4].GetComponent<SpriteRenderer>().enabled = true;
        myRenderers[4].GetComponent<SpriteRenderer>().sprite = armySprites[armySprites.Length - 1];
        endPosition = myRenderers[4].transform.position;
        startPosition = new Vector3(endPosition.x, endPosition.y + 6f, endPosition.z);
        landingSequenceTimer = 0;
        landingSequenceDesiredTime = 6f;
        isInitializingLandingSequence = true;
    }

    void LeavingSequenceAnimation()
    {

    }

    private void Update()
    {
        if (isInitializingLandingSequence)
        {
            myRenderers[4].transform.position = Vector3.Lerp(startPosition, endPosition, landingSequenceTimer/landingSequenceDesiredTime);

            landingSequenceTimer += Time.deltaTime;
            Debug.Log(landingSequenceTimer);
            if(landingSequenceTimer > landingSequenceDesiredTime)
            {
                Debug.Log("Landed");
                isInitializingLandingSequence = false;
                myRenderers[4].transform.position = endPosition;
                OnLanded?.Invoke();
            }
        }
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
        if(myState == TileStates.Clickable)
        {
            myState = TileStates.Conquered;
            myRenderers[0].material.mainTexture = tileTextures[3];
            OnTakeover -= CheckForPlayability;
            OnTakeover?.Invoke(myPositionInTheArray);
        }else if(myState == TileStates.Conquered)
        {
            OnNeedUIElementsForTile?.Invoke(this);
        }
    }

    public void OnMouseEnter()
    {
        if (myState == TileStates.Conquered) return;
        if (myState == TileStates.UnClickable) return;
    }

    private void OnMouseExit()
    {
        if (myState == TileStates.Conquered) return;
        if (myState == TileStates.UnClickable) return;
    }
    #endregion

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
