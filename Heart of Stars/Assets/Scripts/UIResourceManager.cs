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
    Spacecraft myShip;

    public GameObject interactiblesContainer;
    [SerializeField]
    GameObject troopActionButton;
    [SerializeField]
    GameObject shipActionButton;
    [SerializeField]
    GameObject dependenceButtonPrefab;
    [SerializeField]
    GameObject[] Containers;

    [SerializeField]
    Transform mainFunctionsContainer;
    [SerializeField]
    Transform resourceContainer;
    [SerializeField]
    Transform resourcePivot;
    List<Resource> resourceButtons;

    [Header("Ship UI")]
    [SerializeField]    
    Transform shipContentContainer;
    [SerializeField]
    GameObject shipInventoryPrefab;
    [SerializeField]
    TMP_Text shipTitleText;
    [SerializeField]
    TMP_Text shipStorageAmountsText;
    GameObject[] inventoryResourcePanels;

    [Header("Troop/Enemy")]
    [SerializeField]
    TMP_Text floatingText;
    [SerializeField]
    TMP_Text troopText;

    [SerializeField]
    Slider troopSlider;
    [SerializeField]
    Slider visualBattleTimer;

    int troopCount;
    bool needVisualTimer = false;
    float battleTime = 0f;
    float battleTimer = 0f;

    #region UnityEngine
    private void OnEnable()
    {
        Main.OnDestroyLevel += TurnAndBurn;
        ShipInventoryPanel.OnUpdateSliderForShip += UpdateStorageUI;
    }
    private void OnDisable()
    {
        Main.OnDestroyLevel -= TurnAndBurn;
        ShipInventoryPanel.OnUpdateSliderForShip -= UpdateStorageUI;

    }
    private void FixedUpdate()
    {
        if (needVisualTimer)
        {
            ShowVisualTimer();
        }
    }
    #endregion

    #region Timer
    public void SetTimerAndStart(float time)
    {
        battleTime = time;
        battleTimer = 0f;
        visualBattleTimer.gameObject.SetActive(true);
        visualBattleTimer.maxValue = time;
        needVisualTimer = true;
    }
    void ShowVisualTimer()
    {
        visualBattleTimer.value = battleTimer += Time.fixedDeltaTime;
        if (battleTimer > battleTime)
        {
            needVisualTimer = false;
            battleTimer = 0f;
            visualBattleTimer.gameObject.SetActive(false);
        }
    }
    #endregion

    #region Setup
    public void SetMyTileAndMain(HexTileInfo tile, Main m)
    {
        myTile = tile;
        main = m;

        myTile.SetFloatingText(floatingText);
    }
    public void CreateResourceButtons()
    {
        if(resourceButtons == null) resourceButtons = new List<Resource>();
        int pivotCount = -1;
        foreach (ResourceData data in myTile.myResources)
        {
            if (data.itemName == "soldier" || data.itemName == "enemy") continue; // These resources will be managed other ways.
            
            GameObject obs = Instantiate(dependenceButtonPrefab, resourcePivot);

            //This creates a fanning effect.
            if(pivotCount != -1) resourcePivot.Rotate(0f, 0f, -45f - (22.5f * pivotCount));
            obs.transform.position = new Vector3(obs.transform.position.x, obs.transform.position.y + 100f);
            if (pivotCount != -1) resourcePivot.Rotate(0f, 0f, 22.5f + (22.5f * pivotCount));
            pivotCount++;
            
            Resource r = obs.GetComponent<Resource>();
            r.AssignTile(myTile);
            r.SetUpResource(data, false, main);
            resourceButtons.Add(r);
        }
        troopSlider.onValueChanged.AddListener(SetTroopTextForMove);

        foreach(Resource res in resourceButtons)
        {
            res.ResetRotation();
        }
    }
    public void CheckResourceButtons()
    {
        if(resourceButtons.Count != myTile.myResources.Length - 2)
        {
            foreach(Resource resource in resourceButtons)
            {
                Destroy(resource.gameObject);
            }
            resourceButtons.Clear();

            CreateResourceButtons();
        }
    }
    #endregion

    #region UI Management
    public void ResetUI()
    {
        mainFunctionsContainer.gameObject.SetActive(true);
        troopActionButton.SetActive(myTile.GetSoldierCount() > 0);
        shipActionButton.SetActive(myTile.hasShip && myTile.myShip.status == "Waiting");
        foreach(GameObject obj in Containers)
        {
            obj.gameObject.SetActive(false);
        }
        ResetTroopText();      
    }
    public void ResetTroopText()
    {
        troopCount = myTile.GetSoldierCount();
        troopText.text = troopCount.ToString();
        troopSlider.maxValue = troopCount;
        troopSlider.value = troopSlider.maxValue;
    }
    public void SetTroopTextForMove(float amount)
    {
        troopCount = Mathf.RoundToInt(amount);
        troopText.text = amount.ToString();
    }
    public void ActivateTilePickInteraction(string type) //Accessed via button
    {
        myTile.StartDrawingRayForPicking();
        myTile.SetupReceivingOfTroopsForOriginator();
        OnTilePickInteraction(type, myTile.myPositionInTheArray, troopCount);
        myTile.AdjustSoldiers(troopCount * -1);
        DeactivateSelf();
        ResetUI();
    }
    public void SetupShipInventory()
    {
        if (inventoryResourcePanels != null)
        {
            foreach (GameObject obj in inventoryResourcePanels)
            {
                Destroy(obj);
            }
        }

        myShip = myTile.myShip;
        shipTitleText.text = $"{myShip.Name} Inventory";
        UpdateStorageUI();

        ResourceData[] ar = myShip.myResources;
        ResourceData[] tileAr = myTile.myResources;
        inventoryResourcePanels = new GameObject[ar.Length];
        for(int i = 0; i < ar.Length; i++)
        {
            GameObject go = Instantiate(shipInventoryPrefab, shipContentContainer);
            inventoryResourcePanels[i] = go;
            ResourceData dat = new ResourceData(ar[1]);
            foreach(ResourceData d in tileAr)
            {
                if(d.itemName == ar[i].itemName)
                {
                    dat = d;
                }
            }
            go.GetComponent<ShipInventoryPanel>().Setup(myShip, ar[i], dat);
        }

    }
    void UpdateStorageUI()
    {
        if (myShip == null) myShip = myTile.myShip;
        shipStorageAmountsText.text = $"{myShip.storageCount} / {myShip.storageMax}";
    }
    public void GetShipMenu()
    {
        main.GetShipMenu();
    }//Accessed via button
    public void GetShipInfo()
    {
        myShip.GetShipInfo();
    }//Accessed via button
    #endregion

    #region Life Cycle
    public void ActivateSelf()
    {
        interactiblesContainer.SetActive(true);
    }
    public void DeactivateSelf()
    {
        interactiblesContainer.SetActive(false);
    }
    private void TurnAndBurn()
    {
        Destroy(this.gameObject);
    }
    #endregion
}
