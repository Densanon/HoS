using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ResourceData 
{
    public string itemName;
    public string displayName;
    public bool visibile;
    public int currentAmount;
    public int autoAmount;
    public float autoTime;
    public string requirements;

    public ResourceData(string name, string display, string reqs, bool vis, int cur, int autoA, float autoT)
    {
        itemName = name;
        displayName = display;
        requirements = reqs;
        visibile = vis;
        currentAmount = cur;
        autoAmount = autoA;
        autoTime = autoT;
    }

    public ResourceData(ResourceData data)
    {
        itemName = data.itemName;
        displayName = data.displayName;
        requirements = data.requirements;
        visibile = data.visibile;
        currentAmount = data.currentAmount;
        autoAmount = data.autoAmount;
        autoTime = data.autoTime;
    }

    public string DigitizeForSerialization()
    {
        return $"{itemName},{displayName},{requirements},{visibile},{currentAmount},{autoAmount},{autoTime};";
    }
}
