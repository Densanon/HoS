using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ResourceData 
{
    public string itemName { get; private set; }
    public string displayName { get; private set; }
    public bool visible { get; private set; }
    public int currentAmount { get; private set; }
    public int autoAmount { get; private set; }
    public float autoTime { get; private set; }
    public string requirements { get; private set; }

    public ResourceData(string name, string display, string reqs, bool vis, int cur, int autoA, float autoT)
    {
        itemName = name;
        displayName = display;
        requirements = reqs;
        visible = vis;
        currentAmount = cur;
        autoAmount = autoA;
        autoTime = autoT;
    }

    public ResourceData(ResourceData data)
    {
        itemName = data.itemName;
        displayName = data.displayName;
        requirements = data.requirements;
        visible = data.visible;
        currentAmount = data.currentAmount;
        autoAmount = data.autoAmount;
        autoTime = data.autoTime;
    }

    public void AdjustCurrentAmount(int amount)
    {
        currentAmount += amount;
    }

    public void AdjustAutoAmount(int amount)
    {
        autoAmount += amount;
    }

    public void AdjustAutoTimer(float amount)
    {
        autoTime += amount;
    }

    public void AdjustVisibility(bool vis)
    {
        visible = vis;
    }

    public string DigitizeForSerialization()
    {
        return $"{itemName},{displayName},{requirements},{visible},{currentAmount},{autoAmount},{autoTime};";
    }
}
