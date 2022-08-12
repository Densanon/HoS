using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class General : MonoBehaviour
{
    LocationManager myLocationManager;
    GeneralsContainerManager myManager;

    [SerializeField]
    TMP_Text nameText;
    [SerializeField]
    TMP_Text locationText;
    [SerializeField]
    TMP_Text statusText;
    [SerializeField]
    GameObject myUIContainer;
    [SerializeField]
    Image myUIImage;

    public enum GeneralType { Basic, NonBasic }
    public GeneralType myType { private set; get; }

    public enum GeneralState { Moving, Combat, Searching, Stop }
    public GeneralState myGeneralState { private set; get; }

    public enum Direction { North, NorthEast, SouthEast, South, SouthWest, NorthWest }
    public Direction generalDirection { private set; get; }

    public string Name { private set; get; }
    public bool Active { private set; get; }
    List<int> directionsAvailable;
    bool isSearching, isMoving, isFighting, directionIsPlayable, needCombat, isStopped;
    public Vector2 myLocation { private set; get; }
    public Vector2 targetLocation { private set; get; }
    public float generalSwitchStateTime { private set; get; }
    private float timeStamp;
    
    System.Random rand = new System.Random();

    private void Update()
    {
        if (Active)
        {
            ExecuteGeneralState();
        }
    }
    public void BecomeActiveGeneral()
    {
        myManager.SetActiveGeneral(this);
    }
    public void ActivateGeneral()
    {
        Debug.Log($"General {Name} trying to be activated.");
        if (myLocation == null || myLocationManager.tileInfoList[Mathf.RoundToInt(myLocation.x)][Mathf.RoundToInt(myLocation.y)].GetSoldierCount() < 1)
        {
            Main.PushMessage($"General {Name}", "The General you are accessing does not have a troop to use. You will need to set" +
             " the troop location before you can use them.");
            return;
        }
        isSearching = false;
        myGeneralState = GeneralState.Searching;
        if(timeStamp != 0)
        {
            CompareTimeStamp();
        }
        isStopped = false;
        Active = true;
    }
    public void Stop()
    {
        Active = false;
        myGeneralState = GeneralState.Stop;
        statusText.text = $"{myGeneralState}";
    }
    public void GetTimeStamp()
    {
        timeStamp = Time.realtimeSinceStartup;
    }
    public void CompareTimeStamp()
    {
        float elapsedTIme = Time.realtimeSinceStartup - timeStamp;
        //Do something for the difference.
    }
    public void BasicSetup(GeneralsContainerManager manager, LocationManager locManager, GeneralType type)
    {
        myManager = manager;
        myLocationManager = locManager;
        myType = type;
        SetRandomName();
        generalSwitchStateTime = 4f;
        statusText.text = "Stop";
    }
    public void SetManager(LocationManager man)
    {
        myLocationManager = man;
    }
    public void SetGeneralType(GeneralType type)
    {
        myType = type;
    }
    public void SetGeneralTroopLocation(Vector2 tile)
    {
        Debug.Log($"General {Name} location set to {tile}.");
        myLocation = tile;
        locationText.text = $"{tile}";
        statusText.text = $"{myGeneralState}";
    }
    public void RequestTroopLocation()
    {
        myManager.AssignTileToGeneral(this);
    }
    public void SetGeneralName(string name)
    {
        Name = name;
        nameText.text = name;
    }
    public void TurnOffUI()
    {
        myUIImage.enabled = false;
        myUIContainer.SetActive(false);
    }
    public void TurnOnUI()
    {
        myUIImage.enabled = true;
        myUIContainer.SetActive(true);
    }
    void SetRandomName()
    {
        System.Random rand = new System.Random();
        int x = rand.Next(0, 3);
        switch (x)
        {
            case 0:
                SetGeneralName("Jeff");
                break;
            case 1:
                SetGeneralName("Jordan");
                break;
            case 2:
                SetGeneralName("Seth");
                break;
            case 3:
                SetGeneralName("ThatOtherGuy");
                break;
        }
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
        Debug.Log($"{Name} is getting new direction.");
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
        Debug.Log($"{Name} checking Direction Playable.");
        switch (generalDirection)
        {
            case Direction.North:
                directionIsPlayable = CheckIfTileIsInNeedOfConquering(myLocationManager.CheckUpLocation(myLocation));
                break;
            case Direction.NorthEast:
                if (myLocation.x % 2 == 1)
                {
                    directionIsPlayable = CheckIfTileIsInNeedOfConquering(myLocationManager.CheckRightUpLocation(myLocation));
                    break;
                }
                directionIsPlayable = CheckIfTileIsInNeedOfConquering(myLocationManager.CheckRightEqualLocation(myLocation));
                break;
            case Direction.SouthEast:
                if (myLocation.x % 2 == 1)
                {
                    directionIsPlayable = CheckIfTileIsInNeedOfConquering(myLocationManager.CheckRightEqualLocation(myLocation));
                    break;
                }
                directionIsPlayable = CheckIfTileIsInNeedOfConquering(myLocationManager.CheckRightDownLocation(myLocation));
                break;
            case Direction.South:
                directionIsPlayable = CheckIfTileIsInNeedOfConquering(myLocationManager.CheckDownLocation(myLocation));
                break;
            case Direction.SouthWest:
                if (myLocation.x % 2 == 1)
                {
                    directionIsPlayable = CheckIfTileIsInNeedOfConquering(myLocationManager.CheckLeftEqualLocation(myLocation));
                    break;
                }
                directionIsPlayable = CheckIfTileIsInNeedOfConquering(myLocationManager.CheckLeftDownLocation(myLocation));
                break;
            case Direction.NorthWest:
                if (myLocation.x % 2 == 1)
                {
                    directionIsPlayable = CheckIfTileIsInNeedOfConquering(myLocationManager.CheckLeftUpLocation(myLocation));
                    break;
                }
                directionIsPlayable = CheckIfTileIsInNeedOfConquering(myLocationManager.CheckLeftEqualLocation(myLocation));
                break;
        }
        Debug.Log($"Direction was playable: {directionIsPlayable}");
        Debug.Log($"Directions still to be checked {directionsAvailable.Count}");
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
        Debug.Log($"Checking if tile is in need of conquering {tile}");
        if (tile.x == -1) return false;
        targetLocation = tile;
        HexTileInfo info = myLocationManager.tileInfoList[Mathf.RoundToInt(tile.x)][Mathf.RoundToInt(tile.y)];
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
                    Debug.Log($"{Name} is searching.");
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
                    Debug.Log($"{Name} is Moving to {targetLocation} from {myLocation}.");
                    isMoving = true;
                    HexTileInfo current = myLocationManager.tileInfoList[Mathf.RoundToInt(myLocation.x)][Mathf.RoundToInt(myLocation.y)];
                    int soldierAmount = current.GetSoldierCount();
                    current.AdjustSoldiers(soldierAmount * -1);
                    HexTileInfo target = myLocationManager.tileInfoList[Mathf.RoundToInt(targetLocation.x)][Mathf.RoundToInt(targetLocation.y)];
                    target.ReceiveGeneralMove(soldierAmount, generalSwitchStateTime);
                    StartCoroutine(SwitchGeneralState());
                }
                break;
            case GeneralState.Combat:
                if (!isFighting)
                {
                    Debug.Log($"{Name} is Fighting at {targetLocation} from {myLocation}.");
                    isFighting = true;
                    HexTileInfo current = myLocationManager.tileInfoList[Mathf.RoundToInt(myLocation.x)][Mathf.RoundToInt(myLocation.y)];
                    int soldierAmount = current.GetSoldierCount();
                    current.AdjustSoldiers(soldierAmount * -1);
                    HexTileInfo target = myLocationManager.tileInfoList[Mathf.RoundToInt(targetLocation.x)][Mathf.RoundToInt(targetLocation.y)];
                    target.potentialAmountToReceive = soldierAmount;
                    target.SetResourceTradingBuddy(myLocation);
                    Debug.Log($"Enemies:{target.enemies.currentAmount} - Soldiers{soldierAmount} = {target.enemies.currentAmount - soldierAmount}");
                    if (target.enemies.currentAmount - soldierAmount <= 0)
                    {
                        myLocation = targetLocation;
                        locationText.text = $"{myLocation}";
                        StartCoroutine(SwitchGeneralState());
                    }
                    else
                    {
                        Debug.Log($"{Name} either tied or lost.");
                        myGeneralState = GeneralState.Stop;
                        statusText.text = $"{myGeneralState}";
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
                    Debug.Log($"{Name} has Stopped.");
                    isStopped = true;
                    Stop();
                }
                break;
        }
    }
    IEnumerator SwitchGeneralState()
    {
        Debug.Log($"Switching state from {myGeneralState}");
        Debug.Log($"Wait time{generalSwitchStateTime}");
        if (myGeneralState == GeneralState.Stop) yield break;
        statusText.text = "Deciding";

        yield return new WaitForSeconds(generalSwitchStateTime);
        Debug.Log($"Wait Over.");

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
            locationText.text = $"{myLocation}";
            isSearching = false;
            myGeneralState = GeneralState.Searching;
        }
        else if (myGeneralState == GeneralState.Combat)
        {
            isSearching = false;
            myGeneralState = GeneralState.Searching;
        }
        Debug.Log($"to {myGeneralState}");
        statusText.text = $"{myGeneralState}";
    }
}
