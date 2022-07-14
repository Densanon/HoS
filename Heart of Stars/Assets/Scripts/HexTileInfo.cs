using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class HexTileInfo : MonoBehaviour
{
    public static Action<Transform> OnStartingTile = delegate { };
    public static Action<Vector2> OnTakeover = delegate { };
    public static Action<Vector2> TypeTransform = delegate { };

    [SerializeField]
    Material[] mats;

    [SerializeField]
    Renderer myRenderer;

    public Vector2 myPositionInTheArray;

    public float frequency;

    public enum TileStates { UnClickable, Clickable, Conquered}
    TileStates myState = TileStates.UnClickable;

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
        if ((myState == TileStates.UnClickable && pos.x == myPositionInTheArray.x && (pos.y + 1 == myPositionInTheArray.y || pos.y - 1 == myPositionInTheArray.y)) || //Same column +- row
            (myState == TileStates.UnClickable && pos.x % 2 == 0 && pos.x + 1 == myPositionInTheArray.x && (pos.y == myPositionInTheArray.y || pos.y - 1 == myPositionInTheArray.y)) || //Even +Column +=row
            (myState == TileStates.UnClickable && pos.x % 2 == 1 && pos.x + 1 == myPositionInTheArray.x && (pos.y == myPositionInTheArray.y || pos.y + 1 == myPositionInTheArray.y)) || //Odd +Column -=row 
            (myState == TileStates.UnClickable && pos.x % 2 == 0 && pos.x - 1 == myPositionInTheArray.x && (pos.y == myPositionInTheArray.y || pos.y - 1 == myPositionInTheArray.y)) || //Even -Column +=row 
            (myState == TileStates.UnClickable && pos.x % 2 == 1 && pos.x - 1 == myPositionInTheArray.x && (pos.y == myPositionInTheArray.y || pos.y + 1 == myPositionInTheArray.y))) //Odd -Column -=row 
        {
            if(Mathf.RoundToInt(UnityEngine.Random.Range(0f,2f)*frequency) == 1)
            {
                    TurnLand();
            }
        }
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
            myRenderer.material = mats[1];
            OnTakeover -= CheckForPlayability;
        }
    }

    public void SetUpTileLocation(int column, int row)
    {
        myPositionInTheArray = new Vector2(column, row);
    }

    public void TurnLand()
    {
        i_tileType = 1;
        //myRenderer.material = mats[1];
        TypeTransform -= TryTransform;
        TypeTransform?.Invoke(myPositionInTheArray);
    }

    public void SetTileState(string state, int tileType)
    {
        if(tileType == 0)
        {
            DeactivateSelf();
            return;
        }

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
        i_tileType = tileType;
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

    public void DeactivateSelf()
    {
        OnTakeover -= CheckForPlayability;
        TypeTransform -= TryTransform;
        gameObject.SetActive(false);
    }

    public string DigitizeForSerialization()
    {
        return $"{myState},{i_tileType};";
    }
}
