using UnityEngine;
using UnityEditor;
using System.IO;

public class SaveUtils
{
    [MenuItem("FaunaFuse/Clear All Save Data")]
    public static void ClearAllData()
    {
        // 1. Clear PlayerPrefs
        PlayerPrefs.DeleteAll();
        Debug.Log("PlayerPrefs Cleared.");

        // 2. Clear SaveSystem File
        string path = Path.Combine(Application.persistentDataPath, "gamedata.json");
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"Deleted Save File at: {path}");
        }
        else
        {
            Debug.Log("No Save File found to delete.");
        }

        // 3. If Game is Running, reset Runtime Data
        if (Application.isPlaying)
        {
            if (Systems.SaveSystem.Instance) Systems.SaveSystem.Instance.ClearSave();
            Debug.Log("Runtime Save Data Reset.");
        }
    }
}
