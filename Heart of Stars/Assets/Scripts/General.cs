using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class General : MonoBehaviour
{
    LocationManager myLocationManager;
    GeneralsContainerManager myManager;

    #region UI elements
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
    #endregion

    #region Enums
    public enum GeneralType { Basic, NonBasic }
    public GeneralType MyType { private set; get; }

    public enum GeneralState { Moving, Combat, Searching, Stop }
    public GeneralState MyGeneralState { private set; get; }

    public enum Direction { North, NorthEast, SouthEast, South, SouthWest, NorthWest }
    public Direction GeneralDirection { private set; get; }
    #endregion

    public string Name { private set; get; }
    public bool Active { private set; get; }
    List<int> directionsAvailable;
    bool isSearching, isMoving, isFighting, directionIsPlayable, needCombat;
    public Vector2 MyLocation { private set; get; }
    public Vector2 TargetLocation { private set; get; }
    public float GeneralSwitchStateTime { private set; get; }
    private float timeStamp;
    
    readonly System.Random Rand = new();

    #region UnityEngine
    private void Update()
    {
        if (Active)
        {
            ExecuteGeneralState();
        }
    }
    #endregion

    #region Setup and Assignment
    public void BasicSetup(GeneralsContainerManager manager, LocationManager locManager, GeneralType type)
    {
        myManager = manager;
        myLocationManager = locManager;
        MyType = type;
        SetRandomName();
        GeneralSwitchStateTime = 4f;
        statusText.text = "Stop";
    }
    void SetRandomName()
    {
        int x = Rand.Next(0, 4);
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
    public void SetGeneralName(string name)
    {
        Name = name;
        nameText.text = name;
    }
    public void SetGeneralTroopLocation(Vector2 tile)
    {
        Debug.Log($"General {Name} location set to {tile}.");
        MyLocation = tile;
        locationText.text = $"{tile}";
        statusText.text = $"{MyGeneralState}";
    }
    public void RequestTroopLocation() //Accessed via button
    {
        myManager.AssignTileToGeneral(this);
    }
    public void SetGeneralType(GeneralType type) //Not implemented yet
    {
        MyType = type;
    }
    #endregion

    #region External Commands
    public void Stop()
    {
        StopGeneral();
    }
    public void Stop(string message)
    {
        StopGeneral();
        Main.PushMessage($"General {Name}", message);
    }
    void StopGeneral()
    {
        Active = false;
        MyGeneralState = GeneralState.Stop;
        statusText.text = $"{MyGeneralState}";
    }
    public void ActivateGeneral()
    {
        if (MyLocation == null || myLocationManager.tileInfoList[Mathf.RoundToInt(MyLocation.x)][Mathf.RoundToInt(MyLocation.y)].GetUnitCount() < 1)
        {
            Main.PushMessage($"General {Name}", "The General you are accessing does not have a troop to use. You will need to set" +
             " the troop location before you can use them.");
            return;
        }
        isSearching = false;
        MyGeneralState = GeneralState.Searching;
        if(timeStamp != 0)
        {
            CompareTimeStamp();
        }
        Active = true;
    }
    public void BecomeActiveGeneral()// Accessed via button
    {
        myManager.SetActiveGeneral(this);
    } 
    public void GetTimeStamp()
    {
        timeStamp = Time.realtimeSinceStartup;
    }
    public void CompareTimeStamp()
    {
        float elapsedTIme = Time.realtimeSinceStartup - timeStamp;
        //Do something for the difference. Not implemented.
    }
    #endregion

    #region UI
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
    #endregion

    #region Brain Functions
    void ExecuteGeneralState()
    {
        switch (MyGeneralState)
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
                    HexTileInfo current = myLocationManager.tileInfoList[Mathf.RoundToInt(MyLocation.x)][Mathf.RoundToInt(MyLocation.y)];
                    int unitAmount = current.GetUnitCount();
                    current.AdjustUnits(unitAmount * -1);
                    HexTileInfo target = myLocationManager.tileInfoList[Mathf.RoundToInt(TargetLocation.x)][Mathf.RoundToInt(TargetLocation.y)];
                    target.ReceiveGeneralMove(unitAmount, GeneralSwitchStateTime);

                    StartCoroutine(SwitchGeneralState());
                }
                break;
            case GeneralState.Combat:
                if (!isFighting)
                {
                    isFighting = true;
                    HexTileInfo tile = myLocationManager.tileInfoList[Mathf.RoundToInt(MyLocation.x)][Mathf.RoundToInt(MyLocation.y)];
                    int unitAmount = tile.GetUnitCount();
                    tile.AdjustUnits(unitAmount * -1);
                    HexTileInfo target = myLocationManager.tileInfoList[Mathf.RoundToInt(TargetLocation.x)][Mathf.RoundToInt(TargetLocation.y)];
                    target.potentialAmountToReceive = unitAmount;
                    target.SetResourceTradingBuddy(MyLocation);
                    target.StartCoroutine(target.BattleSequence());
                    if (target.enemies.currentAmount - unitAmount <= 0)
                    {
                        MyLocation = TargetLocation;
                        locationText.text = $"{MyLocation}";
                        StartCoroutine(SwitchGeneralState());
                        return;
                    }

                    Stop("I have run into a  worthy adversary who has bested my efforts. Please, let me know what you would have me do.");
                }
                break;
        }
    }
    IEnumerator SwitchGeneralState()
    {
        if (MyGeneralState == GeneralState.Stop) yield break;
        statusText.text = "Deciding";

        yield return new WaitForSeconds(GeneralSwitchStateTime);

        if (MyGeneralState == GeneralState.Searching)
        {
            if (needCombat)
            {
                isFighting = false;
                MyGeneralState = GeneralState.Combat;
            }
            else
            {
                isMoving = false;
                MyGeneralState = GeneralState.Moving;
            }
        }
        else if (MyGeneralState == GeneralState.Moving)
        {
            MyLocation = TargetLocation;
            locationText.text = $"{MyLocation}";
            isSearching = false;
            MyGeneralState = GeneralState.Searching;
        }
        else if (MyGeneralState == GeneralState.Combat)
        {
            isSearching = false;
            MyGeneralState = GeneralState.Searching;
        }
        statusText.text = $"{MyGeneralState}";
    }
    void ResetDirectionsAvailable()
    {
        if (directionsAvailable == null) directionsAvailable = new List<int>();
        directionsAvailable.Clear();
        for (int i = 0; i < 6; i++)
        {
            //Setting an integer value 0-5 to associate with a direction
            directionsAvailable.Add(i);
        }
    }
    void GetNewDirection()
    {
        int x = directionsAvailable[Rand.Next(0, directionsAvailable.Count)];
        switch (x)
        {
            case 0:
                GeneralDirection = Direction.North;
                directionsAvailable.Remove(0); //Removing the integer we associate it with from the options to choose from.
                break;
            case 1:
                GeneralDirection = Direction.NorthEast;
                directionsAvailable.Remove(1);
                break;
            case 2:
                GeneralDirection = Direction.SouthEast;
                directionsAvailable.Remove(2);
                break;
            case 3:
                GeneralDirection = Direction.South;
                directionsAvailable.Remove(3);
                break;
            case 4:
                GeneralDirection = Direction.SouthWest;
                directionsAvailable.Remove(4);
                break;
            case 5:
                GeneralDirection = Direction.NorthWest;
                directionsAvailable.Remove(5);
                break;
        }
    }
    void CheckDirectionIsPlayable()
    {
        switch (GeneralDirection)
        {
            case Direction.North:
                directionIsPlayable = CheckIfTileIsInNeedOfConquering(myLocationManager.CheckUpLocation(MyLocation));
                break;
            case Direction.NorthEast:
                if (MyLocation.x % 2 == 1)//Because of the nature of the hextile, where something looks doesn't always line up with the array position and needs to be picked correctly.
                {//Basically just checking whether the tile is in an even or odd column.
                    directionIsPlayable = CheckIfTileIsInNeedOfConquering(myLocationManager.CheckRightUpLocation(MyLocation));
                    break;
                }
                directionIsPlayable = CheckIfTileIsInNeedOfConquering(myLocationManager.CheckRightEqualLocation(MyLocation));
                break;
            case Direction.SouthEast:
                if (MyLocation.x % 2 == 1)
                {
                    directionIsPlayable = CheckIfTileIsInNeedOfConquering(myLocationManager.CheckRightEqualLocation(MyLocation));
                    break;
                }
                directionIsPlayable = CheckIfTileIsInNeedOfConquering(myLocationManager.CheckRightDownLocation(MyLocation));
                break;
            case Direction.South:
                directionIsPlayable = CheckIfTileIsInNeedOfConquering(myLocationManager.CheckDownLocation(MyLocation));
                break;
            case Direction.SouthWest:
                if (MyLocation.x % 2 == 1)
                {
                    directionIsPlayable = CheckIfTileIsInNeedOfConquering(myLocationManager.CheckLeftEqualLocation(MyLocation));
                    break;
                }
                directionIsPlayable = CheckIfTileIsInNeedOfConquering(myLocationManager.CheckLeftDownLocation(MyLocation));
                break;
            case Direction.NorthWest:
                if (MyLocation.x % 2 == 1)
                {
                    directionIsPlayable = CheckIfTileIsInNeedOfConquering(myLocationManager.CheckLeftUpLocation(MyLocation));
                    break;
                }
                directionIsPlayable = CheckIfTileIsInNeedOfConquering(myLocationManager.CheckLeftEqualLocation(MyLocation));
                break;
        }
        if (directionIsPlayable)
        {
            StartCoroutine(SwitchGeneralState());
            return;
        }

        if (!directionIsPlayable && directionsAvailable.Count == 0)
        {
            Main.PushMessage($"General {Name}", "I have run out of places around me that aren't conquered. You shoud" +
                " continue on without me, or move the units where I may start again.");
            directionIsPlayable = true;
            MyGeneralState = GeneralState.Stop;
        }
    }
    bool CheckIfTileIsInNeedOfConquering(Vector2 tile) //Setting target and checking if the tile is in the Clickable state and has units
    {
        if (tile.x == -1) return false; //if the tile.x is -1 then the location doesn't exist

        TargetLocation = tile;
        HexTileInfo info = myLocationManager.tileInfoList[Mathf.RoundToInt(tile.x)][Mathf.RoundToInt(tile.y)];
        needCombat = info.enemies.currentAmount > 0;
        return info.myState == HexTileInfo.TileStates.Clickable;
    }
    #endregion
}
