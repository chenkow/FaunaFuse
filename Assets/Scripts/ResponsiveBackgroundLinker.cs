using UnityEngine;

[ExecuteAlways]
public class ResponsiveBackgroundLinker : MonoBehaviour
{
    private void Update()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            if (canvas.worldCamera == null)
            {
                // More robust search for the camera
                canvas.worldCamera = Camera.main;
                if (canvas.worldCamera == null)
                {
                    Camera[] cameras = Object.FindObjectsOfType<Camera>();
                    foreach (Camera cam in cameras)
                    {
                        if (cam.name == "Main Camera" || cam.CompareTag("MainCamera"))
                        {
                            canvas.worldCamera = cam;
                            break;
                        }
                    }
                }
            }
            canvas.planeDistance = 15;
            canvas.sortingOrder = -100;
            
            // Fix Layering: Force this Canvas (and its children) to be behind siblings
            transform.SetAsFirstSibling();
            
            // Fix Internal Layering: Force the background UI to the back of the SafeArea
            Transform safeArea = transform.Find("SafeAreaContainer");
            if (safeArea != null)
            {
                safeArea.SetAsFirstSibling(); // SafeArea at back of Canvas
                Transform background = safeArea.Find("ResponsiveBoardBackground");
                if (background != null)
                {
                    background.SetAsFirstSibling(); // Background at back of SafeArea
                }
            }
        }
    }



}
