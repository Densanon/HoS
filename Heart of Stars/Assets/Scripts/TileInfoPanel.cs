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

    private void Awake()
    {
        HexTileInfo.OnNeedUIElementsForTile += ActivateTile;
        gameObject.SetActive(false); 
    }

    public void ActivateTile(HexTileInfo tile)
    {
        if(tile != activeHexTile)
        {
            activeHexTile = tile;
            depthOfInformation = 0;
            gameObject.SetActive(true);
            Vector3 pos = Input.mousePosition;
            Debug.Log(pos);
            transform.position = new Vector3( pos.x + 350f, pos.y, pos.z);
        }
    }
}
