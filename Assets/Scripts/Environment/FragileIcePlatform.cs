using System.Collections;
using UnityEngine;

namespace IceWorldRunner
{
    /// <summary>
    /// Thin, fragile ice platform that cracks upon being stepped on and collapses after a short delay.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
    public class FragileIcePlatform : MonoBehaviour
    {
        [Header("Timing")]
        public float collapseDelay = 0.75f;
        public float respawnDelay = 3.5f;

        [Header("Visual Feedback")]
        public float shakeIntensity = 0.08f;

        private BoxCollider2D boxCol;
        private SpriteRenderer spriteRenderer;
        private Vector3 initialPosition;
        private bool isTriggered = false;

        private void Awake()
        {
            boxCol = GetComponent<BoxCollider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            initialPosition = transform.position;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (isTriggered) return;

            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null && collision.contacts[0].normal.y < -0.5f)
            {
                StartCoroutine(CrackingAndCollapseRoutine());
            }
        }

        private IEnumerator CrackingAndCollapseRoutine()
        {
            isTriggered = true;
            SoundManager.Instance?.PlayCrack();

            float elapsed = 0f;
            while (elapsed < collapseDelay)
            {
                // Rapid shivering shake
                float offsetX = Random.Range(-shakeIntensity, shakeIntensity);
                float offsetY = Random.Range(-shakeIntensity, shakeIntensity);
                transform.position = initialPosition + new Vector3(offsetX, offsetY, 0);

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Collapse platform
            SoundManager.Instance?.PlayShatter();
            CameraController.Instance?.Shake(0.15f, 0.2f);
            boxCol.enabled = false;
            spriteRenderer.enabled = false;
            transform.position = initialPosition;

            // Wait for respawn
            yield return new WaitForSeconds(respawnDelay);

            // Re-enable
            boxCol.enabled = true;
            spriteRenderer.enabled = true;
            isTriggered = false;
        }

        public void ResetPlatform()
        {
            StopAllCoroutines();
            transform.position = initialPosition;
            if (boxCol != null) boxCol.enabled = true;
            if (spriteRenderer != null) spriteRenderer.enabled = true;
            isTriggered = false;
        }
    }
}
