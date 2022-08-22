using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Traits
{
    public int Acidity { protected set; get; }
    public int AcousticAbsorption { protected set; get; }
    public int Compressability { protected set; get; }
    public int Density { protected set; get; }
    public int ElectricalCharge { protected set; get; }
    public int FrequencyEmission { protected set; get; }
    public int Flammability { protected set; get; }
    public int Luminescence { protected set; get; }
    public int Temperature { protected set; get; }
    public int Magnetism { protected set; get; }
    public int Pressure { protected set; get; }
    public int Radioactivity { protected set; get; }
    public int Reflectivity { protected set; get; }
    public int Smell { protected set; get; }
    public int Transparency { protected set; get; }

    public void SetAcidity(int value) { Acidity = value; }
    public void SetAcousticAbsorption(int value) { AcousticAbsorption = value; }
    public void SetCompressability(int value) { Compressability = value; }
    public void SetDensity(int value) { Density = value; }
    public void SetElectricalCharge(int value) { ElectricalCharge = value; }
    public void SetFrequencyEmission(int value) { FrequencyEmission = value; }
    public void SetFlammability(int value) { Flammability = value; }
    public void SetLuminescence(int value) { Luminescence = value; }
    public void SetTemperature(int value) { Temperature = value; }
    public void SetMagnetism(int value) { Magnetism = value; }
    public void SetPressure(int value) { Pressure = value; }
    public void SetRadioactivity(int value) { Radioactivity = value; }
    public void SetReflectivity(int value) { Reflectivity = value; }
    public void SetSmell(int value) { Smell = value; }
    public void SetTransparency(int value) { Transparency = value; }
}
