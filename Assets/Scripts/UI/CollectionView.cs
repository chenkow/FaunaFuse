using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Core;
using Systems;

namespace UI
{
    public class CollectionView : MonoBehaviour
    {
        [Header("UI References")]
        public Transform contentContainer;
        public GalleryTileUI galleryTilePrefab;
        
        [Header("Shared Assets")]
        public Sprite glassSprite;
        public Sprite starEmpty;
        public Sprite starFilled;
        public List<Sprite> rarityBackgrounds; // Order: Common, Uncommon, Rare, Epic, Legendary

        [Header("Detail View")]
        public GameObject detailPanel;
        public Image detailImage;
        public TextMeshProUGUI detailName;
        public TextMeshProUGUI detailLevel;
        public TextMeshProUGUI detailTrivia;
        public Button closeButton;

        private List<GalleryTileUI> spawnedTiles = new List<GalleryTileUI>();

        private void OnEnable()
        {
            RefreshCollection();
        }

        private void Start()
        {
            if(closeButton) closeButton.onClick.AddListener(CloseDetail);
        }

        public void CloseDetail()
        {
            if(detailPanel) detailPanel.SetActive(false);
        }

        public void ShowDetail(AnimalSO animal)
        {
            if (animal == null) return;
            if (detailPanel) detailPanel.SetActive(true);
            
            if(detailImage) detailImage.sprite = animal.icon;
            if(detailName) detailName.text = animal.animalName;
            if(detailLevel) detailLevel.text = $"Level {animal.level}";
            
            string trivia = string.IsNullOrEmpty(animal.trivia) ? "Trivia coming soon..." : animal.trivia;
            if(detailTrivia) detailTrivia.text = trivia;
        }

        public void RefreshCollection()
        {
            // Clear old tiles
            foreach (var tile in spawnedTiles)
            {
                if(tile != null) Destroy(tile.gameObject);
            }
            spawnedTiles.Clear();

            // Check requirements
            if (BoardManager.Instance == null || BoardManager.Instance.allAnimals == null) return;
            if (CardRewardSystem.Instance == null) return;
            if (galleryTilePrefab == null) 
            {
                Debug.LogError("GalleryTilePrefab not assigned in CollectionView!");
                return;
            }

            // Get all earned cards from CardRewardSystem
            var earnedCards = CardRewardSystem.Instance.EarnedCards;
            
            if (earnedCards == null || earnedCards.Count == 0)
            {
                Debug.Log("[CollectionView] No cards earned yet.");
                return;
            }

            // Create a tile for EACH earned card
            foreach (var card in earnedCards)
            {
                AnimalSO animal = GetAnimalByLevel(card.animalLevel);
                if (animal == null) continue;

                GalleryTileUI newTile = Instantiate(galleryTilePrefab, contentContainer);
                newTile.gameObject.SetActive(true);
                newTile.name = $"Card_{animal.animalName}_{card.starCount}Star";
                spawnedTiles.Add(newTile);
                
                // Pass shared assets
                newTile.glassSprite = glassSprite;
                newTile.starEmpty = starEmpty;
                newTile.starFilled = starFilled;

                // Get rarity background (use star count - 1 as index: 1★=0, 5★=4)
                Sprite rarityBg = null;
                int rarityIndex = card.starCount - 1;
                if (rarityBackgrounds != null && rarityIndex >= 0 && rarityIndex < rarityBackgrounds.Count)
                {
                    rarityBg = rarityBackgrounds[rarityIndex];
                }

                // Setup tile with card's star count
                newTile.SetupWithCard(animal, card.starCount, rarityBg);
            }

            Debug.Log($"[CollectionView] Displayed {earnedCards.Count} cards.");
        }

        private AnimalSO GetAnimalByLevel(int level)
        {
            if (BoardManager.Instance?.allAnimals == null) return null;
            
            foreach (var animal in BoardManager.Instance.allAnimals)
            {
                if (animal.level == level) return animal;
            }
            return null;
        }
    }
}
