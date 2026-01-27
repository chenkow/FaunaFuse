using UnityEngine;
using TMPro; 
using System.Collections;

namespace Core
{
    public class Tile : MonoBehaviour
    {
        public int x;
        public int y;
        public int Level { get; private set; }
        public AnimalSO Data { get; private set; }
        public bool MergedThisTurn { get; set; }

        [SerializeField] private SpriteRenderer iconRenderer;
        [SerializeField] private TextMeshPro levelText; 
        
        private Vector3 baseScale; // Store the desired "100%" scale

        public void Init(int x, int y, AnimalSO data)
        {
            this.x = x;
            this.y = y;
            this.Level = data.level;
            this.Data = data;
            
            // Capture the scale set by BoardManager (e.g. 1.1) as our "Base"
            // If scale is (1,1,1) it will be 1. If (1.1, 1.1, 1), it be 1.1.
            this.baseScale = transform.localScale; 
            if(baseScale == Vector3.zero) baseScale = Vector3.one;

            UpdateVisuals();
            PlaySpawnAnimation();
        }

        public void Upgrade(AnimalSO newData)
        {
             this.Level = newData.level;
             this.Data = newData;
             UpdateVisuals();
             PlayMergeAnimation();
        }

        public void SetCoords(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        private void UpdateVisuals()
        {
            if (Data != null)
            {
                if (iconRenderer)
                {
                    iconRenderer.sprite = Data.icon;
                    
                    if(Data.icon) {
                         // Calculate scale to fit sprite in tile
                         Vector3 bounds = iconRenderer.sprite.bounds.size;
                         float max = Mathf.Max(bounds.x, bounds.y);
                         float scale = (max > 0) ? (1f / max) : 1f;
                         iconRenderer.transform.localScale = Vector3.one * scale;
                         
                         // Compensate for pivot offset: sprite.bounds.center gives the offset
                         // from the pivot point to the visual center of the sprite
                         Vector3 pivotOffset = iconRenderer.sprite.bounds.center * scale;
                         iconRenderer.transform.localPosition = -pivotOffset;
                    }
                    else
                    {
                         iconRenderer.transform.localScale = Vector3.one;
                         iconRenderer.transform.localPosition = Vector3.zero;
                    }
                }
                
                if (levelText) 
                {
                    levelText.text = Level.ToString();
                    levelText.enabled = (Data.icon == null);
                }
            }
        }

        public void PlaySpawnAnimation() 
        { 
            StartCoroutine(AnimateScale(Vector3.zero, baseScale, 0.2f)); 
        }
        
        public void PlayMergeAnimation() 
        { 
            // Pulse: Go 20% bigger than base, then back to base
            StartCoroutine(AnimateScale(baseScale * 1.2f, baseScale, 0.15f)); 
        }
        
        public void MoveTo(Vector3 targetPos)
        {
            StopAllCoroutines(); 
            // Ensure we are at base scale before moving (in case anim was interrupted)
            transform.localScale = baseScale;
            StartCoroutine(AnimateMove(targetPos, 0.15f));
        }

        private IEnumerator AnimateScale(Vector3 start, Vector3 end, float duration)
        {
            float t = 0;
            transform.localScale = start;
            while (t < 1)
            {
                t += Time.deltaTime / duration;
                transform.localScale = Vector3.Lerp(start, end, t); 
                yield return null;
            }
            transform.localScale = end;
        }

        private IEnumerator AnimateMove(Vector3 target, float duration)
        {
            Vector3 start = transform.localPosition;
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime / duration;
                transform.localPosition = Vector3.Lerp(start, target, t);
                yield return null;
            }
            transform.localPosition = target;
        }
    }
}