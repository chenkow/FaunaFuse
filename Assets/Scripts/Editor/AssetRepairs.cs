using UnityEngine;
using UnityEditor;
using Core;
using System.Collections.Generic;
using UI;

namespace EditorUtils
{
    public class AssetRepairs : MonoBehaviour
    {
        [MenuItem("FaunaFuse/ADMIN/Repair All Assets")]
        public static void RepairAssets()
        {
            Debug.Log("Starting Asset Repair...");

            // 1. Repair AnimalSO Icons
            string[] animalGuids = AssetDatabase.FindAssets("t:AnimalSO");
            int repairedCount = 0;
            foreach (string guid in animalGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AnimalSO animal = AssetDatabase.LoadAssetAtPath<AnimalSO>(path);
                if (animal != null)
                {
                    if (string.IsNullOrEmpty(animal.animalName)) continue;

                    string searchName = animal.animalName.ToLower().Trim();
                    string[] foundGuids = AssetDatabase.FindAssets($"{searchName} t:Sprite", new[] { "Assets/Resources/Gallery/Animals" });
                    
                    if (foundGuids.Length > 0)
                    {
                         string spritePath = AssetDatabase.GUIDToAssetPath(foundGuids[0]);
                         Sprite newSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                         if (newSprite != null)
                         {
                             animal.icon = newSprite;
                             EditorUtility.SetDirty(animal);
                             repairedCount++;
                         }
                    }
                    else
                    {
                        Debug.LogWarning($"Could not find sprite for {animal.animalName} in Resources/Gallery/Animals");
                    }
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"Repaired {repairedCount} AnimalSO icons.");

            // 2. Run Gallery Setup (Force it)
            GallerySetup.Setup(); 
        }
    }
}
