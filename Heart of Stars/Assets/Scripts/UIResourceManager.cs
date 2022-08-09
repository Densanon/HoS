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
    GameObject[] Containers;
    [SerializeField]
    Transform mainFunctionsContainer;
    [SerializeField]
    Transform resourceContainer;
    [SerializeField]
    GameObject troopActionButton;
    [SerializeField]
    TMP_Text troopText;
    [SerializeField]
    Slider troopSlider;
    int troopCount;

    [SerializeField]
    GameObject dependenceButtonPrefab;
    HoverAbleResourceButton[] resourceButtons;

    public void SetMyTileAndMain(HexTileInfo tile, Main m)
    {
        myTile = tile;
        main = m;
    }
    public void CreateResourceButtons()
    {
        List<HoverAbleResourceButton> temp = new List<HoverAbleResourceButton>();
        foreach (ResourceData data in myTile.myResources)
        {
            if (data.itemName == "soldier") continue;
            GameObject obs = Instantiate(dependenceButtonPrefab, resourceContainer);
            obs.transform.position = new Vector3(obs.transform.position.x, obs.transform.position.y + 100f);
            resourceContainer.Rotate(0f, 0f, 360f / (myTile.myResources.Length - 1));
            Resource r = obs.GetComponent<Resource>();
            r.AssignTile(myTile);
            r.SetUpResource(data, false, main);
            temp.Add(obs.GetComponent<HoverAbleResourceButton>());
        }
        resourceButtons = temp.ToArray();
        troopSlider.onValueChanged.AddListener(SetTroopTextForMove);

        ResetUI();
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
        gameObject.SetActive(true);
    }
    public void DeactivateSelf()
    {
        gameObject.SetActive(false);
        activeMouseHoverInteractions = 0;
    }
}
