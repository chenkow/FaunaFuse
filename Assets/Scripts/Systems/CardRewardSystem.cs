using UnityEngine;
using System;
using System.Collections.Generic;

namespace Systems
{
    public class CardRewardSystem : MonoBehaviour
    {
        public static CardRewardSystem Instance { get; private set; }

        // Star chances: 5% for 1★, 4% for 2★, 3% for 3★, 2% for 4★, 1% for 5★
        private readonly float[] starChances = { 0.05f, 0.04f, 0.03f, 0.02f, 0.01f };
        
        // List of ALL earned cards (each card is a separate entry)
        private List<SavedCard> earnedCards = new List<SavedCard>();
        
        public event Action<int, int> OnCardRewarded; // (animalLevel, starCount)

        // Public accessor for CollectionView
        public List<SavedCard> EarnedCards => earnedCards;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            LoadFromSaveSystem();
        }

        private void LoadFromSaveSystem()
        {
            if (SaveSystem.Instance?.Data?.animalCards != null)
            {
                earnedCards = new List<SavedCard>(SaveSystem.Instance.Data.animalCards);
                Debug.Log($"[CardReward] Loaded {earnedCards.Count} cards from save.");
            }
        }

        /// <summary>
        /// Called after each merge. Rolls for card reward chance.
        /// </summary>
        public void TryRewardCard(int animalLevel)
        {
            float roll = UnityEngine.Random.value; // 0.0 to 1.0
            float cumulative = 0f;
            
            // Roll from highest rarity (5★) to lowest (1★)
            for (int stars = 5; stars >= 1; stars--)
            {
                cumulative += starChances[stars - 1];
                if (roll <= cumulative)
                {
                    // Won a card!
                    AwardCard(animalLevel, stars);
                    return;
                }
            }
            
            // 85% chance - no card
            Debug.Log($"[CardReward] No card this time (roll: {roll:F3})");
        }

        private void AwardCard(int animalLevel, int stars)
        {
            // Add as a NEW card entry (each card is unique)
            SavedCard newCard = new SavedCard
            {
                animalLevel = animalLevel,
                starCount = stars
            };
            
            earnedCards.Add(newCard);
            SaveToSystem();
            
            Debug.Log($"[CardReward] ★ NEW CARD! Animal Level {animalLevel} with {stars} stars! (Total cards: {earnedCards.Count})");
            OnCardRewarded?.Invoke(animalLevel, stars);
        }

        /// <summary>
        /// Gets the highest star count for a specific animal (for display purposes)
        /// </summary>
        public int GetHighestStarCount(int animalLevel)
        {
            int highest = 0;
            foreach (var card in earnedCards)
            {
                if (card.animalLevel == animalLevel && card.starCount > highest)
                {
                    highest = card.starCount;
                }
            }
            return highest;
        }

        /// <summary>
        /// Checks if any card exists for this animal
        /// </summary>
        public bool HasCard(int animalLevel)
        {
            foreach (var card in earnedCards)
            {
                if (card.animalLevel == animalLevel) return true;
            }
            return false;
        }

        /// <summary>
        /// Gets all cards for a specific animal
        /// </summary>
        public List<SavedCard> GetCardsForAnimal(int animalLevel)
        {
            List<SavedCard> cards = new List<SavedCard>();
            foreach (var card in earnedCards)
            {
                if (card.animalLevel == animalLevel)
                {
                    cards.Add(card);
                }
            }
            return cards;
        }

        private void SaveToSystem()
        {
            if (SaveSystem.Instance?.Data == null) return;
            
            SaveSystem.Instance.Data.animalCards = new List<SavedCard>(earnedCards);
            SaveSystem.Instance.Save();
        }
    }
}
