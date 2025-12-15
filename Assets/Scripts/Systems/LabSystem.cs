using UnityEngine;
using System.Collections.Generic;
using Core;

namespace Systems
{
    public class LabSystem : MonoBehaviour
    {
        public static LabSystem Instance { get; private set; }

        public List<UpgradeSO> upgradesAvailable;
        public List<int> UpgradeLevels { get; private set; } = new List<int>(); // Parallel to upgradesAvailable

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            // Initialize levels
            if (upgradesAvailable == null) upgradesAvailable = new List<UpgradeSO>();

            // Load from Save
            if (SaveSystem.Instance != null && SaveSystem.Instance.Data != null)
            {
                UpgradeLevels = new List<int>(SaveSystem.Instance.Data.upgradeLevels);
            }

            // Pad list if new upgrades added
            while (UpgradeLevels.Count < upgradesAvailable.Count)
            {
                UpgradeLevels.Add(0);
            }
        }

        public bool TryPurchaseUpgrade(int index)
        {
            if (index < 0 || index >= upgradesAvailable.Count) return false;
            
            UpgradeSO upgrade = upgradesAvailable[index];
            int currentLvl = UpgradeLevels[index];
            
            if (currentLvl >= upgrade.maxLevel) return false;

            int cost = upgrade.GetCost(currentLvl);
            if (DNASystem.Instance.SpendDNA(cost))
            {
                UpgradeLevels[index]++;
                ApplyUpgradeEffect(upgrade.type, UpgradeLevels[index]); // Immediate effect if any
                SaveSystem.Instance?.Save();
                return true;
            }
            return false;
        }

        private void ApplyUpgradeEffect(UpgradeType type, int level)
        {
            // Logic for applying permanent effects (e.g. max hearts increase)
            if (type == UpgradeType.MaxHearts)
            {
                // HeartSystem.Instance.IncreaseMaxHearts(level);
            }
            // Other effects are pulled by systems (e.g. BoardManager checks MergeBonus level)
        }
        
        public int GetUpgradeLevel(UpgradeType type)
        {
            for(int i=0; i<upgradesAvailable.Count; i++)
            {
                if (upgradesAvailable[i].type == type) return UpgradeLevels[i];
            }
            return 0;
        }
    }
}