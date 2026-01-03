using UnityEngine;
using System;

namespace Systems
{
    public class DiamondSystem : MonoBehaviour
    {
        public static DiamondSystem Instance { get; private set; }
        
        public int TotalDiamonds { get; private set; }
        
        public event Action<int> OnDiamondChanged;

        private void Awake()
        {
            if (Instance == null) 
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);
        }

        private void Start()
        {
            if (SaveSystem.Instance != null && SaveSystem.Instance.Data != null)
            {
                TotalDiamonds = SaveSystem.Instance.Data.diamonds;
                OnDiamondChanged?.Invoke(TotalDiamonds);
            }
        }

        public void AddDiamonds(int amount)
        {
            TotalDiamonds += amount;
            SaveSystem.Instance?.Save();
            OnDiamondChanged?.Invoke(TotalDiamonds);
        }

        public bool SpendDiamonds(int amount)
        {
            if (TotalDiamonds >= amount)
            {
                TotalDiamonds -= amount;
                SaveSystem.Instance?.Save();
                OnDiamondChanged?.Invoke(TotalDiamonds);
                return true;
            }
            return false;
        }
    }
}
