using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core;
using UnityEngine.EventSystems;

namespace UI
{
    /// <summary>
    /// Individual perk card shown in Level Up selection screen.
    /// </summary>
    public class PerkCardUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI Elements")]
        public Image iconImage;
        public Image frameImage;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI descriptionText;
        public TextMeshProUGUI levelText;
        
        [Header("Visuals")]
        public Color normalColor = Color.white;
        public Color hoverColor = new Color(1f, 0.9f, 0.5f);
        
        private PerkSO perkData;
        private System.Action<PerkSO> onClickCallback;
        
        public void Setup(PerkSO perk, System.Action<PerkSO> onClick)
        {
            perkData = perk;
            onClickCallback = onClick;
            
            if (perk == null) return;
            
            Debug.Log($"Setting up PerkCard for: {perk.perkName}. Refs: Icon={iconImage}, Name={nameText}, Desc={descriptionText}");
            
            if (iconImage)
            {
                if (perk.icon) 
                {
                    iconImage.sprite = perk.icon;
                    iconImage.color = Color.white;
                    iconImage.gameObject.SetActive(true);
                }
                else
                {
                    // Clean fallback: Hide missing icon so text centers/expands
                    iconImage.gameObject.SetActive(false); 
                }
            }

            if (nameText) 
            {
                nameText.text = perk.perkName;
                nameText.color = Color.black;
                nameText.enableAutoSizing = true;
                nameText.fontSizeMin = 10;
                nameText.fontSizeMax = 72;
                nameText.alignment = TextAlignmentOptions.Center;
                
                // Force Transform properties
                nameText.transform.localScale = Vector3.one;
                var localPos = nameText.transform.localPosition;
                nameText.transform.localPosition = new Vector3(localPos.x, localPos.y, 0);
                nameText.transform.SetAsLastSibling(); // Draw on top
                
                // Force Rect to be visible if it's somehow zero
                var rt = nameText.rectTransform;
                if (rt.sizeDelta.x < 10 || rt.sizeDelta.y < 10)
                {
                    rt.sizeDelta = new Vector2(200, 50); // Fallback size
                }
                Debug.Log($"NameText: '{nameText.text}' Color: {nameText.color} Rect: {rt.rect} Active: {nameText.gameObject.activeInHierarchy} Scale: {nameText.transform.localScale}");
            }
            
            if (descriptionText) 
            {
                descriptionText.text = perk.description;
                descriptionText.color = Color.black;
                descriptionText.enableAutoSizing = true;
                descriptionText.fontSizeMin = 10;
                descriptionText.fontSizeMax = 48;
                descriptionText.alignment = TextAlignmentOptions.Center;
                
                // Force Transform properties
                descriptionText.transform.localScale = Vector3.one;
                var localPos = descriptionText.transform.localPosition;
                descriptionText.transform.localPosition = new Vector3(localPos.x, localPos.y, 0);
                descriptionText.transform.SetAsLastSibling(); // Draw on top

                 var rt = descriptionText.rectTransform;
                if (rt.sizeDelta.x < 10 || rt.sizeDelta.y < 10)
                {
                    rt.sizeDelta = new Vector2(200, 100); // Fallback size
                }
                 Debug.Log($"DescText: '{descriptionText.text}' Color: {descriptionText.color} Rect: {rt.rect} Active: {descriptionText.gameObject.activeInHierarchy} Scale: {descriptionText.transform.localScale}");
            }
            
            // Show current level if already owned
            int currentLevel = GetCurrentLevel(perk);
            if (levelText)
            {
                if (currentLevel > 0)
                    levelText.text = $"Lv.{currentLevel} → Lv.{currentLevel + 1}";
                else
                    levelText.text = "NEW!";
            }
        }
        
        private int GetCurrentLevel(PerkSO perk)
        {
            if (Managers.PerkManager.Instance == null) return 0;
            
            var existing = Managers.PerkManager.Instance.activePerks.Find(p => p.Definition == perk);
            return existing?.Level ?? 0;
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            onClickCallback?.Invoke(perkData);
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            transform.localScale = Vector3.one * 1.05f;
            if (frameImage) frameImage.color = hoverColor;
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            transform.localScale = Vector3.one;
            if (frameImage) frameImage.color = normalColor;
        }
    }
}
