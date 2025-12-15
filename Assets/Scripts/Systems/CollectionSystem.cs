using UnityEngine;
using System.Collections.Generic;
using System;

namespace Systems
{
    public class CollectionSystem : MonoBehaviour
    {
        public static CollectionSystem Instance { get; private set; }
        
        public List<int> UnlockedIds { get; private set; } = new List<int>();
        
        public event Action<int> OnAnimalUnlocked;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            // Load from SaveSystem
            if (SaveSystem.Instance != null && SaveSystem.Instance.Data != null)
            {
                UnlockedIds = new List<int>(SaveSystem.Instance.Data.unlockedAnimals);
            }
        }

        public void UnlockAnimal(int levelId)
        {
            if (!UnlockedIds.Contains(levelId))
            {
                UnlockedIds.Add(levelId);
                OnAnimalUnlocked?.Invoke(levelId);
                SaveSystem.Instance?.Save();
            }
        }

        public bool IsUnlocked(int levelId)
        {
            return UnlockedIds.Contains(levelId);
        }
    }
}