#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
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

        [MenuItem("FaunaFuse/BuildV91 (Event Debug)")]
        public static void BuildScene()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Error", "Cannot build scene while in Play Mode. Please exit Play Mode.", "OK");
                return;
            }

            // AUTO-FIX NAMES & TRIVIA BEFORE BUILDING
            FixAnimalNames();
            UpdateTriviaText();
            SetupUpgrades(); // New: Create default upgrades if missing
            Debug.Log("Names and Trivia updated automatically.");

            try
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
             
             // --- V18 LOGIC: Properly Anchored Top Bar ---
             
             GameObject safeAreaContainer = new GameObject("SafeAreaContainer");
             safeAreaContainer.transform.SetParent(canvasObj.transform, false);
             // Stretch to fill canvas
             RectTransform rt = safeAreaContainer.AddComponent<RectTransform>();
             rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
             rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
             
             // Ensure it has SafeArea component if it exists
             safeAreaContainer.AddComponent<SafeArea>(); 

             // Managers (Scene Specific - Destroyed on Reload)
             GameObject managers = new GameObject("Managers");
             UIManager uiManager = managers.AddComponent<UIManager>();
             NavigationManager navManager = managers.AddComponent<NavigationManager>();
             if(!managers.GetComponent<GameInput.SwipeDetector>()) managers.AddComponent<GameInput.SwipeDetector>();

             // Systems (Global - Persist across scenes)
             // We create separate objects for each system to avoid Singleton Destroy(gameObject) conflicts
             CreateSystem<Systems.DNASystem>("DNASystem");
             CreateSystem<Systems.HeartSystem>("HeartSystem");
             CreateSystem<Systems.CollectionSystem>("CollectionSystem");
             CreateSystem<Systems.LabSystem>("LabSystem");
             CreateSystem<Systems.SaveSystem>("SaveSystem");
             CreateSystem<Systems.DiamondSystem>("DiamondSystem");
             
             // UI ELEMENTS (Parented to SafeAreaContainer)
             // Top Bar Panel (Visual Background)
             GameObject topBar = CreatePanel(safeAreaContainer.transform, "TopBar", new Color(0.15f, 0.15f, 0.2f, 1f));
             SetRect(topBar, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, 250), Vector2.zero);
             
             // NOTE: DNA/Diamond/Heart stack texts are now set up manually in the scene 
             // with Diamond Button, DNA Button, Heart Button hierarchy.
             // See UIManager.cs for the new field names: diamondStackText, dnaStackText, heartStackText
             
             // CURRENT SCORE (Center Bottom - Pushed Up)
             uiManager.scoreText = CreateTextAnchored(topBar.transform, "Score", "0", 
                 new Vector2(0.33f, 0.15f), new Vector2(0.66f, 0.48f), TextAlignmentOptions.Top); 
             // Align TOP so it sticks to the center line
             uiManager.scoreText.fontSize = 24; 

             // HIGH SCORE (Center Top - Pushed Down)
             uiManager.topHighScoreText = CreateTextAnchored(topBar.transform, "HighScore", "High Score: 0", 
                 new Vector2(0.33f, 0.52f), new Vector2(0.66f, 0.85f), TextAlignmentOptions.Bottom); 
             // Align BOTTOM so it sticks to the center line
             uiManager.topHighScoreText.fontSize = 20;
             uiManager.topHighScoreText.color = new Color(0.9f, 0.9f, 0.5f); // Gold tint


             // VIEWS (Panels)
             // 1. Gameplay View (Transparent container for Board)
             GameObject gameplayView = CreatePanel(safeAreaContainer.transform, "GameplayView", new Color(0,0,0,0));
             SetRect(gameplayView, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
             navManager.gameplayView = gameplayView;
             
             // Undo Button (Floating Top Right below Header)
             GameObject undoBtnObj = CreateButton(gameplayView.transform, "UndoBtn", new Color(1f, 0.6f, 0f), roundedSprite).gameObject;
             RectTransform undoRT = undoBtnObj.GetComponent<RectTransform>();
             undoRT.anchorMin = new Vector2(0.80f, 0.82f);
             undoRT.anchorMax = new Vector2(0.95f, 0.87f);
             undoRT.offsetMin = Vector2.zero;
             undoRT.offsetMax = Vector2.zero;
             
             TextMeshProUGUI undoTxt = undoBtnObj.GetComponentInChildren<TextMeshProUGUI>();
             if(undoTxt) { undoTxt.text = "UNDO"; undoTxt.fontSize = 28; }
             
             uiManager.undoButton = undoBtnObj.GetComponent<Button>();

              // Undo Count Text (Below Undo Button)
              uiManager.undoCountText = CreateTextAnchored(gameplayView.transform, "UndoCount", "0/0", 
                  new Vector2(0.80f, 0.77f), new Vector2(0.95f, 0.82f), TextAlignmentOptions.Top);
              uiManager.undoCountText.fontSize = 24;
              uiManager.undoCountText.color = Color.white;
              uiManager.undoCountText.fontStyle = FontStyles.Bold;
             
             // Restart Button (Floating Top Left below Header)
             GameObject restartBtnObj = CreateButton(gameplayView.transform, "RestartBtn", new Color(0.9f, 0.3f, 0.3f), roundedSprite).gameObject;
             RectTransform restartRT = restartBtnObj.GetComponent<RectTransform>();
             restartRT.anchorMin = new Vector2(0.05f, 0.82f);
             restartRT.anchorMax = new Vector2(0.20f, 0.87f);
             restartRT.offsetMin = Vector2.zero;
             restartRT.offsetMax = Vector2.zero;
             
             TextMeshProUGUI restartTxt = restartBtnObj.GetComponentInChildren<TextMeshProUGUI>();
             if(restartTxt) { restartTxt.text = "RESET"; restartTxt.fontSize = 28; }
             
             uiManager.restartButton = restartBtnObj.GetComponent<Button>();

             // 2. Collection View
             GameObject collectionView = CreatePanel(safeAreaContainer.transform, "CollectionView", new Color(0.12f, 0.12f, 0.18f));
             SetRect(collectionView, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
             collectionView.SetActive(false);
             navManager.collectionView = collectionView;
             
             // Attach Component
             UI.CollectionView cvScript = collectionView.AddComponent<UI.CollectionView>();

             // TITLE
             // Moved Flush to Top (was 0.88-0.95, now 0.93-1.0)
             TextMeshProUGUI colTitle = CreateTextAnchored(collectionView.transform, "Title", "COLLECTION",
                 new Vector2(0, 0.93f), new Vector2(1, 1f), TextAlignmentOptions.Center);
             colTitle.fontSize = 64;
             colTitle.fontStyle = FontStyles.Bold;
             colTitle.color = new Color(1f, 0.8f, 0.2f); // Gold-ish
             
             // SCROLL VIEW STRUCTURE
             // ScrollRect
             ScrollRect sr = collectionView.AddComponent<ScrollRect>();
             sr.horizontal = false; sr.vertical = true;
             sr.scrollSensitivity = 25;

             // Viewport (Mask)
             // CRITICAL: Color must have Alpha=1 for Mask to show content! We use white.
             GameObject viewport = CreatePanel(collectionView.transform, "Viewport", Color.white);
             // Adjust anchors to sit BETWEEN Title and NavBar
             SetRect(viewport, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
             RectTransform vpRt = viewport.GetComponent<RectTransform>();
             vpRt.anchorMin = new Vector2(0, 0.12f); // Above Navbar
             vpRt.anchorMax = new Vector2(1, 0.93f); // Below Title
             
             viewport.AddComponent<Mask>().showMaskGraphic = false;
             Image vpImg = viewport.GetComponent<Image>();
             vpImg.maskable = true;

             sr.viewport = vpRt;

             // Content
             GameObject content = CreatePanel(viewport.transform, "Content", new Color(0,0,0,0));
             SetRect(content, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), Vector2.zero, Vector2.zero); // Top Anchor
             
             sr.content = content.GetComponent<RectTransform>();

             // Grid Layout
             GridLayoutGroup glg = content.AddComponent<GridLayoutGroup>();
             glg.cellSize = new Vector2(280, 330); 
             glg.spacing = new Vector2(40, 40);
             // Viewport handles safe area now. Just some margin for aesthetics.
             glg.padding = new RectOffset(80, 80, 40, 40); 
             glg.childAlignment = TextAnchor.UpperCenter; 
             glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
             glg.constraintCount = 3; 
             
             ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
             csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

             cvScript.contentContainer = content.transform;

             // SLOT TEMPLATE (Hidden by default)
             GameObject templateSlot = CreatePanel(content.transform, "SlotTemplate", new Color(0.2f, 0.2f, 0.25f));
             templateSlot.GetComponent<Image>().sprite = roundedSprite;
             templateSlot.GetComponent<Image>().type = Image.Type.Sliced;
             // INTERACTION: Make it a Button
             templateSlot.AddComponent<Button>();
             
             // Icon (Container + Mask)
             GameObject iconObj = CreatePanel(templateSlot.transform, "Icon", Color.white);
             SetRect(iconObj, new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.5f), new Vector2(200, 200), Vector2.zero);
             // Masking Setup
             iconObj.GetComponent<Image>().sprite = roundedSprite; 
             iconObj.AddComponent<Mask>().showMaskGraphic = true; // Show the rounded white shape behind

             // Inner Animal Image (This gets the Sprite)
             GameObject animalImgObj = CreatePanel(iconObj.transform, "AnimalImage", Color.white);
             // Fill Parent
             SetRect(animalImgObj, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
             
             // Name
             TextMeshProUGUI nameTxt = CreateTextAnchored(templateSlot.transform, "Name", "Animal Name",
                 new Vector2(0, 0.2f), new Vector2(1, 0.3f), TextAlignmentOptions.Center);
             nameTxt.fontSize = 28;
             nameTxt.color = Color.white;

             // Level
             TextMeshProUGUI lvlTxt = CreateTextAnchored(templateSlot.transform, "Level", "Lvl 1",
                 new Vector2(0, 0.05f), new Vector2(1, 0.15f), TextAlignmentOptions.Center);
             lvlTxt.fontSize = 20;
             lvlTxt.color = Color.yellow;

             // Auto-Setup CardUI for the new system
             CardUI cardUI = templateSlot.AddComponent<CardUI>();
             cardUI.nameText = nameTxt;
             cardUI.levelText = lvlTxt;
             if(animalImgObj) cardUI.iconImage = animalImgObj.GetComponent<Image>();
             cardUI.startBg = templateSlot.GetComponent<Image>();
             
             cvScript.cardPrefab = cardUI;
             templateSlot.SetActive(false); // Hide template

             // ==========================================================================================
             // DETAIL VIEW (Popup) - REDESIGN V48
             // ==========================================================================================
             // 1. Fullscreen Dimmer (Background)
             GameObject detailPanel = CreatePanel(collectionView.transform, "DetailPanel", new Color(0, 0, 0, 0.85f)); 
             SetRect(detailPanel, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
             // INTERACTION: Click background to close
             Button bgCloseBtn = detailPanel.AddComponent<Button>();
             bgCloseBtn.transition = Selectable.Transition.None;
             detailPanel.SetActive(false); // Hidden by default

             // 2. The Main Card (Modal)
             GameObject detailCard = CreatePanel(detailPanel.transform, "Card", new Color(0.2f, 0.2f, 0.28f)); // Dark Slate Blue
             // Center it, fixed width/height (approx 80% screen width)
             SetRect(detailCard, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(800, 1200), Vector2.zero);
             detailCard.GetComponent<Image>().sprite = roundedSprite;
             detailCard.GetComponent<Image>().type = Image.Type.Sliced;
             
             // 3. Header / Title -> BECOMES ANIMAL NAME
             TextMeshProUGUI headerTxt = CreateTextAnchored(detailCard.transform, "Header", "ANIMAL NAME", 
                 new Vector2(0, 0.88f), new Vector2(1, 0.98f), TextAlignmentOptions.Center);
             headerTxt.fontSize = 52; 
             headerTxt.fontStyle = FontStyles.Bold;
             headerTxt.color = Color.white;

             // 4. Animal Image Container (Top of Card)
             GameObject imageBg = CreatePanel(detailCard.transform, "ImageBg", new Color(0.15f, 0.15f, 0.22f)); 
             SetRect(imageBg, new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f), new Vector2(500, 500), new Vector2(0, 50));
             imageBg.GetComponent<Image>().sprite = roundedSprite;
             // VISUAL FIX: Mask to ensure image is rounded
             imageBg.AddComponent<Mask>().showMaskGraphic = true;
             
             // Actual Image
             GameObject innerBigImg = CreatePanel(imageBg.transform, "DetailImage", Color.white);
             // Slight padding (20px)
             SetRect(innerBigImg, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(20, 20), new Vector2(-20, -20));
             Image dImg = innerBigImg.GetComponent<Image>();
             dImg.preserveAspect = true;

             // 5. Info Section
             // Name Spot -> NOW SHOWS LEVEL
             TextMeshProUGUI levelTxt = CreateTextAnchored(detailCard.transform, "LevelText", "Level 10",
                 new Vector2(0, 0.46f), new Vector2(1, 0.52f), TextAlignmentOptions.Center);
             levelTxt.fontSize = 42; 
             levelTxt.color = new Color(1f, 0.8f, 0.2f); // Gold

             // 6. Trivia (Scrollable) -> EXPANDED UPWARDS & SCROLLABLE
             GameObject triviaBox = CreatePanel(detailCard.transform, "TriviaBox", new Color(0.15f, 0.15f, 0.22f));
             // Anchors: same as before
             SetRect(triviaBox, new Vector2(0.1f, 0.10f), new Vector2(0.9f, 0.44f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
             // REMOVED: triviaBox.GetComponent<Image>().sprite = roundedSprite; // Keep it square (default)
             
             // Add ScrollRect
             ScrollRect scroll = triviaBox.AddComponent<ScrollRect>();
             scroll.horizontal = false;
             scroll.vertical = true;
             scroll.scrollSensitivity = 20f;
             scroll.movementType = ScrollRect.MovementType.Elastic;
 
             // Viewport (Mask)
             GameObject triviaViewport = CreatePanel(triviaBox.transform, "Viewport", Color.clear); 
             // FIX: The Image color must be visible (Alpha 1) for the Mask to work, even if showMaskGraphic is false.
             // If Color is Clear (Alpha 0), the mask hides everything!
             Image tVpImg = triviaViewport.GetComponent<Image>();
             tVpImg.color = Color.white; 
             // REMOVED: tVpImg.sprite = roundedSprite; // Keep mask square too
             
             // Full stretch (match parent exactly for now to avoid offset confusion)
             SetRect(triviaViewport, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
             
             Mask vpMask = triviaViewport.AddComponent<Mask>();
             vpMask.showMaskGraphic = false; // Hide the white image, but use it for masking

             // Content
             GameObject triviaContent = CreatePanel(triviaViewport.transform, "Content", Color.clear);
             // Top-Stretch anchor
             SetRect(triviaContent, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), Vector2.zero, Vector2.zero); 
             // IMPORTANT: ContentSizeFitter for auto height
             ContentSizeFitter tCsf = triviaContent.AddComponent<ContentSizeFitter>();
             tCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
             
             // Vertical Layout Group for padding
             VerticalLayoutGroup vlg = triviaContent.AddComponent<VerticalLayoutGroup>();
             vlg.childControlHeight = true;
             vlg.childControlWidth = true;
             vlg.childForceExpandHeight = false; // Don't force height, let text dictate it
             vlg.childForceExpandWidth = true;
             vlg.padding = new RectOffset(20, 20, 10, 10); // Move padding here

             // Assign ScrollRect references
             scroll.viewport = triviaViewport.GetComponent<RectTransform>();
             scroll.content = triviaContent.GetComponent<RectTransform>();

             // Trivia Text inside Content
             GameObject textObj = new GameObject("TriviaText");
             textObj.transform.SetParent(triviaContent.transform, false);
             
             // Reset Rect to be safe (Layout Group will override, but good practice)
             RectTransform textRT = textObj.AddComponent<RectTransform>();
             textRT.anchorMin = Vector2.zero; textRT.anchorMax = Vector2.one;
             textRT.sizeDelta = Vector2.zero;
             
             TextMeshProUGUI dTrivia = textObj.AddComponent<TextMeshProUGUI>();
             dTrivia.text = "Loading data...";
             dTrivia.fontSize = 32; // Slightly larger
             dTrivia.color = Color.white; // Pure white for visibility
             dTrivia.alignment = TextAlignmentOptions.TopLeft;
             dTrivia.textWrappingMode = TextWrappingModes.Normal;
             dTrivia.overflowMode = TextOverflowModes.Overflow;
             
             // IMPORTANT: Force the Text object to conform to Layout Group
             // We don't need to set anchors because VerticalLayoutGroup handles it.
             // But we DO need to ensure width matches parent so wrapping works.
             
             // Update Build Menu Name
             // Already V52, but let's keep it to ensure it rebuilds correctly with logic fix.

             // ASSIGN REFERENCES
             // Use background button as the Close Button
             cvScript.detailPanel = detailPanel;
             cvScript.detailImage = dImg;
             cvScript.detailName = headerTxt; // Header is now Name
             cvScript.detailLevel = levelTxt; // Old Name spot is now Level
             cvScript.detailTrivia = dTrivia;
             cvScript.closeButton = bgCloseBtn;
             
             // 3. Lab View
             GameObject labView = CreatePanel(safeAreaContainer.transform, "LabView", new Color(0.12f, 0.12f, 0.15f)); // Dark background
             SetRect(labView, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
             labView.SetActive(false);
             navManager.labView = labView;

             // Add LabView Component
             UI.LabView labScript = labView.AddComponent<UI.LabView>();

             // Lab Header
             TextMeshProUGUI labTitle = CreateTextAnchored(labView.transform, "Title", "DNA LAB", new Vector2(0.3f, 0.92f), new Vector2(0.7f, 1), TextAlignmentOptions.Center);
             labTitle.fontSize = 52;
             labTitle.fontStyle = FontStyles.Bold;
             
             // Lab DNA Display (Top Right)
             TextMeshProUGUI labDna = CreateTextAnchored(labView.transform, "LabDNA", "DNA: ...", new Vector2(0.6f, 0.96f), new Vector2(0.95f, 1f), TextAlignmentOptions.Right);
             labDna.fontSize = 30;
             labDna.color = Color.green;
             labScript.dnaText = labDna;
             
             // Lab Diamond Display (Below DNA)
             TextMeshProUGUI labDiamond = CreateTextAnchored(labView.transform, "LabDiamond", "Diamond: 0", new Vector2(0.6f, 0.92f), new Vector2(0.95f, 0.96f), TextAlignmentOptions.Right);
             labDiamond.fontSize = 26;
             labDiamond.color = new Color(0.4f, 0.8f, 1f); // Cyan
             labScript.diamondText = labDiamond;
             
             // TEST DNA BUTTON (Top Left)
             GameObject testBtn = CreateButton(labView.transform, "TestDNA", Color.cyan, roundedSprite).gameObject;
             SetRect(testBtn, new Vector2(0.05f, 0.96f), new Vector2(0.22f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
             TextMeshProUGUI tTxt = testBtn.GetComponentInChildren<TextMeshProUGUI>();
             if(tTxt) { tTxt.text = "+100k"; tTxt.fontSize = 20; }
             labScript.testDnaButton = testBtn.GetComponent<Button>();
             
             // TEST DIAMOND BUTTON (Below DNA button)
             GameObject testDiamondBtn = CreateButton(labView.transform, "TestDiamond", new Color(0.4f, 0.8f, 1f), roundedSprite).gameObject;
             SetRect(testDiamondBtn, new Vector2(0.05f, 0.92f), new Vector2(0.22f, 0.96f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
             TextMeshProUGUI tdTxt = testDiamondBtn.GetComponentInChildren<TextMeshProUGUI>();
             if(tdTxt) { tdTxt.text = "+100 D"; tdTxt.fontSize = 18; }
             labScript.testDiamondButton = testDiamondBtn.GetComponent<Button>();
             
             // Scroll View Container
             GameObject labScrollObj = CreatePanel(labView.transform, "Scroll View", Color.clear);
             SetRect(labScrollObj, new Vector2(0.05f, 0), new Vector2(0.95f, 0.90f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
             ScrollRect labScroll = labScrollObj.AddComponent<ScrollRect>();
             labScroll.horizontal = false;
             labScroll.vertical = true;
             labScroll.scrollSensitivity = 20;

             // Viewport
             GameObject labViewport = CreatePanel(labScrollObj.transform, "Viewport", Color.clear);
             SetRect(labViewport, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
             labViewport.AddComponent<Mask>().showMaskGraphic = false;
             Image labVpImg = labViewport.GetComponent<Image>();
             labVpImg.color = Color.white; 

             // Content
             GameObject labContent = CreatePanel(labViewport.transform, "Content", Color.clear);
             SetRect(labContent, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), Vector2.zero, Vector2.zero);
             ContentSizeFitter labCsf = labContent.AddComponent<ContentSizeFitter>();
             labCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
             VerticalLayoutGroup labVlg = labContent.AddComponent<VerticalLayoutGroup>();
             labVlg.childControlHeight = false; // We set height manually
             labVlg.childControlWidth = true;
             labVlg.childForceExpandHeight = false;
             labVlg.spacing = 30;
             labVlg.padding = new RectOffset(0, 0, 20, 100); // Bottom padding for navbar

             labScroll.viewport = labViewport.GetComponent<RectTransform>();
             labScroll.content = labContent.GetComponent<RectTransform>();
             labScript.contentContainer = labContent.GetComponent<RectTransform>();

             // Note: New LabView creates upgrade cards dynamically, no template needed

             // 4. Shop View
             GameObject shopView = CreatePanel(safeAreaContainer.transform, "ShopView", new Color(0.2f, 0.1f, 0.1f));
             SetRect(shopView, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
             shopView.SetActive(false);
             navManager.shopView = shopView;
             
             // 5. Leaderboard View
             GameObject leaderboardView = CreatePanel(safeAreaContainer.transform, "LeaderboardView", new Color(0.1f, 0.15f, 0.2f));
             SetRect(leaderboardView, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
             leaderboardView.SetActive(false);
             navManager.leaderboardView = leaderboardView;

             // Board (World Space - Not parented to Canvas)
             GameObject boardContainer = new GameObject("BoardContainer");
             // boardContainer.transform.SetParent(gameplayView.transform, false); // REMOVED: Sprites don't render in Overlay Canvas
             
             // Link to Navigation Manager so it toggles with GameplayView
             navManager.mainBoard = boardContainer;
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

             // Footer - Polished
             GameObject navBar = CreatePanel(safeAreaContainer.transform, "BottomNavBar", new Color(0.15f, 0.15f, 0.2f));
             SetRect(navBar, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 180), Vector2.zero);
             
             HorizontalLayoutGroup hlg = navBar.AddComponent<HorizontalLayoutGroup>();
             hlg.childAlignment = TextAnchor.MiddleCenter;
             hlg.padding = new RectOffset(0, 0, 0, 0); // User Request: 0 Padding
             hlg.spacing = 0; // User Request: 0 Spacing
             hlg.childControlWidth = true;
             hlg.childControlHeight = true;
             hlg.childForceExpandWidth = true; // User Request: Checked
             hlg.childForceExpandHeight = true; // User Request: Checked

             // Load Nav Sprites
             Sprite rankSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/leaderboardbutton.png");
             Sprite dexSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/collectionbutton.png");
             Sprite playSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/playbutton.png");
             Sprite labSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/researchlabbutton.png");
             Sprite shopSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/shopbutton.png");

             // Create Buttons with Sliced/Rounded Sprite & Assign to Manager
             navManager.leaderboardBtn = CreateButton(navBar.transform, "Rank", Color.white, rankSpr ? rankSpr : roundedSprite);
             navManager.collectionBtn = CreateButton(navBar.transform, "Dex", Color.white, dexSpr ? dexSpr : roundedSprite);
             navManager.playBtn = CreateButton(navBar.transform, "PLAY", Color.white, playSpr ? playSpr : roundedSprite);
             navManager.labBtn = CreateButton(navBar.transform, "Lab", Color.white, labSpr ? labSpr : roundedSprite);
             navManager.shopBtn = CreateButton(navBar.transform, "Shop", Color.white, shopSpr ? shopSpr : roundedSprite);

             // Post-process Buttons (Make Square & Remove Text if using custom sprite)
             void FixNavBtn(Button btn, Sprite s)
             {
                 if (btn)
                 {
                     LayoutElement le = btn.GetComponent<LayoutElement>();
                     if (le) { le.preferredWidth = 140; le.preferredHeight = 140; }
                     
                     if (s != roundedSprite && s != null)
                     {
                         // Using custom art: Clear text and set Image Type to Simple
                         TextMeshProUGUI txt = btn.GetComponentInChildren<TextMeshProUGUI>();
                         if (txt) txt.text = "";
                         
                         Image img = btn.GetComponent<Image>();
                         if (img) img.type = Image.Type.Simple; // Avoid slicing artifacts
                     }
                 }
             }

             FixNavBtn(navManager.leaderboardBtn, rankSpr);
             FixNavBtn(navManager.collectionBtn, dexSpr);
             FixNavBtn(navManager.playBtn, playSpr);
             FixNavBtn(navManager.labBtn, labSpr);
             FixNavBtn(navManager.shopBtn, shopSpr);
             // Note: GameplayView etc are already assigned above
             
             // GAME OVER PANEL (Overlay)
             GameObject goPanel = CreatePanel(safeAreaContainer.transform, "GameOverPanel", new Color(0,0,0,0.85f));
             SetRect(goPanel, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
             goPanel.SetActive(false); // Hidden by default
             
             // CARD (Popup Container)
             GameObject card = new GameObject("Card");
             card.transform.SetParent(goPanel.transform, false);
             Image cardImg = card.AddComponent<Image>();
             cardImg.sprite = roundedSprite;
             cardImg.color = new Color(0.2f, 0.22f, 0.28f, 1f); // Dark Blue-Grey Theme
             cardImg.type = Image.Type.Sliced; // Attempt sliced, fallback to Simple if not setup
             
             // Card Layout: Center, Width 85%, Height 60% approx
             RectTransform cardRt = card.GetComponent<RectTransform>();
             cardRt.anchorMin = new Vector2(0.5f, 0.5f);
             cardRt.anchorMax = new Vector2(0.5f, 0.5f);
             cardRt.pivot = new Vector2(0.5f, 0.5f);
             cardRt.sizeDelta = new Vector2(850, 1100); // Fixed resolution size for 1080p width reference
             cardRt.anchoredPosition = Vector2.zero;

             // Title: "GAME OVER" - BIGGER
             var title = CreateTextAnchored(card.transform, "Title", "GAME OVER", 
                 new Vector2(0, 0.82f), new Vector2(1, 0.95f), TextAlignmentOptions.Center);
             title.fontSize = 90; // Increased from 72
             title.fontStyle = FontStyles.Bold;
             title.color = new Color(1f, 0.4f, 0.4f); // Soft Red

             // Final Score Label
             var scoreLabel = CreateTextAnchored(card.transform, "ScoreLabel", "SCORE", 
                 new Vector2(0, 0.72f), new Vector2(1, 0.78f), TextAlignmentOptions.Center);
             scoreLabel.fontSize = 32;
             scoreLabel.color = new Color(0.7f, 0.7f, 0.8f); // Light Grey

             // Final Score Value (Big but smaller than title)
             uiManager.finalScoreText = CreateTextAnchored(card.transform, "FinalScore", "0", 
                 new Vector2(0, 0.60f), new Vector2(1, 0.72f), TextAlignmentOptions.Center);
             uiManager.finalScoreText.fontSize = 80; // Decreased from 110
             uiManager.finalScoreText.fontStyle = FontStyles.Bold;
             uiManager.finalScoreText.color = Color.white;
             
             // Separator Line
             GameObject sep = CreatePanel(card.transform, "Separator", new Color(1,1,1,0.1f));
             // Moved to below title (Title Bottom is 0.82, Score Label Top is 0.78) used 0.80 center
             SetRect(sep, new Vector2(0.1f, 0.80f), new Vector2(0.9f, 0.805f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

             // Stats Area (High Score & DNA)
             // High Score
             uiManager.highScoreText = CreateTextAnchored(card.transform, "HighScore", "Best: 1234", 
                 new Vector2(0.1f, 0.45f), new Vector2(0.9f, 0.52f), TextAlignmentOptions.Center);
             uiManager.highScoreText.fontSize = 40;
             uiManager.highScoreText.color = new Color(0.9f, 0.9f, 0.5f); // Gold tint

             // Stats Grid (DNA)
             // Earned DNA
             uiManager.earnedDnaText = CreateTextAnchored(card.transform, "EarnedDNA", "+10 DNA", 
                 new Vector2(0.1f, 0.35f), new Vector2(0.5f, 0.42f), TextAlignmentOptions.Right);
             uiManager.earnedDnaText.fontSize = 32;
             uiManager.earnedDnaText.color = Color.green;

             // Total DNA
             uiManager.totalDnaText = CreateTextAnchored(card.transform, "TotalDNA", "Total: 500", 
                 new Vector2(0.55f, 0.35f), new Vector2(0.9f, 0.42f), TextAlignmentOptions.Left);
             uiManager.totalDnaText.fontSize = 32;
             uiManager.totalDnaText.color = Color.white;

             // Try Again Button (Rounded)
             Button btn = CreateButton(card.transform, "TryAgain", new Color(0.3f, 0.8f, 0.4f), roundedSprite);
             GameObject btnObj = btn.gameObject;
             
             // Apply Rounded Sprite to Button
             Image btnImg = btnObj.GetComponent<Image>();
             if(btnImg) {
                 btnImg.sprite = roundedSprite;
                 btnImg.type = Image.Type.Sliced;
             }

             // Bottom area
             SetRect(btnObj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(500, 140), new Vector2(0, -350));
             
             TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
             btnText.text = "TRY AGAIN";
             btnText.fontSize = 44; // Slightly smaller to breathe
             btnText.fontStyle = FontStyles.Bold;
             btnText.color = Color.white;
             // Ensure perfect centering
             btnText.alignment = TextAlignmentOptions.Center; 
             
             uiManager.tryAgainButton = btn;
             uiManager.gameOverPanel = goPanel;
             
             // Event System (REQUIRED for Buttons)
             if (!Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>())
             {
                 GameObject eventSystem = new GameObject("EventSystem");
                 eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                 // FIX: Use New Input System Module instead of StandaloneInputModule
                 eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
             }

             EditorSceneManager.SaveScene(scene, scenePath);
             Debug.Log("Scene Built V25 (Input System Fix)");
            }
            catch(System.Exception e)
            {
                Debug.LogError("FATAL SCENE BUILD ERROR: " + e.ToString());
            }
        }
        
        // Helpers
        private static GameObject CreatePanel(Transform p, string n, Color c) { 
            GameObject o = new GameObject(n); o.transform.SetParent(p,false); 
            o.AddComponent<Image>().color=c; 
            return o; 
        }
        private static void CreateSystem<T>(string name) where T : Component
        {
            GameObject existing = GameObject.Find(name);
            if (existing) GameObject.DestroyImmediate(existing);
            
            GameObject go = new GameObject(name);
            go.AddComponent<T>();
        }

        private static Button CreateButton(Transform p, string l, Color c, Sprite s = null) {
             GameObject o = new GameObject(l+"Btn"); o.transform.SetParent(p,false);
             Image img = o.AddComponent<Image>(); 
             img.color = c;
             if(s != null) { img.sprite = s; img.type = Image.Type.Sliced; } // Support Rounded Sprite
             
             o.AddComponent<LayoutElement>().minHeight=80;
             // Add button component
             Button btn = o.AddComponent<Button>();
             
             GameObject t = new GameObject("Text"); t.transform.SetParent(o.transform,false);
             // Stretch text to fill button
             RectTransform rt = t.AddComponent<RectTransform>();
             rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; 
             rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
             
             TextMeshProUGUI tmp = t.AddComponent<TextMeshProUGUI>(); 
             tmp.text=l; 
             tmp.color=Color.black; 
             tmp.alignment=TextAlignmentOptions.Center;
             tmp.fontSize = 24;
             tmp.fontStyle = FontStyles.Bold;
             
             return btn;
        }

        // Updated CreateTextAnchored
        private static TextMeshProUGUI CreateTextAnchored(Transform p, string name, string t, Vector2 min, Vector2 max, TextAlignmentOptions align) {
            GameObject o = new GameObject(name); o.transform.SetParent(p,false);
            TextMeshProUGUI tmp = o.AddComponent<TextMeshProUGUI>(); 
            tmp.text=t; 
            tmp.color=Color.white; 
            tmp.alignment=align;
            
            RectTransform r = o.GetComponent<RectTransform>();
            r.anchorMin = min; 
            r.anchorMax = max; 
            r.offsetMin = Vector2.zero; 
            r.offsetMax = Vector2.zero;
            return tmp;
        }
        private static void SetRect(GameObject o, Vector2 min, Vector2 max, Vector2 pv, Vector2 sz, Vector2 pos) {
            RectTransform r=o.GetComponent<RectTransform>(); r.anchorMin=min; r.anchorMax=max; r.pivot=pv; r.sizeDelta=sz; r.anchoredPosition=pos;
        }
        [MenuItem("FaunaFuse/Utils/Fix Animal Names")]
        public static void FixAnimalNames()
        {
            try
            {
                // Find all AnimalSO
                string[] guids = AssetDatabase.FindAssets("t:AnimalSO");
                int fixedCount = 0;

                // Translation Dictionary (Turkish -> English)
                var translations = new System.Collections.Generic.Dictionary<string, string>() {
                    { "KELEBEK", "Butterfly" },
                    { "BALIK", "Fish" },
                    { "SINCAP", "Squirrel" },
                    { "TAVSAN", "Rabbit" },
                    { "KEDI", "Cat" },
                    { "KOPEK", "Dog" },
                    { "KUS", "Bird" },
                    { "AT", "Horse" },
                    { "FIL", "Elephant" },
                    { "ZURAFA", "Giraffe" },
                    { "ASLAN", "Lion" },
                    { "KAPLAN", "Tiger" },
                    { "AYI", "Bear" },
                    { "MAYMUN", "Monkey" },
                    { "PANDA", "Panda" },
                    { "KURBAGA", "Frog" },
                    { "YILAN", "Snake" },
                    { "SAMUR", "Otter" },
                    { "SAMURU", "Otter" }, // Fixed key for 'su_samuru'
                    { "SU SAMURU", "Otter" },
                    { "KAPLUMBAGA", "Turtle" },
                    { "GEYIK", "Deer" },
                    { "GORIL", "Gorilla" },
                    { "BAYKUS", "Owl" },
                    { "TILKI", "Fox" },
                    { "DOMUZ", "Pig" },
                    { "YUNUS", "Dolphin" },
                    { "KURT", "Wolf" },
                    { "KARTAL", "Eagle" }
                };

                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    Core.AnimalSO animal = AssetDatabase.LoadAssetAtPath<Core.AnimalSO>(path);
                    
                    if (animal != null && animal.icon != null)
                    {
                        string oldName = animal.animalName;
                        string rawName = animal.icon.name; // e.g. "1_Kelebek_0"

                        // 1. Clean up: Remove numbers, underscores, "Icon"
                        // Regex: Replace [0-9] and [_] with space, then trim
                        string cleaner = System.Text.RegularExpressions.Regex.Replace(rawName, @"[\d_]", " ").Trim();
                        // Remove "Icon" if present
                        cleaner = cleaner.Replace("Icon", "", System.StringComparison.OrdinalIgnoreCase).Trim();

                        // 2. Translate common words
                        // Split by space to find the main word (e.g. "Kelebek" from "1 Kelebek 0")
                        string[] parts = cleaner.Split(new char[]{' '}, System.StringSplitOptions.RemoveEmptyEntries);
                        string finalName = "";

                        if (parts.Length > 0)
                        {
                            // Take the longest part as the likely name (avoids "0" or "1")
                            string mainWord = parts[0];
                            foreach(var p in parts) if(p.Length > mainWord.Length) mainWord = p;

                            string upperWord = mainWord.ToUpperInvariant();
                            if (translations.ContainsKey(upperWord))
                            {
                                finalName = translations[upperWord];
                            }
                            else
                            {
                                // Fallback: Title Case the main word
                                finalName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(mainWord.ToLower());
                            }
                        }
                        else
                        {
                            finalName = "Unknown";
                        }

                        if (oldName != finalName)
                        {
                            animal.animalName = finalName;
                            EditorUtility.SetDirty(animal);
                            Debug.Log($"Refactored Animal {animal.level}: '{oldName}' -> '{finalName}' (from '{rawName}')");
                            fixedCount++;
                        }
                    }
                }
                
                if (fixedCount > 0)
                {
                    AssetDatabase.SaveAssets();
                    Debug.Log($"<color=green>SUCCESS: Update & Translated {fixedCount} animal names!</color>");
                }
                else
                {
                    Debug.Log("No animal updates needed.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error fixing names: " + e.Message);
            }
        }

        [MenuItem("FaunaFuse/Utils/Update Trivia Text")]
        public static void UpdateTriviaText()
        {
            var animals = AssetDatabase.FindAssets("t:AnimalSO");
            int count = 0;

            // Mapping: English Name (from SO) -> English Trivia
            Dictionary<string, string> triviaMap = new Dictionary<string, string>() {
                { "Butterfly", "Butterflies taste food with their feet to find suitable plants for eggs. Their compound eyes can see ultraviolet (UV) light invisible to humans. Their wings are actually transparent; the colors come from microscopic scales reflecting light. Monarchs undertake a multi-generational migration of over 4,000 km." },
                { "Fish", "Goldfish do not have a 3-second memory; they can remember things for months and recognize their owners. They don't have eyelids, so they sleep with their eyes open by staying still. In proper conditions, they can live up to 40 years, making them one of the longest-lived aquarium pets." },
                { "Squirrel", "Squirrels forget where they bury millions of nuts, accidentally planting countless trees. They can survive falls from almost any height by using their tail for balance and air resistance. They also pretend to bury nuts to fool thieves watching them." },
                { "Rabbit", "Rabbits eat their own droppings (cecotropes) to absorb missed nutrients. They have panoramic vision to spot predators but have a blind spot right in front of their nose. When sensing danger, they thump their powerful hind legs on the ground to warn others." },
                { "Bird", "Budgies can mimic human speech and hold the record for the largest vocabulary (over 1,700 words) of any bird. The color of the cere (nose area) usually indicates their gender: blue for males, brown/white for females. They are highly social and live in large flocks." },
                { "Frog", "Frogs drink water through their skin via osmosis rather than their mouths. They use their eyeballs to help swallow food by retracting them into their head to push food down. Their tongues can snap out at prey faster than the blink of an eye." },
                { "Snake", "Cobras spread their neck ribs to form a hood as a threat display. King Cobras are the only snakes that build nests for their eggs. Some species can spit venom accurately into the eyes of a threat, causing temporary blindness." },
                { "Otter", "Sea otters hold hands while sleeping to keep from drifting apart. They have the densest fur of any animal (up to 1 million hairs per sq inch) to stay warm. They are one of the few animals that use tools, using rocks to crack open clams." },
                { "Turtle", "Sea turtles use the Earth's magnetic field to navigate thousands of miles back to their birth beach. Unlike cartoons, turtles cannot leave their shells; the shell is fused to their spine. Some species can absorb oxygen through their cloaca (butt) to stay submerged longer." },
                { "Panda", "Pandas have a digestive system designed for meat, yet they eat 99% bamboo, requiring them to eat up to 38kg daily. They have a 'false thumb' (a modified wrist bone) to help grasp bamboo stalks. Newborns are 1/900th the size of their mother." },
                { "Horse", "Horses can sleep standing up to be ready to flee predators, thanks to a 'stay apparatus' in their legs. They have near 360-degree vision. Unlike humans, horses physically cannot vomit due to a strong valve in their stomach." },
                { "Deer", "Antlers are the fastest-growing bone tissue in the world. While herbivores, deer have been seen chewing on bones for calcium. A buck's antler size is a strong indicator of its health and genetic quality to potential mates." },
                { "Giraffe", "Giraffes have the same number of neck vertebrae as humans (seven), they are just huge. Their tongues are 45-50cm long and dark purple to prevent sunburn while eating. They have incredibly high blood pressure to pump blood up to their heads." },
                { "Gorilla", "Gorillas share about 98% of their DNA with humans. Mature males develop silver hair on their backs, hence 'Silverbacks'. They express emotions and intentions through over 25 distinct vocalizations and the iconic chest-beating display." },
                { "Elephant", "Elephants have over 40,000 muscles in their trunks, allowing them to pick up a single peanut. Their large ears act as radiators to cool their blood. They are known for their incredible memory and deep social bonds, even mourning their dead." },
                { "Cat", "A cat's whiskers are navigation tools that detect air currents and width changes. Purring isn't just for happiness; the frequency promotes bone density and healing. Adult cats only meow to communicate with humans, not other cats." },
                { "Owl", "Owls fly silently thanks to specialized fringed feathers that break up turbulence. Their eyes are tube-shaped and fixed in place, so they must rotate their heads up to 270 degrees to look around. Their asymmetrical ears allow 3D audio triangulation." },
                { "Fox", "Foxes use the Earth's magnetic field to judge distance when pouncing on prey in snow. Their bushy tails aid balance and act as a warm blanket in winter. They share traits with cats, such as retractable claws and vertical pupils." },
                { "Pig", "Pigs are highly intelligent, outperforming dogs in some cognitive tests. They are actually very clean; they wallow in mud only to cool down (no sweat glands) and for sun protection. Their squeals can be louder than a jet engine (115 dB)." },
                { "Dog", "A dog's nose print is as unique as a human fingerprint. Their sense of smell is 10,000-100,000 times more sensitive than ours, smelling in '3D'. The 'puppy dog eyes' look is an evolutionary adaptation to communicate with humans." },
                { "Dolphin", "Dolphins shut down only half their brain at a time to sleep, staying alert for breathing and danger. They have unique 'signature whistles' that function like names. They use echolocation (biological sonar) to 'see' with sound." },
                { "Wolf", "Wolves howl to assemble the pack and define territory; the harmonics creating an illusion of a larger pack. They are endurance hunters, traveling 50+ miles a day. All wolf pups are born with blue eyes which change color later." },
                { "Bear", "During hibernation, bears do not urinate; their bodies recycle waste into protein to maintain muscle. Despite their bulk, they can sprint faster than Usain Bolt (up to 55 km/h). Polar bears can smell a seal from 30 km away." },
                { "Eagle", "Eagles have vision 4-8 times sharper than humans, spotting prey from miles away. They build the largest tree nests of any bird, some weighing over a ton. They have a second eyelid (nictitating membrane) to protect their eyes." },
                { "Lion", "Lions live in savannas, not jungles. Females do 90% of the hunting, while males protect the pride's territory. A lion's roar is the loudest of any big cat and can be heard from 8 kilometers (5 miles) away." }
            };

            foreach (var guid in animals)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AnimalSO animal = AssetDatabase.LoadAssetAtPath<AnimalSO>(path);

                if (animal != null)
                {
                    if (triviaMap.ContainsKey(animal.animalName))
                    {
                        animal.trivia = triviaMap[animal.animalName];
                        EditorUtility.SetDirty(animal);
                        count++;
                    }
                }
            }
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Trivia Update", $"Updated trivia for {count} animals.", "OK");
        }
        
        [MenuItem("FaunaFuse/Utils/Setup Upgrades")]
        public static void SetupUpgrades()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Upgrades"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
                AssetDatabase.CreateFolder("Assets/Resources", "Upgrades");
            }

            // Undo: 5 levels, cost = 150 × 2^level, research = 15min × 2^level (15m, 30m, 1h, 2h, 4h)
            CreateUpgradeWithTime("Use of Undo", Core.UpgradeType.Undo, "Increases max undo charges.", 150, 2f, 5, 900f, 2f);
            // Heart Recovery: 25 levels, cost = 50 × 1.18^level, research = 0.74 × 1.346^level minutes
            CreateUpgradeWithTime("Heart Recovery", Core.UpgradeType.HeartRefill, "Reduces time to regenerate hearts.", 50, 1.18f, 25, 44.4f, 1.346f);
            // DNA Multiplier: 100 levels, +1% DNA per level, cost = 25 × 1.05^level, research = 60s × 1.0744^level (1min to 24hr)
            CreateUpgradeWithTime("DNA Multiplier", Core.UpgradeType.ExtraDNA, "Earn +1% more DNA per level.", 25, 1.05f, 100, 60f, 1.0744f);
            
            AssetDatabase.SaveAssets();
            
            // Delete old Starter Pack asset if exists
            string oldPath = "Assets/Resources/Upgrades/StarterPack.asset";
            if (AssetDatabase.LoadAssetAtPath<Core.UpgradeSO>(oldPath) != null)
            {
                AssetDatabase.DeleteAsset(oldPath);
            }
        }

        private static void CreateUpgradeWithTime(string name, Core.UpgradeType type, string desc, int cost, float costMult, int max, float baseTime, float timeMult)
        {
            string path = $"Assets/Resources/Upgrades/{name.Replace(" ", "")}.asset";
            Core.UpgradeSO so = AssetDatabase.LoadAssetAtPath<Core.UpgradeSO>(path);
            bool isNew = (so == null);
            if (isNew)
            {
                so = ScriptableObject.CreateInstance<Core.UpgradeSO>();
            }

            so.upgradeName = name;
            so.type = type;
            so.description = desc;
            so.baseCost = cost;
            so.costMultiplier = costMult;
            so.maxLevel = max;
            so.baseUpgradeTime = baseTime;
            so.upgradeTimeMultiplier = timeMult;
            
            // Clear custom array so formula is used
            so.upgradeTimesPerLevel = null;

            if (isNew) AssetDatabase.CreateAsset(so, path);
            else EditorUtility.SetDirty(so);
        }

        private static void CreateUpgradeRaw(string name, Core.UpgradeType type, string desc, int cost, float mult, int max)
        {
            string path = $"Assets/Resources/Upgrades/{name.Replace(" ", "")}.asset";
            // Ensure the type is fully qualified if needed or namespace is imported
            // Core.UpgradeSO is correct as per file imports
            Core.UpgradeSO so = AssetDatabase.LoadAssetAtPath<Core.UpgradeSO>(path);
            bool isNew = (so == null);
            if (isNew)
            {
                so = ScriptableObject.CreateInstance<Core.UpgradeSO>();
            }

            // Always update values to ensure integrity
            so.upgradeName = name;
            so.type = type;
            // so.description = desc; // Preserve description if edited manually? No, force consistency.
            so.description = desc;
            so.baseCost = cost;
            so.costMultiplier = mult;
            so.maxLevel = max;

            if (isNew) AssetDatabase.CreateAsset(so, path);
            else EditorUtility.SetDirty(so);
        }
    }
}
#endif