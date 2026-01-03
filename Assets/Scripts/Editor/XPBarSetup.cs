#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class XPBarSetup : EditorWindow
{
    [MenuItem("Tools/FaunaFuse/Setup XP Bar")]
    public static void SetupXPBar()
    {
        // Find SafeAreaContainer
        var safeArea = GameObject.Find("SafeAreaContainer");
        if (safeArea == null)
        {
            Debug.LogError("SafeAreaContainer not found!");
            return;
        }
        
        // Find or create XPBarContainer
        var xpContainer = safeArea.transform.Find("XPBarContainer");
        if (xpContainer == null)
        {
            var go = new GameObject("XPBarContainer", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(safeArea.transform, false);
            xpContainer = go.transform;
        }
        
        // Position at bottom of screen
        var rect = xpContainer.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.1f, 0.02f);
        rect.anchorMax = new Vector2(0.9f, 0.06f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        // Background color
        var bg = xpContainer.GetComponent<Image>();
        if (bg) bg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        // Add XPBarUI if missing
        var xpUI = xpContainer.GetComponent<UI.XPBarUI>();
        if (xpUI == null) xpUI = xpContainer.gameObject.AddComponent<UI.XPBarUI>();
        
        // Find or create Slider
        var sliderObj = xpContainer.Find("XPSlider");
        Slider slider = null;
        if (sliderObj == null)
        {
            sliderObj = new GameObject("XPSlider", typeof(RectTransform), typeof(Slider)).transform;
            sliderObj.SetParent(xpContainer, false);
        }
        slider = sliderObj.GetComponent<Slider>();
        
        // Setup slider RectTransform - fill parent
        var sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = Vector2.zero;
        sliderRect.anchorMax = Vector2.one;
        sliderRect.offsetMin = new Vector2(5, 5);
        sliderRect.offsetMax = new Vector2(-5, -5);
        
        // Create Fill Area
        var fillArea = sliderObj.Find("Fill Area");
        if (fillArea == null)
        {
            fillArea = new GameObject("Fill Area", typeof(RectTransform)).transform;
            fillArea.SetParent(sliderObj, false);
            var fillRect = fillArea.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
        }
        
        // Create Fill
        var fill = fillArea.Find("Fill");
        if (fill == null)
        {
            fill = new GameObject("Fill", typeof(RectTransform), typeof(Image)).transform;
            fill.SetParent(fillArea, false);
            var fillImgRect = fill.GetComponent<RectTransform>();
            fillImgRect.anchorMin = Vector2.zero;
            fillImgRect.anchorMax = Vector2.one;
            fillImgRect.offsetMin = Vector2.zero;
            fillImgRect.offsetMax = Vector2.zero;
        }
        
        var fillImg = fill.GetComponent<Image>();
        if (fillImg) fillImg.color = new Color(0.3f, 0.8f, 0.3f, 1f); // Green
        
        // Assign fill to slider
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 0.5f;
        slider.interactable = false; // Display only
        
        // Create Level Text
        var levelText = xpContainer.Find("LevelText");
        if (levelText == null)
        {
            levelText = new GameObject("LevelText", typeof(RectTransform), typeof(TextMeshProUGUI)).transform;
            levelText.SetParent(xpContainer, false);
        }
        var levelRect = levelText.GetComponent<RectTransform>();
        levelRect.anchorMin = new Vector2(0, 0);
        levelRect.anchorMax = new Vector2(0.15f, 1);
        levelRect.offsetMin = Vector2.zero;
        levelRect.offsetMax = Vector2.zero;
        
        var levelTMP = levelText.GetComponent<TextMeshProUGUI>();
        if (levelTMP)
        {
            levelTMP.text = "Lv.1";
            levelTMP.alignment = TextAlignmentOptions.Center;
            levelTMP.fontSize = 24;
            levelTMP.color = Color.white;
        }
        
        // Wire references
        xpUI.progressBar = slider;
        xpUI.levelText = levelTMP;
        
        EditorUtility.SetDirty(xpUI);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log("XP Bar setup complete! Position: Bottom of screen.");
    }
}
#endif
