using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TerrainTraits : Traits
{ 
    public int BoilingPoint { private set; get; }
    public int Brittleness { private set; get; }
    public int Elasticity { private set; get; }
    public int FluctuationOfSteepness { private set; get; }
    public int Gravity { private set; get; }
    public int Hardness { private set; get; }
    public int LiquidToxicity { private set; get; }
    public int Sharpness { private set; get; }
    public int SolidityLiquidity { private set; get; }
    public int SteepnessOfTopography { private set; get; }
    public int Structure { private set; get; }
    public int Vegitation { private set; get; }

    public void SetBoilingPoint(int value) { BoilingPoint = value; }
    public void SetBrittleness(int value) { Brittleness = value; }
    public void SetElasticity(int value) { Elasticity = value; }
    public void SetFluctuationOfSteepness(int value) { FluctuationOfSteepness = value; }
    public void SetGravity(int value) { Gravity = value; }
    public void SetHardness(int value) { Hardness = value; }
    public void SetLiquidToxicity(int value) { LiquidToxicity = value; }
    public void SetSharpness(int value) { Sharpness = value; }
    public void SetSolidityLiquidity(int value) { SolidityLiquidity = value; }
    public void SetSteepnessOfTopography(int value) { SteepnessOfTopography = value; }
    public void SetStructure(int value) { Structure = value; }
    public void SetVegitation(int value) { Vegitation = value; }

    public TerrainTraits()
    {

    }

    public TerrainTraits(string memory)
    {
        string[] ar = memory.Split(",");
        //General Traits
        Acidity = int.Parse(ar[0]);
        AcousticAbsorption = int.Parse(ar[1]);
        Compressability = int.Parse(ar[2]);
        Density = int.Parse(ar[3]);
        ElectricalCharge = int.Parse(ar[4]);
        FrequencyEmission = int.Parse(ar[5]);
        Flammability = int.Parse(ar[6]);
        Luminescence = int.Parse(ar[7]);
        Temperature = int.Parse(ar[8]);
        Magnetism = int.Parse(ar[9]);
        Pressure = int.Parse(ar[10]);
        Radioactivity = int.Parse(ar[11]);
        Reflectivity = int.Parse(ar[12]);
        Smell = int.Parse(ar[13]);
        Transparency = int.Parse(ar[14]);
        //Terrain Specific
        BoilingPoint = int.Parse(ar[15]);
        Brittleness = int.Parse(ar[16]);
        Elasticity = int.Parse(ar[17]);
        FluctuationOfSteepness = int.Parse(ar[18]);
        Gravity = int.Parse(ar[19]);
        Hardness = int.Parse(ar[20]);
        LiquidToxicity = int.Parse(ar[21]);
        Sharpness = int.Parse(ar[22]);
        SolidityLiquidity = int.Parse(ar[23]);
        SteepnessOfTopography = int.Parse(ar[24]);
        Structure = int.Parse(ar[25]);
        Vegitation = int.Parse(ar[26]);
    }

    public TerrainTraits(int acidity, int acoustic, int compres, int density, int electric, int emmission, int flammability, int luminescence
        , int temp, int magnetism, int pressure, int radio, int reflect, int smell, int trans, int boil, int brittle, int elastic
        , int freqSteep, int gravity, int hard, int liquiTox, int sharp, int solidity, int steep, int structure, int veg)
    {
        //General Traits
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
        //Terrain Specific
        BoilingPoint = boil;
        Brittleness = brittle;
        Elasticity = elastic;
        FluctuationOfSteepness = freqSteep;
        Gravity = gravity;
        Hardness = hard;
        LiquidToxicity = liquiTox;
        Sharpness = sharp;
        SolidityLiquidity = solidity;
        SteepnessOfTopography = steep;
        Structure = structure;
        Vegitation = veg;
    }

    public void SetGeneralTraits(Traits traits)
    {
        Acidity = traits.Acidity;
        AcousticAbsorption = traits.AcousticAbsorption;
        Compressability = traits.Compressability;
        Density = traits.Density;
        ElectricalCharge = traits.ElectricalCharge;
        FrequencyEmission = traits.FrequencyEmission;
        Flammability = traits.Flammability;
        Luminescence = traits.Luminescence;
        Temperature = traits.Temperature;
        Magnetism = traits.Magnetism;
        Pressure = traits.Pressure;
        Radioactivity = traits.Radioactivity;
        Reflectivity = traits.Reflectivity;
        Smell = traits.Smell;
        Transparency = traits.Transparency;
    }

    public void SetTerrainTraits(int boil, int brittle, int elastic, int freqSteep, int gravity, int hard, 
        int liquiTox, int sharp, int solidity, int steep, int structure, int veg)
    {
        BoilingPoint = boil;
        Brittleness = brittle;
        Elasticity = elastic;
        FluctuationOfSteepness = freqSteep;
        Gravity = gravity;
        Hardness = hard;
        LiquidToxicity = liquiTox;
        Sharpness = sharp;
        SolidityLiquidity = solidity;
        SteepnessOfTopography = steep;
        Structure = structure;
        Vegitation = veg;
    }

    public string DigitizeForSerialization()
    {
        return $"{Acidity},{AcousticAbsorption},{Compressability},{Density},{ElectricalCharge},{FrequencyEmission}," +
            $"{Flammability},{Luminescence},{Temperature},{Magnetism},{Pressure},{Radioactivity},{Reflectivity}," +
            $"{Smell},{Transparency},{BoilingPoint},{Brittleness},{Elasticity},{FluctuationOfSteepness},{Gravity},{Hardness}," +
            $"{LiquidToxicity},{Sharpness},{SolidityLiquidity},{SteepnessOfTopography},{Structure},{Vegitation}";
    }
}
