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
    Texture2D[] Textures;

    [SerializeField]
    Renderer[] myRenderers;

    [SerializeField]
    Vector2[] myNeighbors;

    public Vector2 myPositionInTheArray;

    public float frequency;

    public enum TileStates { UnClickable, Clickable, Conquered, UnPlayable}
    TileStates myState = TileStates.UnPlayable;

    public int i_tileType = 0;

    private void OnEnable()
    {
        OnTakeover += CheckForPlayability;
        TypeTransform += TryTransform;
    }

    private void OnDisable()
    {
        OnTakeover -= CheckForPlayability;
        TypeTransform -= TryTransform;
    }

    private void TryTransform(Vector2 pos)
    {
        if ((myState == TileStates.UnPlayable && pos.x == myPositionInTheArray.x && (pos.y + 1 == myPositionInTheArray.y || pos.y - 1 == myPositionInTheArray.y)) || //Same column +- row
            (myState == TileStates.UnPlayable && pos.x % 2 == 0 && pos.x + 1 == myPositionInTheArray.x && (pos.y == myPositionInTheArray.y || pos.y - 1 == myPositionInTheArray.y)) || //Even +Column +=row
            (myState == TileStates.UnPlayable && pos.x % 2 == 1 && pos.x + 1 == myPositionInTheArray.x && (pos.y == myPositionInTheArray.y || pos.y + 1 == myPositionInTheArray.y)) || //Odd +Column -=row 
            (myState == TileStates.UnPlayable && pos.x % 2 == 0 && pos.x - 1 == myPositionInTheArray.x && (pos.y == myPositionInTheArray.y || pos.y - 1 == myPositionInTheArray.y)) || //Even -Column +=row 
            (myState == TileStates.UnPlayable && pos.x % 2 == 1 && pos.x - 1 == myPositionInTheArray.x && (pos.y == myPositionInTheArray.y || pos.y + 1 == myPositionInTheArray.y))) //Odd -Column -=row 
        {
            if(Mathf.RoundToInt(UnityEngine.Random.Range(0f,2f)*frequency) == 1)
            {
                    TurnLand();
            }
        }
    }

    public void SetNeighbors(Vector2[] vs)
    {
        myNeighbors = vs;
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
            myRenderers[0].material.mainTexture = Textures[2];
            OnTakeover -= CheckForPlayability;
        }
    }

    public void SetUpTileLocation(int column, int row)
    {
        myPositionInTheArray = new Vector2(column, row);

        DeactivateDetailLayer();
    }

    public void TurnLand()
    {
        i_tileType = 1;
        myRenderers[0].material.mainTexture = Textures[1];
        myState = TileStates.UnClickable;
        TypeTransform -= TryTransform;
        TypeTransform?.Invoke(myPositionInTheArray);
    }

    public void SetTileState(string state, int tileType, string neighbors)
    {
        switch (state)
        {
            case "UnPlayable":
                myRenderers[0].material.mainTexture = Textures[0];
                myState = TileStates.UnPlayable;
                break;
            case "UnClickable":
                myRenderers[0].material.mainTexture = Textures[1];
                myState = TileStates.UnClickable;
                break;
            case "Clickable":
                myRenderers[0].material.mainTexture = Textures[2];
                myState = TileStates.Clickable;
                OnTakeover -= CheckForPlayability;
                break;
            case "Conquered":
                myRenderers[0].material.mainTexture = Textures[3];
                myState = TileStates.Conquered;
                OnTakeover -= CheckForPlayability;
                break;
        }

        i_tileType = tileType;

        List<Vector2> temp = new List<Vector2>();
        string[] ar = neighbors.Split("'");
        foreach(string s in ar)
        {
            string[] st = s.Split(",");
            Vector2 v = new Vector2(float.Parse(st[0]), float.Parse(st[1].Remove(0,1)));
            temp.Add(v);
        }

        myNeighbors = new Vector2[temp.Count];
        for(int i = 0; i < myNeighbors.Length; i++)
        {
            myNeighbors[i] = temp[i];
        }
    }

    public void StartingPoint()
    {
        myRenderers[0].material.mainTexture = Textures[3];
        myState = TileStates.Conquered;
        OnStartingTile?.Invoke(transform);
        OnTakeover -= CheckForPlayability;
        OnTakeover?.Invoke(myPositionInTheArray);
    }

    private void OnMouseDown()
    {
        if(myState == TileStates.Clickable)
        {
            myState = TileStates.Conquered;
            myRenderers[0].material.mainTexture = Textures[3];
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

    public void DeactivateSelf()
    {
        OnTakeover -= CheckForPlayability;
        TypeTransform -= TryTransform;
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

        return $"{myState}:{i_tileType}:{s};";
    }
}
