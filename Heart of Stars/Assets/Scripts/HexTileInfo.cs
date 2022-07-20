using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class HexTileInfo : MonoBehaviour
{
    public static Action<Transform> OnStartingTile = delegate { };
    public static Action<Vector2> OnTakeover = delegate { };
    public static Action<Vector2> TypeTransform = delegate { };
    public static Action<Vector2> LookingForNeighbors = delegate { };

    [SerializeField]
    Texture2D[] tileTextures;

    [SerializeField]
    Renderer[] myRenderers;

    [SerializeField]
    Vector2[] myNeighbors;

    public Vector2 myPositionInTheArray;

    public float frequencyOfLandDistribution;

    public enum TileStates { UnClickable, Clickable, Conquered, UnPlayable}
    TileStates myState = TileStates.UnPlayable;

    public int myTileType = 0;

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

        DeactivateDetailLayer();
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
        myTileType = 1;
        myRenderers[0].material.mainTexture = tileTextures[1];
        myState = TileStates.UnClickable;
        TypeTransform -= TryToTransformToLand;
        TypeTransform?.Invoke(myPositionInTheArray);
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
        myRenderers[0].material.mainTexture = tileTextures[3];
        myState = TileStates.Conquered;
        OnStartingTile?.Invoke(transform);
        OnTakeover -= CheckForPlayability;
        OnTakeover?.Invoke(myPositionInTheArray);
    }
    #endregion

    #region Setup Tiles From Memory
    public void SetAllTileInfoFromMemory(string state, int tileType, string neighbors)
    {
        SetTileStateFromString(state);

        myTileType = tileType;

        SetNeighborsFromString(neighbors);
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

    void DeactivateDetailLayer()
    {
        myRenderers[1].gameObject.SetActive(false);
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

        return $"{myState}:{myTileType}:{s};";
    }
}
