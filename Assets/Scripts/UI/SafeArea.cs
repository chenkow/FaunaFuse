using UnityEngine;

namespace UI
{
    [ExecuteAlways]
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

        [Header("Debug")]
        public bool simulateInEditor = false;
        public Rect editorSafeRect = new Rect(0, 100, 1080, 2200); // Approximate default

        void Refresh()
        {
            if (panel == null) return;
            
            Rect safeArea = Screen.safeArea;
            
    #if UNITY_EDITOR
            if (simulateInEditor && !Application.isPlaying)
            {
                // If Screen.safeArea is full screen, use debug rect
                if (safeArea.width == Screen.width && safeArea.height == Screen.height)
                {
                    // Default Simulation: iPhone 13 Pro Max approx notch (Top 132px, Bottom 102px on 2778h)
                    // Scaled to current screen
                    float bottom = Screen.height * 0.04f;
                    float top = Screen.height * 0.05f;
                    safeArea = new Rect(0, bottom, Screen.width, Screen.height - top - bottom);
                }
            }
    #endif

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
            
            // Clamp to 0-1
            anchorMin.x = Mathf.Clamp01(anchorMin.x);
            anchorMin.y = Mathf.Clamp01(anchorMin.y);
            anchorMax.x = Mathf.Clamp01(anchorMax.x);
            anchorMax.y = Mathf.Clamp01(anchorMax.y);

            panel.anchorMin = anchorMin;
            panel.anchorMax = anchorMax;
            
            // Reset offsets to zero so it stretches exactly to anchors
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;
        }
    }
}