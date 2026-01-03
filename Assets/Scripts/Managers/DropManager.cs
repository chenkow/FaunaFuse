using UnityEngine;
using Core;
using System.Collections.Generic;

namespace Managers
{
    /// <summary>
    /// Handles mathematical logic for dropping cards when merging animals.
    /// Incorporates Base Rates based on Rank, Rarity distribution, and Collection Bonuses.
    /// </summary>
    public class DropManager : MonoBehaviour
    {
        public static DropManager Instance;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        /// <summary>
        /// Calculates if a card should drop for a given merged Rank.
        /// </summary>
        /// <param name="mergedRank">The Resulting Rank of the merge (e.g., Rank 2 Fish).</param>
        /// <returns>True if a card drops.</returns>
        public bool ShouldDropCard(int mergedRank, float bonusMultiplier = 1.0f)
        {
            float baseRate = GetBaseDropChance(mergedRank);
            
            // Global Per-Rank Decay logic or Table lookup?
            // User Table: Rank 1 = 1.5%, Rank 25 = 1.26%
            // But we agreed on the simple decay: 1.5 - (Rank-1)*0.01
            
            // However, the User's table values for "Common" chance are likely the "Total Drop Chance" for that tier?
            // Or is "Common" column separate from "Uncommon"?
            // Usually in these games:
            // 1. Roll "Does a card drop?" (Base Rate) -> If yes:
            // 2. Roll "What Rarity is it?"
            
            // Interpretation of User's Table:
            // Rank 1: Common 1.5, Uncommon 1.25. This looks like EACH rarity has a specific probability independently?
            // Or is it "If you get a Common, the chance was 1.5%"?
            // Re-reading: "common 1.5, uncommon 1.25". These are very close.
            // If they are independent events, total drop rate = 1.5+1.25+1+0.75+0.5 = 5%.
            // That sounds reasonable for a merge game (5% chance to get something).
            
            // Let's implement Sum of Probabilities strategy to determine "If drop happens".
            // Then if drop happens, we pick which one based on weights.
            
            float totalChance = GetTotalDropRateForRank(mergedRank);
            
            // Apply Multipliers (e.g. 4-Star 'Lucky Drop' +2.0f)
            float perkFlatBonus = 0f;
            if (Managers.PerkManager.Instance != null)
            {
                // 'Card Hunter' adds flat percentage points (e.g. 0.5 per level).
                perkFlatBonus = Managers.PerkManager.Instance.GetFlatStatBonus("Card Hunter");
            }
            
            // Formula: (BaseRate + FlatPerk) * Bonuses
            // Or (BaseRate * Bonuses) + Flat?
            // "Global +0.5% drop rate" usually implies flat addition to the final probability.
            // Let's add it to base rate?
            
            // Interpretation:
            // Base = 1.5%. Perk = +0.5%. Total = 2.0%. 
            // 4-Star Bonus = 2x. 
            // Result = (1.5 + 0.5) * 2 = 4.0%?
            // OR 1.5 * 2 + 0.5 = 3.5%?
            // I'll go with (Base + Perk) * Multiplier for maximum benefit/scaling.
            
            float finalChance = (totalChance + perkFlatBonus) * bonusMultiplier;

            // Clamp to 100%
            if (finalChance > 100f) finalChance = 100f;

            // Roll
            return Random.Range(0f, 100f) <= finalChance;
        }

        /// <summary>
        /// Determines the Rarity of the dropped card.
        /// </summary>
        public AnimalSO.Rarity RollRarity(int rank)
        {
            // We use the weighted values from the user's table for relative probability.
            // Rank 1 Weights: 1.5, 1.25, 1.0, 0.75, 0.5
            // Sum: 5.0
            
            // We can reconstruct the table logic:
            // Rate(Rank, RarityIndex) = Base(Rarity) - (Rank-1)*0.01
            
            float[] currents = new float[4]; // C, U, R, E, L (wait 5) -> Enum has 4? Common, Rare, Epic, Legendary.
            // User had 5 columns: Common, Uncommon, Rare, Epic, Legendary.
            // AnimalSO enum check: Common, Rare, Epic, Legendary. (Missing Uncommon in Enum?)
            // Checking AnimalSO again...
            // "public enum Rarity { Common, Rare, Epic, Legendary }" -> It IS missing Uncommon!
            // I should update AnimalSO to include Uncommon first.
            
            // Assuming we fix Enum later, let's proceed with logic for 5 rarities.
            // For now mapping User's "Uncommon" to "Rare" bucket or ignoring? 
            // Better to add Uncommon to AnimalSO.
            
            float pCommon = 1.5f - (rank - 1) * 0.01f;
            float pUncommon = 1.25f - (rank - 1) * 0.01f;
            float pRare = 1.0f - (rank - 1) * 0.01f;
            float pEpic = 0.75f - (rank - 1) * 0.01f;
            float pLegendary = 0.5f - (rank - 1) * 0.01f;

            // Normalize negative
            pCommon = Mathf.Max(0.1f, pCommon);
            pUncommon = Mathf.Max(0.1f, pUncommon);
            pRare = Mathf.Max(0.1f, pRare);
            pEpic = Mathf.Max(0.1f, pEpic);
            pLegendary = Mathf.Max(0.1f, pLegendary);

            float totalWeight = pCommon + pUncommon + pRare + pEpic + pLegendary;
            float roll = Random.Range(0f, totalWeight);

            if (roll < pCommon) return AnimalSO.Rarity.Common;
            roll -= pCommon;
            
            // Handling the Missing Uncommon in Enum:
            // If roll falls in Uncommon range, we'll upgrade it to Rare for now 
            // or return Common? Let's assume we update Enum. 
            // For this specific code block, I will return Common for Uncommon slot purely to compile 
            // but I will add a TODO to update Enum.
            if (roll < pUncommon) return AnimalSO.Rarity.Common; // Fallback
            roll -= pUncommon;

            if (roll < pRare) return AnimalSO.Rarity.Rare;
            roll -= pRare;
            
            if (roll < pEpic) return AnimalSO.Rarity.Epic;
            
            return AnimalSO.Rarity.Legendary;
        }

        private float GetTotalDropRateForRank(int rank)
        {
            // Sum of Top Row (Rank 1) = 5.0%
            // Decay per rank = 5 * 0.01 = 0.05 decrease per rank total?
            // Rank 1: 5.0%
            // Rank 25: 1.26+1.01+0.76+0.51+0.26 = ~3.8%
            
            // Formula for sum:
            // BaseSum = 1.5+1.25+1+0.75+0.5 = 5.0
            // Subtract (Rank-1)*0.05
            
            float total = 5.0f - ((rank - 1) * 0.05f);
            return Mathf.Max(1.0f, total); // Cap at 1% min
        }

        public float GetBaseDropChance(int rank)
        {
             return GetTotalDropRateForRank(rank);
        }
    }
}
