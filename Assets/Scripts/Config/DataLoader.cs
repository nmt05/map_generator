using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    public int Level;
    public int NumberOfEnemies;
}

[Serializable]
public class SaveData
{
    public int savedLevel = 1;
}

[Serializable]
public class GameDatasetWrapper
{
    public SaveData SaveData = new SaveData();
    public List<GameData> LevelsDataset = new List<GameData>();
}

public class GameModel
{
    private string filePath;

    public GameModel()
    {
        filePath = Path.Combine(Application.dataPath, "data.json");
    }

    public void SaveData(GameDatasetWrapper data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);
        Debug.Log($"Data saved to: {filePath}");
    }

    public GameDatasetWrapper LoadData()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            GameDatasetWrapper data = JsonUtility.FromJson<GameDatasetWrapper>(json);

            if (data.SaveData == null)
            {
                data.SaveData = new SaveData();
            }

            return data;
        }

        Debug.LogWarning("Save file not found! Returning default data.");
        GameDatasetWrapper defaultData = new GameDatasetWrapper();
        defaultData.SaveData.savedLevel = 1;
        defaultData.LevelsDataset.Add(new GameData { Level = 1, NumberOfEnemies = 1 });
        SaveData(defaultData);
        return defaultData;
    }

    public int LoadSavedLevel()
    {
        GameDatasetWrapper data = LoadData();
        return data.SaveData.savedLevel;
    }

    public void SaveCurrentLevel(int level)
    {
        GameDatasetWrapper data = LoadData();
        data.SaveData.savedLevel = level;
        SaveData(data);
        Debug.Log("Saved progress at Level: " + level);
    }
}
