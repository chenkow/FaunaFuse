using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Core;
using Managers;

namespace UI
{
    /// <summary>
    /// Vampire Survivors-style Level Up UI.
    /// Shows 3 random perk choices when player levels up.
    /// </summary>
    public class LevelUpUI : MonoBehaviour
    {
        [Header("References")]
        public GameObject panel;
        public Transform cardContainer;
        public PerkCardUI cardPrefab;
        
        [Header("Perk Pool")]
        public List<PerkSO> allPerks = new List<PerkSO>();
        
        private List<PerkCardUI> spawnedCards = new List<PerkCardUI>();
        
        public static LevelUpUI Instance { get; private set; }
        
        private void Awake()
        {
            Instance = this;
        }
        
        private void Start()
        {
            if (panel) panel.SetActive(false);
            
            // Subscribe to PerkManager events
            SubscribeToPerkManager();
        }
        
        private void OnEnable()
        {
            SubscribeToPerkManager();
        }
        
        private void SubscribeToPerkManager()
        {
            if (PerkManager.Instance != null)
            {
                // Remove first to avoid double subscription
                PerkManager.Instance.OnLevelUp -= ShowLevelUp;
                PerkManager.Instance.OnLevelUp += ShowLevelUp;
                Debug.Log("LevelUpUI subscribed to PerkManager.OnLevelUp");
            }
            else
            {
                Debug.LogWarning("PerkManager.Instance is null!");
            }
        }
        
        private void OnDestroy()
        {
            if (PerkManager.Instance != null)
            {
                PerkManager.Instance.OnLevelUp -= ShowLevelUp;
            }
        }
        
        public void ShowLevelUp(int newLevel)
        {
            Debug.Log($"ShowLevelUp called! Level: {newLevel}, panel: {panel}, cardPrefab: {cardPrefab}, allPerks: {allPerks?.Count}");
            
            if (panel == null)
            {
                Debug.LogError("LevelUpUI.panel is NULL!");
                return;
            }
            if (cardPrefab == null)
            {
                Debug.LogWarning("LevelUpUI.cardPrefab is NULL - will skip card creation");
            }
            
            // Pause Game
            // Time.timeScale = 0f; // Disabled to fix freeze/invisibility issue
            panel.SetActive(true);
            Debug.Log("Panel activated!");
            
            // FIX: Force correct RectTransform positioning (fullscreen overlay)
            var panelRect = panel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                panelRect.anchorMin = Vector2.zero;
                panelRect.anchorMax = Vector2.one;
                panelRect.offsetMin = Vector2.zero;
                panelRect.offsetMax = Vector2.zero;
                panelRect.localScale = Vector3.one;
                
                // Bring to front
                panelRect.SetAsLastSibling();
                Debug.Log("LevelUpPanel positioned to fullscreen and brought to front");
            }
            
            // Ensure CanvasGroup is visible
            var canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
            }
            
            // FIX: Force visible background color (Keep dark background for panel)
            var panelImage = panel.GetComponent<UnityEngine.UI.Image>();
            if (panelImage != null)
            {
                panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
                panelImage.raycastTarget = true;
            }

            // Ensure panel is in front by adding Canvas component with high sort order
            var panelCanvas = panel.GetComponent<Canvas>();
            if (panelCanvas == null) panelCanvas = panel.AddComponent<Canvas>();
            panelCanvas.overrideSorting = true;
            panelCanvas.sortingOrder = 999;
            
            var raycaster = panel.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            if (raycaster == null) panel.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            Debug.Log($"Panel setup complete. Active: {panel.activeSelf}, Position: {panelRect?.anchoredPosition}");
            
            // Clear old cards
            foreach (var card in spawnedCards)
            {
                if (card != null) Destroy(card.gameObject);
            }
            spawnedCards.Clear();
            
            // FIX: Force CardContainer layout
            var containerRect = cardContainer.GetComponent<RectTransform>();
            if (containerRect != null)
            {
                containerRect.anchorMin = new Vector2(0.1f, 0.3f);
                containerRect.anchorMax = new Vector2(0.9f, 0.7f);
                containerRect.offsetMin = Vector2.zero;
                containerRect.offsetMax = Vector2.zero;
                containerRect.sizeDelta = Vector2.zero; // Reset size for anchors to take over
            }
            
            // Ensure HorizontalLayoutGroup
            var layout = cardContainer.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            if (layout == null) layout = cardContainer.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false; // Let us control width
            layout.childControlHeight = false;
            layout.spacing = 50;

            // Pick 3 random perks
            List<PerkSO> choices = GetRandomPerks(3);

            foreach (var perk in choices)
            {
                PerkCardUI card = Instantiate(cardPrefab, cardContainer);
                card.Setup(perk, OnPerkSelected);
                
                // FIX: Force scale and size (CRITICAL FOR VISIBILITY)
                var rt = card.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.localScale = Vector3.one;
                    rt.sizeDelta = new Vector2(250, 350); // Standard card size
                    
                    // Add layout element if missing
                    var le = card.GetComponent<UnityEngine.UI.LayoutElement>();
                    if (le == null) le = card.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
                    le.minWidth = 250;
                    le.minHeight = 350;
                    le.preferredWidth = 250;
                    le.preferredHeight = 350;
                }
                
                // Ensure root image is visible but standard color
                var img = card.GetComponent<UnityEngine.UI.Image>();
                if (img == null) img = card.gameObject.AddComponent<UnityEngine.UI.Image>();
                img.color = Color.white; // Reset to white so prefab design shows
                
                spawnedCards.Add(card);
            }
        }
        
        private List<PerkSO> GetRandomPerks(int count)
        {
            List<PerkSO> available = new List<PerkSO>(allPerks);
            List<PerkSO> result = new List<PerkSO>();
            
            // Shuffle and pick
            for (int i = 0; i < count && available.Count > 0; i++)
            {
                int index = Random.Range(0, available.Count);
                result.Add(available[index]);
                available.RemoveAt(index);
            }
            
            return result;
        }
        
        private void OnPerkSelected(PerkSO selectedPerk)
        {
            // Apply perk to PerkManager
            if (PerkManager.Instance != null)
            {
                PerkManager.Instance.SelectPerk(selectedPerk);
            }
            
            // Hide UI
            panel.SetActive(false);
            
            // Resume Game
            Time.timeScale = 1f;
            
            Debug.Log($"Selected Perk: {selectedPerk.perkName}");
        }
    }
}
