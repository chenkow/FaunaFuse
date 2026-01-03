#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class CleanupAndFix : EditorWindow
{
    [MenuItem("Tools/FaunaFuse/Cleanup and Fix All")]
    public static void CleanupAll()
    {
        // 1. Remove duplicate LevelUpPanels - keep only first one with LevelUpUI
        var levelUpPanels = Object.FindObjectsByType<UI.LevelUpUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Debug.Log($"Found {levelUpPanels.Length} LevelUpUI components");
        
        if (levelUpPanels.Length > 1)
        {
            for (int i = 1; i < levelUpPanels.Length; i++)
            {
                Debug.Log($"Deleting duplicate: {levelUpPanels[i].gameObject.name}");
                Object.DestroyImmediate(levelUpPanels[i].gameObject);
            }
        }
        
        // Also delete any LevelUpPanel without LevelUpUI script
        var allPanels = GameObject.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var panel in allPanels)
        {
            if (panel.name == "LevelUpPanel" && panel.GetComponent<UI.LevelUpUI>() == null)
            {
                Debug.Log($"Deleting LevelUpPanel without script: {panel.gameObject.GetInstanceID()}");
                Object.DestroyImmediate(panel.gameObject);
            }
        }
        
        // 2. Fix XP Bar position - move to above bottom nav bar
        var xpContainers = Object.FindObjectsByType<UI.XPBarUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (xpContainers.Length > 0)
        {
            var rect = xpContainers[0].GetComponent<RectTransform>();
            // Position just above the navigation bar (which is at bottom)
            rect.anchorMin = new Vector2(0.05f, 0.15f);  // Above nav bar
            rect.anchorMax = new Vector2(0.95f, 0.20f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            EditorUtility.SetDirty(xpContainers[0]);
            Debug.Log("XP Bar repositioned above navigation bar");
        }
        
        // 3. Ensure LevelUpUI has required references
        if (levelUpPanels.Length > 0)
        {
            var levelUpUI = levelUpPanels[0];
            
            // Set panel reference to self
            levelUpUI.panel = levelUpUI.gameObject;
            
            // FIX: Set LevelUpPanel to stretch fullscreen
            var panelRect = levelUpUI.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            Debug.Log("LevelUpPanel RectTransform set to fullscreen overlay");
            
            // Set dark background color
            var panelImage = levelUpUI.GetComponent<UnityEngine.UI.Image>();
            if (panelImage != null)
            {
                panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            }
            
            // Find or create CardContainer
            var cardContainer = levelUpUI.transform.Find("CardContainer");
            if (cardContainer == null)
            {
                var go = new GameObject("CardContainer", typeof(RectTransform), typeof(UnityEngine.UI.HorizontalLayoutGroup));
                go.transform.SetParent(levelUpUI.transform, false);
                cardContainer = go.transform;
                
                // Position in center
                var cardRect = go.GetComponent<RectTransform>();
                cardRect.anchorMin = new Vector2(0.1f, 0.3f);
                cardRect.anchorMax = new Vector2(0.9f, 0.7f);
                cardRect.offsetMin = Vector2.zero;
                cardRect.offsetMax = Vector2.zero;
                
                // Layout settings
                var layout = go.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
                layout.spacing = 20;
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;
            }
            levelUpUI.cardContainer = cardContainer;
            
            // Load card prefab
            var prefab = AssetDatabase.LoadAssetAtPath<UI.PerkCardUI>("Assets/Prefabs/UI/PerkCard.prefab");
            if (prefab != null)
                levelUpUI.cardPrefab = prefab;
            
            // Load all perks
            levelUpUI.allPerks.Clear();
            var perkGuids = AssetDatabase.FindAssets("t:PerkSO", new[] { "Assets/Data/Perks" });
            foreach (var guid in perkGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var perk = AssetDatabase.LoadAssetAtPath<Core.PerkSO>(path);
                if (perk != null)
                    levelUpUI.allPerks.Add(perk);
            }
            
            EditorUtility.SetDirty(levelUpUI);
            Debug.Log($"LevelUpUI configured: {levelUpUI.allPerks.Count} perks loaded, panel & container set");
        }
        
        // 4. Save scene
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log("Cleanup complete! Save the scene.");
    }
}
#endif
