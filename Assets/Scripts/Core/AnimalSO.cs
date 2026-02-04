using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "NewAnimal", menuName = "FaunaFuse/Animal Data")]
    public class AnimalSO : ScriptableObject
    {
        public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }

        public int level;
        public string animalName;
        public Sprite icon; // Gameplay Sprite (with background)
        public Sprite gallerySprite; // Gallery Sprite (no background)
        public Rarity rarity;
        public int dnaReward;
        [TextArea] public string trivia;

        [Header("Drop Settings")]
        [Tooltip("Global multiplier for this specific animal's drop rate.")]
        public float dropRateModifier = 1.0f;

        [Header("Collection Bonuses")]
        public StarBonusData bonuses;

        [System.Serializable]
        public struct StarBonusData
        {
            [Tooltip("2 Star: 1.2 = +20%")]
            public float dnaMultiplier; 
            [Tooltip("3 Star: 1.5 = +50%")]
            public float expMultiplier;
            [Tooltip("4 Star: 2.0 = 2x")]
            public float dropChanceMultiplier;
            [Tooltip("5 Star: 0.15 = 15%")]
            public float spawnRankBonusChance;
            [Tooltip("5 Star: 0.05 = 5%")]
            public float mergeSkipChance;

            // Defaults for the 'Standard' set
            public static StarBonusData Default => new StarBonusData
            {
                dnaMultiplier = 1.2f,
                expMultiplier = 1.5f,
                dropChanceMultiplier = 2.0f,
                spawnRankBonusChance = 0.15f,
                mergeSkipChance = 0.05f
            };
        }
    }
}