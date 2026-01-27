using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Controls the visual state of a navigation button.
    /// Shows background image when active, hides it when inactive.
    /// </summary>
    public class NavButtonController : MonoBehaviour
    {
        [Tooltip("The colored background image (shown when active)")]
        public Image backgroundImage;

        /// <summary>
        /// Sets the active state of this navigation button.
        /// </summary>
        /// <param name="isActive">True to show background, false to hide it</param>
public void SetActive(bool isActive, int direction = 0)
        {
            if (backgroundImage != null)
            {
                StopAllCoroutines();
                StartCoroutine(FadeBackground(isActive));
            }
        }

private void Start()
        {
            // Ensure correct sibling order: BackgroundImage first (behind), then GlassImage
            if (backgroundImage != null)
            {
                backgroundImage.transform.SetAsFirstSibling();
            }
            
            // Start with background hidden
            SetActive(false);
        }
    

private System.Collections.IEnumerator FadeBackground(bool fadeIn)
        {
            float duration = 0.2f;
            float elapsed = 0f;
            
            // Reset scale and position
            RectTransform bgRect = backgroundImage.rectTransform;
            bgRect.localScale = new Vector3(0.98f, 0.98f, 1f);
            bgRect.anchoredPosition = Vector2.zero;
            
            Color startColor = backgroundImage.color;
            Color targetColor = startColor;
            targetColor.a = fadeIn ? 1f : 0f;
            
            if (fadeIn) backgroundImage.enabled = true;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                backgroundImage.color = Color.Lerp(startColor, targetColor, t);
                yield return null;
            }
            
            backgroundImage.color = targetColor;
            
            if (!fadeIn) backgroundImage.enabled = false;
        }
}
}