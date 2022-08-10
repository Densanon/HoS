using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIResourceManager : MonoBehaviour
{
    public static Action<string, Vector2, int> OnTilePickInteraction = delegate { };

    Main main;
    HexTileInfo myTile;

    public int activeMouseHoverInteractions;
    [SerializeField]
    public GameObject interactiblesContainer;
    [SerializeField]
    GameObject[] Containers;
    [SerializeField]
    Transform mainFunctionsContainer;
    [SerializeField]
    Transform resourceContainer;
    [SerializeField]
    Transform resourcePivot;
    [SerializeField]
    GameObject troopActionButton;
    [SerializeField]
    TMP_Text troopText;
    [SerializeField]
    Slider troopSlider;
    int troopCount;

    [SerializeField]
    TMP_Text floatingText;
    [SerializeField]
    Slider visualBattleTimer;
    bool needBattleTimer = false;
    float battleTime = 0f;
    float battleTimer = 0f;

    [SerializeField]
    GameObject dependenceButtonPrefab;
    HoverAbleResourceButton[] resourceButtons;

    private void OnEnable()
    {
        Main.OnDestroyLevel += TurnAndBurn;
    }

    private void TurnAndBurn()
    {
        Main.OnDestroyLevel -= TurnAndBurn;
        Destroy(this.gameObject);
    }

    private void FixedUpdate()
    {
        if (needBattleTimer)
        {           
            visualBattleTimer.value = battleTimer += Time.fixedDeltaTime;
            if (battleTimer > battleTime)
            {
                needBattleTimer = false;
                battleTimer = 0f;
                visualBattleTimer.gameObject.SetActive(false);
            }
        }
    }

    public void SetBattleTimerAndStart(float time)
    {
        battleTime = time;
        visualBattleTimer.gameObject.SetActive(true);
        visualBattleTimer.maxValue = time;
        needBattleTimer = true;
    }

    public void SetMyTileAndMain(HexTileInfo tile, Main m)
    {
        myTile = tile;
        main = m;

        myTile.SetFloatingText(floatingText);
    }
    public void CreateResourceButtons()
    {
        List<HoverAbleResourceButton> temp = new List<HoverAbleResourceButton>();
        List<Resource> rotationList = new List<Resource>();
        int pivotCount = -1;
        foreach (ResourceData data in myTile.myResources)
        {
            if (data.itemName == "soldier" || data.itemName == "enemy") continue;
            
            GameObject obs = Instantiate(dependenceButtonPrefab, resourcePivot);
            if(pivotCount != -1) resourcePivot.Rotate(0f, 0f, -45f - (22.5f * pivotCount));
            obs.transform.position = new Vector3(obs.transform.position.x, obs.transform.position.y + 100f);
            if (pivotCount != -1) resourcePivot.Rotate(0f, 0f, 22.5f + (22.5f * pivotCount));
            pivotCount++;
            
            Resource r = obs.GetComponent<Resource>();
            r.AssignTile(myTile);
            r.SetUpResource(data, false, main);
            rotationList.Add(r);
            temp.Add(obs.GetComponent<HoverAbleResourceButton>());
        }
        resourceButtons = temp.ToArray();
        troopSlider.onValueChanged.AddListener(SetTroopTextForMove);

        foreach(Resource res in rotationList)
        {
            res.ResetRotation();
        }
        rotationList.Clear();

        //ResetUI();
    }
    public void ResetUI()
    {
        //Debug.Log("Reseting the UI.");
        mainFunctionsContainer.gameObject.SetActive(true);
        troopActionButton.SetActive(myTile.GetSoldierCount() > 0);
        foreach(GameObject obj in Containers)
        {
            //Debug.Log($"{obj.name} is getting turned off.");
            obj.gameObject.SetActive(false);
        }
        ResetTroopText();      
    }

    public void SetTroopTextForMove(float amount)
    {
        troopCount = Mathf.RoundToInt(amount);
        troopText.text = amount.ToString();
    }
    public void ResetTroopText()
    {
        troopCount = myTile.GetSoldierCount();
        troopText.text = troopCount.ToString();
        troopSlider.maxValue = troopCount;
        troopSlider.value = troopSlider.maxValue;
    }

    public void AddMouseHoverInteraction()
    {
        activeMouseHoverInteractions++;
    }
    public void SubtractMouseHoverInteraction()
    {
        activeMouseHoverInteractions--;
        if(activeMouseHoverInteractions < 0) activeMouseHoverInteractions = 0;
    }
    public bool CheckMouseOnUI()
    {
        Debug.Log("Mouse actions.");
        if (activeMouseHoverInteractions > 0) return true;

        Debug.Log("Manual buttons.");
        foreach(HoverAbleResourceButton button in resourceButtons)
        {
            if (button.isHovering) { 
                Debug.Log("One of the buttons is active.");
                return true; }
                
        }
        Debug.Log("All good here.");
        return false;
    }
    public void ActivateTilePickInteraction(string type)
    {
        myTile.StartDrawingRayForPicking();
        myTile.SetupReceivingOfTroopsForOriginator();
        OnTilePickInteraction(type, myTile.myPositionInTheArray, troopCount);
        myTile.AdjustSoldiers(troopCount * -1);
        DeactivateSelf();
        ResetUI();
    }
    public void ActivateSelf()
    {
        interactiblesContainer.SetActive(true);
    }
    public void DeactivateSelf()
    {
        interactiblesContainer.SetActive(false);
        activeMouseHoverInteractions = 0;
    }
}
