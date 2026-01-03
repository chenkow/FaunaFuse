using UnityEngine;

namespace Core
{
    public enum UpgradeType
    {
        MergeBonus,
        StartLevel,
        Undo,
        SpawnRate,
        ScoreMultiplier,
        StartingAnimals,
        ExtraDNA,
        HeartRefill, // Instant
        MaxHearts // Cap increase
    }

    [CreateAssetMenu(fileName = "NewUpgrade", menuName = "FaunaFuse/Upgrade Data")]
    public class UpgradeSO : ScriptableObject
    {
        public UpgradeType type;
        public string upgradeName;
        public string description;
        public Sprite icon;
        public int maxLevel;
        public int baseCost;
        public float costMultiplier;
        
        // Effects per level (could be formula or array)
        public float[] effectValues; 

        [Header("Time-Based Upgrade Settings")]
        [Tooltip("Custom upgrade times per level in seconds. If set, overrides formula.")]
        public float[] upgradeTimesPerLevel;
        [Tooltip("Base upgrade time in seconds for level 1 (used if upgradeTimesPerLevel is empty)")]
        public float baseUpgradeTime = 30f;
        [Tooltip("Multiplier for each subsequent level (used if upgradeTimesPerLevel is empty)")]
        public float upgradeTimeMultiplier = 1.5f;
        [Tooltip("Diamond cost per 30 seconds of remaining time")]
        public int instantFinishCostPer30Sec = 1;

        public int GetCost(int currentLevel)
        {
            return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, currentLevel));
        }
        
        public float GetEffect(int currentLevel)
        {
            if (currentLevel <= 0) return 0;
            if (effectValues == null || effectValues.Length == 0) return 0;
            
            int idx = Mathf.Clamp(currentLevel - 1, 0, effectValues.Length - 1);
            return effectValues[idx];
        }

        /// <summary>
        /// Get the upgrade duration in seconds for a specific level
        /// </summary>
        public float GetUpgradeTime(int targetLevel)
        {
            if (targetLevel <= 0) return 0;
            
            // Use custom array if available
            if (upgradeTimesPerLevel != null && upgradeTimesPerLevel.Length > 0)
            {
                int idx = Mathf.Clamp(targetLevel - 1, 0, upgradeTimesPerLevel.Length - 1);
                return upgradeTimesPerLevel[idx];
            }
            
            // Fallback to formula
            return baseUpgradeTime * Mathf.Pow(upgradeTimeMultiplier, targetLevel - 1);
        }

        /// <summary>
        /// Get the diamond cost to instantly finish based on remaining seconds.
        /// Uses smooth formula: diamondPerMinute = max(1.0 - (hours × 0.02), 0.5)
        /// Longer durations get better rates (volume discount).
        /// </summary>
        public int GetInstantFinishCost(float remainingSeconds)
        {
            if (remainingSeconds <= 0) return 0;
            
            float remainingMinutes = remainingSeconds / 60f;
            float remainingHours = remainingSeconds / 3600f;
            
            // Smooth formula: starts at 1.0 diamond/min, decreases by 0.02 per hour
            // Minimum: 0.5 diamond/min (reached at 25 hours)
            float diamondPerMinute = Mathf.Max(1.0f - (remainingHours * 0.02f), 0.5f);
            
            return Mathf.CeilToInt(remainingMinutes * diamondPerMinute);
        }
    }
}