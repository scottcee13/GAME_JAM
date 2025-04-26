using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class TGData
{
    public static void SaveLevelData(float highScore, string path, string fileName)
    {
        if (!Directory.Exists(Application.persistentDataPath + path))
            Directory.CreateDirectory(Application.persistentDataPath + path);
        BinaryFormatter bf = new();
        FileStream file = File.Create(Application.persistentDataPath + $"{path}/{fileName}");
        LevelData data = new() { highScore = highScore };
        bf.Serialize(file, data);
        file.Close();
        Debug.Log("Game data saved!");
    }

    public static LevelData GetLevelData(string path, string fileName)
    {
        if (!File.Exists(Application.persistentDataPath + $"{path}/{fileName}"))
        {
            Debug.Log("No save data found.");
            return new LevelData();
        }

        BinaryFormatter bf = new();
        FileStream file = File.Open(Application.persistentDataPath + $"{path}/{fileName}", FileMode.Open);
        LevelData data = (LevelData)bf.Deserialize(file);
        Debug.Log("Game data loaded!");
        file.Close();
        return data;
    }

    public static void DeleteAllLevelData()
    {
        string path = Application.persistentDataPath;
        DirectoryInfo directory = new DirectoryInfo(path);
        directory.Delete(true);
        Directory.CreateDirectory(path);
    }

    public static string ConvertToTimeFormat(float timer)
    {
        return Mathf.FloorToInt(timer / 60).ToString() + ":" + Mathf.FloorToInt(timer % 60).ToString("D2") + "." + Mathf.FloorToInt((timer * 1000) % 1000).ToString("000");
    }
}

[Serializable]
public class LevelData
{
    public float highScore = float.MaxValue;
}
