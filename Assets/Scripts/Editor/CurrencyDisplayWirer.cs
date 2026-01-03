using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class CurrencyDisplayWirer : MonoBehaviour
{
    [MenuItem("Tools/Wire Currency Displays")]
    public static void WireDisplays()
    {
        var uiManager = FindObjectOfType<UI.UIManager>();
        if (!uiManager)
        {
            Debug.LogError("No UIManager found!");
            return;
        }

        // Load Sprites
        Sprite heartSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/heartbutton.png");
        Sprite dnaSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/dnabutton.png");
        Sprite diamondSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/diamondbutton.png");

        if (!heartSprite || !dnaSprite || !diamondSprite)
        {
            Debug.LogError("Missing sprites! Check Assets/Art/ paths.");
            Debug.Log($"Heart: {heartSprite}, DNA: {dnaSprite}, Diamond: {diamondSprite}");
            return;
        }

        // Configure Each
        ConfigureDisplay("HeartDisplay", uiManager.heartText, heartSprite);
        ConfigureDisplay("DNADisplay", uiManager.dnaText, dnaSprite);
        ConfigureDisplay("DiamondDisplay", uiManager.diamondText, diamondSprite);

        Debug.Log("Currency Displays Wired Successfully!");
    }

    private static void ConfigureDisplay(string name, TextMeshProUGUI textComp, Sprite icon)
    {
        if (textComp == null)
        {
            Debug.LogWarning($"{name}: Text component is null in UIManager.");
            return;
        }

        GameObject textObj = textComp.gameObject;
        Transform originalParent = textObj.transform.parent;
        int originalSiblingIndex = textObj.transform.GetSiblingIndex();

        // Check if already wrapped
        if (originalParent.name == name)
        {
            Debug.Log($"{name} already seems configured. Updating Sprite only.");
            var img = originalParent.GetComponent<Image>();
            if (img) img.sprite = icon;
            return;
        }

        // Create Container
        GameObject container = new GameObject(name);
        container.transform.SetParent(originalParent, false);
        container.transform.SetSiblingIndex(originalSiblingIndex);

        // Add Image
        Image image = container.AddComponent<Image>();
        image.sprite = icon;
        image.preserveAspect = true; // IMPORTANT for buttons

        // Sizing - standard button size
        RectTransform rect = container.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(100, 100); 

        // Reparent Text
        textObj.transform.SetParent(container.transform, false);
        
        // CLEANUP: Remove any interfering layout components on the Text
        var fitter = textObj.GetComponent<ContentSizeFitter>();
        if (fitter) Object.DestroyImmediate(fitter);
        var layoutEle = textObj.GetComponent<LayoutElement>();
        if (layoutEle) Object.DestroyImmediate(layoutEle);

        // RESET Transform basics
        textObj.transform.localScale = Vector3.one;
        textObj.transform.localPosition = Vector3.zero;
        textObj.transform.localRotation = Quaternion.identity;

        // Setup Text Layout - EMBEDDED INSIDE (Bottom Center)
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        
        // Anchor to bottom 35% of the button
        textRect.anchorMin = new Vector2(0.05f, 0.05f); 
        textRect.anchorMax = new Vector2(0.95f, 0.40f); 
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.offsetMin = Vector2.zero; // Snap to anchors
        textRect.offsetMax = Vector2.zero; // Snap to anchors
        
        textComp.alignment = TextAlignmentOptions.Center; // Vertically and Horizontally centered in that bottom strip
        textComp.enableAutoSizing = true;
        textComp.fontSizeMin = 8;
        textComp.fontSizeMax = 40; // Allow it to go quite big if space permits
        textComp.color = Color.white; 
        textComp.text = "0"; 

        // Add Outline for readability
        var outline = textObj.GetComponent<UnityEngine.UI.Outline>();
        if (outline == null) outline = textObj.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = new Color(0, 0, 0, 1);
        outline.effectDistance = new Vector2(2, -2);
        
        // Ensure changes are recorded
        Undo.RegisterCreatedObjectUndo(container, $"Create {name}");
        Undo.SetTransformParent(textObj.transform, container.transform, $"Reparent {name} Text");
        
        // Mark as dirty to ensure save
        EditorUtility.SetDirty(container);
        EditorUtility.SetDirty(textObj);
    }
}
