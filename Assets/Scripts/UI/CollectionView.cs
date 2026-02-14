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

        [Header("Merge Buttons")]
        private Button mergeButton;
        private Button batchMergeButton;
        private GameObject mergeButtonBar;

        private List<GalleryTileUI> spawnedTiles = new List<GalleryTileUI>();
        private bool buttonsCreated = false;

        private void OnEnable()
        {
            if (!buttonsCreated) CreateMergeButtons();
            RefreshCollection();
        }

        private void Start()
        {
            if(closeButton) closeButton.onClick.AddListener(CloseDetail);
            if (!buttonsCreated) CreateMergeButtons();
        }

        private void CreateMergeButtons()
        {
            if (buttonsCreated) return;
            buttonsCreated = true;

            RectTransform myRect = GetComponent<RectTransform>();
            if (myRect == null) return;

            // --- Create bottom button bar ---
            mergeButtonBar = new GameObject("MergeButtonBar");
            mergeButtonBar.transform.SetParent(transform, false);
            RectTransform barRect = mergeButtonBar.AddComponent<RectTransform>();
            
            // Anchor to bottom, stretch horizontally
            barRect.anchorMin = new Vector2(0, 0);
            barRect.anchorMax = new Vector2(1, 0);
            barRect.pivot = new Vector2(0.5f, 0);
            barRect.sizeDelta = new Vector2(0, 100);
            barRect.anchoredPosition = Vector2.zero;

            // Background for the bar
            Image barBg = mergeButtonBar.AddComponent<Image>();
            barBg.color = new Color(0.08f, 0.08f, 0.14f, 0.95f);

            // Horizontal layout
            HorizontalLayoutGroup hLayout = mergeButtonBar.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 20;
            hLayout.padding = new RectOffset(30, 30, 10, 10);
            hLayout.childAlignment = TextAnchor.MiddleCenter;
            hLayout.childControlWidth = true;
            hLayout.childControlHeight = true;
            hLayout.childForceExpandWidth = true;
            hLayout.childForceExpandHeight = true;

            // --- MERGE Button ---
            mergeButton = CreateStyledButton(mergeButtonBar.transform, "MergeBtn", "MERGE",
                new Color(0.18f, 0.65f, 0.45f)); // Teal green

            // --- BATCH MERGE Button ---
            batchMergeButton = CreateStyledButton(mergeButtonBar.transform, "BatchMergeBtn", "BATCH MERGE",
                new Color(0.55f, 0.28f, 0.85f)); // Purple

            // Placeholder click handlers (TODO: implement merge logic)
            mergeButton.onClick.AddListener(() => Debug.Log("[CollectionView] Merge button clicked (placeholder)"));
            batchMergeButton.onClick.AddListener(() => Debug.Log("[CollectionView] Batch Merge button clicked (placeholder)"));

            // Make sure button bar renders on top
            mergeButtonBar.transform.SetAsLastSibling();
        }

        private Button CreateStyledButton(Transform parent, string name, string label, Color bgColor)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            // Button image (rounded look)
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = bgColor;
            btnImg.type = Image.Type.Sliced;

            // Button component
            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
            btn.colors = colors;

            // Text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = label;
            tmpText.fontSize = 28;
            tmpText.fontStyle = FontStyles.Bold;
            tmpText.color = Color.white;
            tmpText.alignment = TextAlignmentOptions.Center;

            // Stretch text to fill button
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return btn;
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

            // Dynamically resize grid to fit current screen width
            AdjustGridToFitScreen();

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

        private void AdjustGridToFitScreen()
        {
            if (contentContainer == null) return;
            
            GridLayoutGroup grid = contentContainer.GetComponent<GridLayoutGroup>();
            if (grid == null) return;
            
            // Get available width from the Viewport (parent of Content)
            RectTransform parentRect = contentContainer.parent as RectTransform;
            if (parentRect == null) parentRect = contentContainer as RectTransform;
            
            // Force layout rebuild to get correct rect
            Canvas.ForceUpdateCanvases();
            float availableWidth = parentRect.rect.width;
            
            // Fallback: if still 0, try the CollectionView's own rect
            if (availableWidth <= 0)
            {
                RectTransform myRect = GetComponent<RectTransform>();
                if (myRect != null) availableWidth = myRect.rect.width;
            }
            
            if (availableWidth <= 0) return;
            
            int columns = 4;
            float padding = 20f;
            float spacing = 20f;
            
            float totalPadding = padding * 2f;
            float totalSpacing = spacing * (columns - 1);
            float cellWidth = (availableWidth - totalPadding - totalSpacing) / columns;
            cellWidth = Mathf.Max(cellWidth, 80f);
            
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columns;
            grid.cellSize = new Vector2(cellWidth, cellWidth); // square cells
            grid.spacing = new Vector2(spacing, spacing);
            grid.padding = new RectOffset(
                Mathf.RoundToInt(padding), Mathf.RoundToInt(padding),
                grid.padding.top, grid.padding.bottom
            );
            
            Debug.Log($"[CollectionView] Grid adjusted: width={availableWidth}, cellSize={cellWidth}");
        }
    }
}
