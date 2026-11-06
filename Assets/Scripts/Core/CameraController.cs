using UnityEngine;

namespace IceWorldRunner
{
    /// <summary>
    /// Smooth follow camera with forward lookahead and procedural screen shake.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        public static CameraController Instance { get; private set; }

        [Header("Target & Offsets")]
        public Transform target;
        public Vector2 offset = new Vector2(4.5f, 1.8f);
        public float smoothTimeX = 0.12f;
        public float smoothTimeY = 0.25f;

        [Header("Limits")]
        public float minY = -2.0f;
        public float maxY = 8.0f;

        private float currentVelX;
        private float currentVelY;

        [Header("Screen Shake")]
        private float shakeTimeRemaining;
        private float shakeMagnitude;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            // Target positions with lookahead
            float targetX = target.position.x + offset.x;
            float targetY = Mathf.Clamp(target.position.y + offset.y, minY, maxY);

            float posX = Mathf.SmoothDamp(transform.position.x, targetX, ref currentVelX, smoothTimeX);
            float posY = Mathf.SmoothDamp(transform.position.y, targetY, ref currentVelY, smoothTimeY);

            Vector3 newPos = new Vector3(posX, posY, transform.position.z);

            // Apply screen shake offset
            if (shakeTimeRemaining > 0)
            {
                Vector2 shakeOffset = Random.insideUnitCircle * shakeMagnitude;
                newPos += new Vector3(shakeOffset.x, shakeOffset.y, 0);
                shakeTimeRemaining -= Time.deltaTime;
            }

            transform.position = newPos;
        }

        public void Shake(float duration = 0.25f, float magnitude = 0.25f)
        {
            shakeTimeRemaining = duration;
            shakeMagnitude = magnitude;
        }
    }
}
