using UnityEngine;
using UnityEditor;
using Core;
using System.Collections.Generic;

namespace EditorUtils
{
    public class SimpleRepair : MonoBehaviour
    {
        [MenuItem("FaunaFuse/ADMIN/Simple Repair Icons")]
        public static void RepairIcons()
        {
            Debug.Log("Starting Simple Icon Repair...");
            string[] animalGuids = AssetDatabase.FindAssets("t:AnimalSO");
            int count = 0;

            foreach (string guid in animalGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AnimalSO animal = AssetDatabase.LoadAssetAtPath<AnimalSO>(path);
                
                if (animal != null && !string.IsNullOrEmpty(animal.animalName))
                {
                    string searchName = animal.animalName.ToLower().Trim(); 
                    if (searchName == "su samuru") searchName = "otter"; 
                    
                    string[] foundGuids = AssetDatabase.FindAssets($"{searchName} t:Sprite", new[] { "Assets/Resources/Gallery/Animals" });
                    
                    if (foundGuids.Length > 0)
                    {
                        string spritePath = AssetDatabase.GUIDToAssetPath(foundGuids[0]);
                        Sprite newSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                        
                        if (newSprite != null)
                        {
                            animal.icon = newSprite;
                            EditorUtility.SetDirty(animal);
                            count++;
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"No sprite found for: {animal.animalName}");
                    }
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"Simple Repair Complete. Updated {count} animals.");
        }
    }
}
