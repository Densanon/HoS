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

    #region Enumerators
    public enum GeneralType { Basic, NonBasic }
    public GeneralType myType { private set; get; }

    public enum GeneralState { Moving, Combat, Searching, Stop }
    public GeneralState myGeneralState { private set; get; }

    public enum Direction { North, NorthEast, SouthEast, South, SouthWest, NorthWest }
    public Direction generalDirection { private set; get; }
    #endregion

    public string Name { private set; get; }
    public bool Active { private set; get; }
    List<int> directionsAvailable;
    bool isSearching, isMoving, isFighting, directionIsPlayable, needCombat;
    public Vector2 myLocation { private set; get; }
    public Vector2 targetLocation { private set; get; }
    public float generalSwitchStateTime { private set; get; }
    private float timeStamp;
    
    System.Random rand = new System.Random();

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
        myType = type;
        SetRandomName();
        generalSwitchStateTime = 4f;
        statusText.text = "Stop";
    }
    void SetRandomName()
    {
        System.Random rand = new System.Random();
        int x = rand.Next(0, 4);
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
        myLocation = tile;
        locationText.text = $"{tile}";
        statusText.text = $"{myGeneralState}";
    }
    public void RequestTroopLocation() //Accessed via button
    {
        myManager.AssignTileToGeneral(this);
    }
    public void SetGeneralType(GeneralType type) //Not implemented yet
    {
        myType = type;
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
        myGeneralState = GeneralState.Stop;
        statusText.text = $"{myGeneralState}";
    }
    public void ActivateGeneral()
    {
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
                    isFighting = true;
                    HexTileInfo current = myLocationManager.tileInfoList[Mathf.RoundToInt(myLocation.x)][Mathf.RoundToInt(myLocation.y)];
                    int soldierAmount = current.GetSoldierCount();
                    current.AdjustSoldiers(soldierAmount * -1);
                    HexTileInfo target = myLocationManager.tileInfoList[Mathf.RoundToInt(targetLocation.x)][Mathf.RoundToInt(targetLocation.y)];
                    target.potentialAmountToReceive = soldierAmount;
                    target.SetResourceTradingBuddy(myLocation);
                    target.StartCoroutine(target.BattleSequence());
                    if (target.enemies.currentAmount - soldierAmount <= 0)
                    {
                        myLocation = targetLocation;
                        locationText.text = $"{myLocation}";
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
        if (myGeneralState == GeneralState.Stop) yield break;
        statusText.text = "Deciding";

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
            locationText.text = $"{myLocation}";
            isSearching = false;
            myGeneralState = GeneralState.Searching;
        }
        else if (myGeneralState == GeneralState.Combat)
        {
            isSearching = false;
            myGeneralState = GeneralState.Searching;
        }
        statusText.text = $"{myGeneralState}";
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
        int x = directionsAvailable[rand.Next(0, directionsAvailable.Count)];
        switch (x)
        {
            case 0:
                generalDirection = Direction.North;
                directionsAvailable.Remove(0); //Removing the integer we associate it with from the options to choose from.
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
                directionIsPlayable = CheckIfTileIsInNeedOfConquering(myLocationManager.CheckUpLocation(myLocation));
                break;
            case Direction.NorthEast:
                if (myLocation.x % 2 == 1)//Because of the nature of the hextile, where something looks doesn't always line up with the array position and needs to be picked correctly.
                {//Basically just checking whether the tile is in an even or odd column.
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
        if (directionIsPlayable)
        {
            StartCoroutine(SwitchGeneralState());
            return;
        }

        if (!directionIsPlayable && directionsAvailable.Count == 0)
        {
            Main.PushMessage($"General {Name}", "I have run out of places around me that aren't conquered. You shoud" +
                " continue on without me, or move the troops where I may start again.");
            directionIsPlayable = true;
            myGeneralState = GeneralState.Stop;
        }
    }
    bool CheckIfTileIsInNeedOfConquering(Vector2 tile) //Setting target and checking if the tile is in the Clickable state and has troops
    {
        if (tile.x == -1) return false; //if the tile.x is -1 then the location doesn't exist

        targetLocation = tile;
        HexTileInfo info = myLocationManager.tileInfoList[Mathf.RoundToInt(tile.x)][Mathf.RoundToInt(tile.y)];
        needCombat = info.enemies.currentAmount > 0;
        return info.myState == HexTileInfo.TileStates.Clickable;
    }
    #endregion
}
