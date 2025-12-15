#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UI;
using Core;

namespace Utils
{
    public class SceneBuilder_V2
    {
        [MenuItem("FaunaFuse/Create Tile Prefab V10 (Masked)")]
        public static void CreateTilePrefab()
        {
             GameObject go = new GameObject("Tile");
             Sprite roundedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/RoundedSquare.png");
             SpriteMask mask = go.AddComponent<SpriteMask>();
             mask.sprite = roundedSprite;
             
             GameObject icon = new GameObject("Icon");
             icon.transform.SetParent(go.transform);
             SpriteRenderer iconSr = icon.AddComponent<SpriteRenderer>();
             iconSr.sortingOrder = 10; 
             iconSr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask; 
             
             GameObject textObj = new GameObject("Text");
             textObj.transform.SetParent(go.transform);
             TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
             tmp.alignment = TextAlignmentOptions.Center;
             tmp.fontSize = 6;
             tmp.color = Color.white;
             tmp.sortingOrder = 11; 
             
             Tile tile = go.AddComponent<Tile>();
             SerializedObject so = new SerializedObject(tile);
             so.FindProperty("iconRenderer").objectReferenceValue = iconSr;
             so.FindProperty("levelText").objectReferenceValue = tmp;
             so.ApplyModifiedProperties();
             
             if (!System.IO.Directory.Exists("Assets/Prefabs")) System.IO.Directory.CreateDirectory("Assets/Prefabs");
             PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/Tile.prefab");
             Object.DestroyImmediate(go);
        }

        [MenuItem("FaunaFuse/Build Main Scene V17 (Recovered V15)")]
        public static void BuildScene()
        {
             string scenePath = "Assets/Scenes/MainGame.unity";
             var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
             
             Sprite roundedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/RoundedSquare.png");
             if(roundedSprite == null) Debug.LogError("Could not load RoundedSquare.png");

             // Camera
             GameObject cam = new GameObject("Main Camera");
             cam.tag = "MainCamera";
             Camera camera = cam.AddComponent<Camera>();
             camera.orthographic = true;
             camera.orthographicSize = 5f; 
             camera.backgroundColor = new Color(0.1f, 0.1f, 0.12f); 
             cam.transform.position = new Vector3(0, 0, -10);
             cam.AddComponent<AudioListener>(); 

             // Canvas
             GameObject canvasObj = new GameObject("Canvas");
             Canvas canvas = canvasObj.AddComponent<Canvas>();
             canvas.renderMode = RenderMode.ScreenSpaceOverlay;
             CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
             scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
             scaler.referenceResolution = new Vector2(1080, 1920);
             scaler.matchWidthOrHeight = 0.5f;
             canvasObj.AddComponent<GraphicRaycaster>();
             
             // --- REVERT TO V15 LOGIC: Simple Container ---
             
             GameObject safeAreaContainer = new GameObject("SafeAreaContainer");
             safeAreaContainer.transform.SetParent(canvasObj.transform, false);
             // Stretch to fill canvas
             RectTransform rt = safeAreaContainer.AddComponent<RectTransform>();
             rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
             rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
             
             // Ensure it has SafeArea component if it exists
             safeAreaContainer.AddComponent<SafeArea>(); 

             // Managers
             GameObject managers = new GameObject("Managers");
             UIManager uiManager = managers.AddComponent<UIManager>();
             NavigationManager navManager = managers.AddComponent<NavigationManager>();
             if(!managers.GetComponent<GameInput.SwipeDetector>()) managers.AddComponent<GameInput.SwipeDetector>();
             
             // UI ELEMENTS (Parented to SafeAreaContainer)
             GameObject topBar = CreatePanel(safeAreaContainer.transform, "TopBar", new Color(0.15f, 0.15f, 0.2f, 1f));
             SetRect(topBar, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, 250), Vector2.zero);
             uiManager.dnaText = CreateText(topBar.transform, "DNA: 0", Vector2.zero);
             uiManager.heartText = CreateText(topBar.transform, "Hearts: 5", Vector2.zero);
             uiManager.heartTimerText = CreateText(topBar.transform, "", Vector2.zero);

             // Board
             GameObject boardContainer = new GameObject("BoardContainer");
             GameObject boardBg = new GameObject("BoardBackground");
             boardBg.transform.SetParent(boardContainer.transform);
             boardBg.SetActive(false); 

             GameObject slotsContainer = new GameObject("Slots");
             slotsContainer.transform.SetParent(boardContainer.transform);
             
             // 1.1 Scale Logic (KEPT)
             float tileSize = 1.1f; 
             float padding = 0.02f; 
             int width = 4; int height = 4;
             float totalWidth = (width * tileSize) + ((width - 1) * padding);
             float totalHeight = (height * tileSize) + ((height - 1) * padding);
             Vector3 origin = new Vector3(-totalWidth / 2 + tileSize / 2, -totalHeight / 2 + tileSize / 2, 0);

             for(int x=0; x<width; x++) {
                 for(int y=0; y<height; y++) {
                     GameObject slot = new GameObject($"Slot_{x}_{y}");
                     slot.transform.SetParent(slotsContainer.transform);
                     slot.transform.localPosition = origin + new Vector3(x*(tileSize+padding), y*(tileSize+padding), 0);
                     SpriteRenderer ssr = slot.AddComponent<SpriteRenderer>();
                     ssr.sprite = roundedSprite;
                     ssr.color = new Color(0.3f, 0.3f, 0.35f, 1f); 
                     ssr.sortingOrder = -5; 
                     ssr.transform.localScale = new Vector3(tileSize, tileSize, 1f); 
                 }
             }
             
             GameObject tileLayer = new GameObject("TileLayer");
             tileLayer.transform.SetParent(boardContainer.transform);
             
             // Manager Logic
             BoardManager boardMgr = managers.AddComponent<BoardManager>();
             SerializedObject soBoard = new SerializedObject(boardMgr);
             soBoard.FindProperty("boardContainer").objectReferenceValue = boardContainer.transform;
             soBoard.FindProperty("tileLayer").objectReferenceValue = tileLayer.transform;
             soBoard.FindProperty("tileSize").floatValue = tileSize;
             soBoard.FindProperty("padding").floatValue = padding;
             
             Tile tilePrefab = AssetDatabase.LoadAssetAtPath<Tile>("Assets/Prefabs/Tile.prefab");
             if (tilePrefab) soBoard.FindProperty("tilePrefab").objectReferenceValue = tilePrefab;
             soBoard.ApplyModifiedProperties();

             // Footer
             GameObject navBar = CreatePanel(safeAreaContainer.transform, "BottomNavBar", new Color(0.15f, 0.15f, 0.2f));
             SetRect(navBar, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 200), Vector2.zero);
             HorizontalLayoutGroup hlg = navBar.AddComponent<HorizontalLayoutGroup>();
             CreateButton(navBar.transform, "Rank", Color.yellow);
             CreateButton(navBar.transform, "Dex", Color.cyan);
             navManager.playBtn = CreateButton(navBar.transform, "PLAY", Color.green);
             CreateButton(navBar.transform, "Lab", Color.magenta);
             CreateButton(navBar.transform, "Shop", Color.red);
             
             navManager.gameplayView = new GameObject("GameplayView");
             navManager.collectionView = new GameObject("CollectionView");
             
             EditorSceneManager.SaveScene(scene, scenePath);
             Debug.Log("Scene Built V17 (Recovered)");
        }
        
        // Helpers
        private static GameObject CreatePanel(Transform p, string n, Color c) { 
            GameObject o = new GameObject(n); o.transform.SetParent(p,false); 
            o.AddComponent<Image>().color=c; 
            return o; 
        }
        private static Button CreateButton(Transform p, string l, Color c) {
             GameObject o = new GameObject(l+"Btn"); o.transform.SetParent(p,false);
             o.AddComponent<Image>().color=c;
             o.AddComponent<LayoutElement>().minHeight=80;
             GameObject t = new GameObject("Text"); t.transform.SetParent(o.transform,false);
             TextMeshProUGUI tmp = t.AddComponent<TextMeshProUGUI>(); tmp.text=l; tmp.color=Color.black; tmp.alignment=TextAlignmentOptions.Center;
             t.GetComponent<RectTransform>().anchorMax = Vector2.one;
             return o.AddComponent<Button>();
        }
        private static TextMeshProUGUI CreateText(Transform p, string t, Vector2 v) {
            GameObject o = new GameObject("Text"); o.transform.SetParent(p,false);
            TextMeshProUGUI tmp = o.AddComponent<TextMeshProUGUI>(); tmp.text=t; tmp.color=Color.white; tmp.alignment=TextAlignmentOptions.Center;
            return tmp;
        }
        private static void SetRect(GameObject o, Vector2 min, Vector2 max, Vector2 pv, Vector2 sz, Vector2 pos) {
            RectTransform r=o.GetComponent<RectTransform>(); r.anchorMin=min; r.anchorMax=max; r.pivot=pv; r.sizeDelta=sz; r.anchoredPosition=pos;
        }
    }
}
#endif