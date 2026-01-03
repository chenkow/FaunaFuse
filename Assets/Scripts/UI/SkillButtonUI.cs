using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core;

namespace UI
{
    public class SkillButtonUI : MonoBehaviour
    {
        [Header("UI Elements")]
        public Image iconImage;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI countText;
        public Button button;
        
        private PerkSO perkData;
        private System.Action<PerkSO> onClickCallback;
        
        public void Setup(PerkSO perk, int uses, System.Action<PerkSO> onClick)
        {
            perkData = perk;
            onClickCallback = onClick;
            
            if (iconImage && perk.icon) iconImage.sprite = perk.icon;
            if (nameText) nameText.text = perk.perkName;
            if (countText) countText.text = $"x{uses}";
            
            if (button) button.onClick.AddListener(OnClick);
        }
        
        private void OnClick()
        {
            onClickCallback?.Invoke(perkData);
        }
    }
}
