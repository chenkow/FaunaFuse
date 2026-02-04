using UnityEngine;
using UnityEditor;
using Core;
using System.IO;

public class SpriteAssigner : EditorWindow
{
    [MenuItem("Tools/FaunaFuse/Assign Sprites")]
    public static void Assign()
    {
        Debug.Log("Starting Sprite Assignment... (Force Import)");

        // 1. Load all AnimalSO
        string[] guids = AssetDatabase.FindAssets("t:AnimalSO");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnimalSO animal = AssetDatabase.LoadAssetAtPath<AnimalSO>(path);
            
            if (animal == null) continue;
            
            int level = animal.level;
            
            // 2. Find Gameplay Sprite (1-butterfly.png etc) in Assets/Art/animals_gameplay
            // Format: "{level}-*.png" or "{level}_*.png" depending on exact file
            string gameplayFolder = "Assets/Art/animals_gameplay";
            Sprite gameplaySprite = FindSpriteByLevel(gameplayFolder, level, '-', true); // Hyphen
            
            // 3. Find Gallery Sprite (1_butterfly.png) in Assets/Art/animals_no_background
            string galleryFolder = "Assets/Art/animals_no_background";
            Sprite gallerySprite = FindSpriteByLevel(galleryFolder, level, '_', true); // Underscore
            
            // 4. Assign
            bool changed = false;
            if (gameplaySprite != null && animal.icon != gameplaySprite)
            {
                animal.icon = gameplaySprite;
                changed = true;
                Debug.Log($"[Gameplay] Assigned {gameplaySprite.name} to {animal.name}");
            }
            
            if (gallerySprite != null && animal.gallerySprite != gallerySprite)
            {
                animal.gallerySprite = gallerySprite;
                changed = true;
                Debug.Log($"[Gallery] Assigned {gallerySprite.name} to {animal.name}");
            }
            
            if (changed) EditorUtility.SetDirty(animal);
        }
        
        AssetDatabase.SaveAssets();
        Debug.Log("Sprite Assignment Complete!");
    }
    
    private static Sprite FindSpriteByLevel(string folder, int level, char separator, bool recursive) 
    {
        // We look for files starting with "X" where X is the level.
        // But File names are "1-butterfly.png".
        // AssetDatabase.FindAssets is name-based.
        
        // Strategy: Get all sprites in folder, checking regex or split.
        // Optimization: Since we know the folder structure...
        
        string[] spriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { folder });
        foreach(string guid in spriteGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string filename = Path.GetFileName(path); 
            
            // Check if starts with level + separator
            if (filename.StartsWith(level.ToString() + separator) || filename.StartsWith(level.ToString() + " ")) 
            {
                 // Handle 1 vs 10, 11 etc.
                 // If level is 1, ensure next char is separator.
                 // "1-butterfly" -> starts with "1-" -> Match.
                 return AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
        }
        return null;
    }
}
