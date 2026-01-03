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
        public CardUI cardPrefab; // Replaces 'slotTemplate'

        [Header("Detail View")]
        public GameObject detailPanel;
        public Image detailImage;
        public TextMeshProUGUI detailName;
        public TextMeshProUGUI detailLevel;
        public TextMeshProUGUI detailTrivia;
        public Button closeButton;

        private List<CardUI> spawnedCards = new List<CardUI>();

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
            // Ensure we have data
            if (BoardManager.Instance == null || BoardManager.Instance.allAnimals == null) return;
            
            // Clear old slots
            foreach (var card in spawnedCards)
            {
                if(card != null) Destroy(card.gameObject);
            }
            spawnedCards.Clear();

            if (cardPrefab == null) 
            {
                Debug.LogError("CardPrefab is assigned in CollectionView!");
                return;
            }

            var animals = BoardManager.Instance.allAnimals;
            var collectionSys = CollectionSystem.Instance;

            foreach (var animal in animals)
            {
                CardUI newCard = Instantiate(cardPrefab, contentContainer);
                newCard.gameObject.SetActive(true);
                newCard.name = $"Card_{animal.animalName}";
                
                bool isUnlocked = collectionSys != null && collectionSys.IsUnlocked(animal.level);
                
                newCard.Setup(animal, isUnlocked, ShowDetail);
                
                spawnedCards.Add(newCard);
            }
        }
    }
}
