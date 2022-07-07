using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class HexTileInfo : MonoBehaviour
{
    public static Action<Transform> OnStartingTile = delegate { };
    public static Action<Vector2> OnTakeover = delegate { };

    [SerializeField]
    Material[] mats;

    [SerializeField]
    Renderer myRenderer;

    public Vector2 myPositionInTheArray;

    public enum TileStates { UnClickable, Clickable, Conquered}
    TileStates myState = TileStates.UnClickable;

    private void OnEnable()
    {
        OnTakeover += CheckForPlayability;
    }

    private void OnDisable()
    {
        OnTakeover -= CheckForPlayability;
    }

    private void CheckForPlayability(Vector2 pos)
    {
        if((myState == TileStates.UnClickable && pos.x == myPositionInTheArray.x && (pos.y +1 == myPositionInTheArray.y || pos.y - 1 == myPositionInTheArray.y)) || //Same column +- row
            (myState == TileStates.UnClickable && myPositionInTheArray.x < 10 && pos.x + 1 == myPositionInTheArray.x && (pos.y == myPositionInTheArray.y || pos.y + 1 == myPositionInTheArray.y)) || //+Column +=row below half
            (myState == TileStates.UnClickable && myPositionInTheArray.x > 9 && pos.x + 1 == myPositionInTheArray.x && (pos.y == myPositionInTheArray.y || pos.y - 1 == myPositionInTheArray.y)) || //+Column -=row after half
            (myState == TileStates.UnClickable && myPositionInTheArray.x < 10 && pos.x - 1 == myPositionInTheArray.x && (pos.y == myPositionInTheArray.y || pos.y - 1 == myPositionInTheArray.y)) || //-Column +=row below half
            (myState == TileStates.UnClickable && myPositionInTheArray.x > 9 && pos.x - 1 == myPositionInTheArray.x && (pos.y == myPositionInTheArray.y || pos.y + 1 == myPositionInTheArray.y))) //-Column -=row after half
        {
            myState = TileStates.Clickable;
            myRenderer.material = mats[1];
            OnTakeover -= CheckForPlayability;
        }
    }

    public void SetUpTileLocation(int column, int row)
    {
        myPositionInTheArray = new Vector2(column, row);
    }

    public void SetTileState(string state)
    {
        switch (state)
        {
            case "UnClickable":
                myRenderer.material = mats[0];
                myState = TileStates.UnClickable;
                break;
            case "Clickable":
                myRenderer.material = mats[1];
                myState = TileStates.Clickable;
                OnTakeover -= CheckForPlayability;
                break;
            case "Conquered":
                myRenderer.material = mats[2];
                myState = TileStates.Conquered;
                OnTakeover -= CheckForPlayability;
                break;
        }
    }

    public void StartingPoint()
    {
        myRenderer.material = mats[2];
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
            myRenderer.material = mats[2];
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

    public string DigitizeForSerialization()
    {
        return $"{myState};";
    }
}
