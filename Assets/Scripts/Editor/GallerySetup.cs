using UnityEngine;
using UnityEditor;
using UI;
using UnityEngine.UI;
using System.Collections.Generic;

public class GallerySetup
{
    [MenuItem("FaunaFuse/Setup Gallery")]
    public static void Setup()
    {
        Debug.Log("Starting Gallery Setup...");

        // 0. REPAIR: Trigger correct assignment
        AssignSprites();

        // 1. Fix Prefab
        string prefabPath = "Assets/Prefabs/UI/GalleryTile.prefab";
        GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);
        
        if (prefabContents)
        {
            var uiScript = prefabContents.GetComponent<GalleryTileUI>();
            if (!uiScript)
            {
                uiScript = prefabContents.AddComponent<GalleryTileUI>();
                Debug.Log("Attached GalleryTileUI to prefab.");
            }
            
            // Fix: Enable Preserve Aspect on AnimalLayer
            Transform animalLayer = prefabContents.transform.Find("AnimalLayer");
            if (animalLayer)
            {
                Image animImg = animalLayer.GetComponent<Image>();
                if (animImg) 
                {
                    animImg.preserveAspect = true;
                    Debug.Log("Enabled Preserve Aspect on AnimalLayer.");
                }
            }

            // Allow Awake to handle wiring usually, but we can stick references if we want.
            // But Awake is fine for runtime.
            
            PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabContents);
        }
        else
        {
            Debug.LogError("Could not load GalleryTile prefab!");
        }

        // 2. Setup CollectionView in Scene
        // ROBUST SEARCH: Iterate roots to find inactive CollectionView
        CollectionView collectionView = null;
        
        // Try standard search first
        collectionView = Object.FindFirstObjectByType<CollectionView>(FindObjectsInactive.Include);
        
        // Fallback: Manually search scene roots
        if (collectionView == null)
        {
            UnityEngine.SceneManagement.Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            GameObject[] roots = activeScene.GetRootGameObjects();
            foreach (GameObject root in roots)
            {
                collectionView = root.GetComponentInChildren<CollectionView>(true);
                if (collectionView != null) break;
            }
        }
        
        // Failsafe: Try known path
        if (collectionView == null)
        {
            GameObject go = GameObject.Find("Canvas/SafeAreaContainer/CollectionView");
            if(go) collectionView = go.GetComponent<CollectionView>();
            
            // If inactive, GameObject.Find won't work.
            // We are already iterating roots, so manual traversal is best if we know the path.
            if (collectionView == null)
            {
                 GameObject canvas = GameObject.Find("Canvas");
                 if (canvas)
                 {
                     Transform deep = canvas.transform.Find("SafeAreaContainer/CollectionView");
                     if (deep) collectionView = deep.GetComponent<CollectionView>();
                 }
            }
        }

        if (collectionView)
        {
            Undo.RecordObject(collectionView, "Setup CollectionView Assets");
            
            // Assign Prefab
            var loadedPrefab = AssetDatabase.LoadAssetAtPath<GalleryTileUI>(prefabPath);
            if(loadedPrefab) collectionView.galleryTilePrefab = loadedPrefab;

            // Load Assets (Safe Mode for Multiple Sprites)
            collectionView.glassSprite = LoadSpriteSafe("Assets/Art/glass/glass.png");
            collectionView.starEmpty = LoadSpriteSafe("Assets/Art/star/star_empty.png");
            collectionView.starFilled = LoadSpriteSafe("Assets/Art/star/star_filled.png");
            
            collectionView.rarityBackgrounds = new List<Sprite>();
            collectionView.rarityBackgrounds.Add(LoadSpriteSafe("Assets/Art/rarity/common.png"));
            collectionView.rarityBackgrounds.Add(LoadSpriteSafe("Assets/Art/rarity/uncommon.png"));
            collectionView.rarityBackgrounds.Add(LoadSpriteSafe("Assets/Art/rarity/rare.png"));
            collectionView.rarityBackgrounds.Add(LoadSpriteSafe("Assets/Art/rarity/epic.png"));
            collectionView.rarityBackgrounds.Add(LoadSpriteSafe("Assets/Art/rarity/legendary.png"));

            // Fix: Set Padding to avoid clipping
            Transform viewport = collectionView.transform.Find("Viewport");
            if (viewport)
            {
                Transform content = viewport.Find("Content");
                if (content)
                {
                    GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
                    if (grid)
                    {
                        grid.padding.top = 20; // Adjusted per user request
                        grid.padding.bottom = 50;
                        Debug.Log("Set Gallery Grid Padding (Top: 20).");
                    }
                }
            }

            EditorUtility.SetDirty(collectionView);
            
            // FORCE SAVE SCENE
            if (!Application.isPlaying)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(collectionView.gameObject.scene);
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(collectionView.gameObject.scene);
            }
            
            Debug.Log($"CollectionView Configured! Glass: {collectionView.glassSprite?.name}, Star: {collectionView.starEmpty?.name}");
        }
        else
        {
            Debug.LogError("CollectionView not found in scene! Make sure the correct scene is open.");
        }
    }

    [MenuItem("FaunaFuse/ADMIN/Only Repair Icons")]
    public static void RepairIconsOnly()
    {
        Debug.Log("Starting Robust Icon Repair...");
        string[] animalGuids = AssetDatabase.FindAssets("t:AnimalSO");
        int count = 0;

        for (int i = 0; i < animalGuids.Length; i++)
        {
            string guid = animalGuids[i];
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Core.AnimalSO animal = AssetDatabase.LoadAssetAtPath<Core.AnimalSO>(path);
            
            if (animal != null && !string.IsNullOrEmpty(animal.animalName))
            {
                string searchName = animal.animalName.ToLower().Trim();
                if (searchName == "su samuru") searchName = "otter"; // Manual fix

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
            }

            // Memory Safety: Clean up every 10 assets
            if (i % 10 == 0) 
            {
                EditorUtility.UnloadUnusedAssetsImmediate();
                System.GC.Collect();
            }
        }
        
        AssetDatabase.SaveAssets();
        EditorUtility.UnloadUnusedAssetsImmediate();
        Debug.Log($"SUCCESS: Repaired {count} AnimalSO icons.");
    }
    [MenuItem("FaunaFuse/Assign Sprites")]
    public static void AssignSprites()
    {
        Debug.Log("Starting Sprite Assignment (via GallerySetup)...");

        // 1. Load all AnimalSO
        string[] guids = AssetDatabase.FindAssets("t:AnimalSO");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Core.AnimalSO animal = AssetDatabase.LoadAssetAtPath<Core.AnimalSO>(path);
            
            if (animal == null) continue;
            
            int level = animal.level;
            
            // 2. Gameplay Sprite: REVERT to 'animals_gameplay' (Has Backgrounds)
            // Indexing: 1-based (1-butterfly.png)
            string gameplayFolder = "Assets/Art/animals_gameplay";
            Sprite gameplaySprite = FindSpriteByLevel(gameplayFolder, level, '-', true); 
            
            // 3. Find Gallery Sprite (1_butterfly.png) in Assets/Art/animals_no_background
            // This folder uses 1-based indexing (1_butterfly = Level 1)
            string galleryFolder = "Assets/Art/animals_no_background";
            Sprite gallerySprite = FindSpriteByLevel(galleryFolder, level, '_', true); 
            
            // 4. Assign
            bool changed = false;
            
            // Gameplay (Icon)
            if (gameplaySprite != null && animal.icon != gameplaySprite)
            {
                animal.icon = gameplaySprite;
                changed = true;
                Debug.Log($"[Gameplay] Assigned {gameplaySprite.name} to {animal.name} (Source: {gameplayFolder})");
            }
            
            // Gallery (GallerySprite)
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
        string[] spriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { folder });
        foreach(string guid in spriteGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string filename = System.IO.Path.GetFileName(path); 
            
            // Check if starts with level + separator or space
            if (filename.StartsWith(level.ToString() + separator) || filename.StartsWith(level.ToString() + " ")) 
            {
                 return AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
        }
        return null;
    }

    private static Sprite LoadSpriteSafe(string path)
    {
        // Try direct load first (Works for Single Sprite mode)
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (s != null) return s;

        // Fallback: Load All Assets (Works for Multiple Sprite mode)
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (Object obj in assets)
        {
            if (obj is Sprite sprite)
            {
                return sprite;
            }
        }
        
        Debug.LogWarning($"Could not load sprite at: {path}");
        return null;
    }
}
