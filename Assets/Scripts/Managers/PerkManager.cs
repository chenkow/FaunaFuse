using UnityEngine;
using System.Collections.Generic;
using Core;

namespace Managers
{
    public class PerkManager : MonoBehaviour
    {
        public static PerkManager Instance;

        [Header("XP Settings")]
        public int currentLevel = 1;
        public float currentXP = 0;
        public float xpToNextLevel = 100;
        public float xpGrowthFactor = 1.2f;

        [Header("Perks State")]
        public List<PerkRuntimeData> activePerks = new List<PerkRuntimeData>();

        // Events
        public System.Action<int> OnLevelUp;
        public System.Action<float, float> OnXPChange;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void AddXP(float amount)
        {
            // Apply XP Multipliers from perks
            float multiplier = GetTotalStatBonus(PerkSO.PerkType.Passive_Economy, "Expedite"); 
            // Note: We need a better way to ID perks. Name string is fragile but simple for now.
            if(multiplier <= 0) multiplier = 1f; // Base
            
            currentXP += amount * multiplier;
            
            if (currentXP >= xpToNextLevel)
            {
                LevelUp();
            }
            
            OnXPChange?.Invoke(currentXP, xpToNextLevel);
        }

        private void LevelUp()
        {
            currentXP -= xpToNextLevel;
            currentLevel++;
            xpToNextLevel *= xpGrowthFactor;

            // Trigger UI
            Debug.Log($"Level Up! Now Level {currentLevel}");
            OnLevelUp?.Invoke(currentLevel);
            
            // Direct call as backup (for inactive objects that can't subscribe)
            if (UI.LevelUpUI.Instance != null)
            {
                Debug.Log("Calling LevelUpUI.ShowLevelUp directly");
                UI.LevelUpUI.Instance.ShowLevelUp(currentLevel);
            }
            else
            {
                // Try to find it in scene (including inactive)
                var levelUpUI = FindAnyObjectByType<UI.LevelUpUI>(FindObjectsInactive.Include);
                if (levelUpUI != null)
                {
                    Debug.Log("Found LevelUpUI via FindAnyObjectByType");
                    levelUpUI.ShowLevelUp(currentLevel);
                }
                else
                {
                    Debug.LogWarning("LevelUpUI not found!");
                }
            }
        }

        public void SelectPerk(PerkSO perk)
        {
            // Check if we already have it
            var existing = activePerks.Find(p => p.Definition == perk);
            if (existing != null)
            {
                existing.Level++;
            }
            else
            {
                activePerks.Add(new PerkRuntimeData(perk));
            }
        }
        
        public float GetTotalStatBonus(PerkSO.PerkType type, string partialName)
        {
            float total = 1.0f;
            foreach(var p in activePerks)
            {
                if(p.Definition.type == type && p.Definition.perkName.Contains(partialName))
                {
                    total += (p.Level * p.Definition.valuePerLevel) / 100f;
                }
            }
            return total;
        }

        public float GetFlatStatBonus(string partialName)
        {
            float total = 0f;
            foreach(var p in activePerks)
            {
                if(p.Definition.perkName.Contains(partialName))
                {
                    // e.g. Level 3 * 0.5 = 1.5
                    total += p.Level * p.Definition.valuePerLevel;
                }
            }
            return total;
        }

        [System.Serializable]
        public class PerkRuntimeData
        {
            public PerkSO Definition;
            public int Level;
            public float CooldownTimer;

            public PerkRuntimeData(PerkSO def)
            {
                Definition = def;
                Level = 1;
                CooldownTimer = 0;
            }
        }
    }
}
