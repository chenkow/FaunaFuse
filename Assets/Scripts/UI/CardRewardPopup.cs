using UnityEngine;
using UnityEngine.UI;
using Core;
using System.Collections;

namespace UI
{
    /// <summary>
    /// Kart kazanıldığında galeri tile tarzında (rarity bg + glass efekt + yıldızlar)
    /// ekranın üstünde pop efektiyle belirir, sonra Gallery butonuna kayarak kaybolur.
    /// Root-level OverlayCanvas (Sort Order 999) ile her şeyin önünde render edilir.
    /// </summary>
    public class CardRewardPopup : MonoBehaviour
    {
        public static CardRewardPopup Instance { get; private set; }

        [Header("Gallery Button")]
        [SerializeField] private RectTransform galleryButtonRect;

        [Header("Sprites")]
        [SerializeField] private Sprite starFilled;
        [SerializeField] private Sprite starEmpty;

        [Header("Animation")]
        [SerializeField] private float popDuration = 0.4f;
        [SerializeField] private float holdDuration = 1.5f;
        [SerializeField] private float slideDuration = 1.0f;

        // Rarity sprites loaded from Resources
        private Sprite glassSprite;
        private Sprite[] raritySprites; // 0=common, 1=uncommon, 2=rare, 3=epic, 4=legendary

        // Runtime UI
        private Canvas overlayCanvas;
        private RectTransform cardRoot;
        private Image rarityBg;
        private Image animalImg;
        private Image glassOverlay;
        private Image[] starImgs;
        private CanvasGroup canvasGroup;
        private Coroutine animCoroutine;
        private int currentStarCount;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            LoadSprites();
            CreateOverlayUI();
        }

        private void Start()
        {
            if (Systems.CardRewardSystem.Instance != null)
                Systems.CardRewardSystem.Instance.OnCardRewarded += OnCardRewarded;
        }

        private void OnDestroy()
        {
            if (Systems.CardRewardSystem.Instance != null)
                Systems.CardRewardSystem.Instance.OnCardRewarded -= OnCardRewarded;
        }

        private void LoadSprites()
        {
            // Load glass sprite
            glassSprite = Resources.Load<Sprite>("Gallery/glass");
            if (glassSprite == null)
            {
                // Try loading from Art folder as Texture2D and getting sprite
                var glassTex = Resources.Load<Texture2D>("Gallery/glass");
                if (glassTex != null)
                    glassSprite = Sprite.Create(glassTex, new Rect(0, 0, glassTex.width, glassTex.height), Vector2.one * 0.5f);
            }

            // Load rarity sprites (try Sprite first, fallback to Texture2D)
            raritySprites = new Sprite[5];
            string[] names = { "common", "uncommon", "rare", "epic", "legendary" };
            for (int i = 0; i < 5; i++)
            {
                raritySprites[i] = Resources.Load<Sprite>($"Gallery/{names[i]}");
                if (raritySprites[i] == null)
                {
                    var tex = Resources.Load<Texture2D>($"Gallery/{names[i]}");
                    if (tex != null)
                        raritySprites[i] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
                }
            }
        }

        private void CreateOverlayUI()
        {
            // === Root-level Overlay Canvas ===
            GameObject canvasGO = new GameObject("CardNotificationCanvas");
            DontDestroyOnLoad(canvasGO);
            overlayCanvas = canvasGO.AddComponent<Canvas>();
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvas.sortingOrder = 999;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            // === Card Root (same proportions as gallery tile) ===
            float cardW = 200f;
            float cardH = 220f;

            GameObject cardGO = new GameObject("CardNotification");
            cardGO.transform.SetParent(canvasGO.transform, false);
            cardRoot = cardGO.AddComponent<RectTransform>();
            cardRoot.sizeDelta = new Vector2(cardW, cardH);
            canvasGroup = cardGO.AddComponent<CanvasGroup>();

            // === 1. Rarity Background Layer ===
            GameObject bgGO = new GameObject("RarityLayer");
            bgGO.transform.SetParent(cardGO.transform, false);
            rarityBg = bgGO.AddComponent<Image>();
            RectTransform bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // === 2. Animal Icon Layer ===
            GameObject iconGO = new GameObject("AnimalLayer");
            iconGO.transform.SetParent(cardGO.transform, false);
            animalImg = iconGO.AddComponent<Image>();
            animalImg.preserveAspect = true;
            RectTransform iconRect = iconGO.GetComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(cardW * 0.75f, cardW * 0.75f); // 150x150
            iconRect.anchoredPosition = new Vector2(0, 10);

            // === 3. Glass Overlay Layer ===
            GameObject glassGO = new GameObject("GlassLayer");
            glassGO.transform.SetParent(cardGO.transform, false);
            glassOverlay = glassGO.AddComponent<Image>();
            RectTransform glassRect = glassGO.GetComponent<RectTransform>();
            glassRect.anchorMin = Vector2.zero;
            glassRect.anchorMax = Vector2.one;
            glassRect.sizeDelta = Vector2.zero;
            glassRect.offsetMin = Vector2.zero;
            glassRect.offsetMax = Vector2.zero;
            glassOverlay.color = new Color(1f, 1f, 1f, 0.6f); // semi-transparent

            // === 4. Stars Layer (matching GalleryTile prefab proportions) ===
            // GalleryTile: tile=250x250, starsContainer=100x100, anchor=bottom-center, y=-40, pivot=(1.1,0), spacing=-10
            // Scaled for 200x220 card (ratio ~0.8x)
            GameObject starsGO = new GameObject("StarsLayer");
            starsGO.transform.SetParent(cardGO.transform, false);
            RectTransform starsRect = starsGO.AddComponent<RectTransform>();
            starsRect.anchorMin = new Vector2(0.5f, 0f);  // bottom-center anchor
            starsRect.anchorMax = new Vector2(0.5f, 0f);
            starsRect.pivot = new Vector2(1.1f, 0f);      // same pivot as gallery tile
            starsRect.sizeDelta = new Vector2(80, 80);
            starsRect.anchoredPosition = new Vector2(0, -32); // scaled from -40
            HorizontalLayoutGroup hlg = starsGO.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = -8;  // negative spacing like gallery tile (-10 scaled)
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;   // gallery uses false
            hlg.childControlHeight = false;  // gallery uses false
            hlg.childForceExpandWidth = true;  // gallery uses true
            hlg.childForceExpandHeight = true; // gallery uses true

            starImgs = new Image[5];
            for (int i = 0; i < 5; i++)
            {
                GameObject starGO = new GameObject($"Star_{i + 1}");
                starGO.transform.SetParent(starsGO.transform, false);
                starImgs[i] = starGO.AddComponent<Image>();
                RectTransform starRect = starGO.GetComponent<RectTransform>();
                starRect.sizeDelta = new Vector2(42, 40); // scaled from 52x50
            }

            // Start hidden
            cardRoot.gameObject.SetActive(false);
        }

        private void OnCardRewarded(int animalLevel, int starCount)
        {
            AnimalSO animal = GetAnimalByLevel(animalLevel);
            if (animal != null)
                Show(animal, starCount);
        }

        public void Show(AnimalSO animal, int starCount)
        {
            if (cardRoot == null) return;
            if (animCoroutine != null) StopCoroutine(animCoroutine);

            SetupVisuals(animal, starCount);
            animCoroutine = StartCoroutine(PlayAnimation());
        }

        private void SetupVisuals(AnimalSO animal, int starCount)
        {
            int idx = Mathf.Clamp(starCount - 1, 0, 4);

            // 1. Rarity background sprite + color tint
            if (rarityBg)
            {
                if (raritySprites != null && raritySprites[idx] != null)
                    rarityBg.sprite = raritySprites[idx];
                rarityBg.color = GetStarColor(starCount);
            }

            // 2. Animal icon
            Sprite icon = animal.gallerySprite ?? animal.icon;
            if (icon == null)
            {
                // Fallback: Resources load
                string resourceName = $"{animal.level}_{animal.animalName.ToLower()}";
                icon = Resources.Load<Sprite>($"Gallery/Animals/{resourceName}");
            }
            if (icon != null)
            {
                animalImg.sprite = icon;
                animalImg.enabled = true;
                animalImg.color = Color.white;
            }

            // 3. Glass overlay
            if (glassOverlay && glassSprite != null)
            {
                glassOverlay.sprite = glassSprite;
                glassOverlay.color = new Color(1f, 1f, 1f, 0.6f);
            }

            // 4. Stars
            for (int i = 0; i < 5; i++)
            {
                if (starImgs[i] != null)
                {
                    starImgs[i].sprite = (i < starCount) ? starFilled : starEmpty;
                    starImgs[i].color = Color.white;
                }
            }

            currentStarCount = starCount;
        }

        private IEnumerator PlayAnimation()
        {
            // Position relative to top of canvas (below top nav bar)
            // Canvas pivot is center, so top edge = canvasHeight/2
            // Offset 200px from top ensures it sits just below the nav bar on all devices
            float canvasH = ((RectTransform)overlayCanvas.transform).rect.height;
            float topOffset = 375f; // distance from top of canvas to card center
            Vector2 startPos = new Vector2(0, canvasH / 2f - topOffset);
            Vector2 endPos = GetGalleryScreenPos();

            cardRoot.anchoredPosition = startPos;
            cardRoot.localScale = Vector3.zero;
            canvasGroup.alpha = 1f;
            cardRoot.gameObject.SetActive(true);

            // === POP IN ===
            float elapsed = 0f;
            while (elapsed < popDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / popDuration);
                float scale;
                if (t < 0.7f)
                    scale = Mathf.Lerp(0f, 1.15f, t / 0.7f);
                else
                    scale = Mathf.Lerp(1.15f, 1f, (t - 0.7f) / 0.3f);
                cardRoot.localScale = Vector3.one * scale;
                yield return null;
            }
            cardRoot.localScale = Vector3.one;

            // === CONFETTI BURSTS (based on star count) ===
            StartCoroutine(SpawnMultiBurstConfetti(startPos, currentStarCount));

            // === HOLD ===
            yield return new WaitForSecondsRealtime(holdDuration);

            // === SLIDE TO GALLERY (bezier arc path) ===
            Vector2 slideStart = cardRoot.anchoredPosition;
            // Control point: offset to the right to create an arc curve
            Vector2 controlPoint = new Vector2(
                slideStart.x - 250f,  // arc to the left
                (slideStart.y + endPos.y) * 0.5f  // vertically centered
            );
            elapsed = 0f;
            while (elapsed < slideDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / slideDuration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);

                // Quadratic Bezier: B(t) = (1-t)²·P0 + 2(1-t)t·P1 + t²·P2
                float oneMinusT = 1f - eased;
                Vector2 pos = oneMinusT * oneMinusT * slideStart
                            + 2f * oneMinusT * eased * controlPoint
                            + eased * eased * endPos;

                cardRoot.anchoredPosition = pos;
                cardRoot.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.25f, eased);
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, Mathf.Clamp01((t - 0.4f) / 0.6f));
                yield return null;
            }

            cardRoot.gameObject.SetActive(false);
            animCoroutine = null;
        }

        private Vector2 GetGalleryScreenPos()
        {
            if (galleryButtonRect != null)
            {
                // DexBtn is inside a Screen Space - Camera canvas
                // We need to find the render camera for correct world-to-screen conversion
                Canvas parentCanvas = galleryButtonRect.GetComponentInParent<Canvas>();
                Camera renderCam = null;
                if (parentCanvas != null)
                    renderCam = parentCanvas.worldCamera;
                if (renderCam == null)
                    renderCam = Camera.main;

                Vector3 worldPos = galleryButtonRect.position;
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(renderCam, worldPos);

                RectTransform overlayRect = overlayCanvas.GetComponent<RectTransform>();
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    overlayRect, screenPoint, null, out Vector2 localPoint);
                return localPoint;
            }
            return new Vector2(-180, -900);
        }

        private Color GetStarColor(int stars)
        {
            switch (stars)
            {
                case 1: return new Color(0.6f, 0.6f, 0.6f, 1f);   // Gray
                case 2: return new Color(0.2f, 0.8f, 0.3f, 1f);   // Green
                case 3: return new Color(0.2f, 0.5f, 1f, 1f);     // Blue
                case 4: return new Color(0.7f, 0.3f, 0.9f, 1f);   // Purple
                case 5: return new Color(1f, 0.6f, 0.1f, 1f);     // Orange
                default: return Color.white;
            }
        }

        private AnimalSO GetAnimalByLevel(int level)
        {
            if (BoardManager.Instance?.allAnimals == null) return null;
            foreach (var animal in BoardManager.Instance.allAnimals)
                if (animal.level == level) return animal;
            return null;
        }
        // ==================== CONFETTI EFFECT ====================
        private readonly Color[] confettiColors = {
            new Color(1f, 0.3f, 0.3f),   // Red
            new Color(1f, 0.8f, 0.2f),   // Gold
            new Color(0.3f, 0.9f, 0.4f), // Green
            new Color(0.3f, 0.6f, 1f),   // Blue
            new Color(0.9f, 0.4f, 1f),   // Purple
            new Color(1f, 0.5f, 0.8f),   // Pink
            Color.white
        };

        /// <summary>
        /// Returns burst offset positions based on star count.
        /// 1★: center
        /// 2★: upper-left, upper-right
        /// 3★: upper-left, top, upper-right  
        /// 4★: upper-left, upper-right, left, right
        /// 5★: upper-left, upper-right, top, left, right
        /// </summary>
        private Vector2[] GetBurstOffsets(int stars)
        {
            float spread = 120f;
            switch (stars)
            {
                case 1: return new[] { Vector2.zero };
                case 2: return new[] {
                    new Vector2(-spread, spread * 0.6f),   // upper-left
                    new Vector2(spread, spread * 0.6f)     // upper-right
                };
                case 3: return new[] {
                    new Vector2(-spread, spread * 0.5f),   // upper-left
                    new Vector2(0, spread),                // top
                    new Vector2(spread, spread * 0.5f)     // upper-right
                };
                case 4: return new[] {
                    new Vector2(-spread * 0.7f, spread),   // upper-left
                    new Vector2(spread * 0.7f, spread),    // upper-right
                    new Vector2(-spread, -spread * 0.2f),  // left
                    new Vector2(spread, -spread * 0.2f)    // right
                };
                default: return new[] { // 5★
                    new Vector2(-spread * 0.8f, spread),   // upper-left
                    new Vector2(spread * 0.8f, spread),    // upper-right
                    new Vector2(0, spread * 1.2f),         // top
                    new Vector2(-spread * 1.1f, 0),        // left
                    new Vector2(spread * 1.1f, 0)          // right
                };
            }
        }

        private IEnumerator SpawnMultiBurstConfetti(Vector2 center, int stars)
        {
            Vector2[] offsets = GetBurstOffsets(stars);
            for (int b = 0; b < offsets.Length; b++)
            {
                SpawnSingleBurst(center + offsets[b]);
                if (b < offsets.Length - 1)
                    yield return new WaitForSecondsRealtime(0.12f);
            }
        }

        private void SpawnSingleBurst(Vector2 origin)
        {
            int count = 30;
            RectTransform canvasRect = overlayCanvas.GetComponent<RectTransform>();

            for (int i = 0; i < count; i++)
            {
                GameObject p = new GameObject($"Confetti_{i}");
                p.transform.SetParent(canvasRect, false);
                
                Image img = p.AddComponent<Image>();
                img.color = confettiColors[Random.Range(0, confettiColors.Length)];
                img.raycastTarget = false;

                RectTransform rt = p.GetComponent<RectTransform>();
                float size = Random.Range(10f, 22f);
                rt.sizeDelta = new Vector2(size, size * Random.Range(0.5f, 1.5f));
                rt.anchoredPosition = origin + new Vector2(Random.Range(-20f, 20f), Random.Range(-15f, 15f));
                rt.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float speed = Random.Range(200f, 800f);
                Vector2 velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;

                StartCoroutine(AnimateConfetti(rt, img, velocity));
            }
        }

        private IEnumerator AnimateConfetti(RectTransform rt, Image img, Vector2 velocity)
        {
            float lifetime = Random.Range(0.6f, 1.2f);
            float elapsed = 0f;
            float gravity = -800f;
            float rotSpeed = Random.Range(-360f, 360f);
            Color startColor = img.color;

            while (elapsed < lifetime)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / lifetime;

                // Apply gravity to velocity
                velocity.y += gravity * Time.unscaledDeltaTime;
                rt.anchoredPosition += velocity * Time.unscaledDeltaTime;

                // Spin
                rt.Rotate(0, 0, rotSpeed * Time.unscaledDeltaTime);

                // Fade out in last 40%
                float alpha = t > 0.6f ? Mathf.Lerp(1f, 0f, (t - 0.6f) / 0.4f) : 1f;
                img.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

                // Shrink slightly
                float scale = Mathf.Lerp(1f, 0.3f, t);
                rt.localScale = Vector3.one * scale;

                yield return null;
            }

            Destroy(rt.gameObject);
        }
    }
}
