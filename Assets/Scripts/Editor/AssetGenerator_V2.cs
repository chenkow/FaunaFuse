#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Core;

namespace Utils
{
    public class AssetGenerator_V2
    {
        [MenuItem("FaunaFuse/Generate Animals V2")]
        public static void GenerateAnimals()
        {
            // Ensure Resources path exists
            string path = "Assets/Resources/Data/ScriptableObjects";
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }

            string[] names = new string[] {
                "Ant", "Beetle", "Cat", "Dog", "Eagle",
                "Fox", "Goat", "Horse", "Iguana", "Jaguar",
                "Kangaroo", "Lion", "Monkey", "Narwhal", "Owl",
                "Penguin", "Quokka", "Rabbit", "Snake", "Tiger",
                "Urchin", "Vulture", "Whale", "Xerus", "Yak"
            };

            for (int i = 0; i < 25; i++)
            {
                int level = i + 1;
                string animalName = (i < names.Length) ? names[i] : "Animal " + level;
                
                string fileName = $"{level}_{animalName}.asset";
                string fullPath = $"{path}/{fileName}";
                
                AnimalSO asset = AssetDatabase.LoadAssetAtPath<AnimalSO>(fullPath);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<AnimalSO>();
                    AssetDatabase.CreateAsset(asset, fullPath);
                }

                asset.level = level;
                asset.animalName = animalName;
                asset.dnaReward = (int)Mathf.Pow(2, level);
                asset.trivia = $"A level {level} friend.";
                
                EditorUtility.SetDirty(asset);
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated 25 AnimalSOs in Resources!");
        }
    }
}
#endif