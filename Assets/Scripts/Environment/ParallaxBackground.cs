using UnityEngine;

namespace IceWorldRunner
{
    /// <summary>
    /// Multi-layer infinite scrolling parallax background.
    /// Automatically tiles elements as the player advances.
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        [System.Serializable]
        public class ParallaxLayer
        {
            public Transform transform;
            public float parallaxSpeedX = 0.2f;
            public float tileWidth = 24.0f;
            [HideInInspector] public Vector3 lastCameraPos;
        }

        public ParallaxLayer[] layers;
        private Camera cam;
        private Vector3 lastCameraPosition;

        private void Start()
        {
            cam = Camera.main;
            if (cam != null)
            {
                lastCameraPosition = cam.transform.position;
            }
        }

        private void LateUpdate()
        {
            if (cam == null || layers == null) return;

            Vector3 delta = cam.transform.position - lastCameraPosition;

            foreach (var layer in layers)
            {
                if (layer.transform == null) continue;

                // Move layer by camera delta scaled by layer speed
                layer.transform.position += new Vector3(delta.x * layer.parallaxSpeedX, 0, 0);

                // Check for infinite tiling reposition
                float distanceToCam = cam.transform.position.x - layer.transform.position.x;
                if (Mathf.Abs(distanceToCam) >= layer.tileWidth)
                {
                    float offset = (distanceToCam > 0 ? 1 : -1) * layer.tileWidth;
                    layer.transform.position += new Vector3(offset, 0, 0);
                }
            }

            lastCameraPosition = cam.transform.position;
        }
    }
}
