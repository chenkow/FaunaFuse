using UnityEngine;
using UnityEngine.UI;
using Core;
using System.Collections.Generic;

namespace UI
{
    public class GalleryTileUI : MonoBehaviour
    {
        [Header("Layers")]
        public Image rarityBackground;
        public Image animalIcon;
        public Image glassOverlay;
        public Transform starContainer;

        [Header("Asset References")]
        public Sprite glassSprite;
        public Sprite starEmpty;
        public Sprite starFilled;

        // Star-based layer colors
        private static readonly Color Color1Star = new Color(0.6f, 0.6f, 0.6f, 1f);      // Gray (Common)
        private static readonly Color Color2Star = new Color(0.2f, 0.8f, 0.3f, 1f);      // Green (Uncommon)
        private static readonly Color Color3Star = new Color(0.2f, 0.5f, 1f, 1f);        // Blue (Rare)
        private static readonly Color Color4Star = new Color(0.7f, 0.3f, 0.9f, 1f);      // Purple (Epic)
        private static readonly Color Color5Star = new Color(1f, 0.6f, 0.1f, 1f);        // Orange (Legendary)

        private AnimalSO _currentAnimal;

        private void Awake()
        {
            // Auto-wire references if missing
            if (!rarityBackground) rarityBackground = transform.Find("RarityLayer")?.GetComponent<Image>();
            if (!animalIcon) animalIcon = transform.Find("AnimalLayer")?.GetComponent<Image>();
            if (!glassOverlay) glassOverlay = transform.Find("GlassLayer")?.GetComponent<Image>();
            if (!starContainer) starContainer = transform.Find("StarsLayer");
        }

        /// <summary>
        /// NEW: Setup tile with a specific card's star count
        /// </summary>
        public void SetupWithCard(AnimalSO animal, int starCount, Sprite rarityBgSprite)
        {
            _currentAnimal = animal;
            
            // Ensure references are valid
            if (!rarityBackground) Awake();

            // 1. Setup Rarity Background with star-based color tint
            if (rarityBackground)
            {
                rarityBackground.sprite = rarityBgSprite;
                rarityBackground.color = GetStarColor(starCount);
            }

            // 2. Setup Glass
            if (glassOverlay && glassSprite)
            {
                glassOverlay.sprite = glassSprite;
            }

            // 3. Setup Animal Icon
            if (animalIcon)
            {
                Sprite iconSprite = GetAnimalSprite(animal);
                if (iconSprite != null)
                {
                    animalIcon.sprite = iconSprite;
                    animalIcon.enabled = true;
                    animalIcon.color = Color.white;
                }
                else
                {
                    animalIcon.enabled = false;
                }
            }

            // 4. Setup Stars
            SetupStars(starCount);
        }

        /// <summary>
        /// LEGACY: Old setup method (kept for compatibility)
        /// </summary>
        public void Setup(AnimalSO animal, bool isUnlocked, Sprite rarityBgSprite)
        {
            // Get star count from CardRewardSystem (highest for this animal)
            int starCount = 0;
            if (Systems.CardRewardSystem.Instance != null)
            {
                starCount = Systems.CardRewardSystem.Instance.GetHighestStarCount(animal.level);
            }

            // If no card, hide tile
            if (starCount <= 0)
            {
                gameObject.SetActive(false);
                return;
            }

            SetupWithCard(animal, starCount, rarityBgSprite);
        }

        private Color GetStarColor(int stars)
        {
            switch (stars)
            {
                case 1: return Color1Star;
                case 2: return Color2Star;
                case 3: return Color3Star;
                case 4: return Color4Star;
                case 5: return Color5Star;
                default: return Color.white;
            }
        }

        private Sprite GetAnimalSprite(AnimalSO animal)
        {
            // Prefer gallerySprite
            if (animal.gallerySprite != null)
            {
                return animal.gallerySprite;
            }

            // Fallback: icon
            if (animal.icon != null)
            {
                return animal.icon;
            }

            // Fallback: Resources load
            string resourceName = $"{animal.level}_{animal.animalName.ToLower()}";
            return Resources.Load<Sprite>($"Gallery/Animals/{resourceName}");
        }

        private void SetupStars(int currentStars)
        {
            if (!starContainer) return;

            int maxStars = 5;
            for (int i = 0; i < starContainer.childCount && i < maxStars; i++)
            {
                Transform child = starContainer.GetChild(i);
                Image starImg = child.GetComponent<Image>();
                
                if (starImg)
                {
                    bool isFilled = i < currentStars;
                    starImg.sprite = isFilled ? starFilled : starEmpty;
                    starImg.gameObject.SetActive(true);
                }
            }
        }
    }
}
