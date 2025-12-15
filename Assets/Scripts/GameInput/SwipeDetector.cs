using System;
using UnityEngine;
using UnityEngine.InputSystem; 

namespace GameInput
{
    public class SwipeDetector : MonoBehaviour
    {
        public static event Action<Vector2> OnSwipe;

        [SerializeField] private float minSwipeDistance = 50f;
        
        private Vector2 fingerDownPos;
        private Vector2 fingerUpPos;
        private bool isSwiping = false;

        private void Start()
        {
            Debug.Log("SwipeDetector Initialized (Input System Check)");
            if (Pointer.current == null) Debug.LogError("Pointer.current is NULL! Input System might not be ready or no pointer device detected.");
        }

        private void Update()
        {
            if (Pointer.current == null) return;

            // Pressed
            if (Pointer.current.press.wasPressedThisFrame)
            {
                isSwiping = true;
                fingerDownPos = Pointer.current.position.ReadValue();
                fingerUpPos = fingerDownPos;
                Debug.Log($"Pointer Down: {fingerDownPos}");
            }

            // Released
            if (isSwiping && Pointer.current.press.wasReleasedThisFrame)
            {
                isSwiping = false;
                fingerUpPos = Pointer.current.position.ReadValue();
                Debug.Log($"Pointer Up: {fingerUpPos}");
                DetectSwipe();
            }
        }

        private void DetectSwipe()
        {
            float distance = Vector2.Distance(fingerDownPos, fingerUpPos);
            Debug.Log($"Swipe Distance: {distance}");
            
            if (distance < minSwipeDistance) 
            {
                Debug.Log("Swipe too short.");
                return;
            }

            Vector2 direction = fingerUpPos - fingerDownPos;
            
            // Normalize
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                if (direction.x > 0) InvokeSwipe(Vector2.right);
                else InvokeSwipe(Vector2.left);
            }
            else
            {
                if (direction.y > 0) InvokeSwipe(Vector2.up);
                else InvokeSwipe(Vector2.down);
            }
        }

        private void InvokeSwipe(Vector2 dir)
        {
            Debug.Log($"Invoking Swipe: {dir}");
            OnSwipe?.Invoke(dir);
        }
    }
}