using System.Collections.Generic;
using UnityEngine;

public class Trait 
{
    public string UniqueName { private set; get; }
    public string GameElement { private set; get; }
    public int RangeMinimum { private set; get; }
    public int RangeMaximum { private set; get; }
    public int RangeValue { private set; get; }
    public string Description { private set; get; }
    public string Requirements { private set; get; }
    public Color32 TraitColor { private set; get; }
    public int[] Weights { private set; get; }

    public Trait(Trait trait)
    {
        UniqueName = trait.UniqueName;
        GameElement = trait.GameElement;
        RangeMinimum = trait.RangeMinimum;
        RangeMaximum = trait.RangeMaximum;
        RangeValue = trait.RangeValue;
        Description = trait.Description;
        Requirements = trait.Requirements;
        TraitColor = trait.TraitColor;
        Weights = trait.Weights;
    }
    public Trait(string name, string element, int min, int max, string description, string requirements, string weights)
    {
        UniqueName = name;
        GameElement = element;
        RangeMinimum = min;
        RangeMaximum = max;
        RangeValue = 0;
        Description = description;
        Requirements = requirements;
        SetWeights(weights);
        TraitColor = Color.white;
    }
    public Trait(string name, string element, int min, int max,int value, string description, string requirements, string weights)
    {
        UniqueName = name;
        GameElement = element;
        RangeMinimum = min;
        RangeMaximum = max;
        RangeValue = value;
        Description = description;
        Requirements = requirements;
        SetWeights(weights);
        TraitColor = Color.white;
    }
    public Trait(string name, string element)
    {
        UniqueName = name;
        GameElement = element;
        TraitColor = Color.white;
        Weights = new int[0];
    }
    public void Randomize()
    {
        if(Weights.Length > 1)
        {
            RangeValue = Weights[Random.Range(0, Weights.Length)];
            return;
        }
        RangeValue = Random.Range(RangeMinimum, RangeMaximum + 1);
    }
    void SetWeights(string weights)
    {
        if (!string.IsNullOrEmpty(weights))
        {
            List<int> temp = new();
            string[] ar = weights.Split(".");
            foreach (string s in ar)
            {
                temp.Add(int.Parse(s));
            }
            Weights = temp.ToArray();
            return;
        }
        Weights = new int[0];
    }
    public void SetColorWithHexColor(string colorCode)
    {
        ColorUtility.TryParseHtmlString(colorCode, out Color color);
        TraitColor = color;
    }
    public void SetValue(int value)
    {
        RangeValue = value;
    }

    public string DigitizeForSerialization()
    {
        if (UniqueName == "color") return $"{UniqueName},{GameElement},{TraitColor};";

        string s = "";
        if (Weights.Length > 0)
        {
            foreach(int i in Weights)
            {
                s += $"{i}.";
            }
            s = s.Remove(s.Length - 1);
        }

        return $"{UniqueName},{GameElement},{RangeMinimum},{RangeMaximum},{RangeValue},{Description},{Requirements},{s};";
    }
}
