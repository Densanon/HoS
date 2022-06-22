using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem 
{
    static string FileData;

    public static void WipeString()
    {
        FileData = "";
    }
    
    public static void SaveResource(ResourceData data)
    {
        FileData = FileData + data.DigitizeForSerialization();
    }

    public static void SaveFile()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + $"/resource_shalom";
        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, FileData);
        stream.Close();

        WipeString();
    }


    public static string LoadFile()
    {
        WipeString();

        string path = Application.persistentDataPath + $"/resource_shalom";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            FileData = formatter.Deserialize(stream) as string;
            stream.Close();

            return FileData;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            return null;
        }
    }
}
