#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Core; // To access AnimalSO

namespace Utils
{
    public class AssetGenerator
    {
        [MenuItem("FaunaFuse/Generate Animals")]
        public static void GenerateAnimals()
        {
             string path = "Assets/Data/ScriptableObjects";
             if (!AssetDatabase.IsValidFolder(path))
             {
                 System.IO.Directory.CreateDirectory(path); // Should exist though
             }

             string[] names = new string[] {
                 "Butterfly", "Fish", "Squirrel", "Rabbit", "Bird", 
                 "Frog", "Snake", "Otter", "Turtle", "Panda", 
                 "Horse", "Deer", "Giraffe", "Gorilla", "Elephant", 
                 "Cat", "Owl", "Fox", "Pig", "Dog", 
                 "Dolphin", "Wolf", "Bear", "Eagle", "Lion"
             };

             for (int i = 0; i < 25; i++)
             {
                 int level = i + 1;
                 string animalName = (i < names.Length) ? names[i] : "Unknown";
                 
                 AnimalSO asset = ScriptableObject.CreateInstance<AnimalSO>();
                 asset.level = level;
                 asset.animalName = animalName;
                 // 2^level
                 // Formula: Level 1->2 cost is 2. But this is reward?
                 // Prompt: "DNA puan değeri (2^seviye)"
                 asset.dnaReward = (int)Mathf.Pow(2, level);
                 asset.trivia = $"This is a level {level} {animalName}.";

                 string fileName = $"Animal_{level:00}_{animalName}.asset";
                 string fullPath = $"{path}/{fileName}";

                 // Check if exists
                 if (AssetDatabase.LoadAssetAtPath<AnimalSO>(fullPath) == null)
                 {
                     AssetDatabase.CreateAsset(asset, fullPath);
                 }
             }
             
             AssetDatabase.SaveAssets();
             AssetDatabase.Refresh();
             Debug.Log("Generated 25 AnimalSOs!");
        }
    }
}
#endif