using System.Collections;
using UnityEngine;

namespace IceWorldRunner
{
    /// <summary>
    /// Lethal hanging icicle that shakes and drops when the player passes beneath.
    /// Shatters on impact with ground or player.
    /// </summary>
    [RequireComponent(typeof(PolygonCollider2D), typeof(Rigidbody2D))]
    public class FallingIcicle : MonoBehaviour
    {
        [Header("Drop Trigger")]
        public float triggerDistanceX = 4.0f;
        public float triggerDistanceY = 8.0f;
        public float shakeDuration = 0.35f;
        public float fallGravity = 3.5f;

        private Rigidbody2D rb;
        private PolygonCollider2D col;
        private Transform playerTarget;
        private Vector3 startPos;
        private bool isTriggered = false;
        private bool hasFallen = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<PolygonCollider2D>();
            startPos = transform.position;

            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
        }

        private void Start()
        {
            if (GameManager.Instance != null && GameManager.Instance.playerTransform != null)
            {
                playerTarget = GameManager.Instance.playerTransform;
            }
            else
            {
                PlayerController pc = FindAnyObjectByType<PlayerController>();
                if (pc != null) playerTarget = pc.transform;
            }
        }

        private void Update()
        {
            if (isTriggered || playerTarget == null) return;

            float dx = playerTarget.position.x - transform.position.x;
            float dy = transform.position.y - playerTarget.position.y;

            // Trigger when player is approaching just ahead or directly underneath
            if (dx >= -1.0f && dx <= triggerDistanceX && dy > 0 && dy <= triggerDistanceY)
            {
                StartCoroutine(ShakeAndFallRoutine());
            }
        }

        private IEnumerator ShakeAndFallRoutine()
        {
            isTriggered = true;

            // Warning shiver
            float elapsed = 0f;
            while (elapsed < shakeDuration)
            {
                float ox = Random.Range(-0.06f, 0.06f);
                transform.position = startPos + new Vector3(ox, 0, 0);
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Drop
            hasFallen = true;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = fallGravity;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!hasFallen) return;

            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.HitHazard("Icicle");
                Shatter();
                return;
            }

            if (other.CompareTag("Ground") || other.CompareTag("Ice") || other.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                Shatter();
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!hasFallen) return;

            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.HitHazard("Icicle");
            }

            Shatter();
        }

        public void Shatter()
        {
            SoundManager.Instance?.PlayShatter();
            CameraController.Instance?.Shake(0.12f, 0.15f);

            // Shard effect or cleanup
            Destroy(gameObject);
        }

        public void ResetIcicle()
        {
            StopAllCoroutines();
            transform.position = startPos;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
            isTriggered = false;
            hasFallen = false;
        }
    }
}
