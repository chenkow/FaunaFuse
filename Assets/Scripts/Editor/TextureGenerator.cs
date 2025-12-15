#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace Utils
{
    public class TextureGenerator
    {
        [MenuItem("FaunaFuse/Generate Rounded Sprite (SDF)")]
        public static void GenerateRoundedSprite()
        {
            int size = 512; 
            float radius = 80f; 
            
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] colors = new Color[size * size];
            
            Vector2 center = new Vector2(size / 2.0f, size / 2.0f);
            Vector2 halfSize = new Vector2(size / 2.0f, size / 2.0f);
            // Safe margin of 1 pixel to avoid edge bleed, though not strictly necessary with clear
            Vector2 boxExtents = halfSize - new Vector2(radius, radius);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Pixel center
                    Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
                    
                    // Relative to center
                    Vector2 rel = p - center;
                    Vector2 absRel = new Vector2(Mathf.Abs(rel.x), Mathf.Abs(rel.y));
                    
                    // Distance to the inner box (the straight/square part)
                    Vector2 d = absRel - boxExtents;
                    
                    // Max(d, 0) is the vector from the inner box edge to the point (0 if inside)
                    Vector2 outsideOffset = new Vector2(Mathf.Max(d.x, 0), Mathf.Max(d.y, 0));
                    
                    // Distance from the rounded corner center (which is at the corner of the inner box)
                    float dist = outsideOffset.magnitude;
                    
                    // If dist <= radius, we are inside the rounded shape
                    // Adding simple Anti-Aliasing (AA) at the edge
                    float alpha = 1.0f - Mathf.Clamp01(dist - radius);
                    
                    colors[y * size + x] = new Color(1, 1, 1, alpha);
                }
            }
            
            tex.SetPixels(colors);
            tex.Apply();

            string dir = "Assets/Art";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            string path = dir + "/RoundedSquare.png";
            
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            
            AssetDatabase.Refresh();
            
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = size; 
                importer.alphaIsTransparency = true;
                importer.filterMode = FilterMode.Bilinear;
                importer.compressionQuality = 100; // High quality
                importer.SaveAndReimport();
            }
            
            Debug.Log($"Generated RoundedSquare.png ({size}x{size}) with SDF Logic!");
        }
    }
}
#endif