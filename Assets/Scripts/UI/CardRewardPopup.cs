using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core;
using System.Collections;

namespace UI
{
    public class CardRewardPopup : MonoBehaviour
    {
        public static CardRewardPopup Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private RectTransform cardContainer;
        [SerializeField] private Image cardBackground;
        [SerializeField] private Image animalIcon;
        [SerializeField] private Transform starsContainer;
        [SerializeField] private RectTransform galleryButtonTarget; // Target to slide towards

        [Header("Assets")]
        [SerializeField] private Sprite starFilled;
        [SerializeField] private Sprite starEmpty;

        [Header("Animation Settings")]
        [SerializeField] private float popDuration = 0.3f;
        [SerializeField] private float holdDuration = 0.8f;
        [SerializeField] private float slideDuration = 0.5f;
        [SerializeField] private AnimationCurve popCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        // Star-based colors (matching GalleryTileUI)
        private static readonly Color Color1Star = new Color(0.6f, 0.6f, 0.6f, 1f);      // Gray
        private static readonly Color Color2Star = new Color(0.2f, 0.8f, 0.3f, 1f);      // Green
        private static readonly Color Color3Star = new Color(0.2f, 0.5f, 1f, 1f);        // Blue
        private static readonly Color Color4Star = new Color(0.7f, 0.3f, 0.9f, 1f);      // Purple
        private static readonly Color Color5Star = new Color(1f, 0.6f, 0.1f, 1f);        // Orange

        private Vector2 centerPosition;
        private Coroutine animationCoroutine;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
            
            if (cardContainer)
            {
                centerPosition = cardContainer.anchoredPosition;
                cardContainer.localScale = Vector3.zero;
                cardContainer.gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            // Subscribe to CardRewardSystem events
            if (Systems.CardRewardSystem.Instance != null)
            {
                Systems.CardRewardSystem.Instance.OnCardRewarded += OnCardRewarded;
            }
        }

        private void OnDestroy()
        {
            if (Systems.CardRewardSystem.Instance != null)
            {
                Systems.CardRewardSystem.Instance.OnCardRewarded -= OnCardRewarded;
            }
        }

        private void OnCardRewarded(int animalLevel, int starCount)
        {
            AnimalSO animal = GetAnimalByLevel(animalLevel);
            if (animal != null)
            {
                Show(animal, starCount);
            }
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

        public void Show(AnimalSO animal, int starCount)
        {
            if (cardContainer == null) return;
            
            // Stop any running animation
            if (animationCoroutine != null) StopCoroutine(animationCoroutine);

            // Setup card visuals
            SetupCard(animal, starCount);

            // Start animation sequence
            animationCoroutine = StartCoroutine(AnimateCard());

            Debug.Log($"[CardRewardPopup] Showing: {animal.animalName} with {starCount} stars!");
        }

        private void SetupCard(AnimalSO animal, int starCount)
        {
            // Set background color based on stars
            if (cardBackground)
            {
                cardBackground.color = GetStarColor(starCount);
            }

            // Set animal icon
            if (animalIcon)
            {
                Sprite iconSprite = animal.gallerySprite ?? animal.icon;
                if (iconSprite != null)
                {
                    animalIcon.sprite = iconSprite;
                    animalIcon.enabled = true;
                }
            }

            // Setup stars
            if (starsContainer)
            {
                for (int i = 0; i < starsContainer.childCount && i < 5; i++)
                {
                    Image star = starsContainer.GetChild(i).GetComponent<Image>();
                    if (star)
                    {
                        star.sprite = (i < starCount) ? starFilled : starEmpty;
                        star.gameObject.SetActive(true);
                    }
                }
            }
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

        private IEnumerator AnimateCard()
        {
            // Reset position and show
            cardContainer.anchoredPosition = centerPosition;
            cardContainer.localScale = Vector3.zero;
            cardContainer.gameObject.SetActive(true);

            // === PHASE 1: Pop In ===
            float elapsed = 0f;
            while (elapsed < popDuration)
            {
                elapsed += Time.deltaTime;
                float t = popCurve.Evaluate(elapsed / popDuration);
                // Overshoot for "pop" effect
                float scale = t * 1.1f;
                if (t > 0.8f) scale = Mathf.Lerp(1.1f, 1f, (t - 0.8f) / 0.2f);
                cardContainer.localScale = Vector3.one * scale;
                yield return null;
            }
            cardContainer.localScale = Vector3.one;

            // === PHASE 2: Hold ===
            yield return new WaitForSeconds(holdDuration);

            // === PHASE 3: Slide to Gallery Button ===
            Vector2 startPos = cardContainer.anchoredPosition;
            Vector2 targetPos = GetGalleryButtonPosition();
            Vector3 startScale = Vector3.one;
            Vector3 endScale = Vector3.one * 0.3f;

            elapsed = 0f;
            while (elapsed < slideDuration)
            {
                elapsed += Time.deltaTime;
                float t = slideCurve.Evaluate(elapsed / slideDuration);
                
                cardContainer.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                cardContainer.localScale = Vector3.Lerp(startScale, endScale, t);
                
                // Fade out
                if (cardBackground)
                {
                    Color c = cardBackground.color;
                    c.a = Mathf.Lerp(1f, 0f, t);
                    cardBackground.color = c;
                }
                
                yield return null;
            }

            // Hide
            cardContainer.gameObject.SetActive(false);
            animationCoroutine = null;
        }

        private Vector2 GetGalleryButtonPosition()
        {
            if (galleryButtonTarget != null)
            {
                // Convert gallery button position to local space
                return galleryButtonTarget.anchoredPosition;
            }
            
            // Fallback: bottom center
            return new Vector2(0, -600);
        }
    }
}
