using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class General : MonoBehaviour
{
    LocationManager myManager;

    [SerializeField]
    public string Name { private set; get; }
    public string name;

    public enum GeneralType { Basic, NonBasic }
    public GeneralType myType { private set; get; }

    public enum GeneralState { Moving, Combat, Searching, Stop }
    public GeneralState myGeneralState { private set; get; }

    public enum Direction { North, NorthEast, SouthEast, South, SouthWest, NorthWest }
    public Direction generalDirection { private set; get; }

    List<int> directionsAvailable;
    bool isSearching, isMoving, isFighting, directionIsPlayable, needCombat, isStopped;
    public Vector2 myLocation { private set; get; }
    public Vector2 targetLocation { private set; get; }
    public float generalSwitchStateTime { private set; get; }
    
    System.Random rand = new System.Random();

    private void Update()
    {
        if (Main.usingGeneral)
        {
            ExecuteGeneralState();
        }
    }
    public void ActivateGeneral()
    {
        Debug.Log($"General {Name} trying to be activated.");
        if (myLocation == null)
        {
            Main.PushMessage($"General {Name}", "The General you are accessing does not have a troop to use. You will need to set" +
             " the troop location before you can use them.");
            return;
        }
        myGeneralState = GeneralState.Searching;
    }
    public void BasicSetup(LocationManager manager, GeneralType type)
    {
        myManager = manager;
        myType = type;
        Name = "";
    }
    public void SetManager(LocationManager man)
    {
        myManager = man;
    }
    public void SetGeneralType(GeneralType type)
    {
        myType = type;
    }
    public void SetGeneralTroopLocation(Vector2 tile)
    {
        Debug.Log($"General {Name} location set to {tile}.");
        myLocation = tile;
    }
    public void SetGeneralName(string name)
    {
        Name = name;
        this.name = name;
    }
    void ResetDirectionsAvailable()
    {
        if (directionsAvailable == null) directionsAvailable = new List<int>();
        directionsAvailable.Clear();
        for (int i = 0; i < 6; i++)
        {
            directionsAvailable.Add(i);
        }
    }
    void GetNewDirection()
    {
        int x = directionsAvailable[rand.Next(0, directionsAvailable.Count)];
        switch (x)
        {
            case 0:
                generalDirection = Direction.North;
                directionsAvailable.Remove(0);
                break;
            case 1:
                generalDirection = Direction.NorthEast;
                directionsAvailable.Remove(1);
                break;
            case 2:
                generalDirection = Direction.SouthEast;
                directionsAvailable.Remove(2);
                break;
            case 3:
                generalDirection = Direction.South;
                directionsAvailable.Remove(3);
                break;
            case 4:
                generalDirection = Direction.SouthWest;
                directionsAvailable.Remove(4);
                break;
            case 5:
                generalDirection = Direction.NorthWest;
                directionsAvailable.Remove(5);
                break;
        }
    }
    void CheckDirectionIsPlayable()
    {
        switch (generalDirection)
        {
            case Direction.North:
                directionIsPlayable = CheckIfTileIsInNeedOfConquering(myManager.CheckUpLocation(myLocation));
                break;
            case Direction.NorthEast:
                if (myLocation.x % 2 == 1)
                {
                    directionIsPlayable = CheckIfTileIsInNeedOfConquering(myManager.CheckRightUpLocation(myLocation));
                    break;
                }
                directionIsPlayable = CheckIfTileIsInNeedOfConquering(myManager.CheckRightEqualLocation(myLocation));
                break;
            case Direction.SouthEast:
                if (myLocation.x % 2 == 1)
                {
                    directionIsPlayable = CheckIfTileIsInNeedOfConquering(myManager.CheckRightEqualLocation(myLocation));
                    break;
                }
                directionIsPlayable = CheckIfTileIsInNeedOfConquering(myManager.CheckRightDownLocation(myLocation));
                break;
            case Direction.South:
                directionIsPlayable = CheckIfTileIsInNeedOfConquering(myManager.CheckDownLocation(myLocation));
                break;
            case Direction.SouthWest:
                if (myLocation.x % 2 == 1)
                {
                    directionIsPlayable = CheckIfTileIsInNeedOfConquering(myManager.CheckLeftEqualLocation(myLocation));
                    break;
                }
                directionIsPlayable = CheckIfTileIsInNeedOfConquering(myManager.CheckLeftDownLocation(myLocation));
                break;
            case Direction.NorthWest:
                if (myLocation.x % 2 == 1)
                {
                    directionIsPlayable = CheckIfTileIsInNeedOfConquering(myManager.CheckLeftUpLocation(myLocation));
                    break;
                }
                directionIsPlayable = CheckIfTileIsInNeedOfConquering(myManager.CheckLeftEqualLocation(myLocation));
                break;
        }

        if (directionIsPlayable) StartCoroutine(SwitchGeneralState());

        if (!directionIsPlayable && directionsAvailable.Count == 0)
        {
            Main.PushMessage($"General {Name}", "I have run out of places around me that aren't conquered. You shoud" +
                " continue on without me, or move the troops where I may start again.");
            directionIsPlayable = true;
            myGeneralState = GeneralState.Stop;
        }
    }
    bool CheckIfTileIsInNeedOfConquering(Vector2 tile)
    {
        if (tile.x == -1) return false;
        targetLocation = tile;
        HexTileInfo info = myManager.tileInfoList[Mathf.RoundToInt(tile.x)][Mathf.RoundToInt(tile.y)];
        needCombat = info.enemies.currentAmount > 0;
        return info.myState == HexTileInfo.TileStates.Clickable;
    }
    void ExecuteGeneralState()
    {
        switch (myGeneralState)
        {
            case GeneralState.Searching:
                if (!isSearching)
                {
                    isSearching = true;
                    directionIsPlayable = false;
                    ResetDirectionsAvailable();
                    while (!directionIsPlayable)
                    {
                        GetNewDirection();
                        CheckDirectionIsPlayable();
                    }
                }
                break;
            case GeneralState.Moving:
                if (!isMoving)
                {
                    isMoving = true;
                    HexTileInfo current = myManager.tileInfoList[Mathf.RoundToInt(myLocation.x)][Mathf.RoundToInt(myLocation.y)];
                    int soldierAmount = current.GetSoldierCount();
                    current.AdjustSoldiers(soldierAmount * -1);
                    HexTileInfo target = myManager.tileInfoList[Mathf.RoundToInt(targetLocation.x)][Mathf.RoundToInt(targetLocation.y)];
                    target.ReceiveGeneralMove(soldierAmount, generalSwitchStateTime);
                    StartCoroutine(SwitchGeneralState());
                }
                break;
            case GeneralState.Combat:
                if (!isFighting)
                {
                    isFighting = true;
                    HexTileInfo current = myManager.tileInfoList[Mathf.RoundToInt(myLocation.x)][Mathf.RoundToInt(myLocation.y)];
                    int soldierAmount = current.GetSoldierCount();
                    current.AdjustSoldiers(soldierAmount * -1);
                    HexTileInfo target = myManager.tileInfoList[Mathf.RoundToInt(targetLocation.x)][Mathf.RoundToInt(targetLocation.y)];
                    target.potentialAmountToReceive = soldierAmount;
                    target.SetResourceTradingBuddy(myLocation);
                    if (target.enemies.currentAmount - soldierAmount < 0)
                    {
                        myLocation = targetLocation;
                        StartCoroutine(SwitchGeneralState());
                    }
                    else
                    {
                        myGeneralState = GeneralState.Stop;
                        if (Main.cantLose)
                        {
                            StartCoroutine(SwitchGeneralState());
                        }
                    }
                    target.StartCoroutine(target.BattleSequence());
                }
                break;
            case GeneralState.Stop:
                if (!isStopped)
                {
                    isStopped = true;
                }
                break;
        }
    }
    IEnumerator SwitchGeneralState()
    {

        yield return new WaitForSeconds(generalSwitchStateTime);

        if (myGeneralState == GeneralState.Searching)
        {
            if (needCombat)
            {
                isFighting = false;
                myGeneralState = GeneralState.Combat;
            }
            else
            {
                isMoving = false;
                myGeneralState = GeneralState.Moving;
            }
        }
        else if (myGeneralState == GeneralState.Moving)
        {
            myLocation = targetLocation;
            isSearching = false;
            myGeneralState = GeneralState.Searching;
        }
        else if (myGeneralState == GeneralState.Combat)
        {
            isSearching = false;
            myGeneralState = GeneralState.Searching;
        }
        else if(myGeneralState == GeneralState.Stop)
        {
            isSearching = false;
            myGeneralState = GeneralState.Searching;
        }
    }
}
