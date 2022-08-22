using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AtmosphericTraits : Traits
{ 
    public int Humidity { private set; get; }
    public int LightColor { private set; get; }
    public int Toxicity { private set; get; }

    public void SetHumidity (int value) { Humidity = value;}
    public void SetLightColor(int value) { LightColor = value; }
    public void SetToxicity(int value) { Toxicity = value; }
    
    public AtmosphericTraits(int acidity, int acoustic, int compres, int density, int electric, int emmission, int flammability, int luminescence
        ,int temp, int magnetism, int pressure, int radio, int reflect, int smell, int trans, int humid, int lightColor, int toxicity)
    {
        //Generic Traits
        Acidity = acidity;
        AcousticAbsorption = acoustic;
        Compressability = compres;
        Density = density;
        ElectricalCharge = electric;
        FrequencyEmission = emmission;
        Flammability = flammability;
        Luminescence = luminescence;
        Temperature = temp;
        Magnetism = magnetism;
        Pressure = pressure;
        Radioactivity = radio;
        Reflectivity = reflect;
        Smell = smell;
        Transparency = trans;
        //Atmospheric Specific
        Humidity = humid;
        LightColor = lightColor;
        Toxicity = toxicity;
    }

    public void SetGeneralTraits(int acidity, int acoustic, int compres, int density, int electric, int emmission, int flammability, int luminescence
        , int temp, int magnetism, int pressure, int radio, int reflect, int smell, int trans)
    {
        Acidity = acidity;
        AcousticAbsorption = acoustic;
        Compressability = compres;
        Density = density;
        ElectricalCharge = electric;
        FrequencyEmission = emmission;
        Flammability = flammability;
        Luminescence = luminescence;
        Temperature = temp;
        Magnetism = magnetism;
        Pressure = pressure;
        Radioactivity = radio;
        Reflectivity = reflect;
        Smell = smell;
        Transparency = trans;
    }

    public void SetAtmosphericTraits(int humid, int lightColor, int toxicity)
    {
        Humidity = humid;
        LightColor = lightColor;
        Toxicity = toxicity;
    }
}
