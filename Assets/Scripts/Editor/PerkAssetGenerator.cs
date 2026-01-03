#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Core;

public class PerkAssetGenerator : EditorWindow
{
    [MenuItem("Tools/FaunaFuse/Generate All Perks")]
    public static void GeneratePerks()
    {
        string folder = "Assets/Data/Perks";
        
        if (!AssetDatabase.IsValidFolder(folder))
        {
            AssetDatabase.CreateFolder("Assets/Data", "Perks");
        }
        
        // 1. Vacuum (Kara Delik)
        CreatePerk(folder, "Vacuum", "Periodically consumes Rank 1 animals for XP. Cooldown decreases with level.", 
            PerkSO.PerkType.Passive_Board, 5, 2f, false, 10f);
        
        // 2. Magnet
        CreatePerk(folder, "Magnet", "Pulls nearby identical animals when dragging.",
            PerkSO.PerkType.Passive_Board, 3, 1f, false, 0f);
        
        // 3. DNA Miner
        CreatePerk(folder, "DNA_Miner", "Stacking +10% DNA bonus per level.",
            PerkSO.PerkType.Passive_Economy, -1, 10f, false, 0f);
        
        // 4. Expedite
        CreatePerk(folder, "Expedite", "Stacking +10% XP bonus per level.",
            PerkSO.PerkType.Passive_Economy, -1, 10f, false, 0f);
        
        // 5. Evolution
        CreatePerk(folder, "Evolution", "Increased chance for animals to spawn at Rank +1.",
            PerkSO.PerkType.Passive_Board, 5, 5f, false, 0f);
        
        // 6. Joker Rain
        CreatePerk(folder, "Joker_Rain", "Spawns a Joker Slime (wildcard) periodically.",
            PerkSO.PerkType.Passive_Board, 3, 1f, false, 30f);
        
        // 7. Magic Wand
        CreatePerk(folder, "Magic_Wand", "Randomly levels up an animal every X merges.",
            PerkSO.PerkType.Passive_Board, -1, 1f, false, 0f);
        
        // 8. Twins
        CreatePerk(folder, "Twins", "10% chance for merges to produce 2 animals.",
            PerkSO.PerkType.Passive_Board, 5, 2f, false, 0f);
        
        // 9. Shuffle
        CreatePerk(folder, "Shuffle", "Randomizes all animal positions on the board. One-time use.",
            PerkSO.PerkType.Active_Skill, -1, 1f, true, 0f);
        
        // 10. Card Hunter
        CreatePerk(folder, "Card_Hunter", "Stacking +0.5% Global Drop Rate chance.",
            PerkSO.PerkType.Passive_Economy, -1, 0.5f, false, 0f);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("All 10 Perks generated successfully in " + folder);
    }
    
    private static void CreatePerk(string folder, string name, string desc, 
        PerkSO.PerkType type, int maxLvl, float valuePerLvl, bool oneTime, float cooldown)
    {
        string path = $"{folder}/Perk_{name}.asset";
        
        if (AssetDatabase.LoadAssetAtPath<PerkSO>(path) != null)
        {
            Debug.Log($"Perk {name} already exists, skipping.");
            return;
        }
        
        PerkSO perk = ScriptableObject.CreateInstance<PerkSO>();
        perk.perkName = name.Replace("_", " ");
        perk.description = desc;
        perk.type = type;
        perk.maxLevel = maxLvl;
        perk.valuePerLevel = valuePerLvl;
        perk.isOneTimeUse = oneTime;
        perk.cooldown = cooldown;
        
        AssetDatabase.CreateAsset(perk, path);
        Debug.Log($"Created: {path}");
    }
}
#endif
