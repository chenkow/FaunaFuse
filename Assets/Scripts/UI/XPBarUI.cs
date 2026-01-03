using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    /// <summary>
    /// Displays XP progress bar and current level.
    /// </summary>
    public class XPBarUI : MonoBehaviour
    {
        [Header("UI Elements")]
        public Slider progressBar;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI xpText;
        
        private void Start()
        {
            if (Managers.PerkManager.Instance != null)
            {
                Managers.PerkManager.Instance.OnXPChange += UpdateXP;
                Managers.PerkManager.Instance.OnLevelUp += UpdateLevel;
                
                // Initial update
                UpdateLevel(Managers.PerkManager.Instance.currentLevel);
                UpdateXP(Managers.PerkManager.Instance.currentXP, Managers.PerkManager.Instance.xpToNextLevel);
            }
        }
        
        private void OnDestroy()
        {
            if (Managers.PerkManager.Instance != null)
            {
                Managers.PerkManager.Instance.OnXPChange -= UpdateXP;
                Managers.PerkManager.Instance.OnLevelUp -= UpdateLevel;
            }
        }
        
        private void UpdateXP(float current, float max)
        {
            if (progressBar) progressBar.value = current / max;
            if (xpText) xpText.text = $"{Mathf.FloorToInt(current)}/{Mathf.FloorToInt(max)}";
        }
        
        private void UpdateLevel(int level)
        {
            if (levelText) levelText.text = $"Lv.{level}";
        }
    }
}
