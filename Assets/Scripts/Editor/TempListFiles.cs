#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public class TempListFiles 
{
    [MenuItem("FaunaFuse/Debug List Art")]
    public static void ListArt()
    {
        string path = "Assets/Art";
        if(!Directory.Exists(path)) {
            Debug.LogError("Assets/Art does not exist!");
            return;
        }
        
        string[] files = Directory.GetFiles(path);
        foreach(var f in files) {
            Debug.Log($"Found: {f}");
        }
    }
}
#endif