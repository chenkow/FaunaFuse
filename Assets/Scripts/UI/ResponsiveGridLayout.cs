using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Dynamically adjusts GridLayoutGroup cell size based on available width.
    /// Ensures the grid always fits within the parent container regardless of screen size.
    /// </summary>
    [RequireComponent(typeof(GridLayoutGroup))]
    [ExecuteAlways]
    public class ResponsiveGridLayout : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private int columns = 4;
        [SerializeField] private float horizontalPadding = 20f;
        [SerializeField] private float spacingX = 20f;
        [SerializeField] private float cellAspectRatio = 1f; // width / height

        private GridLayoutGroup grid;
        private RectTransform rectTransform;
        private float lastWidth = -1f;

        private void OnEnable()
        {
            grid = GetComponent<GridLayoutGroup>();
            rectTransform = GetComponent<RectTransform>();
            UpdateLayout();
        }

        private void OnRectTransformDimensionsChange()
        {
            UpdateLayout();
        }

        private void Update()
        {
            // Fallback check for editor and runtime changes
            if (rectTransform != null && Mathf.Abs(rectTransform.rect.width - lastWidth) > 1f)
            {
                UpdateLayout();
            }
        }

        private void UpdateLayout()
        {
            if (grid == null || rectTransform == null) return;
            
            float availableWidth = rectTransform.rect.width;
            if (availableWidth <= 0) return;
            if (Mathf.Abs(availableWidth - lastWidth) < 1f) return;
            
            lastWidth = availableWidth;

            // Calculate cell width to fill available space
            float totalPadding = horizontalPadding * 2f;
            float totalSpacing = spacingX * (columns - 1);
            float cellWidth = (availableWidth - totalPadding - totalSpacing) / columns;
            
            // Ensure minimum size
            cellWidth = Mathf.Max(cellWidth, 50f);
            float cellHeight = cellWidth / cellAspectRatio;

            // Apply settings
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columns;
            grid.cellSize = new Vector2(cellWidth, cellHeight);
            grid.spacing = new Vector2(spacingX, grid.spacing.y);
            grid.padding.left = Mathf.RoundToInt(horizontalPadding);
            grid.padding.right = Mathf.RoundToInt(horizontalPadding);
        }
    }
}
