using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "NewPerk", menuName = "FaunaFuse/Perk Data")]
    public class PerkSO : ScriptableObject
    {
        public string perkName;
        [TextArea] public string description;
        public Sprite icon;
        
        public PerkType type;
        public int maxLevel = -1; // -1 for Infinite
        public float valuePerLevel = 1.0f; // e.g., 10 for +10%

        [Header("Behavior")]
        public bool isOneTimeUse; // For "Shuffle", "Wipe"
        public float cooldown = 0f; // For "Vacuum"
        
        public enum PerkType
        {
            Passive_Economy, // DNA, XP
            Passive_Board,   // Magnet, Vacuum, Evolution
            Active_Skill,    // Shuffle, Wipe
            Instant          // Heal?
        }
    }
}
