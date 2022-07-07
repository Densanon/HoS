using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem 
{
    static string FileData;
    static string Address;
    static string Resources;
    static string Tiles;

    public static void WipeString()
    {
        FileData = "";
        Address = "";
        Resources = "";
        Tiles = "";
    }
    
    public static void SaveResource(ResourceData data, bool last)
    {
        if (!last)
        {
            Resources = Resources + data.DigitizeForSerialization();
        }
        else
        {
            string s = data.DigitizeForSerialization();
            s = s.Remove(s.Length - 1);
            Resources = Resources + s;
        }
    }

    public static void SaveResourceLibrary()
    {
        FileData = Resources;
        SaveFile("/resource_shalom");
    }

    public static void SaveTile(HexTileInfo data, bool last)
    {
        if (!last)
        {
            Tiles = Tiles + data.DigitizeForSerialization();
        }
        else
        {
            string s = data.DigitizeForSerialization();
            s = s.Remove(s.Length - 1);
            Tiles = Tiles + s;

        }
    }

    public static void SaveAddressForLocation(string address)
    {
        Address = address;
    }

    public static void SaveCurrentAddress(string address)
    {
        FileData = address;
        SaveFile("/address_nissi");
    }

    public static void SaveLocationData()
    {
        FileData = Tiles + "|" + Resources;
    }

    public static void SaveLocationList(string locations)
    {
        FileData = locations;
        SaveFile("/Locations_Jireh");
    }

    public static void SaveFile(string file)
    {
        if (FileData == "" || FileData == null){return;}

        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + file;
        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, FileData);
        stream.Close();

        Debug.Log($"SaveString: for {file}:{FileData}");
        WipeString();

    }


    public static string LoadFile(string file)
    {
        WipeString();

        string path = Application.persistentDataPath + file;
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
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
            catch(Exception e)
            {
                stream.Close();
                File.Delete(path);
            }
            Debug.Log($"Here is the file: for {file}:{s}");
            

            return s;
        }
        else
        {
            Debug.Log("Save file not found in " + path);
            return null;
        }
    }

    public static void SeriouslyDeleteAllSaveFiles()
    {
        string path = Application.persistentDataPath + "/resource_shalom";
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("I found the file, and have deleted it.");
        }
        path = Application.persistentDataPath + "/address_nissi";
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("I found the file, and have deleted it.");
        }

        string s = LoadFile("/Locations_Jireh");
        if(s != null)
        {
            string[] ar = s.Split(",");
            foreach(string str in ar)
            {
                path = Application.persistentDataPath + "/" + str;
                if (File.Exists(path))
                {
                    File.Delete(path);
                    Debug.Log("I found the file, and have deleted it.");
                }
            }
        }

        path = Application.persistentDataPath + "/Locations_Jireh";
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("I found the file, and have deleted it.");
        }
    }
}
