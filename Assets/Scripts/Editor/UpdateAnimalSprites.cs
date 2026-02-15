using UnityEngine;
using UnityEditor;
using Core;

public class UpdateAnimalSprites : EditorWindow
{
    [MenuItem("FaunaFuse/Utils/Update Animal Tile Sprites")]
    public static void Execute()
    {
        string spritePath = "Assets/Art/UI/2.3-Play Scene/Gameplay Area/Animal Tiles/";
        
        // Mapping: AnimalSO level (1-25) -> sprite filename prefix (0-24)
        string[] spriteNames = {
            "0_butterfly", "1_goldfish", "2_squirrel", "3_rabbit", "4_bird",
            "5_frog", "6_snake", "7_otter", "8_turtle", "9_panda",
            "10_horse", "11_deer", "12_giraffe", "13_gorilla", "14_elephant",
            "15_cat", "16_owl", "17_fox", "18_pig", "19_dog",
            "20_dolphin", "21_wolf", "22_bear", "23_eagle", "24_lion"
        };

        string[] guids = AssetDatabase.FindAssets("t:AnimalSO", new[] { "Assets/Data/ScriptableObjects" });
        int updated = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnimalSO animal = AssetDatabase.LoadAssetAtPath<AnimalSO>(path);
            if (animal == null) continue;

            int index = animal.level - 1; // level 1 = index 0
            if (index < 0 || index >= spriteNames.Length) continue;

            string fullPath = spritePath + spriteNames[index] + ".png";
            Sprite newSprite = AssetDatabase.LoadAssetAtPath<Sprite>(fullPath);

            if (newSprite != null)
            {
                animal.icon = newSprite;
                EditorUtility.SetDirty(animal);
                updated++;
                Debug.Log($"Updated {animal.animalName} (Level {animal.level}) icon -> {spriteNames[index]}");
            }
            else
            {
                Debug.LogWarning($"Sprite not found: {fullPath}");
            }
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Update Complete", $"Updated {updated} animal tile sprites.", "OK");
    }
}
