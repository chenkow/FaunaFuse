#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class PrefabFixer : EditorWindow
{
    [MenuItem("Tools/FaunaFuse/Fix Prefabs")]
    public static void FixPrefabs()
    {
        // Fix PerkCard prefab
        string perkCardPath = "Assets/Prefabs/UI/PerkCard.prefab";
        var perkCardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(perkCardPath);
        
        if (perkCardPrefab != null)
        {
            var existingUI = perkCardPrefab.GetComponent<UI.PerkCardUI>();
            if (existingUI == null)
            {
                // Add component to prefab
                var prefab = PrefabUtility.LoadPrefabContents(perkCardPath);
                prefab.AddComponent<UI.PerkCardUI>();
                
                // Wire references
                var perkCardUI = prefab.GetComponent<UI.PerkCardUI>();
                var iconChild = prefab.transform.Find("Icon");
                var nameChild = prefab.transform.Find("PerkName");
                var descChild = prefab.transform.Find("Description");
                
                if (iconChild != null) perkCardUI.iconImage = iconChild.GetComponent<UnityEngine.UI.Image>();
                if (nameChild != null) perkCardUI.nameText = nameChild.GetComponent<TMPro.TextMeshProUGUI>();
                if (descChild != null) perkCardUI.descriptionText = descChild.GetComponent<TMPro.TextMeshProUGUI>();
                // PerkCardUI uses IPointerClickHandler, no button component needed
                
                PrefabUtility.SaveAsPrefabAsset(prefab, perkCardPath);
                PrefabUtility.UnloadPrefabContents(prefab);
                
                Debug.Log("PerkCard prefab fixed! PerkCardUI component added.");
            }
            else
            {
                Debug.Log("PerkCard already has PerkCardUI component.");
            }
        }
        else
        {
            Debug.LogError("PerkCard prefab not found at " + perkCardPath);
        }
        
        // Fix SkillButton prefab
        string skillButtonPath = "Assets/Prefabs/UI/SkillButton.prefab";
        var skillButtonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(skillButtonPath);
        
        if (skillButtonPrefab != null)
        {
            var existingUI = skillButtonPrefab.GetComponent<UI.SkillButtonUI>();
            if (existingUI == null)
            {
                var prefab = PrefabUtility.LoadPrefabContents(skillButtonPath);
                prefab.AddComponent<UI.SkillButtonUI>();
                
                var skillUI = prefab.GetComponent<UI.SkillButtonUI>();
                var iconChild = prefab.transform.Find("Icon");
                var countChild = prefab.transform.Find("Count");
                
                if (iconChild != null) skillUI.iconImage = iconChild.GetComponent<UnityEngine.UI.Image>();
                if (countChild != null) skillUI.countText = countChild.GetComponent<TMPro.TextMeshProUGUI>();
                skillUI.button = prefab.GetComponent<UnityEngine.UI.Button>();
                
                PrefabUtility.SaveAsPrefabAsset(prefab, skillButtonPath);
                
                Debug.Log("SkillButton prefab fixed! SkillButtonUI component added.");
            }
        }
        
        // FIX: Tile Prefab Mask Interaction (Gameplay Visibility Fix)
        string tilePath = "Assets/Prefabs/Tile.prefab";
        GameObject tileContent = PrefabUtility.LoadPrefabContents(tilePath);
        if (tileContent)
        {
            Transform icon = tileContent.transform.Find("Icon");
            if (icon)
            {
                var sr = icon.GetComponent<SpriteRenderer>();
                if (sr && sr.maskInteraction != SpriteMaskInteraction.None)
                {
                    sr.maskInteraction = SpriteMaskInteraction.None;
                    Debug.Log("FIXED: Disabled Mask Interaction on Tile Icon to modify visibility.");
                    
                    // Also check sorting layer
                    if(sr.sortingOrder == 0) sr.sortingOrder = 10; // Ensure it's above background
                }
            }
            PrefabUtility.SaveAsPrefabAsset(tileContent, tilePath);
            PrefabUtility.UnloadPrefabContents(tileContent);
        }
        
        AssetDatabase.Refresh();
        Debug.Log("All prefabs fixed!");
    }
}
#endif
