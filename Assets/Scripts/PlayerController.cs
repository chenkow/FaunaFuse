using UnityEngine;

namespace GeoRunner
{
    public class PlayerController : MonoBehaviour
    {
        public float moveSpeed = 5f;
        private bool isMovingRight = false;

        void Update()
        {
            // Simple click/tap detection
            if (Input.GetMouseButtonDown(0))
            {
                SwitchLane();
            }

            // Move forward constantly
            transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
        }

        void SwitchLane()
        {
            isMovingRight = !isMovingRight;
            // Simple toggle for now - visual polish later
            float xPos = isMovingRight ? 2f : -2f;
            
            Vector3 newPos = transform.position;
            newPos.x = xPos;
            transform.position = newPos;
        }
    }
}
