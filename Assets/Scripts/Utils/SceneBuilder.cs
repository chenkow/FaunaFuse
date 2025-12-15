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
    public class SceneBuilder
    {
        [MenuItem("FaunaFuse/Create Tile Prefab")]
        public static void CreateTilePrefab()
        {
             GameObject go = new GameObject("Tile");
             // Visuals
             GameObject bg = new GameObject("Background");
             bg.transform.SetParent(go.transform);
             SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
             sr.color = new Color(0.95f, 0.95f, 0.95f); // Slightly off-white
             
             // Icon
             GameObject icon = new GameObject("Icon");
             icon.transform.SetParent(go.transform);
             SpriteRenderer iconSr = icon.AddComponent<SpriteRenderer>();
             iconSr.sortingOrder = 1;
             
             // Text
             GameObject textObj = new GameObject("Text");
             textObj.transform.SetParent(go.transform);
             TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
             tmp.alignment = TextAlignmentOptions.Center;
             tmp.fontSize = 6;
             tmp.color = Color.black;
             tmp.sortingOrder = 2;
             
             // Script
             Tile tile = go.AddComponent<Tile>();
             SerializedObject so = new SerializedObject(tile);
             so.FindProperty("iconRenderer").objectReferenceValue = iconSr;
             so.FindProperty("levelText").objectReferenceValue = tmp;
             so.ApplyModifiedProperties();
             
             if (!System.IO.Directory.Exists("Assets/Prefabs")) System.IO.Directory.CreateDirectory("Assets/Prefabs");
             PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/Tile.prefab");
             Object.DestroyImmediate(go);
             Debug.Log("Tile Prefab Created");
        }

        [MenuItem("FaunaFuse/Build Main Scene")]
        public static void BuildScene()
        {
             string scenePath = "Assets/Scenes/MainGame.unity";
             var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
             
             // Camera
             GameObject cam = new GameObject("Main Camera");
             cam.tag = "MainCamera";
             Camera camera = cam.AddComponent<Camera>();
             camera.orthographic = true;
             camera.orthographicSize = 5;
             camera.backgroundColor = new Color(0.1f, 0.1f, 0.15f); // Dark Background
             camera.clearFlags = CameraClearFlags.SolidColor;
             cam.transform.position = new Vector3(0, 0, -10);
             cam.AddComponent<AudioListener>(); 

             // Canvas
             GameObject canvasObj = new GameObject("Canvas");
             Canvas canvas = canvasObj.AddComponent<Canvas>();
             canvas.renderMode = RenderMode.ScreenSpaceOverlay;
             CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
             scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
             scaler.referenceResolution = new Vector2(1080, 1920);
             canvasObj.AddComponent<GraphicRaycaster>();

             // Managers Container
             GameObject managers = new GameObject("Managers");
             UIManager uiManager = managers.AddComponent<UIManager>();
             NavigationManager navManager = managers.AddComponent<NavigationManager>();
             
             // --- Top Bar ---
             GameObject topBar = CreatePanel(canvas.transform, "TopBar", new Color(0.2f, 0.2f, 0.2f, 1f));
             SetRect(topBar, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, 200), Vector2.zero);
             
             TextMeshProUGUI dnaTxt = CreateText(topBar.transform, "DNA: 0", new Vector2(-250, 0));
             TextMeshProUGUI heartTxt = CreateText(topBar.transform, "Hearts: 5", new Vector2(250, 20));
             TextMeshProUGUI timerTxt = CreateText(topBar.transform, "", new Vector2(250, -40));
             timerTxt.fontSize = 24;

             uiManager.dnaText = dnaTxt;
             uiManager.heartText = heartTxt;
             uiManager.heartTimerText = timerTxt;

             // --- View Container ---
             GameObject viewContainer = CreatePanel(canvas.transform, "ViewContainer", Color.clear);
             RectTransform viewRect = viewContainer.GetComponent<RectTransform>();
             viewRect.anchorMin = Vector2.zero;
             viewRect.anchorMax = Vector2.one;
             viewRect.offsetMin = new Vector2(0, 250); // Bottom Bar Height
             viewRect.offsetMax = new Vector2(0, -200); // Top Bar Height

             // Views
             GameObject gameplayView = CreatePanel(viewContainer.transform, "GameplayView", Color.clear);
             GameObject collectionView = CreatePanel(viewContainer.transform, "CollectionView", new Color(0.1f, 0.1f, 0.2f));
             CreateText(collectionView.transform, "Collection Screen", Vector2.zero);
             
             GameObject labView = CreatePanel(viewContainer.transform, "LabView", new Color(0.1f, 0.2f, 0.1f));
             CreateText(labView.transform, "Lab (Upgrades)", Vector2.zero);
             
             GameObject shopView = CreatePanel(viewContainer.transform, "ShopView", new Color(0.2f, 0.1f, 0.1f));
             CreateText(shopView.transform, "IAP Shop", Vector2.zero);
             
             GameObject leaderboardView = CreatePanel(viewContainer.transform, "LeaderboardView", new Color(0.2f, 0.2f, 0.3f));
             CreateText(leaderboardView.transform, "Leaderboard", Vector2.zero);

             // --- Gameplay Setup ---
             GameObject boardContainer = new GameObject("BoardContainer");
             
             BoardManager boardMgr = managers.AddComponent<BoardManager>();
             SerializedObject soBoard = new SerializedObject(boardMgr);
             soBoard.FindProperty("boardContainer").objectReferenceValue = boardContainer.transform;
             
             // Assign Tile Prefab
             Tile tilePrefab = AssetDatabase.LoadAssetAtPath<Tile>("Assets/Prefabs/Tile.prefab");
             if (tilePrefab) soBoard.FindProperty("tilePrefab").objectReferenceValue = tilePrefab;
             
             soBoard.ApplyModifiedProperties();

             // --- Bottom Bar ---
             GameObject navBar = CreatePanel(canvas.transform, "BottomNavBar", new Color(0.15f, 0.15f, 0.15f));
             SetRect(navBar, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 250), Vector2.zero);
             
             HorizontalLayoutGroup hlg = navBar.AddComponent<HorizontalLayoutGroup>();
             hlg.childControlWidth = true;
             hlg.childForceExpandWidth = true;
             hlg.padding = new RectOffset(10, 10, 10, 10);
             hlg.spacing = 10;

             Button btnLeader = CreateButton(navBar.transform, "Leaderboard", new Color(0.9f, 0.8f, 0.3f));
             Button btnCol = CreateButton(navBar.transform, "Collection", new Color(0.3f, 0.8f, 0.9f));
             Button btnPlay = CreateButton(navBar.transform, "PLAY", Color.green);
             Button btnLab = CreateButton(navBar.transform, "Lab", new Color(0.8f, 0.3f, 0.9f));
             Button btnShop = CreateButton(navBar.transform, "Shop", new Color(0.9f, 0.3f, 0.3f));

             // Link Nav Manager
             navManager.gameplayView = gameplayView;
             navManager.collectionView = collectionView;
             navManager.labView = labView;
             navManager.shopView = shopView;
             navManager.leaderboardView = leaderboardView;

             navManager.playBtn = btnPlay;
             navManager.collectionBtn = btnCol;
             navManager.labBtn = btnLab;
             navManager.shopBtn = btnShop;
             navManager.leaderboardBtn = btnLeader;

             // Add Systems
             if(!managers.GetComponent<Systems.DNASystem>()) managers.AddComponent<Systems.DNASystem>();
             if(!managers.GetComponent<Systems.HeartSystem>()) managers.AddComponent<Systems.HeartSystem>();
             if(!managers.GetComponent<Systems.SaveSystem>()) managers.AddComponent<Systems.SaveSystem>();
             if(!managers.GetComponent<Systems.CollectionSystem>()) managers.AddComponent<Systems.CollectionSystem>();
             if(!managers.GetComponent<Systems.LabSystem>()) managers.AddComponent<Systems.LabSystem>();
             if(!managers.GetComponent<AudioManager>()) managers.AddComponent<AudioManager>();
             
             // Hide others
             collectionView.SetActive(false);
             labView.SetActive(false);
             shopView.SetActive(false);
             leaderboardView.SetActive(false);

             EditorSceneManager.SaveScene(scene, scenePath);
             Debug.Log("Scene Built V1 (Fixes)!");
        }

        private static GameObject CreatePanel(Transform parent, string name, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            Image img = obj.AddComponent<Image>();
            img.color = color;
            return obj;
        }
        
        private static void SetRect(GameObject obj, Vector2 min, Vector2 max, Vector2 pivot, Vector2 size, Vector2 pos)
        {
            RectTransform r = obj.GetComponent<RectTransform>();
            r.anchorMin = min; r.anchorMax = max; r.pivot = pivot;
            r.sizeDelta = size; r.anchoredPosition = pos;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string content, Vector2 offset)
        {
             GameObject obj = new GameObject("Text");
             obj.transform.SetParent(parent, false);
             TextMeshProUGUI txt = obj.AddComponent<TextMeshProUGUI>();
             txt.text = content;
             txt.fontSize = 40;
             txt.alignment = TextAlignmentOptions.Center;
             txt.color = Color.white;
             RectTransform rect = obj.GetComponent<RectTransform>();
             rect.anchoredPosition = offset;
             rect.sizeDelta = new Vector2(400, 100);
             return txt;
        }

        private static Button CreateButton(Transform parent, string label, Color color)
        {
             GameObject obj = new GameObject(label + "Btn");
             obj.transform.SetParent(parent, false);
             Image img = obj.AddComponent<Image>();
             img.color = color;
             Button btn = obj.AddComponent<Button>();
             
             GameObject textObj = new GameObject("Text");
             textObj.transform.SetParent(obj.transform, false);
             TextMeshProUGUI txt = textObj.AddComponent<TextMeshProUGUI>();
             txt.text = label;
             txt.fontSize = 28;
             txt.alignment = TextAlignmentOptions.Center;
             txt.color = Color.black;
             txt.enableWordWrapping = false;
             
             RectTransform textRect = textObj.GetComponent<RectTransform>();
             textRect.anchorMin = Vector2.zero;
             textRect.anchorMax = Vector2.one;
             textRect.offsetMin = Vector2.zero;
             textRect.offsetMax = Vector2.zero;
             
             return btn;
        }
    }
}
#endif