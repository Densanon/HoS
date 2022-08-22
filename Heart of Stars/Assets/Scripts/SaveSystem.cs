using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem 
{
    static string FileData;
    static string Items;
    static string Tiles;

    public static void WipeString()
    {
        FileData = "";
        Items = "";
        Tiles = "";
    }
    
    public static void SaveItem(ItemData data, bool last)
    {
        if (!last)
        {
            Items += data.DigitizeForSerialization();
        }
        else
        {
            string s = data.DigitizeForSerialization();
            s = s.Remove(s.Length - 1);
            Items += s;
        }
    }
    public static void SaveItemLibrary(string location)
    {
        FileData = Items;
        SaveFile(location);
    }
    public static void SaveTile(HexTileInfo data, bool last)
    {
        if (!last)
        {
            Tiles += data.DigitizeForSerialization();
        }
        else
        {
            string s = data.DigitizeForSerialization();
            s = s.Remove(s.Length - 1);
            Tiles += s;

        }
    }
    public static void SaveShips(string ships)
    {
        FileData = ships;
        SaveFile("/ships_raah");
    }
    public static void SaveCameraSettings(string camStats)
    {
        FileData = camStats;
    }
    public static void SaveCurrentAddress(string address)
    {
        FileData = address;
        SaveFile("/address_nissi");
    }
    public static void SaveLocationData()
    {
        FileData = Tiles;
    }
    public static void SaveLocationList(string locations)
    {
        FileData = locations;
        SaveFile("/locations_jireh");
    }
    public static void SaveFile(string file)
    {
        if (string.IsNullOrEmpty(FileData)) return;

        BinaryFormatter formatter = new();
        string path = Application.persistentDataPath + file;
        FileStream stream = new(path, FileMode.Create);

        formatter.Serialize(stream, FileData);
        stream.Close();

        WipeString();
    }

    public static string LoadFile(string file)
    {
        WipeString();

        string path = Application.persistentDataPath + file;
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new();
            FileStream stream = new(path, FileMode.Open);
            string s = "";
            try
            {
                s = formatter.Deserialize(stream) as string;
                stream.Close();
                if(s == "")
                {
                    File.Delete(path);
                }
            }
            catch(Exception)
            {
                stream.Close();
                File.Delete(path);
            }
            return s;
        }
        else
        {
            return null;
        }
    }

    public static void DeleteAllLocationInformation()
    {
        string path;

        string s = LoadFile("/locations_jireh");
        if (s != null)
        {
            string[] ar = s.Split(";");
            foreach (string str in ar)
            {
                path = Application.persistentDataPath + "/" + str;
                if (File.Exists(path))
                {
                    File.Delete(path);
                    Debug.Log("I deleted a location.");
                }
            }
        }

        path = Application.persistentDataPath + "/locations_jireh";
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("I deleted the address book.");
        }
    }
    public static void SeriouslyDeleteAllSaveFiles()
    {
        string path = Application.persistentDataPath + "/item_shalom";
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("I deleted items.");
        }
        path = Application.persistentDataPath + "/basic_shalom";
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("I deleted basic items.");
        }

        SafeReset();
    }
    public static void SafeReset()
    {
        string path = Application.persistentDataPath + "/address_nissi";
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("I deleted the universe address.");
        }
        path = Application.persistentDataPath + "/ships_raah";
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("I deleted Ships.");
        }

        DeleteAllLocationInformation();
    }
}
