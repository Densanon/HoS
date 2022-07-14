using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationManager : MonoBehaviour
{
    public static Action<HexTileInfo[], string, ResourceData[]> OnSaveMyLocationInfo = delegate { };

    public string s_MyAddress;
    public float frequencySet = 1.5f;
    public int startingPoints = 5;
    public GameObject[] g_MyPlanetPieces;
    public GameObject tilePrefab;
    public HexTileInfo[] tileInfoList;
    public ResourceData[] myResources;
    Vector2[] TileLocations;

    private void Awake()
    {
        Main.OnWorldMap += TurnOffVisibility;
    }

    private Vector2[] FindNeighbors(Vector2 location)
    {
        int count = 0;

        List<Vector2> neg = new List<Vector2>();

        foreach(Vector2 vec in TileLocations)
        {
            if (count == 6) return neg.ToArray();
            Vector2 v = location;
            v.y += 1;
            if(vec == v) //same column +row
            {
                neg.Add(vec);
                count++;
                continue;
            }
            v.y -= 2;
            if (vec == v) //same column -row
            {
                neg.Add(vec);
                count++;
                continue;
            }
            v = location;
            if(location.x % 2 == 1)
            {
                v.x -= 1;
                if (vec == v) //-column same row
                {
                    neg.Add(vec);
                    count++;
                    continue;
                }
                v.y += 1;
                if (vec == v) //-column +row
                {
                    neg.Add(vec);
                    count++;
                    continue;
                }
                v.x += 2;
                if (vec == v) //+column +row
                {
                    neg.Add(vec);
                    count++;
                    continue;
                }
                v.y -= 1;
                if (vec == v) //+column same row
                {
                    neg.Add(vec);
                    count++;
                    continue;
                }
                continue;
            }

            v.x -= 1;
            if (vec == v) //-column same row
            {
                neg.Add(vec);
                count++;
                continue;
            }
            v.y -= 1;
            if (vec == v) //-column -row
            {
                neg.Add(vec);
                count++;
                continue;
            }
            v.x += 2;
            if (vec == v) //+column -row
            {
                neg.Add(vec);
                count++;
                continue;
            }
            v.y += 1;
            if (vec == v) //+column same row
            {
                neg.Add(vec);
                count++;
                continue;
            }
        }

        return neg.ToArray();
    }

    private void OnDestroy()
    {
        Main.OnWorldMap -= TurnOffVisibility;
    }

    void TurnOffVisibility(bool OverWorld)
    {
        if (OverWorld && gameObject.activeInHierarchy)
        {
            gameObject.SetActive(false);
            SaveLocationInfo();
        }
    }

    public void TurnOnVisibility()
    {
        gameObject.SetActive(true);
    }

    public void ReceiveOrders(string[] hextiles, string address, ResourceData[] resources)
    {
        s_MyAddress = address;
        myResources = resources;

        BuildTilesForOrders();

        if (hextiles != null)
        {
            for(int i = 0; i < hextiles.Length; i++)
            {
                string[] ar = hextiles[i].Split(":");
                tileInfoList[i].SetTileState(ar[0], int.Parse(ar[1]), ar[2]);
            }
            return;
        }

        OrganizePieces();

        OnSaveMyLocationInfo?.Invoke(tileInfoList, s_MyAddress, myResources);
    }

    void BuildTilesForOrders()
    {
        List<HexTileInfo> temp = new List<HexTileInfo>();
        List<GameObject> objs = new List<GameObject>();
        List<Vector2> locs = new List<Vector2>();

        for (int x = 0; x < 10; x++)
        {
            bool odd = (x % 2 == 1) ? true : false;
            for (int y = 0; y < 10; y++)
            {
                GameObject obj = Instantiate(tilePrefab, new Vector3(x * 0.75f, 0f, (odd) ? y*.87f + .43f : y*.87f), Quaternion.identity, transform);
                objs.Add(obj);
                HexTileInfo tf = obj.GetComponent<HexTileInfo>();
                tf.SetUpTileLocation(x, y);
                tf.frequency = frequencySet;
                temp.Add(tf);
                locs.Add(tf.myPositionInTheArray);
            }
        }

        g_MyPlanetPieces = new GameObject[objs.Count];
        for (int j = 0; j < objs.Count; j++)
        {
            g_MyPlanetPieces[j] = objs[j];
        }

        tileInfoList = new HexTileInfo[temp.Count];
        for (int i = 0; i < tileInfoList.Length; i++)
        {
            tileInfoList[i] = temp[i];
        }

        TileLocations = new Vector2[locs.Count];
        for(int k = 0; k < TileLocations.Length; k++)
        {
            TileLocations[k] = locs[k];
        }
    }

    public void OrganizePieces()
    {
        for (int p = 0; p < startingPoints; p++)
        {
            float l = 100f;
            float q = (float)p / (float)startingPoints * l;
            //Debug.Log($"Current low: {q}");
            float f = (float)(p + 1) / (float)startingPoints * l;
            //Debug.Log($"Current high: {f}");
            int k = UnityEngine.Random.Range(Mathf.RoundToInt(q), Mathf.RoundToInt(f));
            //Debug.Log(k);
            tileInfoList[k].TurnLand();
        }

        HexTileInfo info = g_MyPlanetPieces[UnityEngine.Random.Range(0, g_MyPlanetPieces.Length)].GetComponent<HexTileInfo>();
        /*while (info.i_tileType != 1)
        {
            info = g_MyPlanetPieces[UnityEngine.Random.Range(0, g_MyPlanetPieces.Length)].GetComponent<HexTileInfo>();
        }*/
        if (info.i_tileType == 1) info.StartingPoint();

        foreach(HexTileInfo hex in tileInfoList)
        {
            hex.SetNeighbors(FindNeighbors(hex.myPositionInTheArray));
        }
    }

    void WipeBoard()
    {
        int j = g_MyPlanetPieces.Length;
        for (int i = 0; i < j; i++)
        {
            Destroy(g_MyPlanetPieces[i]);
        }
    }

    void SaveLocationInfo()
    {
        OnSaveMyLocationInfo?.Invoke(tileInfoList, s_MyAddress, myResources);
    }

    private void OnApplicationQuit()
    {
        SaveLocationInfo();
    }
}
