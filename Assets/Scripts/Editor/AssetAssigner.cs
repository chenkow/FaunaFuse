#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using Core;

namespace Utils
{
    public class AssetAssigner
    {
        [MenuItem("FaunaFuse/Assign Art Assets")]
        public static void AssignAssets()
        {
            string artPath = "Assets/Art";
            string dataPath = "Assets/Resources/Data/ScriptableObjects";
            
            if(!Directory.Exists(artPath))
            {
                Debug.LogError("Assets/Art not found!");
                return;
            }

            string[] artFiles = Directory.GetFiles(artPath, "*.png");
            if(artFiles.Length == 0) artFiles = Directory.GetFiles(artPath, "*.jpg");
            
            // Regex to find starting number: "9_kaplumbaga" -> 9
            Regex numberRegex = new Regex(@"^(\d+)_");

            int assignedCount = 0;

            foreach (string filePath in artFiles)
            {
                string fileName = Path.GetFileName(filePath);
                Match match = numberRegex.Match(fileName);
                
                if (match.Success)
                {
                    if(int.TryParse(match.Groups[1].Value, out int level))
                    {
                        // Find matching SO
                        AnimalSO targetSO = FindSoByLevel(dataPath, level);
                        if(targetSO != null)
                        {
                            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(filePath);
                            if(sprite != null)
                            {
                                targetSO.icon = sprite;
                                EditorUtility.SetDirty(targetSO);
                                assignedCount++;
                            }
                        }
                    }
                }
            }
            
            AssetDatabase.SaveAssets();
            Debug.Log($"Assigned {assignedCount} sprites to Animals!");
        }

        private static AnimalSO FindSoByLevel(string path, int level)
        {
             string[] guids = AssetDatabase.FindAssets("t:AnimalSO", new string[]{path});
             foreach(string guid in guids)
             {
                 string p = AssetDatabase.GUIDToAssetPath(guid);
                 AnimalSO so = AssetDatabase.LoadAssetAtPath<AnimalSO>(p);
                 if(so != null && so.level == level) return so;
             }
             return null;
        }
    }
}
#endif