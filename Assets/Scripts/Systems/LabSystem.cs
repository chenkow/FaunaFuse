using UnityEngine;
using System.Collections.Generic;
using Core;
using System.Linq;
using System;

namespace Systems
{
    /// <summary>
    /// Tracks an in-progress upgrade
    /// </summary>
    public struct ActiveUpgrade
    {
        public UpgradeType type;
        public DateTime startTime;
        public int targetLevel;
        public float durationSeconds;
    }

    public class LabSystem : MonoBehaviour
    {
        public static LabSystem Instance { get; private set; }

        public List<UpgradeSO> upgradesAvailable;
        public List<int> UpgradeLevels { get; private set; } = new List<int>();
        
        // Active upgrade timers
        private Dictionary<UpgradeType, ActiveUpgrade> activeUpgrades = new Dictionary<UpgradeType, ActiveUpgrade>();

        // Events
        public event Action<UpgradeType> OnUpgradePurchased; // Legacy - fires when upgrade COMPLETES
        public event Action<UpgradeType> OnUpgradeStarted;
        public event Action<UpgradeType> OnUpgradeCompleted;
        public event Action OnUpgradeTimerTick; // For UI updates

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            if (upgradesAvailable == null) upgradesAvailable = new List<UpgradeSO>();

            // Auto-load from Resources if empty
            if (upgradesAvailable.Count == 0)
            {
                var loaded = Resources.LoadAll<UpgradeSO>("Upgrades");
                if (loaded != null && loaded.Length > 0)
                {
                    upgradesAvailable.AddRange(loaded);
                    upgradesAvailable = upgradesAvailable.OrderBy(u => u.name).ToList();
                }
            }

            // Load upgrade levels from save
            LoadUpgradeLevels();
            
            // Load active timers from save
            LoadActiveUpgrades();
        }

        private void Update()
        {
            if (activeUpgrades.Count == 0) return;

            // Check for completed upgrades
            var completedKeys = new List<UpgradeType>();
            
            foreach (var kvp in activeUpgrades)
            {
                float remaining = GetRemainingTime(kvp.Key);
                if (remaining <= 0)
                {
                    Debug.Log($"LabSystem: Timer completed for {kvp.Key}!");
                    completedKeys.Add(kvp.Key);
                }
            }

            foreach (var type in completedKeys)
            {
                Debug.Log($"LabSystem: Calling CompleteUpgrade for {type}");
                CompleteUpgrade(type);
            }

            // Fire tick event for UI updates
            OnUpgradeTimerTick?.Invoke();
        }

        private void LoadUpgradeLevels()
        {
            if (SaveSystem.Instance != null && SaveSystem.Instance.Data != null)
            {
                var data = SaveSystem.Instance.Data;
                
                if (data.savedUpgrades != null && data.savedUpgrades.Count > 0)
                {
                    for(int i = 0; i < upgradesAvailable.Count; i++)
                    {
                        var up = upgradesAvailable[i];
                        bool exists = data.savedUpgrades.Any(s => s.type == up.type);
                        if (exists)
                        {
                            var saved = data.savedUpgrades.First(s => s.type == up.type);
                            UpgradeLevels.Add(saved.level);
                            Debug.Log($"LabSystem: Loaded {up.type} at level {saved.level}");
                        }
                        else
                        {
                            UpgradeLevels.Add(0);
                        }
                    }
                }
                else
                {
                    for(int i = 0; i < upgradesAvailable.Count; i++) UpgradeLevels.Add(0);
                }
            }
            else
            {
                for(int i = 0; i < upgradesAvailable.Count; i++) UpgradeLevels.Add(0);
            }
        }

        private void LoadActiveUpgrades()
        {
            if (SaveSystem.Instance?.Data?.activeUpgradeTimers == null) return;

            foreach (var timer in SaveSystem.Instance.Data.activeUpgradeTimers)
            {
                var upgrade = upgradesAvailable.FirstOrDefault(u => u.type == timer.type);
                if (upgrade == null) continue;

                DateTime startTime = DateTime.FromBinary(long.Parse(timer.startTime));
                float duration = upgrade.GetUpgradeTime(timer.targetLevel);

                var active = new ActiveUpgrade
                {
                    type = timer.type,
                    startTime = startTime,
                    targetLevel = timer.targetLevel,
                    durationSeconds = duration
                };

                // Check if already completed
                float elapsed = (float)(DateTime.Now - startTime).TotalSeconds;
                if (elapsed >= duration)
                {
                    // Complete immediately
                    CompleteUpgradeInternal(timer.type, timer.targetLevel);
                }
                else
                {
                    activeUpgrades[timer.type] = active;
                }
            }
        }

        private void SaveActiveUpgrades()
        {
            if (SaveSystem.Instance?.Data == null) return;

            SaveSystem.Instance.Data.activeUpgradeTimers.Clear();
            foreach (var kvp in activeUpgrades)
            {
                SaveSystem.Instance.Data.activeUpgradeTimers.Add(new ActiveUpgradeTimer
                {
                    type = kvp.Key,
                    startTime = kvp.Value.startTime.ToBinary().ToString(),
                    targetLevel = kvp.Value.targetLevel
                });
            }
            SaveSystem.Instance.Save();
        }

        // ===================== PUBLIC API =====================

        /// <summary>
        /// Start an upgrade timer (costs DNA, starts countdown)
        /// </summary>
        public bool StartUpgrade(int index)
        {
            if (index < 0 || index >= upgradesAvailable.Count) return false;

            UpgradeSO upgrade = upgradesAvailable[index];
            int currentLvl = UpgradeLevels[index];

            // Check if already maxed
            if (currentLvl >= upgrade.maxLevel)
            {
                Debug.LogWarning($"StartUpgrade: {upgrade.name} already maxed");
                return false;
            }

            // Check if already in progress
            if (IsUpgradeInProgress(upgrade.type))
            {
                Debug.LogWarning($"StartUpgrade: {upgrade.name} already in progress");
                return false;
            }

            // Check cost
            int cost = upgrade.GetCost(currentLvl);
            if (!DNASystem.Instance.SpendDNA(cost))
            {
                Debug.LogWarning($"StartUpgrade: Not enough DNA for {upgrade.name}");
                return false;
            }

            // Start timer
            int targetLevel = currentLvl + 1;
            float duration = upgrade.GetUpgradeTime(targetLevel);

            var active = new ActiveUpgrade
            {
                type = upgrade.type,
                startTime = DateTime.Now,
                targetLevel = targetLevel,
                durationSeconds = duration
            };

            activeUpgrades[upgrade.type] = active;
            SaveActiveUpgrades();

            Debug.Log($"StartUpgrade: Started {upgrade.name} -> Level {targetLevel} (Duration: {duration}s)");
            OnUpgradeStarted?.Invoke(upgrade.type);

            return true;
        }

        /// <summary>
        /// Complete an upgrade (internal, called when timer finishes)
        /// </summary>
        private void CompleteUpgrade(UpgradeType type)
        {
            if (!activeUpgrades.ContainsKey(type)) return;

            var active = activeUpgrades[type];
            CompleteUpgradeInternal(type, active.targetLevel);
            
            activeUpgrades.Remove(type);
            SaveActiveUpgrades();
        }

        private void CompleteUpgradeInternal(UpgradeType type, int newLevel)
        {
            // Find index and update level
            for (int i = 0; i < upgradesAvailable.Count; i++)
            {
                if (upgradesAvailable[i].type == type)
                {
                    UpgradeLevels[i] = newLevel;
                    break;
                }
            }

            // Save upgrade levels
            SaveUpgradeLevels();

            Debug.Log($"CompleteUpgrade: {type} now at level {newLevel}");
            OnUpgradeCompleted?.Invoke(type);
            OnUpgradePurchased?.Invoke(type); // For backward compatibility
        }

        /// <summary>
        /// Instant finish using diamonds
        /// </summary>
        public bool TryInstantFinish(UpgradeType type)
        {
            if (!activeUpgrades.ContainsKey(type)) return false;

            var upgrade = upgradesAvailable.FirstOrDefault(u => u.type == type);
            if (upgrade == null) return false;

            float remaining = GetRemainingTime(type);
            int cost = upgrade.GetInstantFinishCost(remaining);

            if (cost <= 0)
            {
                // Already complete
                CompleteUpgrade(type);
                return true;
            }

            if (DiamondSystem.Instance == null)
            {
                Debug.LogError("TryInstantFinish: DiamondSystem.Instance is null!");
                return false;
            }

            if (!DiamondSystem.Instance.SpendDiamonds(cost))
            {
                Debug.LogWarning($"TryInstantFinish: Not enough diamonds ({cost} required)");
                return false;
            }

            CompleteUpgrade(type);
            return true;
        }

        private void SaveUpgradeLevels()
        {
            if (SaveSystem.Instance?.Data == null) return;

            SaveSystem.Instance.Data.savedUpgrades.Clear();
            for (int i = 0; i < upgradesAvailable.Count; i++)
            {
                if (UpgradeLevels[i] > 0)
                {
                    SaveSystem.Instance.Data.savedUpgrades.Add(new SavedUpgrade
                    {
                        type = upgradesAvailable[i].type,
                        level = UpgradeLevels[i]
                    });
                }
            }
            SaveSystem.Instance.Save();
        }

        // ===================== HELPERS =====================

        public bool IsUpgradeInProgress(UpgradeType type)
        {
            return activeUpgrades.ContainsKey(type);
        }

        public float GetRemainingTime(UpgradeType type)
        {
            if (!activeUpgrades.ContainsKey(type)) return 0;

            var active = activeUpgrades[type];
            float elapsed = (float)(DateTime.Now - active.startTime).TotalSeconds;
            return Mathf.Max(0, active.durationSeconds - elapsed);
        }

        public float GetProgress(UpgradeType type)
        {
            if (!activeUpgrades.ContainsKey(type)) return 0;

            var active = activeUpgrades[type];
            float elapsed = (float)(DateTime.Now - active.startTime).TotalSeconds;
            return Mathf.Clamp01(elapsed / active.durationSeconds);
        }

        public int GetUpgradeLevel(UpgradeType type)
        {
            for (int i = 0; i < upgradesAvailable.Count; i++)
            {
                if (upgradesAvailable[i].type == type)
                {
                    if (i < UpgradeLevels.Count) return UpgradeLevels[i];
                    return 0;
                }
            }
            return 0;
        }

        public UpgradeSO GetUpgradeData(UpgradeType type)
        {
            return upgradesAvailable.FirstOrDefault(u => u.type == type);
        }

        public float GetTotalEffectValue(UpgradeType type)
        {
            for (int i = 0; i < upgradesAvailable.Count; i++)
            {
                if (upgradesAvailable[i].type == type)
                {
                    if (i < UpgradeLevels.Count)
                    {
                        int lvl = UpgradeLevels[i];
                        return upgradesAvailable[i].GetEffect(lvl);
                    }
                    return 0;
                }
            }
            return 0f;
        }

        // Legacy compatibility
        public bool TryPurchaseUpgrade(int index)
        {
            return StartUpgrade(index);
        }
    }
}