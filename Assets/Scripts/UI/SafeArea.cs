using UnityEngine;

namespace UI
{
    public class SafeArea : MonoBehaviour
    {
        private RectTransform panel;
        private Rect lastSafeArea = new Rect(0, 0, 0, 0);

        void Awake()
        {
            panel = GetComponent<RectTransform>();
            Refresh();
        }

        void Update()
        {
            if (lastSafeArea != Screen.safeArea)
            {
                Refresh();
            }
        }

        void Refresh()
        {
            if (panel == null) return;
            
            Rect safeArea = Screen.safeArea;
            if (safeArea != lastSafeArea)
            {
                lastSafeArea = safeArea;
                ApplySafeArea(safeArea);
            }
        }

        void ApplySafeArea(Rect r)
        {
            Vector2 anchorMin = r.position;
            Vector2 anchorMax = r.position + r.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            panel.anchorMin = anchorMin;
            panel.anchorMax = anchorMax;
            
            // Reset offsets to zero so it stretches exactly to anchors
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;
        }
    }
}