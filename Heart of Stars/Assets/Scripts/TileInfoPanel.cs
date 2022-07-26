using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileInfoPanel : MonoBehaviour
{
    HexTileInfo activeHexTile;

    [SerializeField]
    GameObject TilePanelInfoPrefab;

    [SerializeField]
    Transform Content;

    [SerializeField]
    int depthOfInformation;

    [SerializeField]
    GameObject LeaveButton;

    private void Awake()
    {
        //HexTileInfo.OnNeedUIElementsForTile += ActivateTile;
        gameObject.SetActive(false); 
    }

    private void Update()
    {
        //Need to implement a scroll off screen when player moves
    }

    public void ActivateTile(HexTileInfo tile)
    {
        if(tile != activeHexTile)
        {
            activeHexTile = tile;
            depthOfInformation = 0;
            gameObject.SetActive(true);
            Vector3 pos = Input.mousePosition;
            transform.position = new Vector3( pos.x + 350f, pos.y, pos.z);
            if (!tile.isStartingPoint && LeaveButton.activeSelf)
            {
                LeaveButton.SetActive(false);
                return;
            }else if(tile.isStartingPoint && !LeaveButton.activeSelf) LeaveButton.SetActive(true);
        }
    }
}
