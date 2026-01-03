using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core;
using UnityEngine.EventSystems;
using System.Collections;

namespace UI
{
    public class CardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Data")]
        public AnimalSO animalData;
        
        [Header("Components")]
        public Image startBg; // Simple gradient or solid color
        public Image iconImage;
        public Image frameImage;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI levelText;
        public GameObject lockedOverlay;
        
        [Header("Rarity Colors")]
        public Color commonColor = new Color(0.8f, 0.8f, 0.8f);
        public Color rareColor = new Color(0.2f, 0.6f, 1f);
        public Color epicColor = new Color(0.8f, 0.2f, 1f);
        public Color legendaryColor = new Color(1f, 0.8f, 0.2f);

        private System.Action<AnimalSO> onClickCallback;
        private bool isUnlocked;
        private Coroutine scalerCoroutine;

        public void Setup(AnimalSO animal, bool unlocked, System.Action<AnimalSO> onClick)
        {
            animalData = animal;
            isUnlocked = unlocked;
            onClickCallback = onClick;

            if (animal == null) return;

            // Basic Info
            if (nameText) nameText.text = isUnlocked ? animal.animalName : "???";
            if (levelText) levelText.text = $"LVL {animal.level}";
            if (iconImage) 
            {
                iconImage.sprite = animal.icon;
                iconImage.color = isUnlocked ? Color.white : Color.black;
            }

            // Locked Visuals
            if (lockedOverlay) lockedOverlay.SetActive(!isUnlocked);

            // Rarity Styling
            ApplyRarityVisuals(animal.rarity);
        }

        private void ApplyRarityVisuals(AnimalSO.Rarity rarity)
        {
            Color themeColor = commonColor;
            switch (rarity)
            {
                case AnimalSO.Rarity.Common: themeColor = commonColor; break;
                case AnimalSO.Rarity.Rare: themeColor = rareColor; break;
                case AnimalSO.Rarity.Epic: themeColor = epicColor; break;
                case AnimalSO.Rarity.Legendary: themeColor = legendaryColor; break;
            }

            if (startBg) startBg.color = themeColor;
            if (frameImage) frameImage.color = themeColor;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isUnlocked) return;
            // Hover Effect: Scale Up
            if (scalerCoroutine != null) StopCoroutine(scalerCoroutine);
            scalerCoroutine = StartCoroutine(ScaleTo(1.1f));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // Reset Scale
            if (scalerCoroutine != null) StopCoroutine(scalerCoroutine);
            scalerCoroutine = StartCoroutine(ScaleTo(1.0f));
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (isUnlocked)
            {
                // Click Effect: Punch
                StartCoroutine(PunchEffect());
                onClickCallback?.Invoke(animalData);
            }
        }
        
        private IEnumerator ScaleTo(float targetScale)
        {
            float timer = 0f;
            float duration = 0.2f;
            Vector3 startVal = transform.localScale;
            Vector3 endVal = Vector3.one * targetScale;
            
            while (timer < duration)
            {
                timer += Time.deltaTime;
                transform.localScale = Vector3.Lerp(startVal, endVal, timer / duration);
                yield return null;
            }
            transform.localScale = endVal;
        }

        private IEnumerator PunchEffect()
        {
            transform.localScale = Vector3.one * 0.9f;
            yield return new WaitForSeconds(0.1f);
            transform.localScale = Vector3.one;
        }
    }
}
