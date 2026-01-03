#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class UIReferenceWirer : EditorWindow
{
    [MenuItem("Tools/FaunaFuse/Wire UI References")]
    public static void WireReferences()
    {
        // Find LevelUpUI (including inactive)
        var levelUpUIs = Object.FindObjectsByType<UI.LevelUpUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var levelUpUI = levelUpUIs.Length > 0 ? levelUpUIs[0] : null;
        if (levelUpUI != null)
        {
            // Find panel (self)
            levelUpUI.panel = levelUpUI.gameObject;
            
            // Find CardContainer
            var cardContainer = levelUpUI.transform.Find("CardContainer");
            if (cardContainer != null)
                levelUpUI.cardContainer = cardContainer;
            
            // Find PerkCard prefab
            var perkCardPrefab = AssetDatabase.LoadAssetAtPath<UI.PerkCardUI>("Assets/Prefabs/UI/PerkCard.prefab");
            if (perkCardPrefab != null)
                levelUpUI.cardPrefab = perkCardPrefab;
            
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
            Debug.Log($"LevelUpUI wired: {levelUpUI.allPerks.Count} perks loaded.");
        }
        else
        {
            Debug.LogWarning("LevelUpUI not found in scene!");
        }
        
        // Find XPBarUI (including inactive)
        var xpBarUIs = Object.FindObjectsByType<UI.XPBarUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var xpBarUI = xpBarUIs.Length > 0 ? xpBarUIs[0] : null;
        if (xpBarUI != null)
        {
            // Find slider child
            var slider = xpBarUI.GetComponentInChildren<Slider>();
            if (slider != null)
                xpBarUI.progressBar = slider;
            
            // Find level text
            var texts = xpBarUI.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var txt in texts)
            {
                if (txt.name.Contains("Level"))
                    xpBarUI.levelText = txt;
                else if (txt.name.Contains("XP"))
                    xpBarUI.xpText = txt;
            }
            
            EditorUtility.SetDirty(xpBarUI);
            Debug.Log("XPBarUI wired.");
        }
        
        // Find ActiveSkillsPanel (including inactive)
        var skillsPanels = Object.FindObjectsByType<UI.ActiveSkillsPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var skillsPanel = skillsPanels.Length > 0 ? skillsPanels[0] : null;
        if (skillsPanel != null)
        {
            // Find SelectionOverlay
            var overlay = GameObject.Find("SelectionOverlay");
            if (overlay != null)
                skillsPanel.selectionOverlay = overlay;
            
            // Find prompt text
            if (overlay != null)
            {
                var prompt = overlay.GetComponentInChildren<TextMeshProUGUI>();
                if (prompt != null)
                    skillsPanel.selectionPromptText = prompt;
            }
            
            // Find button container (self children)
            skillsPanel.buttonContainer = skillsPanel.transform;
            
            // Load SkillButton prefab
            var skillBtnPrefab = AssetDatabase.LoadAssetAtPath<UI.SkillButtonUI>("Assets/Prefabs/UI/SkillButton.prefab");
            if (skillBtnPrefab != null)
                skillsPanel.buttonPrefab = skillBtnPrefab;
            
            EditorUtility.SetDirty(skillsPanel);
            Debug.Log("ActiveSkillsPanel wired.");
        }
        
        // Save scene
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log("All UI references wired successfully!");
    }
}
#endif
