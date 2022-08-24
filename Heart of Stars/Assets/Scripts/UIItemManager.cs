using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIItemManager : MonoBehaviour
{
    public static Action<string, Vector2, int> OnTilePickInteraction = delegate { };

    Main main;
    HexTileInfo myTile;
    Spacecraft myShip;

    public GameObject interactiblesContainer;
    [SerializeField]
    GameObject unitActionButton;
    [SerializeField]
    GameObject shipActionButton;
    [SerializeField]
    GameObject itemActionButton;
    [SerializeField]
    GameObject dependenceButtonPrefab;
    [SerializeField]
    GameObject[] Containers;

    [SerializeField]
    Transform mainFunctionsContainer;
    [SerializeField]
    Transform UIPivot;
    List<Item> itemButtons;

    [Header("Ship UI")]
    [SerializeField]    
    Transform shipContentContainer;
    [SerializeField]
    GameObject shipInventoryPrefab;
    [SerializeField]
    TMP_Text shipTitleText;
    [SerializeField]
    TMP_Text shipStorageAmountsText;
    GameObject[] inventoryItemPanels;

    [Header("Troop/Enemy")]
    [SerializeField]
    TMP_Text floatingText;
    [SerializeField]
    TMP_Text unitText;

    [SerializeField]
    Slider unitSlider;
    [SerializeField]
    Slider timerVisual;

    int unitCount;
    bool needVisualTimer = false;
    float currentTime = 0f;
    float timerAmount = 0f;

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
    private void Update()
    {
        if (needVisualTimer) ShowVisualTimer();
    }
    #endregion

    #region Setup
    public void SetMyTileAndMain(HexTileInfo tile, Main m)
    {
        myTile = tile;
        main = m;
        myTile.SetFloatingText(floatingText);
    }
    public void CreateItemButtons()
    {
        if(itemButtons == null) itemButtons = new List<Item>();
        int pivotCount = -1;
        foreach (ItemData data in myTile.myItemData)
        {
            if (data.itemName == "soldier" || data.itemName == "enemy") continue; // These resources will be managed other ways.

            GameObject obs = Instantiate(dependenceButtonPrefab, UIPivot);

            //This creates a fanning effect.
            if(pivotCount != -1) UIPivot.Rotate(0f, 0f, -45f - (22.5f * pivotCount));
            obs.transform.position = new Vector3(obs.transform.position.x, obs.transform.position.y + 100f);
            if (pivotCount != -1) UIPivot.Rotate(0f, 0f, 22.5f + (22.5f * pivotCount));
            pivotCount++;
            
            Item i = obs.GetComponent<Item>();
            i.AssignTile(myTile);
            i.SetUpItem(data, false, main);
            itemButtons.Add(i);
        }
        unitSlider.onValueChanged.AddListener(SetUnitTextForMove);

        foreach(Item item in itemButtons)
        {
            item.ResetRotation();
        }
    }
    public void CheckResourceButtons() //Accessed via button
    {
        if(itemButtons.Count != myTile.myItemData.Length - 2)
        {
            foreach(Item item in itemButtons)
            {
                Destroy(item.gameObject);
            }
            itemButtons.Clear();

            CreateItemButtons();
        }
    }
    #endregion

    #region Timer
    public void SetTimerAndStart(float time)
    {
        currentTime = time;
        timerAmount = 0f;
        timerVisual.gameObject.SetActive(true);
        timerVisual.maxValue = time;
        needVisualTimer = true;
    }
    void ShowVisualTimer()
    {
        timerVisual.value = timerAmount += Time.deltaTime;
        if (timerAmount > currentTime)
        {
            needVisualTimer = false;
            timerAmount = 0f;
            timerVisual.gameObject.SetActive(false);
        }
    }
    #endregion

    #region UI Management
    public void ResetUI()
    {
        mainFunctionsContainer.gameObject.SetActive(true);
        unitActionButton.SetActive(myTile.GetUnitCount() > 0);
        shipActionButton.SetActive(myTile.hasShip && myTile.myShip.status == "Waiting");
        itemActionButton.SetActive(myTile.myItemData.Length > 2);
        foreach(GameObject obj in Containers)
        {
            obj.SetActive(false);
        }
        ResetUnitText();      
    }
    public void ResetUnitText()
    {
        unitCount = myTile.GetUnitCount();
        unitText.text = unitCount.ToString();
        unitSlider.maxValue = unitCount;
        unitSlider.value = unitSlider.maxValue;
    }
    public void SetUnitTextForMove(float amount)
    {
        unitCount = Mathf.RoundToInt(amount);
        unitText.text = amount.ToString();
    }
    public void ActivateTilePickInteraction(string type) //Accessed via button
    {
        myTile.StartDrawingRayForPicking();
        myTile.SetupReceivingOfUnitsForOriginator();
        OnTilePickInteraction(type, myTile.myPositionInTheArray, unitCount);
        myTile.AdjustUnits(unitCount * -1);
        DeactivateSelf();
        ResetUI();
    }
    public void SetupShipInventory() //Accessed via button
    {
        if (inventoryItemPanels != null)
        {
            foreach (GameObject obj in inventoryItemPanels)
            {
                Destroy(obj);
            }
        }

        myShip = myTile.myShip;
        shipTitleText.text = $"{myShip.Name} Inventory";
        UpdateStorageUI();

        ItemData[] ar = myShip.MyItemsData;
        ItemData[] tileAr = myTile.myItemData;
        inventoryItemPanels = new GameObject[ar.Length];
        for(int i = 0; i < ar.Length; i++)
        {
            GameObject go = Instantiate(shipInventoryPrefab, shipContentContainer);
            inventoryItemPanels[i] = go;
            ItemData item = new(ar[1]);
            foreach(ItemData d in tileAr)
            {
                if(d.itemName == ar[i].itemName)
                {
                    item = d;
                    break;
                }
            }
            go.GetComponent<ShipInventoryPanel>().Setup(myShip, ar[i], item);
        }
    }
    void UpdateStorageUI()
    {
        if (myShip == null) myShip = myTile.myShip;
        shipStorageAmountsText.text = $"{myShip.StorageCount} / {myShip.StorageMax}";
    }
    public void GetShipMenu() //Accessed via button
    {
        main.GetShipMenu();
    }
    public void GetShipInfo() //Accessed via button
    {
        myShip.GetShipInfo();
    }
    public void ResetUILocationOnScreen()
    {
        myTile.SetButtonsOnScreenPosition();
    }
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
        Main.OnDestroyLevel -= TurnAndBurn;
        Destroy(this.gameObject);
    }
    #endregion
}
