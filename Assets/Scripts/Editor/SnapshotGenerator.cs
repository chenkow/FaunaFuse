using UnityEngine;
using UnityEditor;
using System.IO;

[ExecuteInEditMode]
public class SnapshotGenerator : MonoBehaviour
{
    private void Start()
    {
        GenerateIcon();
        // Self-destruct to avoid looping
        if (Application.isPlaying) Destroy(this.gameObject);
        else DestroyImmediate(this.gameObject);
    }

    public void GenerateIcon()
    {
        string fbPath = "Assets/Art/LeaderboardIcon_New.fbx";
        GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fbPath);
        if (modelPrefab == null)
             modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/ImageToStl.com_Copilot3D-ff45a766-118b-4d22-8cfd-419c56f7f35b.glb.fbx");
        
        if (modelPrefab == null)
        {
            Debug.LogError("Could not find model at " + fbPath);
            return;
        }

        // Setup Scene Objects
        GameObject go = Instantiate(modelPrefab);
        go.transform.position = new Vector3(5000, 5000, 5000); 
        go.transform.rotation = Quaternion.Euler(0, 150, 0); 

        GameObject camObj = new GameObject("SnapshotCam");
        Camera cam = camObj.AddComponent<Camera>();
        cam.transform.position = go.transform.position + new Vector3(0, 1f, -2.5f); // Closer
        cam.transform.LookAt(go.transform);
        cam.backgroundColor = new Color(0,0,0,0);
        cam.clearFlags = CameraClearFlags.SolidColor;

        // Render
        int res = 512;
        RenderTexture rt = new RenderTexture(res, res, 24);
        cam.targetTexture = rt;
        Texture2D screenShot = new Texture2D(res, res, TextureFormat.RGBA32, false);

        cam.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, res, res), 0, 0);
        screenShot.Apply();

        // Save
        byte[] bytes = screenShot.EncodeToPNG();
        string path = "Assets/Art/LeaderboardIcon_Render.png";
        File.WriteAllBytes(path, bytes);

        // Cleanup
        RenderTexture.active = null;
        cam.targetTexture = null;
        if(Application.isPlaying) {
             Destroy(rt); Destroy(camObj); Destroy(go);
        } else {
             DestroyImmediate(rt); DestroyImmediate(camObj); DestroyImmediate(go);
        }

        AssetDatabase.Refresh();
        
        // Import Settings
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        Debug.Log("Snapshot SAVED to " + path);
    }
}
