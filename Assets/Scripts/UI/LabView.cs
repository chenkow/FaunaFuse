using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Systems;
using Core;

namespace UI
{
    public class LabView : MonoBehaviour
    {
        [Header("References")]
        public RectTransform contentContainer;
        public TextMeshProUGUI dnaText;
        public TextMeshProUGUI diamondText;
        public Button testDnaButton;
        public Button testDiamondButton;

        private List<UpgradeCardData> activeCards = new List<UpgradeCardData>();

        private class UpgradeCardData
        {
            public GameObject cardObject;
            public UpgradeSO data;
            public int index;
            public TextMeshProUGUI timeText;
            public Image progressFill;
            public Button upgradeButton;
            public Button instantFinishButton;
            public TextMeshProUGUI costText;
            public TextMeshProUGUI nameText;
            public TextMeshProUGUI descText;
        }

        private void OnEnable()
        {
            RefreshUI();
            if (DNASystem.Instance != null) DNASystem.Instance.OnDNAChanged += OnCurrencyChanged;
            if (DiamondSystem.Instance != null) DiamondSystem.Instance.OnDiamondChanged += OnCurrencyChanged;
            if (LabSystem.Instance != null)
            {
                LabSystem.Instance.OnUpgradeStarted += OnUpgradeEvent;
                LabSystem.Instance.OnUpgradeCompleted += OnUpgradeEvent;
                LabSystem.Instance.OnUpgradeTimerTick += UpdateTimers;
            }
        }

        private void OnDisable()
        {
            if (DNASystem.Instance != null) DNASystem.Instance.OnDNAChanged -= OnCurrencyChanged;
            if (DiamondSystem.Instance != null) DiamondSystem.Instance.OnDiamondChanged -= OnCurrencyChanged;
            if (LabSystem.Instance != null)
            {
                LabSystem.Instance.OnUpgradeStarted -= OnUpgradeEvent;
                LabSystem.Instance.OnUpgradeCompleted -= OnUpgradeEvent;
                LabSystem.Instance.OnUpgradeTimerTick -= UpdateTimers;
            }
        }

        private void OnCurrencyChanged(int _)
        {
            // Refresh entire UI to update button states based on new balance
            RefreshUI();
        }
        private void OnUpgradeEvent(UpgradeType _) => RefreshUI();

        private void Start()
        {
            if (testDnaButton) testDnaButton.onClick.AddListener(() => {
                if (DNASystem.Instance) DNASystem.Instance.AddDNA(100000);
            });
            if (testDiamondButton) testDiamondButton.onClick.AddListener(() => {
                if (DiamondSystem.Instance) DiamondSystem.Instance.AddDiamonds(100);
            });
        }

        private void Update()
        {
            if (LabSystem.Instance == null) return;
            
            // Check for any upgrades that just completed
            foreach (var cardData in activeCards)
            {
                bool wasInProgress = cardData.instantFinishButton.gameObject.activeSelf;
                bool isNowInProgress = LabSystem.Instance.IsUpgradeInProgress(cardData.data.type);
                
                // If it was in progress but no longer is, it just completed
                if (wasInProgress && !isNowInProgress)
                {
                    Debug.Log($"LabView.Update: Detected completion for {cardData.data.type}");
                    RefreshUI();
                    return; // RefreshUI will rebuild everything
                }
                
                // Update timer display if still in progress
                if (isNowInProgress)
                {
                    float remaining = LabSystem.Instance.GetRemainingTime(cardData.data.type);
                    float progress = LabSystem.Instance.GetProgress(cardData.data.type);
                    
                    int totalMins = Mathf.FloorToInt(remaining / 60);
                    int secs = Mathf.FloorToInt(remaining % 60);
                    
                    if (totalMins >= 60)
                    {
                        int hours = totalMins / 60;
                        int mins = totalMins % 60;
                        cardData.timeText.text = $"{hours:D2}:{mins:D2}:{secs:D2}";
                    }
                    else
                    {
                        cardData.timeText.text = $"{totalMins:D2}:{secs:D2}";
                    }
                    cardData.progressFill.rectTransform.anchorMax = new Vector2(progress, 1);
                }
            }
        }

        private void UpdateCurrencyDisplay()
        {
            if (DNASystem.Instance && dnaText) dnaText.text = $"DNA: {DNASystem.Instance.TotalDNA}";
            if (DiamondSystem.Instance && diamondText) diamondText.text = $"Diamond: {DiamondSystem.Instance.TotalDiamonds}";
        }

        public void RefreshUI()
        {
            if (LabSystem.Instance == null) return;

            foreach (var card in activeCards)
            {
                if (card.cardObject) Destroy(card.cardObject);
            }
            activeCards.Clear();

            UpdateCurrencyDisplay();

            var upgrades = LabSystem.Instance.upgradesAvailable;
            for (int i = 0; i < upgrades.Count; i++)
            {
                CreateCard(upgrades[i], i);
            }
        }

        private void CreateCard(UpgradeSO data, int index)
        {
            int level = LabSystem.Instance.GetUpgradeLevel(data.type);
            bool isInProgress = LabSystem.Instance.IsUpgradeInProgress(data.type);
            bool isMaxed = level >= data.maxLevel;

            // === CARD CONTAINER ===
            GameObject card = new GameObject($"Card_{data.name}");
            card.transform.SetParent(contentContainer, false);
            
            RectTransform cardRect = card.AddComponent<RectTransform>();
            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.12f, 0.12f, 0.18f);
            cardBg.raycastTarget = false;

            LayoutElement cardLayout = card.AddComponent<LayoutElement>();
            cardLayout.minHeight = 140;
            cardLayout.preferredHeight = 140;
            cardLayout.flexibleWidth = 1;

            // === LEFT: ICON (Fixed 120px) ===
            GameObject iconContainer = new GameObject("IconContainer");
            iconContainer.transform.SetParent(card.transform, false);
            RectTransform iconRect = iconContainer.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0, 0);
            iconRect.anchorMax = new Vector2(0, 1);
            iconRect.pivot = new Vector2(0, 0.5f);
            iconRect.sizeDelta = new Vector2(120, 0);
            iconRect.anchoredPosition = new Vector2(10, 0);

            Image iconBg = iconContainer.AddComponent<Image>();
            iconBg.color = new Color(0.2f, 0.2f, 0.28f);
            iconBg.raycastTarget = false;

            // Icon Image
            GameObject iconImg = new GameObject("Icon");
            iconImg.transform.SetParent(iconContainer.transform, false);
            RectTransform iconImgRect = iconImg.AddComponent<RectTransform>();
            iconImgRect.anchorMin = Vector2.zero;
            iconImgRect.anchorMax = Vector2.one;
            iconImgRect.offsetMin = new Vector2(8, 8);
            iconImgRect.offsetMax = new Vector2(-8, -8);
            Image img = iconImg.AddComponent<Image>();
            img.sprite = data.icon;
            img.preserveAspect = true;
            img.raycastTarget = false;

            // === CENTER: CONTENT AREA ===
            GameObject contentArea = new GameObject("ContentArea");
            contentArea.transform.SetParent(card.transform, false);
            RectTransform contentRect = contentArea.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.offsetMin = new Vector2(140, 10); // Left margin after icon
            contentRect.offsetMax = new Vector2(-80, -10); // Right margin for buttons

            // Name Header (Top)
            GameObject nameHeader = new GameObject("NameHeader");
            nameHeader.transform.SetParent(contentArea.transform, false);
            RectTransform nameRect = nameHeader.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.7f);
            nameRect.anchorMax = new Vector2(1, 1);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            
            Image nameBg = nameHeader.AddComponent<Image>();
            nameBg.color = new Color(0.85f, 0.7f, 0.1f); // Gold
            nameBg.raycastTarget = false;

            TextMeshProUGUI nameText = CreateText(nameHeader.transform, data.upgradeName, 18, FontStyles.Bold, Color.black);
            nameText.alignment = TextAlignmentOptions.Center;

            // Description (Middle)
            GameObject descPanel = new GameObject("DescPanel");
            descPanel.transform.SetParent(contentArea.transform, false);
            RectTransform descRect = descPanel.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0, 0.3f);
            descRect.anchorMax = new Vector2(1, 0.65f);
            descRect.offsetMin = Vector2.zero;
            descRect.offsetMax = Vector2.zero;

            Image descBg = descPanel.AddComponent<Image>();
            descBg.color = new Color(0.18f, 0.18f, 0.25f);
            descBg.raycastTarget = false;

            TextMeshProUGUI descText = CreateText(descPanel.transform, data.description, 12, FontStyles.Normal, Color.white);
            descText.alignment = TextAlignmentOptions.Center;

            // Progress Bar (Bottom)
            GameObject progressBar = new GameObject("ProgressBar");
            progressBar.transform.SetParent(contentArea.transform, false);
            RectTransform progRect = progressBar.AddComponent<RectTransform>();
            progRect.anchorMin = new Vector2(0, 0);
            progRect.anchorMax = new Vector2(1, 0.25f);
            progRect.offsetMin = Vector2.zero;
            progRect.offsetMax = Vector2.zero;

            Image progBg = progressBar.AddComponent<Image>();
            progBg.color = new Color(0.1f, 0.1f, 0.15f);
            progBg.raycastTarget = false;

            // Progress Fill
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(progressBar.transform, false);
            RectTransform fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0.5f, 1); // Will be updated
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.3f, 0.75f, 0.3f);
            fillImg.raycastTarget = false;

            // Time/Level Text
            TextMeshProUGUI timeText = CreateText(progressBar.transform, $"{level}/{data.maxLevel}", 14, FontStyles.Bold, Color.white);
            timeText.alignment = TextAlignmentOptions.Center;

            // === RIGHT: BUTTONS ===
            GameObject buttonPanel = new GameObject("ButtonPanel");
            buttonPanel.transform.SetParent(card.transform, false);
            RectTransform btnPanelRect = buttonPanel.AddComponent<RectTransform>();
            btnPanelRect.anchorMin = new Vector2(1, 0);
            btnPanelRect.anchorMax = new Vector2(1, 1);
            btnPanelRect.pivot = new Vector2(1, 0.5f);
            btnPanelRect.sizeDelta = new Vector2(70, 0);
            btnPanelRect.anchoredPosition = new Vector2(-5, 0);

            // Instant Finish Button (Top)
            GameObject instantBtn = CreateButton(buttonPanel.transform, "⚡", new Color(0.2f, 0.6f, 0.9f));
            RectTransform instantRect = instantBtn.GetComponent<RectTransform>();
            instantRect.anchorMin = new Vector2(0, 0.55f);
            instantRect.anchorMax = new Vector2(1, 0.95f);
            instantRect.offsetMin = new Vector2(5, 0);
            instantRect.offsetMax = new Vector2(-5, 0);
            Button instantFinishBtn = instantBtn.GetComponent<Button>();
            instantFinishBtn.onClick.AddListener(() => OnInstantFinishClicked(data.type));

            // Upgrade Button (Bottom)
            GameObject upgradeBtn = CreateButton(buttonPanel.transform, "+", new Color(0.2f, 0.7f, 0.2f));
            RectTransform upgradeRect = upgradeBtn.GetComponent<RectTransform>();
            upgradeRect.anchorMin = new Vector2(0, 0.05f);
            upgradeRect.anchorMax = new Vector2(1, 0.45f);
            upgradeRect.offsetMin = new Vector2(5, 0);
            upgradeRect.offsetMax = new Vector2(-5, 0);
            Button upgradeBtnComp = upgradeBtn.GetComponent<Button>();
            TextMeshProUGUI costText = upgradeBtn.GetComponentInChildren<TextMeshProUGUI>();
            int capturedIndex = index;
            Debug.Log($"Button setup for index {capturedIndex}, interactable will be set in UpdateCardState");
            upgradeBtnComp.onClick.AddListener(() => OnUpgradeClicked(capturedIndex));

            // Store references
            var cardData = new UpgradeCardData
            {
                cardObject = card,
                data = data,
                index = index,
                timeText = timeText,
                progressFill = fillImg,
                upgradeButton = upgradeBtnComp,
                instantFinishButton = instantFinishBtn,
                costText = costText,
                nameText = nameText,
                descText = descText
            };
            activeCards.Add(cardData);

            UpdateCardState(cardData);
        }

        private void UpdateCardState(UpgradeCardData cardData)
        {
            var data = cardData.data;
            int level = LabSystem.Instance.GetUpgradeLevel(data.type);
            bool isInProgress = LabSystem.Instance.IsUpgradeInProgress(data.type);
            bool isMaxed = level >= data.maxLevel;

            if (isInProgress)
            {
                float remaining = LabSystem.Instance.GetRemainingTime(data.type);
                float progress = LabSystem.Instance.GetProgress(data.type);
                
                int mins = Mathf.FloorToInt(remaining / 60);
                int secs = Mathf.FloorToInt(remaining % 60);
                cardData.timeText.text = $"{mins:D2}:{secs:D2}";
                
                cardData.progressFill.rectTransform.anchorMax = new Vector2(progress, 1);
                
                cardData.instantFinishButton.gameObject.SetActive(true);
                cardData.upgradeButton.gameObject.SetActive(false);
            }
            else
            {
                float fillAmount = level > 0 ? (float)level / data.maxLevel : 0;
                cardData.progressFill.rectTransform.anchorMax = new Vector2(fillAmount, 1);
                cardData.timeText.text = $"{level}/{data.maxLevel}";
                
                cardData.instantFinishButton.gameObject.SetActive(false);
                cardData.upgradeButton.gameObject.SetActive(true);

                if (isMaxed)
                {
                    cardData.costText.text = "MAX";
                    cardData.upgradeButton.interactable = false;
                    cardData.upgradeButton.GetComponent<Image>().color = Color.gray;
                }
                else
                {
                    int cost = data.GetCost(level);
                    cardData.costText.text = cost.ToString();
                    bool canAfford = DNASystem.Instance != null && DNASystem.Instance.TotalDNA >= cost;
                    cardData.upgradeButton.interactable = canAfford;
                    cardData.upgradeButton.GetComponent<Image>().color = canAfford ? new Color(0.2f, 0.7f, 0.2f) : new Color(0.5f, 0.2f, 0.2f);
                }
            }
        }

        private void UpdateTimers()
        {
            bool needsRefresh = false;
            
            foreach (var cardData in activeCards)
            {
                if (LabSystem.Instance.IsUpgradeInProgress(cardData.data.type))
                {
                    float remaining = LabSystem.Instance.GetRemainingTime(cardData.data.type);
                    float progress = LabSystem.Instance.GetProgress(cardData.data.type);
                    
                    if (remaining <= 0)
                    {
                        // Timer just completed, need full refresh
                        needsRefresh = true;
                    }
                    else
                    {
                        int totalMins = Mathf.FloorToInt(remaining / 60);
                        int secs = Mathf.FloorToInt(remaining % 60);
                        
                        if (totalMins >= 60)
                        {
                            int hours = totalMins / 60;
                            int mins = totalMins % 60;
                            cardData.timeText.text = $"{hours:D2}:{mins:D2}:{secs:D2}";
                        }
                        else
                        {
                            cardData.timeText.text = $"{totalMins:D2}:{secs:D2}";
                        }
                        cardData.progressFill.rectTransform.anchorMax = new Vector2(progress, 1);
                    }
                }
            }
            
            if (needsRefresh)
            {
                RefreshUI();
            }
        }

        private void OnUpgradeClicked(int index)
        {
            Debug.Log($"OnUpgradeClicked: index={index}");
            if (LabSystem.Instance == null)
            {
                Debug.LogError("LabSystem.Instance is null!");
                return;
            }
            bool success = LabSystem.Instance.StartUpgrade(index);
            Debug.Log($"StartUpgrade result: {success}");
            if (success) RefreshUI();
        }

        private void OnInstantFinishClicked(UpgradeType type)
        {
            Debug.Log($"OnInstantFinishClicked: type={type}");
            if (LabSystem.Instance == null)
            {
                Debug.LogError("OnInstantFinishClicked: LabSystem.Instance is null!");
                return;
            }
            bool success = LabSystem.Instance.TryInstantFinish(type);
            Debug.Log($"TryInstantFinish result: {success}");
            if (success) RefreshUI();
        }

        // === HELPERS ===
        private TextMeshProUGUI CreateText(Transform parent, string text, float size, FontStyles style, Color color)
        {
            GameObject obj = new GameObject("Text");
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.raycastTarget = false;
            return tmp;
        }

        private GameObject CreateButton(Transform parent, string text, Color color)
        {
            GameObject obj = new GameObject("Button");
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            
            Image img = obj.AddComponent<Image>();
            img.color = color;
            
            obj.AddComponent<Button>();

            TextMeshProUGUI tmp = CreateText(obj.transform, text, 18, FontStyles.Bold, Color.white);
            tmp.alignment = TextAlignmentOptions.Center;
            
            return obj;
        }
    }
}
