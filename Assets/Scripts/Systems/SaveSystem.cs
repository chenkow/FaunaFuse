using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace Systems
{
    [System.Serializable]
    public struct SavedUpgrade
    {
        public Core.UpgradeType type;
        public int level;
    }

    [System.Serializable]
    public struct ActiveUpgradeTimer
    {
        public Core.UpgradeType type;
        public string startTime; // DateTime.ToBinary().ToString()
        public int targetLevel;
    }

    [System.Serializable]
    public class GameSaveData
    {
        public int dna;
        public int diamonds;
        public int hearts;
        public string nextRefillTime;
        public List<int> unlockedAnimals = new List<int>();
        public List<int> upgradeLevels = new List<int>(); // Deprecated (Legacy)
        public List<SavedUpgrade> savedUpgrades = new List<SavedUpgrade>(); // Robust
        public List<ActiveUpgradeTimer> activeUpgradeTimers = new List<ActiveUpgradeTimer>();
    }

    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }
        private string path;
        public GameSaveData Data { get; private set; }

        private void Awake()
        {
             if (Instance == null) 
             {
                 Instance = this;
                 DontDestroyOnLoad(gameObject);
             }
             else Destroy(gameObject);

             path = Path.Combine(Application.persistentDataPath, "gamedata.json");
             Load();
        }

        public void Save()
        {
             if (Data == null) Data = new GameSaveData();
             
             // Sync from systems
             if (DNASystem.Instance) Data.dna = DNASystem.Instance.TotalDNA;
             if (DiamondSystem.Instance) Data.diamonds = DiamondSystem.Instance.TotalDiamonds;
             if (HeartSystem.Instance) 
             {
                 Data.hearts = HeartSystem.Instance.CurrentHearts;
                 Data.nextRefillTime = HeartSystem.Instance.NextRefillTime.ToBinary().ToString();
             }
             if (CollectionSystem.Instance) Data.unlockedAnimals = CollectionSystem.Instance.UnlockedIds;
             if (LabSystem.Instance) Data.upgradeLevels = LabSystem.Instance.UpgradeLevels;

             string json = JsonUtility.ToJson(Data, true);
             File.WriteAllText(path, json);
        }

        public void Load()
        {
             if (File.Exists(path))
             {
                 string json = File.ReadAllText(path);
                 Data = JsonUtility.FromJson<GameSaveData>(json);
             }
             else
             {
                 Data = new GameSaveData();
                 Data.hearts = 5;
                 Data.dna = 0;
                 Data.diamonds = 10; // Starting diamonds for testing
             }
             
             // Sync back is tricky if systems awake AFTER SaveSystem.
             // Systems should poll SaveSystem.Instance.Data in their Start()
        }

        private void OnApplicationFocus(bool focus)
        {
            if (!focus) Save();
        }

        private void OnApplicationQuit()
        {
            Save();
        }
        public void ClearSave()
        {
             if (File.Exists(path))
             {
                 File.Delete(path);
             }
             Data = new GameSaveData();
        }
    }
}