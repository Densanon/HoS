using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexTileInfo : MonoBehaviour
{
    [SerializeField]
    Material[] mats;

    [SerializeField]
    Renderer myRenderer;

    enum TileStates { UnClickable, Clickable, Conquered}
    TileStates myState = TileStates.Clickable;

    public void SetUpTileInformation()
    {

    }

    public void OnMouseEnter()
    {
        myRenderer.material = mats[1];
        if (myState == TileStates.UnClickable) return;
    }

    private void OnMouseExit()
    {
        myRenderer.material = mats[0];
        if (myState == TileStates.UnClickable) return;
    }
}
