using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem 
{
    
    public static void SaveResource(ResourceData data)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + $"/resource_{data.itemName}";
        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static ResourceData LoadResource(string itemName)
    {
        string path = Application.persistentDataPath + $"/resource_{itemName}";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            ResourceData data = formatter.Deserialize(stream) as ResourceData;
            stream.Close();

            return data;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            return null;
        }
    }
}
