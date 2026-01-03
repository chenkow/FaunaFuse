using UnityEngine;
using System;

namespace Systems
{
    public class DNASystem : MonoBehaviour
    {
        public static DNASystem Instance { get; private set; }
        
        public int TotalDNA { get; private set; }
        
        public event Action<int> OnDNAChanged;

        private void Awake()
        {
            if (Instance == null) 
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);
            
            // For now, simple load. Will hook into SaveSystem later.
            // TotalDNA = PlayerPrefs.GetInt("TotalDNA", 0); 
            // Better: Let Start handle load from SaveSystem
        }

        private void Start()
        {
            if (SaveSystem.Instance != null && SaveSystem.Instance.Data != null)
            {
                TotalDNA = SaveSystem.Instance.Data.dna;
                OnDNAChanged?.Invoke(TotalDNA);
            }
        }

        public void AddDNA(int amount)
        {
            TotalDNA += amount;
            // PlayerPrefs.SetInt("TotalDNA", TotalDNA);
            SaveSystem.Instance?.Save();
            OnDNAChanged?.Invoke(TotalDNA);
        }

        public bool SpendDNA(int amount)
        {
            if (TotalDNA >= amount)
            {
                TotalDNA -= amount;
                // PlayerPrefs.SetInt("TotalDNA", TotalDNA);
                SaveSystem.Instance?.Save();
                OnDNAChanged?.Invoke(TotalDNA);
                return true;
            }
            return false;
        }
    }
}