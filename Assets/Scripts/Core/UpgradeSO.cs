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

        public int GetCost(int currentLevel)
        {
            return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, currentLevel));
        }
        
        public float GetEffect(int currentLevel)
        {
            if (currentLevel <= 0) return 0;
            int idx = Mathf.Clamp(currentLevel - 1, 0, effectValues.Length - 1);
            return effectValues[idx];
        }
    }
}